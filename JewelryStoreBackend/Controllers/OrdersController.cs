using System.ComponentModel.DataAnnotations;
using JewelryStoreBackend.Filters;
using JewelryStoreBackend.Models.Other;
using JewelryStoreBackend.Models.Request;
using JewelryStoreBackend.Models.Response;
using JewelryStoreBackend.Models.Response.Order;
using JewelryStoreBackend.Repository.Interfaces;
using JewelryStoreBackend.Script;
using JewelryStoreBackend.Security;
using JewelryStoreBackend.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace JewelryStoreBackend.Controllers;

[Authorize]
[ApiController]
[ApiVersion("1.0")]
[Produces("application/json")]
[Route("api/v{version:apiVersion}/[controller]")]
public class OrdersController(
    OrderService orderService,
    IOrderRepository orderRepository): ControllerBase
{
    private JwtTokenData GetUserIdFromToken()
    {
        var token = HttpContext.Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
        var dataToken = JwtController.GetJwtTokenData(token);
        return dataToken;
    }
    
    /// <summary>
    /// Начинает оформление заказа
    /// </summary>
    /// <param name="languageCode">Языковой код</param>
    /// <returns></returns>
    /// <response code="200">Успешная инициализация заказа</response>
    /// <response code="404">Товары отсутствуют в корзине</response>
    /// <response code="400">Заказ уже оформляется</response>
    [Authorize]
    [HttpPost("initiate")]
    [ServiceFilter(typeof(ValidateUserIpFilter))]
    [ServiceFilter(typeof(ValidateJwtAccessTokenFilter))]
    [ProducesResponseType(typeof(OrderData), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(BaseResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(BaseResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> InitializeOrder([Required][FromQuery] string languageCode)
    {
        var dataToken = GetUserIdFromToken();
        var (response, order) = await orderService.InitiateOrderAsync(dataToken.UserId, languageCode);
        
        if (!response.Success)
            return StatusCode(response.StatusCode, response);
        
        return Ok(order);
    }
    
    /// <summary>
    /// Отменяет оформление заказа
    /// </summary>
    /// <returns></returns>
    /// <response code="200">Успешная отмена заказа</response>
    /// <response code="500">Ошибка при отмене заказа</response>
    [Authorize]
    [HttpDelete("cancel")]
    [ServiceFilter(typeof(ValidateUserIpFilter))]
    [ServiceFilter(typeof(ValidateJwtAccessTokenFilter))]
    [ProducesResponseType(typeof(BaseResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(BaseResponse), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> CancelOrder()
    {
        var dataToken = GetUserIdFromToken();
        var result = await orderService.CancelOrderAsync(dataToken.UserId);
        return StatusCode(result.StatusCode, result);
    }
    
    /// <summary>
    /// Добавляет купон для заказа
    /// </summary>
    /// <param name="languageCode">Языковой код</param>
    /// <param name="couponCode">Код купона</param>
    /// <returns></returns>
    /// <response code="200">Успешное добавление заказа</response>
    /// <response code="404">Купон или заказ не найден</response>
    /// <response code="403">Купон невозможно применить к товарам</response>
    /// <response code="400">Купон уже применен</response>
    [Authorize]
    [HttpPost("coupon")]
    [ServiceFilter(typeof(ValidateUserIpFilter))]
    [ServiceFilter(typeof(ValidateJwtAccessTokenFilter))]
    [ProducesResponseType(typeof(OrderData), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(BaseResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(BaseResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(BaseResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> SetCoupon([Required][FromQuery] string languageCode, [Required][FromQuery] string couponCode)
    {
        var dataToken = GetUserIdFromToken();
        
        var (response, order) = await orderService.AddCouponAsync(dataToken.UserId, couponCode, languageCode);

        if (!response.Success)
            return StatusCode(response.StatusCode, response);
        
        return Ok(order);
    }
    
    /// <summary>
    /// Удаляет купон для заказа
    /// </summary>
    /// <returns></returns>
    /// <response code="200">Успешное удаление купона</response>
    /// <response code="404">Купон или заказ не найден</response>
    [Authorize]
    [HttpDelete("coupon")]
    [ServiceFilter(typeof(ValidateUserIpFilter))]
    [ServiceFilter(typeof(ValidateJwtAccessTokenFilter))]
    [ProducesResponseType(typeof(OrderData), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(BaseResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteCoupon()
    {
        var dataToken = GetUserIdFromToken();
        
        var (response, order) = await orderService.DeleteCouponAsync(dataToken.UserId);

        if (!response.Success)
            return StatusCode(response.StatusCode, response);
        
        return Ok(order);
    }
    
    /// <summary>
    /// Добавляет доставку
    /// </summary>
    /// <param name="addressId">Идентификатор адреса</param>
    /// <param name="deliveryType">Тип доставки</param>
    /// <returns></returns>
    /// <response code="200">Успешное применение доставки</response>
    /// <response code="404">Заказ или адрес не найден</response>
    /// <response code="408">Не удалось рассчитать стоимость доставки</response>
    [Authorize]
    [HttpPost("shipping-price")]
    [ServiceFilter(typeof(ValidateUserIpFilter))]
    [ServiceFilter(typeof(ValidateJwtAccessTokenFilter))]
    [ProducesResponseType(typeof(OrderData), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(BaseResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(BaseResponse), StatusCodes.Status408RequestTimeout)]
    public async Task<IActionResult> ShippingPrice([Required][FromQuery] string addressId, [Required][FromQuery] DeliveryType deliveryType)
    {
        var dataToken = GetUserIdFromToken();
        
        var (response, order) = await orderService.AddShippingAsync(dataToken.UserId, addressId, deliveryType);
            
        if (!response.Success)
            return StatusCode(response.StatusCode, response);
        
        return Ok(order);
    }
    
    /// <summary>
    /// Удаляет доставку
    /// </summary>
    /// <returns></returns>
    /// <response code="200">Успешное удаление доставки</response>
    /// <response code="404">Заказ или доставка не найдена</response>
    [Authorize]
    [HttpDelete("shipping-price")]
    [ServiceFilter(typeof(ValidateUserIpFilter))]
    [ServiceFilter(typeof(ValidateJwtAccessTokenFilter))]
    [ProducesResponseType(typeof(OrderData), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(BaseResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteShipping()
    {
        var dataToken = GetUserIdFromToken();
        var (response, order) = await orderService.DeleteShippingAsync(dataToken.UserId);
        
        if (!response.Success)
            return StatusCode(response.StatusCode, response);

        return Ok(order);
    }
    
    
    /// <summary>
    /// Возвращает информацию о заказе
    /// </summary>
    /// <returns></returns>
    /// <response code="200">Информация о заказе</response>
    /// <response code="404">Заказ не найден</response>
    [Authorize]
    [HttpGet("current-order-data")]
    [ServiceFilter(typeof(ValidateUserIpFilter))]
    [ServiceFilter(typeof(ValidateJwtAccessTokenFilter))]
    [ProducesResponseType(typeof(OrderData), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(BaseResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetOrderData()
    {
        var dataToken = GetUserIdFromToken();
        var order = await orderRepository.GetOrderDataByIdAsync(dataToken.UserId);

        return Ok(order);
    }

    /// <summary>
    /// Оформление заказа
    /// </summary>
    /// <param name="payment">Данные об оплате</param>
    /// <returns></returns>
    /// <response code="200">Заказ успешно создан</response>
    /// <response code="404">Заказ не найден</response>
    [Authorize]
    [HttpPost("registration-order")]
    [ServiceFilter(typeof(ValidateUserIpFilter))]
    [ServiceFilter(typeof(ValidateJwtAccessTokenFilter))]
    [ProducesResponseType(typeof(OrderCompleted), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(BaseResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RegisterOrder([FromBody] PaymentSelection payment)
    {
        var dataToken = GetUserIdFromToken();
        var (response, _) = await orderService.RegisterOrderAsync(dataToken.UserId, payment);
        return StatusCode(response.StatusCode, response);
    }
    
    /// <summary>
    /// Отмена заказа
    /// </summary>
    /// <param name="orderId">Идентификатор заказа</param>
    /// <returns></returns>
    /// <response code="200">Заказ успешно отменен</response>
    /// <response code="404">Заказ не найден</response>
    /// <response code="403">Заказ невозможно отменить</response>
    [Authorize]
    [HttpDelete("cancelled-order")]
    [ServiceFilter(typeof(ValidateUserIpFilter))]
    [ServiceFilter(typeof(ValidateJwtAccessTokenFilter))]
    [ProducesResponseType(typeof(OrderCompleted), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(BaseResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(BaseResponse), StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> CancelledOrder([Required][FromQuery] string orderId)
    {
        var dataToken = GetUserIdFromToken();
        var response = await orderService.CancelledOrderAsync(dataToken.UserId, orderId);
        
        return StatusCode(response.StatusCode, response);
    }
    
    
    /// <summary>
    /// Выдает заказы для предпросмотра
    /// </summary>
    /// <returns></returns>
    /// <response code="200">Все заказы пользователя</response>
    /// <response code="404">Заказы не найдены</response>
    [Authorize]
    [HttpGet("orders-preview")]
    [ServiceFilter(typeof(ValidateUserIpFilter))]
    [ServiceFilter(typeof(ValidateJwtAccessTokenFilter))]
    [ProducesResponseType(typeof(List<PreviewOrder>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(BaseResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetPreviewOrders()
    {
        var dataToken = GetUserIdFromToken();
        var (response, orders) = await orderService.GetPreviewOrderAsync(dataToken.UserId);
        
        if (!response.Success)
            return StatusCode(response.StatusCode, response);
        
        return Ok(orders);
    }
    
    /// <summary>
    /// Выдает информацию о заказе
    /// </summary>
    /// <param name="orderId">Идентификатор заказа</param>
    /// <returns></returns>
    /// <response code="200">Выдает подробную информацию о заказе</response>
    /// <response code="404">Заказ не найден</response>
    [Authorize]
    [HttpGet("order")]
    [ServiceFilter(typeof(ValidateUserIpFilter))]
    [ServiceFilter(typeof(ValidateJwtAccessTokenFilter))]
    [ProducesResponseType(typeof(OrderDetail), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(BaseResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetOrder([Required][FromQuery] string orderId)
    {
        var dataToken = GetUserIdFromToken();

        var (response, order) = await orderService.GetDetailOrderAsync(dataToken.UserId, orderId);
        
        if (!response.Success)
            return StatusCode(response.StatusCode, response);
        
        return Ok(order);
    }
    
    /// <summary>
    /// Проверяет можно ли отменить заказ или нет
    /// </summary>
    /// <param name="orderId">Идентификатор заказа</param>
    /// <returns></returns>
    /// <response code="200">Флаг возможности отмены заказа: 
    /// - true — отмена разрешена  
    /// - false — отмена невозможна</response>
    /// <response code="404">Заказ не найден</response>
    [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(BaseResponse), StatusCodes.Status404NotFound)]
    [Authorize]
    [HttpGet("check-cancelled-order")]
    [ServiceFilter(typeof(ValidateUserIpFilter))]
    [ServiceFilter(typeof(ValidateJwtAccessTokenFilter))]
    public async Task<IActionResult> CheckCancelledOrder([Required][FromQuery] string orderId)
    {
        var dataToken = GetUserIdFromToken();
        return Ok(await orderService.GetCheckCancelledOrderAsync(dataToken.UserId, orderId));
    }
}