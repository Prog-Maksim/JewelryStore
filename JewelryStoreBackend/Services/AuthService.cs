using JewelryStoreBackend.Enums;
using JewelryStoreBackend.Models.AddressModel;
using JewelryStoreBackend.Models.DB;
using JewelryStoreBackend.Models.DB.User;
using JewelryStoreBackend.Models.Other;
using JewelryStoreBackend.Models.Request;
using JewelryStoreBackend.Models.Request.Profile;
using JewelryStoreBackend.Models.Response;
using JewelryStoreBackend.Repository.Interfaces;
using JewelryStoreBackend.Script;
using JewelryStoreBackend.Security;
using Microsoft.AspNetCore.Identity;
using Address = JewelryStoreBackend.Models.DB.User.Address;

namespace JewelryStoreBackend.Services;

public class AuthService
{
    private readonly IUserRepository _userRepository;
    private readonly PasswordHasher<Users> _passwordHasher = new();

    public AuthService(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }
    
    public async Task<BaseResponse> RegisterUserAsync(RegistrationUser registrationUser, string userIpAddress)
    {
        var existingUser = await _userRepository.GetUserByEmailAsync(registrationUser.Email);
        
        if (existingUser != null)
            return new BaseResponse { Message = "Пользователь с данным email уже существует", Error = "Forbidden", StatusCode = 403 };

        var user = new Users
        {
            PersonId = Guid.NewGuid().ToString(),
            Name = registrationUser.Name,
            Surname = registrationUser.Surname,
            Patronymic = registrationUser.Patronymic,
            Email = registrationUser.Email,
            PasswordVersion = 1,
            DateRegistration = DateTime.Now,
            IpAdressRegistration = userIpAddress,
            AddressRegistration = await DeterminingIpAddress.GetPositionUser(userIpAddress),
            Role = Roles.user,
            State = true
        };
        user.Password = _passwordHasher.HashPassword(user, registrationUser.Password);

        await _userRepository.AddUserAsync(user);
        await _userRepository.SaveChangesAsync();

        return new BaseResponse { Success = true, Message = "Вы успешно создали аккаунт!" };
    }
    
    public async Task<BaseResponse> AuthorizeUserAsync(AuthUser authUser)
    {
        var person = await _userRepository.GetUserByEmailAsync(authUser.Email);
        
        if (person == null)
            return new BaseResponse { Message = "Данный пользователь не найден", Error = "NotFound", StatusCode = 404 };
        if (_passwordHasher.VerifyHashedPassword(person, person.Password, authUser.Password) != PasswordVerificationResult.Success)
            return new BaseResponse { Message = "Логин или пароль не верен!", Error = "Forbidden", StatusCode = 403 };
        if (!person.State)
            return new BaseResponse { Message = "Ваш аккаунт заблокирован!", Error = "Forbidden", StatusCode = 423 };

        var accessToken = JwtController.GenerateJwtAccessToken(person.PersonId, person.Role);
        var refreshToken = JwtController.GenerateJwtRefreshToken(person.PersonId, person.PasswordVersion, person.Role);

        var tokens = new Tokens { PersonId = person.PersonId, AccessToken = accessToken, RefreshToken = refreshToken };

        await _userRepository.AddTokensAsync(tokens);
        await _userRepository.SaveChangesAsync();

        return new RegistrationRequests
        {
            Success = true,
            Message = "Вы успешно авторизовались!",
            AccessToken = accessToken,
            RefreshToken = refreshToken,
            TokenExpires = JwtController.AccessTokenLifetimeDay
        };
    }
    
    public async Task<BaseResponse> RefreshAccessTokenAsync(JwtTokenData dataToken)
    {
        var user = await _userRepository.GetUserByEmailAsync(dataToken.UserId);
        
        if (user == null || !user.State)
            return new BaseResponse { Message = "Пользователь не найден или был заблокирован!", Error = "Forbidden", StatusCode = 423 };

        if (JwtController.ValidateRefreshJwtToken(dataToken, user))
        {
            var accessToken = JwtController.GenerateJwtAccessToken(dataToken.UserId, user.Role);
            var newRefreshToken = JwtController.GenerateJwtRefreshToken(dataToken.UserId, dataToken.Version, user.Role);

            Tokens? tokens = await _userRepository.GetTokenByUserIdAsync(user.PersonId);
            
            tokens.AccessToken = accessToken;
            tokens.RefreshToken = newRefreshToken;

            await _userRepository.SaveChangesAsync();

            return new RegistrationRequests
            {
                Success = true,
                Message = "Токен успешно обновлен!",
                AccessToken = accessToken,
                RefreshToken = newRefreshToken,
                TokenExpires = JwtController.AccessTokenLifetimeDay
            };
        }
        return new BaseResponse { Message = "Не удалось проверить корректность jwt токена", Error = "Forbidden", StatusCode = 403 };
    }

    public async Task<BaseResponse> UpdatePasswordAsync(string userId, UpdatePassword updatePassword)
    {
        var user = await _userRepository.GetUserByIdAsync(userId);
        
        if (user == null)
            return new BaseResponse { Success = false, Message = "Пользователь не найден!", StatusCode = 404, Error = "NotFound" };
        
        if (_passwordHasher.VerifyHashedPassword(user, user.Password, updatePassword.OldPassword) !=
            PasswordVerificationResult.Success)
            return new BaseResponse { Success = false, Message = "Пароль не верен!", StatusCode = 423, Error = "Locked" };

        user.PasswordVersion += 1;
        user.Password = _passwordHasher.HashPassword(user, updatePassword.NewPassword);

        var tokens = await _userRepository.GetTokensByPersonIdAsync(userId);

        await _userRepository.AddTokensToBanAsync(userId, tokens.Select(p => p.AccessToken).ToList());
        _userRepository.DeleteTokensAsync(tokens);
        await _userRepository.SaveChangesAsync();

        return new BaseResponse { Message = "Пароль успешно обновлен!", Success = true };
    }

    public async Task<BaseResponse> AddUserAddress(string userId, AddAddress address)
    {
        List<Root>? results;
        
        try
        {
            results = await GeolocationService.GetGeolocatesAsync($"{address.Country} {address.City} {address.AddressLine1}");
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return new BaseResponse { Success = false, Message = "Не разрешено использовать прокси", StatusCode = 403, Error = "Forbidden" };
        }

        if (results == null || results.Count == 0)
            return new BaseResponse { Success = false, Message = "Указанный адрес неверен!", StatusCode = 400, Error = "BadRequest" };

        Root searchAddress = results.First();

        if (searchAddress.address.country != "Россия" )
            return new BaseResponse { Success = false, Message = "Можно добавлять только адреса в России", StatusCode = 451, Error = "Unavailable For Legal Reasons" };
        
        Address newAddress = new Address
        {
            AddressId = Guid.NewGuid().ToString(),
            PersonId = userId,
            Country = searchAddress.address.country,
            City = searchAddress.address.city,
            AddressLine1 = address.AddressLine1,
            AddressLine2 = address.AddressLine2,
            PostalCode = searchAddress.address.postcode,
            CreateAt = DateTime.Now,
            Lon = searchAddress.lon,
            Lat = searchAddress.lat,
        };
        await _userRepository.AddAddress(newAddress);
        await _userRepository.SaveChangesAsync();
        
        return new BaseResponse { Message = "Новый адрес успешно добавлен!", Success = true };
    }
    
    public async Task<BaseResponse> UpdateUserAddress(string userId, string addressId, AddAddress updateAddress)
    {
        var address = await _userRepository.GetAddressByIdAsync(userId, addressId);

        if (address == null)
            return new BaseResponse { Success = false, Message = "Данный адрес не найден!", StatusCode = 404, Error = "NotFound" };
        
        List<Root>? results;
        
        try
        {
            results = await GeolocationService.GetGeolocatesAsync($"{address.Country} {address.City} {address.AddressLine1}");
        }
        catch (Exception)
        {
            return new BaseResponse { Success = false, Message = "Не разрешено использовать прокси", StatusCode = 407, Error = "Proxy Authentication Required" };
        }

        if (results == null || results.Count == 0)
            return new BaseResponse { Success = false, Message = "Указанный адрес неверен!", StatusCode = 400, Error = "BadRequest" };

        Root searchAddress = results.First();

        if (searchAddress.address.country != "Россия" )
            return new BaseResponse { Success = false, Message = "Можно добавлять только адреса в России", StatusCode = 451, Error = "Unavailable For Legal Reasons" };
        
        address.Country = updateAddress.Country;
        address.City = updateAddress.City;
        address.AddressLine1 = updateAddress.AddressLine1;
        address.AddressLine2 = updateAddress.AddressLine2;
        address.PostalCode = searchAddress.address.postcode;
        address.UpdateAt = DateTime.Now;
        address.Lon = searchAddress.lon;
        address.Lat = searchAddress.lat;
        
        await _userRepository.SaveChangesAsync();
        
        return new BaseResponse { Message = "Адрес успешно обновлен!", Success = true };
    }

    public async Task<BaseResponse> DeleteUserAddress(string userId, string addressId)
    {
        var address = await _userRepository.GetAddressByIdAsync(userId, addressId);
        
        if (address == null)
            return new BaseResponse { Success = false, Message = "Данный адрес не найден!", StatusCode = 404, Error = "NotFound" };
        
        _userRepository.DeleteAddress(address);
        await _userRepository.SaveChangesAsync();
        
        return new BaseResponse { Message = "Адрес успешно удален", Success = true };
    }
    
    public async Task<BaseResponse> UpdatePhoneNumberAsync(string userId, string phoneNumber)
    {
        var existingUser = await _userRepository.GetUserByPhoneNumberAsync(phoneNumber);
        if (existingUser != null)
            return new BaseResponse { Success = false, Message = "Невозможно обновить номер телефона", StatusCode = 403, Error = "Forbidden" };

        var user = await _userRepository.GetUserByIdAsync(userId);
        
        if (user == null)
            return new BaseResponse { Success = false, Message = "Пользователь не найден!", StatusCode = 404, Error = "NotFound" };
        
        user.PhoneNumber = phoneNumber;
        await _userRepository.SaveChangesAsync();

        return new BaseResponse { Success = true, Message = "Номер телефона успешно обновлен!" };
    }
    
    public async Task<BaseResponse> DeletePhoneNumberAsync(string userId)
    {
        var user = await _userRepository.GetUserByIdAsync(userId);
        
        if (user == null)
            return new BaseResponse { Success = false, Message = "Пользователь не найден!", StatusCode = 404, Error = "NotFound" };
        
        user.PhoneNumber = null;
        await _userRepository.SaveChangesAsync();

        return new BaseResponse { Success = true, Message = "Номер телефона успешно удален!" };
    }
}