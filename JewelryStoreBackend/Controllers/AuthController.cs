using JewelryStoreBackend.Enums;
using JewelryStoreBackend.Filters;
using JewelryStoreBackend.Models;
using JewelryStoreBackend.Models.DB;
using JewelryStoreBackend.Models.DB.User;
using JewelryStoreBackend.Models.Request;
using JewelryStoreBackend.Models.Response;
using JewelryStoreBackend.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StackExchange.Redis;

namespace JewelryStoreBackend.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Produces("application/json")]
[Route("api/v{version:apiVersion}/[controller]")]
public class AuthController(ApplicationContext context, IConnectionMultiplexer redis): ControllerBase
{
    private readonly PasswordHasher<Person> _passwordHasher = new();
    
    /// <summary>
    /// Регистрирует нового пользователя
    /// </summary>
    /// <param name="registeredRegistrationUser">Информация о пользователе</param>
    /// <remarks>
    ///Пример запроса:
    /// 
    ///     POST /api/{version}/Auth/registration
    ///     {
    ///         "name": "string",
    ///         "surname": "string",
    ///         "patronymic": "string",
    ///         "email": "string",
    ///         "password": "string"
    ///     }
    /// 
    /// </remarks>
    /// <returns>Успешное создание пользователя</returns>
    /// <response code="200">Успешная регистрация нового пользователя</response>
    /// <response code="400">Невозможно определить IP адрес пользователя</response>
    /// <response code="403">Данный пользователь уже существует</response>
    [AllowAnonymous] [MapToApiVersion("1.0")]
    [HttpPost("registration")]
    [ServiceFilter(typeof(ValidateUserIpFilter))]
    [ProducesResponseType(typeof(BaseResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(BaseResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(BaseResponse), StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> RegistrationUser(RegistrationUser registeredRegistrationUser)
    {
        string? userIpAddress = HttpContext.Request.Headers["X-Forwarded-For"].FirstOrDefault() ?? HttpContext.Connection.RemoteIpAddress?.ToString();
        var person = await context.Users.FirstOrDefaultAsync(p => p.Email == registeredRegistrationUser.Email);

        if (person != null)
        {
            var errorResponce = new BaseResponse
            {
                Message = "Пользователь с данным email уже существует",
                Error = "Forbidden",
                StatusCode = 403
            };
            return StatusCode(errorResponce.StatusCode, errorResponce);
        }

        var user = new Person
        {
            PersonId = Guid.NewGuid().ToString(),
            Name = registeredRegistrationUser.Name,
            Surname = registeredRegistrationUser.Surname,
            Patronymic = registeredRegistrationUser.Patronymic,
            Email = registeredRegistrationUser.Email,
            PasswordVersion = 1,
            DateRegistration = DateTime.Now,
            IpAdressRegistration = userIpAddress,
            AdressRegistration = await DeterminingIpAddress.GetPositionUser(userIpAddress),
            Role = Roles.user,
            State = true
        };
        user.Password = _passwordHasher.HashPassword(user, registeredRegistrationUser.Password);
        
        await context.Users.AddAsync(user);
        await context.SaveChangesAsync();
        
        var responseResult = new BaseResponse
        {
            Success = true,
            Message = "Вы успешно создали аккаунт!"
        };

        return Ok(responseResult);
    }

    /// <summary>
    /// Авторизует созданного пользователя
    /// </summary>
    /// <param name="authUser">Данные пользователя</param>
    /// <returns>Успешная авторизация пользователя</returns>
    /// <remarks>
    ///Пример запроса:
    /// 
    ///     POST /api/{version}/Auth/authorization
    ///     {
    ///         "email": "string",
    ///         "password": "string"
    ///     }
    /// 
    /// </remarks>
    /// <response code="200">Успешная авторизация пользователя</response>
    /// <response code="400">Невозможно определить IP адрес пользователя</response>
    /// <response code="403">Логин или пароль не верен</response>
    /// <response code="404">Пользователь не найден</response>
    /// <response code="423">Аккаунт пользователя был заблокирован</response>
    [AllowAnonymous] [MapToApiVersion("1.0")]
    [HttpPost("authorization")]
    [ServiceFilter(typeof(ValidateUserIpFilter))]
    [ProducesResponseType(typeof(RegistrationRequests), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(BaseResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(BaseResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(BaseResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(BaseResponse), StatusCodes.Status423Locked)]
    public async Task<IActionResult> Authorization(AuthUser authUser)
    {
        var person = await context.Users.FirstOrDefaultAsync(p => p.Email == authUser.Email);

        if (person == null)
        {
            var errorResponce = new BaseResponse
            {
                Message = "Данный пользователь не найден",
                Error = "NotFound",
                StatusCode = 404
            };
            return StatusCode(errorResponce.StatusCode, errorResponce);
        }

        if (_passwordHasher.VerifyHashedPassword(person, person.Password, authUser.Password) !=
            PasswordVerificationResult.Success)
        {
            var errorResponce = new BaseResponse
            {
                Message = "Логин или пароль не верен!",
                Error = "Forbidden",
                StatusCode = 403
            };
            return StatusCode(errorResponce.StatusCode, errorResponce);
        }

        if (!person.State)
        {
            var errorResponce = new BaseResponse
            {
                Message = "Ваш аккаунт заблокирован!",
                Error = "Forbidden",
                StatusCode = 423
            };
            return StatusCode(errorResponce.StatusCode, errorResponce);
        }

        var accessToken = JwtController.GenerateJwtAccessToken(person.PersonId, person.Role);
        var refreshToken = JwtController.GenerateJwtRefreshToken(person.PersonId, person.PasswordVersion, person.Role);

        Tokens tokens = new Tokens
        {
            PersonId = person.PersonId,
            AccessToken = accessToken,
            RefreshToken = refreshToken,
        };
        
        await context.Tokens.AddAsync(tokens);
        await context.SaveChangesAsync();
        
        var responseResult = new RegistrationRequests
        {
            Success = true,
            Message = "Вы успешно авторизовались!",
            access_token = accessToken,
            refresh_token = refreshToken,
            token_expires = JwtController.AccessTokenLifetimeDay
        };

        return Ok(responseResult);
    }
    
    /// <summary>
    /// Обновляет access токен пользователя
    /// </summary>
    /// <returns>Успешное обновление токена</returns>
    /// <remarks>
    ///Пример запроса:
    /// 
    ///     PUT /api/{version}/Auth/refresh-token
    ///
    ///Требуется передать JWT refresh токен в заголовке Authorization
    ///
    ///     "Authorization": "Bearer {refresh токен}"
    /// 
    /// </remarks>
    /// <response code="200">Успешное обновление токена</response>
    /// <response code="400">Невозможно определить IP адрес пользователя</response>
    /// <response code="403">Некорректный токен</response>
    /// <response code="423">Аккаунт пользователя был заблокирован</response>
    [Authorize] [MapToApiVersion("1.0")]
    [HttpPut("refresh-token")]
    [ServiceFilter(typeof(ValidateUserIpFilter))]
    [ProducesResponseType(typeof(RegistrationRequests), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(BaseResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(BaseResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(BaseResponse), StatusCodes.Status423Locked)]
    public async Task<IActionResult> RefreshToken()
    {
        var authHeader = HttpContext.Request.Headers["Authorization"].ToString();
        var token = authHeader.Substring("Bearer ".Length);
        
        var database = redis.GetDatabase();

        var dataToken = JwtController.GetJwtTokenData(token);
        var user = await context.Users.FirstOrDefaultAsync(p => p.PersonId == dataToken.UserId);

        if (user == null || !user.State)
        {
            var errorResponce = new BaseResponse
            {
                Message = "Пользователь не найден или был заблокирован!",
                Error = "Forbidden",
                StatusCode = 423
            };
            return StatusCode(errorResponce.StatusCode, errorResponce);
        }

        if (await JwtController.IsTokenBannedAsync(database, user.PersonId, token))
        {
            List<Tokens> tokens = await context.Tokens.Where(p => p.PersonId == user.PersonId).ToListAsync();
            List<string> tokensToBan = new List<string>();

            foreach (var item in tokens)
            {
                tokensToBan.Add(item.AccessToken);
            }
            
            await JwtController.AddTokensToBan(database, dataToken.UserId, tokensToBan);
            
            tokens.Clear();
            foreach (var item in tokens)
            {
                tokensToBan.Add(item.RefreshToken);
            }
            
            await JwtController.AddTokensToBan(database, dataToken.UserId, tokensToBan, JwtController.RefreshTokenLifetimeDay);

            context.Tokens.RemoveRange(tokens);
            await context.SaveChangesAsync();
            
            var error = new RegistrationRequests
            {
                Success = false,
                Message = "Отказано в доступе!",
                StatusCode = 403,
                Error = "Forbidden"
            };

            return StatusCode(error.StatusCode, error);
        }

        if (await JwtController.ValidateRefreshJwtToken(dataToken, user))
        {
            var accessToken = JwtController.GenerateJwtAccessToken(dataToken.UserId, user.Role);
            var refreshToken = JwtController.GenerateJwtRefreshToken(dataToken.UserId, dataToken.Version, user.Role);
            
            Tokens? tokens = await context.Tokens.FirstOrDefaultAsync(p => p.PersonId == user.PersonId && p.RefreshToken == token);
            
            List<string> tokensToBan = new List<string> { tokens.AccessToken, tokens.RefreshToken };
            await JwtController.AddTokensToBan(database, dataToken.UserId, tokensToBan);
            
            tokens.AccessToken = accessToken;
            tokens.RefreshToken = refreshToken;
            
            await context.SaveChangesAsync();
        
            var responseResult = new RegistrationRequests
            {
                Success = true,
                Message = "Токен успешно обновлен!",
                access_token = accessToken,
                refresh_token = refreshToken,
                token_expires = JwtController.AccessTokenLifetimeDay
            };

            return Ok(responseResult);
        }
        
        var errorMessage = new RegistrationRequests
        {
            Success = false,
            Message = "Не удалось проверить корректность jwt токена",
            StatusCode = 403,
            Error = "Forbidden"
        };

        return StatusCode(errorMessage.StatusCode, errorMessage);
    }
}