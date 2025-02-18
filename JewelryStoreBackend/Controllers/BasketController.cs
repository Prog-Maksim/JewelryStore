using System.ComponentModel.DataAnnotations;
using JewelryStoreBackend.Enums;
using JewelryStoreBackend.Filters;
using JewelryStoreBackend.Models.DB;
using JewelryStoreBackend.Models.Other;
using JewelryStoreBackend.Models.Response;
using JewelryStoreBackend.Models.Response.Basket;
using JewelryStoreBackend.Security;
using JewelryStoreBackend.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace JewelryStoreBackend.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Produces("application/json")]
[Route("api/v{version:apiVersion}/[controller]")]
public class BasketController(BasketService basketService): ControllerBase
{
    private JwtTokenData GetUserIdFromToken()
    {
        var token = HttpContext.Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
        var dataToken = JwtController.GetJwtTokenData(token);
        return dataToken;
    }
    
    /// <summary>
    /// Возвращает все товары в корзине
    /// </summary>
    /// <param name="languageCode">Языковой код</param>
    /// <returns></returns>
    /// <response code="200">Все товары в системе</response>
    /// <response code="400">Невозможно определить IP адрес пользователя</response>
    /// <response code="403">Некорректный токен</response>
    /// <response code="404">Товары не найдены</response>
    [Authorize]
    [HttpGet("product")]
    [ServiceFilter(typeof(ValidateUserIpFilter))]
    [ServiceFilter(typeof(ValidateJwtAccessTokenFilter))]
    [ProducesResponseType(typeof(BasketResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(BaseResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(BaseResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(BaseResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetProductInBasket([Required][FromQuery] string languageCode)
    {
        var dataToken = GetUserIdFromToken();
        var (response, result) = await basketService.GetProductInBasket(dataToken.UserId, languageCode);
        
        if (!response.Success)
            return StatusCode(response.StatusCode, response);
        return Ok(result);
    }

    /// <summary>
    /// Указывает есть ли товар в корзине
    /// </summary>
    /// <param name="productId">Идентификатор товара</param>
    /// <returns></returns>
    /// <response code="200">Есть ли товар в сисетме</response>
    /// <response code="400">Невозможно определить IP адрес пользователя</response>
    /// <response code="403">Некорректный токен</response>
    /// <response code="404">Товар не найден</response>
    [Authorize]
    [HttpGet("check-product")]
    [ServiceFilter(typeof(ValidateUserIpFilter))]
    [ServiceFilter(typeof(ValidateJwtAccessTokenFilter))]
    [ProducesResponseType(typeof(Basket), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(BaseResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(BaseResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(BaseResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> CheckProductInBasket([Required][FromQuery] string productId)
    {
        var dataToken = GetUserIdFromToken();
        var (response, result) = await basketService.CheckProductInBasket(dataToken.UserId, productId);
        
        if (!response.Success)
            return StatusCode(response.StatusCode, response);
        return Ok(result);
    }
    
    /// <summary>
    /// Добавляет товар(ы) в корзину
    /// </summary>
    /// <param name="productId">Идентификатор товара</param>
    /// <param name="quantity">Кол-во товара</param>
    /// <returns></returns>
    /// <response code="200">Успешно</response>
    /// <response code="400">Невозможно определить IP адрес пользователя</response>
    /// <response code="400">Введено отрицательное число в quantity</response>
    /// <response code="403">Некорректный токен</response>
    [Authorize]
    [HttpPost("product")]
    [ServiceFilter(typeof(ValidateUserIpFilter))]
    [ServiceFilter(typeof(ValidateJwtAccessTokenFilter))]
    [ProducesResponseType(typeof(BaseResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(BaseResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(BaseResponse), StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> AddProductInBasket([Required][FromQuery] string productId, [FromQuery] int quantity = 1)
    {
        var dataToken = GetUserIdFromToken();
        var result = await basketService.AddProductInBasket(dataToken.UserId, productId, quantity);
        return StatusCode(result.StatusCode, result);
    }
    
    /// <summary>
    /// Удаляет товар(ы) из корзины
    /// </summary>
    /// <param name="productId">Идентификатор товара</param>
    /// <param name="quantity">Один или все товары</param>
    /// <returns></returns>
    /// <response code="200">Успешно</response>
    /// <response code="400">Невозможно определить IP адрес пользователя</response>
    /// <response code="403">Некорректный токен</response>
    /// <response code="404">Товар не найден</response>
    [Authorize]
    [HttpDelete("product")]
    [ServiceFilter(typeof(ValidateUserIpFilter))]
    [ServiceFilter(typeof(ValidateJwtAccessTokenFilter))]
    [ProducesResponseType(typeof(BaseResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(BaseResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(BaseResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(BaseResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteProductInBasket([Required][FromQuery] string productId, [Required][FromQuery] Count quantity)
    {
        var dataToken = GetUserIdFromToken();
        var result = await basketService.DeleteProductInBasket(dataToken.UserId, productId, quantity);
        return StatusCode(result.StatusCode, result);
    }
}