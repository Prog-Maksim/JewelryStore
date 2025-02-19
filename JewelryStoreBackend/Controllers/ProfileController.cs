using System.ComponentModel.DataAnnotations;
using JewelryStoreBackend.Filters;
using JewelryStoreBackend.Models.Other;
using JewelryStoreBackend.Models.Request;
using JewelryStoreBackend.Models.Request.Profile;
using JewelryStoreBackend.Models.Response;
using JewelryStoreBackend.Repository.Interfaces;
using JewelryStoreBackend.Security;
using JewelryStoreBackend.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Address = JewelryStoreBackend.Models.DB.User.Address;

namespace JewelryStoreBackend.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Produces("application/json")]
[Route("api/v{version:apiVersion}/[controller]")]
public class ProfileController(AuthService authService, IUserRepository userRepository): ControllerBase
{
    private JwtTokenData GetUserIdFromToken()
    {
        var token = HttpContext.Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
        var dataToken = JwtController.GetJwtTokenData(token);
        return dataToken;
    }
    
    /// <summary>
    /// Выдает информацию о пользователе
    /// </summary>
    /// <returns></returns>
    /// <response code="200">Успешно</response>
    [Authorize]
    [HttpGet("person")]
    [ServiceFilter(typeof(ValidateUserIpFilter))]
    [ServiceFilter(typeof(ValidateJwtAccessTokenFilter))]
    [ProducesResponseType(typeof(PersonInform), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetPersonInform()
    {
        var dataToken = GetUserIdFromToken();
        var user = await userRepository.GetUserByIdAsync(dataToken.UserId);

        if (user == null)
        {
            var error = new BaseResponse
            {
                Message = "Пользователь не найден",
                Success = false,
                StatusCode = 404,
                Error = "Not found"
            };
            return StatusCode(error.StatusCode, error);
        }
        
        return Ok(new PersonInform
        {
            Name = user.Name,
            Surname = user.Surname,
            Patronymic = user.Patronymic,
            Email = user.Email,
            Adress = user.Adress,
            PhoneNumber = user.PhoneNumber
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
    [HttpPut("password")]
    [ServiceFilter(typeof(ValidateUserIpFilter))]
    [ServiceFilter(typeof(ValidateJwtAccessTokenFilter))]
    [ProducesResponseType(typeof(BaseResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(BaseResponse), StatusCodes.Status423Locked)]
    public async Task<IActionResult> UpdatePassword(UpdatePassword updatePassword)
    {
        var dataToken = GetUserIdFromToken();
        var response = await authService.UpdatePasswordAsync(dataToken.UserId, updatePassword);
        return StatusCode(response.StatusCode, response);
    }


    /// <summary>
    /// Возвращает все адреса пользователя
    /// </summary>
    /// <returns></returns>
    /// <response code="200">Успешное</response>
    /// <response code="404">Адреса не найдены</response>
    [Authorize]
    [HttpGet("address")]
    [ServiceFilter(typeof(ValidateUserIpFilter))]
    [ServiceFilter(typeof(ValidateJwtAccessTokenFilter))]
    [ProducesResponseType(typeof(List<Address>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(BaseResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetAddresses()
    {
        var dataToken = GetUserIdFromToken();
        var adresses = await userRepository.GetAddressesByUserIdAsync(dataToken.UserId);
        
        if (adresses.Count == 0)
        {
            var error = new BaseResponse
            {
                Success = false,
                Message = "Адреса не найдены!",
                StatusCode = 404,
                Error = "NotFound"
            };

            return StatusCode(error.StatusCode, error);
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
    [HttpPost("address")]
    [ServiceFilter(typeof(ValidateUserIpFilter))]
    [ServiceFilter(typeof(ValidateJwtAccessTokenFilter))]
    [ProducesResponseType(typeof(BaseResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(BaseResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(BaseResponse), StatusCodes.Status451UnavailableForLegalReasons)]
    public async Task<IActionResult> AddAddress([FromBody]AddAddress address)
    {
        var dataToken = GetUserIdFromToken();
        var response = await authService.AddUserAddress(dataToken.UserId, address);
        
        if (!response.Success)
            return StatusCode(response.StatusCode, response);
        
        return Ok(response);
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
    [HttpPut("address")]
    [ServiceFilter(typeof(ValidateUserIpFilter))]
    [ServiceFilter(typeof(ValidateJwtAccessTokenFilter))]
    [ProducesResponseType(typeof(BaseResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(BaseResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(BaseResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(BaseResponse), StatusCodes.Status451UnavailableForLegalReasons)]
    public async Task<IActionResult> UpdateAddress([Required][FromQuery]string addressId, [FromBody]AddAddress newAddress)
    {
        var dataToken = GetUserIdFromToken();
        var response = await authService.UpdateUserAddress(dataToken.UserId, addressId, newAddress);
        
        if (!response.Success)
            return StatusCode(response.StatusCode, response);
        
        return Ok(response);
    }
    
    /// <summary>
    /// Удаляет адрес пользователя
    /// </summary>
    /// <param name="addressId">Идентификатор удаляемого адреса</param>
    /// <returns></returns>
    /// <response code="200">Успешно</response>
    /// <response code="404">Адрес не найден</response>
    [Authorize]
    [HttpDelete("address")]
    [ServiceFilter(typeof(ValidateUserIpFilter))]
    [ServiceFilter(typeof(ValidateJwtAccessTokenFilter))]
    [ProducesResponseType(typeof(BaseResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(BaseResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteAddress([Required][FromQuery]string addressId)
    {
        var dataToken = GetUserIdFromToken();
        var response = await authService.DeleteUserAddress(dataToken.UserId, addressId);
        
        if (!response.Success)
            return StatusCode(response.StatusCode, response);
        
        return Ok(response);
    }

    /// <summary>
    /// Добавляет номер телефона
    /// </summary>
    /// <param name="data">Номер телефона</param>
    /// <returns></returns>
    /// <response code="200">Успешно</response>
    /// <response code="403">Не удалось добавить номер телефона</response>
    [Authorize]
    [HttpPost("phonenumber")]
    [ServiceFilter(typeof(ValidateUserIpFilter))]
    [ServiceFilter(typeof(ValidateJwtAccessTokenFilter))]
    [ProducesResponseType(typeof(BaseResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(BaseResponse), StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> AddPhoneNumber([Required][FromBody] AddPhoneNumberRequest data)
    {
        var dataToken = GetUserIdFromToken();
        var response = await authService.UpdatePhoneNumberAsync(dataToken.UserId, data.PhoneNumber);
        
        if (!response.Success)
            return StatusCode(response.StatusCode, response);
        
        return Ok(response);
    }
    
    /// <summary>
    /// Обновляет номер телефона
    /// </summary>
    /// <param name="data">Номер телефона</param>
    /// <returns></returns>
    /// <response code="200">Успешно</response>
    /// <response code="403">Не удалось обновить номер телефона</response>
    [Authorize]
    [HttpPut("phonenumber")]
    [ServiceFilter(typeof(ValidateUserIpFilter))]
    [ServiceFilter(typeof(ValidateJwtAccessTokenFilter))]
    [ProducesResponseType(typeof(BaseResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(BaseResponse), StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> UpdatePhoneNumber([Required][FromBody] AddPhoneNumberRequest data)
    {
        var dataToken = GetUserIdFromToken();
        var response = await authService.UpdatePhoneNumberAsync(dataToken.UserId, data.PhoneNumber);
        
        if (!response.Success)
            return StatusCode(response.StatusCode, response);
        
        return Ok(response);
    }
    
    /// <summary>
    /// Удаляет номер телефона
    /// </summary>
    /// <returns></returns>
    /// <response code="200">Успешно</response>
    [Authorize]
    [HttpDelete("phonenumber")]
    [ServiceFilter(typeof(ValidateUserIpFilter))]
    [ServiceFilter(typeof(ValidateJwtAccessTokenFilter))]
    [ProducesResponseType(typeof(BaseResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> DeletePhoneNumber()
    {
        var dataToken = GetUserIdFromToken();
        var response = await authService.DeletePhoneNumberAsync(dataToken.UserId);
        
        if (!response.Success)
            return StatusCode(response.StatusCode, response);
        
        return Ok(response);
    }
}