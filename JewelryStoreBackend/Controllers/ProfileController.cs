using System.ComponentModel.DataAnnotations;
using JewelryStoreBackend.Filters;
using JewelryStoreBackend.Models.AddressModel;
using JewelryStoreBackend.Models.DB.User;
using JewelryStoreBackend.Models.Request;
using JewelryStoreBackend.Models.Request.Profile;
using JewelryStoreBackend.Models.Response;
using JewelryStoreBackend.Script;
using JewelryStoreBackend.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StackExchange.Redis;
using Address = JewelryStoreBackend.Models.DB.User.Address;

namespace JewelryStoreBackend.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Produces("application/json")]
[Route("api/v{version:apiVersion}/[controller]")]
public class ProfileController(ApplicationContext context, IConnectionMultiplexer redis): ControllerBase
{
    private readonly PasswordHasher<Person> _passwordHasher = new();
    
    /// <summary>
    /// Выдает информацию о пользователе
    /// </summary>
    /// <returns></returns>
    /// <response code="200">Успешно</response>
    [Authorize]
    [HttpPost("person")]
    [ServiceFilter(typeof(ValidateUserIpFilter))]
    [ServiceFilter(typeof(ValidateJwtAccessTokenFilter))]
    [ProducesResponseType(typeof(PersonInform), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetPersonInform()
    {
        var authHeader = HttpContext.Request.Headers["Authorization"].ToString();
        var token = authHeader.Substring("Bearer ".Length);

        var dataToken = JwtController.GetJwtTokenData(token);
        var user = await context.Users.FirstOrDefaultAsync(u => u.PersonId == dataToken.UserId);
        
        return Ok(new PersonInform
        {
            Name = user.Name,
            Surname = user.Surname,
            Patronymic = user.Patronymic,
            Email = user.Email,
            Adress = user.Adress
        });
    }
    
    /// <summary>
    /// Изменяет пароль пользователя
    /// </summary>
    /// <param name="updatePassword"></param>
    /// <returns></returns>
    /// <response code="200">Успешное изменение пароля</response>
    /// <response code="423">Отказано в изменение пароля</response>
    [Authorize]
    [ServiceFilter(typeof(ValidateUserIpFilter))]
    [ServiceFilter(typeof(ValidateJwtAccessTokenFilter))]
    [HttpPut("password")]
    [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(BaseResponse), StatusCodes.Status423Locked)]
    public async Task<IActionResult> UpdatePassword(UpdatePassword updatePassword)
    {
        var authHeader = HttpContext.Request.Headers["Authorization"].ToString();
        var token = authHeader.Substring("Bearer ".Length);

        var dataToken = JwtController.GetJwtTokenData(token);
        var database = redis.GetDatabase();
        
        var user = await context.Users.FirstOrDefaultAsync(u => u.PersonId == dataToken.UserId);

        if (_passwordHasher.VerifyHashedPassword(user, user.Password, updatePassword.OldPassword) !=
            PasswordVerificationResult.Success)
        {
            var error = new BaseResponse
            {
                Success = false,
                Message = "Пароль не верен!",
                ErrorCode = 423,
                Error = "Locked"
            };

            return StatusCode(error.ErrorCode, error);
        }

        user.PasswordVersion += 1;
        user.Password = _passwordHasher.HashPassword(user, updatePassword.NewPassword);
        
        var tokens = await context.Tokens.Where(p => p.PersonId == dataToken.UserId).ToListAsync();

        await JwtController.AddTokensToBan(database, dataToken.UserId, tokens.Select(p => p.AccessToken).ToList());

        context.RemoveRange(tokens);
        await context.SaveChangesAsync();

        return Ok("Пароль успешно обновлен!");
    }


    /// <summary>
    /// Возвращает все адреса пользователя
    /// </summary>
    /// <returns></returns>
    /// <response code="200">Успешное</response>
    /// <response code="404">Адреса не найдены</response>
    [Authorize]
    [ServiceFilter(typeof(ValidateUserIpFilter))]
    [ServiceFilter(typeof(ValidateJwtAccessTokenFilter))]
    [HttpGet("address")]
    [ProducesResponseType(typeof(List<Address>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(BaseResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetAdresses()
    {
        var authHeader = HttpContext.Request.Headers["Authorization"].ToString();
        var token = authHeader.Substring("Bearer ".Length);

        var dataToken = JwtController.GetJwtTokenData(token);
        
        List<Address> adresses = await context.Address.Where(a => a.PersonId == dataToken.UserId).ToListAsync();
        
        if (adresses.Count == 0)
        {
            var error = new BaseResponse
            {
                Success = false,
                Message = "Адреса не найдены!",
                ErrorCode = 404,
                Error = "NotFound"
            };

            return StatusCode(error.ErrorCode, error);
        }
        
        return Ok(adresses);
    }
    
    /// <summary>
    /// Добвляет новый адрес пользователя
    /// </summary>
    /// <param name="address">Инофрмация об адресе</param>
    /// <returns></returns>
    /// <response code="200">Успешно</response>
    /// <response code="400">Неверный адрес</response>
    /// <response code="451">Запрещено добавлять адреса вне России</response>
    [Authorize]
    [ServiceFilter(typeof(ValidateUserIpFilter))]
    [ServiceFilter(typeof(ValidateJwtAccessTokenFilter))]
    [HttpPost("address")]
    [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(BaseResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(BaseResponse), StatusCodes.Status451UnavailableForLegalReasons)]
    public async Task<IActionResult> AddAdress([FromBody]AddAddress address)
    {
        var authHeader = HttpContext.Request.Headers["Authorization"].ToString();
        var token = authHeader.Substring("Bearer ".Length);

        var dataToken = JwtController.GetJwtTokenData(token);

        List<Root>? results = await GeolocationService.GetGeolocatesAsync($"{address.Country} {address.City} {address.AddressLine1}");

        if (results == null || results.Count == 0)
        {
            var error = new BaseResponse
            {
                Success = false,
                Message = "Указанный адрес неверен!",
                ErrorCode = 400,
                Error = "BadRequest"
            };

            return StatusCode(error.ErrorCode, error);
        }

        Root searchAddress = results.First();

        if (searchAddress.address.country != "Россия" )
        {
            var error = new BaseResponse
            {
                Success = false,
                Message = "Можно добавлять только адреса в России",
                ErrorCode = 451,
                Error = "Unavailable For Legal Reasons"
            };

            return StatusCode(error.ErrorCode, error);
        }
        
        Address newAddress = new Address
        {
            AddressId = Guid.NewGuid().ToString(),
            PersonId = dataToken.UserId,
            Country = searchAddress.address.country,
            City = searchAddress.address.city,
            AddressLine1 = address.AddressLine1,
            AddressLine2 = address.AddressLine2,
            PostalCode = searchAddress.address.postcode,
            CreateAt = DateTime.Now,
            lon = searchAddress.lon,
            lat = searchAddress.lat,
        };
        await context.Address.AddAsync(newAddress);
        await context.SaveChangesAsync();
        
        return Ok("Новый адрес успешно добавлен!");
    }
    
    /// <summary>
    /// Обновляет адрес пользователя
    /// </summary>
    /// <param name="addressId">Идентификатор изменяемого адреса</param>
    /// <param name="newAddress">Новая информация об адресе</param>
    /// <returns></returns>
    /// <response code="200">Успешно</response>
    /// <response code="404">Адрес не найден</response>
    /// <response code="400">Неверный адрес</response>
    /// <response code="451">Запрещено добавлять адреса вне России</response>
    [Authorize]
    [ServiceFilter(typeof(ValidateUserIpFilter))]
    [ServiceFilter(typeof(ValidateJwtAccessTokenFilter))]
    [HttpPut("address")]
    [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(BaseResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(BaseResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(BaseResponse), StatusCodes.Status451UnavailableForLegalReasons)]
    public async Task<IActionResult> UpdateAdress([Required][FromQuery]string addressId, [FromBody]AddAddress newAddress)
    {
        var authHeader = HttpContext.Request.Headers["Authorization"].ToString();
        var token = authHeader.Substring("Bearer ".Length);

        var dataToken = JwtController.GetJwtTokenData(token);

        var address = await context.Address.FirstOrDefaultAsync(p => p.PersonId == dataToken.UserId && p.AddressId == addressId);

        if (address == null)
        {
            var error = new BaseResponse
            {
                Success = false,
                Message = "Адрес не найден!",
                ErrorCode = 404,
                Error = "NotFound"
            };

            return StatusCode(error.ErrorCode, error);
        }
        
        List<Root>? results = await GeolocationService.GetGeolocatesAsync($"{address.Country} {address.City} {newAddress.AddressLine1}");

        if (results == null || results.Any())
        {
            var error = new BaseResponse
            {
                Success = false,
                Message = "Указанный адрес неверен!",
                ErrorCode = 400,
                Error = "BadRequest"
            };

            return StatusCode(error.ErrorCode, error);
        }

        Root searchAddress = results.First();

        if (searchAddress.address.country != "Россия" )
        {
            var error = new BaseResponse
            {
                Success = false,
                Message = "Можно добавлять только адреса в России",
                ErrorCode = 451,
                Error = "Unavailable For Legal Reasons"
            };

            return StatusCode(error.ErrorCode, error);
        }
        
        address.Country = newAddress.Country;
        address.City = newAddress.City;
        address.AddressLine1 = newAddress.AddressLine1;
        address.AddressLine2 = newAddress.AddressLine2;
        address.PostalCode = searchAddress.address.postcode;
        address.UpdateAt = DateTime.Now;
        address.lon = searchAddress.lon;
        address.lat = searchAddress.lat;
        
        await context.SaveChangesAsync();
        
        return Ok("Адрес успешно обновлен");
    }
    
    /// <summary>
    /// Удаляет адрес пользователя
    /// </summary>
    /// <param name="addressId">Идентификатор удаляемого адреса</param>
    /// <returns></returns>
    /// <response code="200">Успешно</response>
    /// <response code="404">Адрес не найден</response>
    [Authorize]
    [ServiceFilter(typeof(ValidateUserIpFilter))]
    [ServiceFilter(typeof(ValidateJwtAccessTokenFilter))]
    [HttpDelete("address")]
    [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(BaseResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteAdress([Required][FromQuery]string addressId)
    {
        var authHeader = HttpContext.Request.Headers["Authorization"].ToString();
        var token = authHeader.Substring("Bearer ".Length);

        var dataToken = JwtController.GetJwtTokenData(token);
        
        var address = await context.Address.FirstOrDefaultAsync(p => p.PersonId == dataToken.UserId && p.AddressId == addressId);

        if (address == null)
        {
            var error = new BaseResponse
            {
                Success = false,
                Message = "Адрес не найден!",
                ErrorCode = 404,
                Error = "NotFound"
            };

            return StatusCode(error.ErrorCode, error);
        }
        
        context.Address.Remove(address);
        await context.SaveChangesAsync();
        
        return Ok("Адрес успешно удален");
    }
}