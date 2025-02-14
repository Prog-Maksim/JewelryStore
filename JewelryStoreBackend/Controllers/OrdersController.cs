using System.ComponentModel.DataAnnotations;
using JewelryStoreBackend.Filters;
using JewelryStoreBackend.Models.Response;
using JewelryStoreBackend.Script;
using JewelryStoreBackend.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace JewelryStoreBackend.Controllers;

[Authorize]
[ApiController]
[ApiVersion("1.0")]
[Produces("application/json")]
[Route("api/v{version:apiVersion}/[controller]")]
public class OrdersController(ApplicationContext context, ProductRepository repository): ControllerBase
{
    // Добавляет(обновляет) купон для заказа
    [Authorize]
    [HttpPost("coupon")]
    [ServiceFilter(typeof(ValidateUserIpFilter))]
    [ServiceFilter(typeof(ValidateJwtAccessTokenFilter))]
    public async Task<IActionResult> SetCoupon()
    {
        return Ok();
    }
    
    // Удаляет купон для заказа
    [Authorize]
    [HttpDelete("coupon")]
    [ServiceFilter(typeof(ValidateUserIpFilter))]
    [ServiceFilter(typeof(ValidateJwtAccessTokenFilter))]
    public async Task<IActionResult> DeleteCoupon()
    {
        return Ok();
    }
    
    // Рассчитывает стоимость доставки со склада
    [AllowAnonymous]
    [HttpGet("shipping-price")]
    [ServiceFilter(typeof(ValidateUserIpFilter))]
    public async Task<IActionResult> GetShippingPrice()
    {
        var coordinate =
            await GeolocationService.GetGeolocateDistanceAsync("47.2935603", "39.71240159999999", "47.2196732", "39.697408");

        if (coordinate == null)
        {
            var error = new BaseResponse
            {
                Success = false,
                Message = "Не удалось рассчитать стоимость, попробуйте позже",
                ErrorCode = 408,
                Error = "Request Timeout"
            };

            return StatusCode(error.ErrorCode, error);
        }
        
        var result = DeliveryCalculator.CalculateDeliveryCost(10, coordinate.routes.First().legs.First().distance / 1000,
            DeliveryType.Base);
        
        return Ok(result);
    }
}