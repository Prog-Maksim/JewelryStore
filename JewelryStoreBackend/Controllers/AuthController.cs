using JewelryStoreBackend.Filters;
using JewelryStoreBackend.Models.Request;
using JewelryStoreBackend.Models.Response;
using JewelryStoreBackend.Security;
using JewelryStoreBackend.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace JewelryStoreBackend.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Produces("application/json")]
[Route("api/v{version:apiVersion}/[controller]")]
public class AuthController(AuthService authService): ControllerBase
{
    /// <summary>
    /// Регистрирует нового пользователя
    /// </summary>
    /// <param name="registrationUser">Информация о пользователе</param>
    /// <returns>Успешное создание пользователя</returns>
    /// <response code="200">Успешная регистрация нового пользователя</response>
    /// <response code="403">Данный пользователь уже существует</response>
    [AllowAnonymous] [MapToApiVersion("1.0")]
    [HttpPost("registration")]
    [ServiceFilter(typeof(ValidateUserIpFilter))]
    [ProducesResponseType(typeof(BaseResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(BaseResponse), StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> RegistrationUser(RegistrationUser registrationUser)
    {
        string? userIpAddress = HttpContext.Request.Headers["X-Forwarded-For"].FirstOrDefault() ?? HttpContext.Connection.RemoteIpAddress?.ToString();

        if (userIpAddress == null)
        {
            var error = new BaseResponse
            {
                Message = "Невозможно определить ip адрес",
                Success = false,
                StatusCode = 400,
                Error = "Bad Request"
            };
            return StatusCode(error.StatusCode, error);
        }
        
        var response = await authService.RegisterUserAsync(registrationUser, userIpAddress);
        
        if (!response.Success)
            return StatusCode(response.StatusCode, response);
        
        return Ok(response);
    }

    /// <summary>
    /// Авторизует созданного пользователя
    /// </summary>
    /// <param name="authUser">Данные пользователя</param>
    /// <returns>Успешная авторизация пользователя</returns>
    /// <response code="200">Успешная авторизация пользователя</response>
    /// <response code="403">Логин или пароль не верен</response>
    /// <response code="404">Пользователь не найден</response>
    /// <response code="423">Аккаунт пользователя был заблокирован</response>
    [AllowAnonymous] [MapToApiVersion("1.0")]
    [HttpPost("authorization")]
    [ServiceFilter(typeof(ValidateUserIpFilter))]
    [ProducesResponseType(typeof(RegistrationRequests), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(BaseResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(BaseResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(BaseResponse), StatusCodes.Status423Locked)]
    public async Task<IActionResult> Authorization(AuthUser authUser)
    {
        var response = await authService.AuthorizeUserAsync(authUser);
        
        if (!response.Success)
            return StatusCode(response.StatusCode, response);
        
        return Ok(response);
    }
    
    /// <summary>
    /// Обновляет access токен пользователя
    /// </summary>
    /// <returns>Успешное обновление токена</returns>
    /// <response code="200">Успешное обновление токена</response>
    /// <response code="403">Некорректный токен</response>
    /// <response code="423">Аккаунт пользователя был заблокирован</response>
    [Authorize] [MapToApiVersion("1.0")]
    [HttpPut("refresh-token")]
    [ServiceFilter(typeof(ValidateUserIpFilter))]
    [ProducesResponseType(typeof(RegistrationRequests), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(BaseResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(BaseResponse), StatusCodes.Status423Locked)]
    public async Task<IActionResult> RefreshToken()
    {
        var authHeader = HttpContext.Request.Headers["Authorization"].ToString();
        var token = authHeader.Substring("Bearer ".Length);
        var dataToken = JwtController.GetJwtTokenData(token);
        
        var response = await authService.RefreshAccessTokenAsync(dataToken);
        
        if (!response.Success)
            return StatusCode(response.StatusCode, response);
        
        return Ok(response);
    }
}