using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using JewelryStoreBackend.Enums;
using JewelryStoreBackend.Filters;
using JewelryStoreBackend.Models.Response;
using JewelryStoreBackend.Models.Response.Order;
using JewelryStoreBackend.Script;
using JewelryStoreBackend.Security;
using JewelryStoreBackend.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StackExchange.Redis;
using Convert = System.Convert;

namespace JewelryStoreBackend.Controllers;

[Authorize]
[ApiController]
[ApiVersion("1.0")]
[Produces("application/json")]
[Route("api/v{version:apiVersion}/[controller]")]
public class OrdersController(ApplicationContext context, ProductRepository repository, IConnectionMultiplexer redis): ControllerBase
{
    private static int TimeExpiredMinute = 60; 
    
    // Начинает оформление заказа (загружает данные товаров в память)
    [Authorize]
    [HttpPost("initiate")]
    [ServiceFilter(typeof(ValidateUserIpFilter))]
    [ServiceFilter(typeof(ValidateJwtAccessTokenFilter))]
    [ProducesResponseType(typeof(OrderData), StatusCodes.Status200OK)]
    public async Task<IActionResult> InitializeOrder([Required][FromQuery] string languageCode)
    {
        var authHeader = HttpContext.Request.Headers["Authorization"].ToString();
        var token = authHeader.Substring("Bearer ".Length);
        var dataToken = JwtController.GetJwtTokenData(token);
        
        var productInBasket = await context.Basket.Where(p => p.PersonId == dataToken.UserId).ToListAsync();
        var user = await context.Users.FirstOrDefaultAsync(p => p.PersonId == dataToken.UserId);
        
        if (productInBasket.Count == 0)
        {
            var error = new BaseResponse
            {
                Success = false,
                Message = "Товары не найдены в корзине",
                ErrorCode = 404,
                Error = "NotFound"
            };
        
            return StatusCode(error.ErrorCode, error);
        }
        
        var database = redis.GetDatabase();
        string key = "order:" + dataToken.UserId;

        if (await database.KeyExistsAsync(key))
        {
            var error = new BaseResponse
            {
                Success = false,
                Message = "Заказ уже оформляется",
                ErrorCode = 400,
                Error = "BadRequest"
            };
        
            return StatusCode(error.ErrorCode, error);
        }

        OrderData orderData = new OrderData { languageCode = languageCode };

        UserOrderData userData = new UserOrderData
        {
            Name = user.Surname + " " + user.Name + " " + user.Patronymic,
            Email = user.Email,
        };
        orderData.userData = userData;
        
        List<ProductOrderData> productOrderData = new List<ProductOrderData>();

        foreach (var item in productInBasket)
        {
            var product = await repository.GetProductByIdAsync(languageCode, item.ProductId);

            ProductOrderData productData = new ProductOrderData
            {
                SKU = product.specifications.First().sku,
                Title = product.title,
                Price = item.Count * product.price.cost,
                Discount = product.price.discount,
                PriceDiscount = item.Count * product.price.costDiscount,
                Percent = product.price.percent,
                Quantity = item.Count,
                ProductImage = product.images.First(),
                ProductType = product.productType,
                ProductAddedData = product.createTimeStamp,
            };
            productOrderData.Add(productData);
        }
        orderData.Items= productOrderData;

        var priceItems = productOrderData.Select(p => p.Price).Sum();
        var priceDiscount = productOrderData.Select(p => p.PriceDiscount).Sum();
        
        PriceOrderData priceOrderData = new PriceOrderData
        {
            TotalPrice = priceItems,
            TotalPriceDiscount = priceDiscount,
            TotalPercentInProduct = CostCalculation.CalculateDiscountPercentage(priceItems, priceDiscount),
            TotalCost = priceDiscount
        };
        orderData.PriceDatails = priceOrderData;

        string jsonData = JsonSerializer.Serialize(orderData);
        TimeSpan expiration = TimeSpan.FromMinutes(TimeExpiredMinute);
        
        await database.StringSetAsync(key, jsonData, expiration);
        
        return Ok(orderData);
    }
    
    // Отменяет оформление заказа (удаляет данные из redis)
    [Authorize]
    [HttpDelete("cancel")]
    [ServiceFilter(typeof(ValidateUserIpFilter))]
    [ServiceFilter(typeof(ValidateJwtAccessTokenFilter))]
    public async Task<IActionResult> CancelOrder()
    {
        var authHeader = HttpContext.Request.Headers["Authorization"].ToString();
        var token = authHeader.Substring("Bearer ".Length);
        var dataToken = JwtController.GetJwtTokenData(token);
        
        var database = redis.GetDatabase();
        string key = "order:" + dataToken.UserId;

        if (!await database.KeyExistsAsync(key))
        {
            var error = new BaseResponse
            {
                Success = false,
                Message = "Нечего отменять",
                ErrorCode = 404,
                Error = "NotFound"
            };
        
            return StatusCode(error.ErrorCode, error);
        }
        
        var isRemoved = await database.KeyDeleteAsync(key);

        if (!isRemoved)
        {
            var error = new BaseResponse
            {
                Success = false,
                Message = "Произошла ошибка при отмене товара",
                ErrorCode = 500,
                Error = "InternalServerError"
            };
        
            return StatusCode(error.ErrorCode, error);
        }
        
        return Ok("Оформление заказа успешно отменено");
    }
    
    // Добавляет(обновляет) купон для заказа
    [Authorize]
    [HttpPost("coupon")]
    [ServiceFilter(typeof(ValidateUserIpFilter))]
    [ServiceFilter(typeof(ValidateJwtAccessTokenFilter))]
    public async Task<IActionResult> SetCoupon([Required][FromQuery] string languageCode, [Required][FromQuery] string couponCode)
    {
        var authHeader = HttpContext.Request.Headers["Authorization"].ToString();
        var token = authHeader.Substring("Bearer ".Length);
        var dataToken = JwtController.GetJwtTokenData(token);
        
        var database = redis.GetDatabase();
        string key = "order:" + dataToken.UserId;
        
        var coupon = await context.Coupon.FirstOrDefaultAsync(c => c.CouponCode == couponCode && c.LanguageCode == languageCode);

        if (coupon == null)
        {
            var error = new BaseResponse
            {
                Success = false,
                Message = "Данный купон не найден",
                ErrorCode = 404,
                Error = "NotFound"
            };
        
            return StatusCode(error.ErrorCode, error);
        }
        if (!await database.KeyExistsAsync(key))
        {
            var error = new BaseResponse
            {
                Success = false,
                Message = "Заказ не найден",
                ErrorCode = 404,
                Error = "NotFound"
            };
        
            return StatusCode(error.ErrorCode, error);
        }
        
        string jsonData = await database.StringGetAsync(key);
        OrderData order = JsonSerializer.Deserialize<OrderData>(jsonData);

        if (order.couponData != null)
        {
            var error = new BaseResponse
            {
                Success = false,
                Message = "Купон уже применен!",
                ErrorCode = 403,
                Error = "Forbidden"
            };
        
            return StatusCode(error.ErrorCode, error);
        }

        CouponOrderData couponOrderData = new()
        {
            CouponCode = coupon.CouponCode,
            Description = coupon.Description,
            Percent = coupon.Percent,
            Title = coupon.Title
        };

        int appliedDiscountProducts = 0;
        
        if (coupon.Action == CouponAction.ALL)
        {
            double priceDiscount = 0;
            
            foreach (var item in order.Items)
            {
                if (!item.Discount)
                {
                    appliedDiscountProducts++;
                    priceDiscount += CostCalculation.CalculateDiscountedPrice(item.Price, coupon.Percent);
                }
                else
                    priceDiscount += item.PriceDiscount;
            }

            if (appliedDiscountProducts == 0)
            {
                var error = new BaseResponse
                {
                    Success = false,
                    Message = "Купон нельзя применить к данным товарам",
                    ErrorCode = 403,
                    Error = "Forbidden"
                };
        
                return StatusCode(error.ErrorCode, error);
            }
            
            order.PriceDatails.PercentTheCoupon = coupon.Percent;
            order.PriceDatails.TotalDiscountTheCoupon = priceDiscount;
            var shippingCost = order.PriceDatails.ShippingCost ?? 0;
            order.PriceDatails.TotalCost = priceDiscount + shippingCost;
        }
        else if (coupon.Action == CouponAction.NEW)
        {
            double priceDiscount = 0;
            
            foreach (var item in order.Items)
            {
                if (item.ProductAddedData > DateTime.UtcNow.AddDays(-14))
                {
                    appliedDiscountProducts++;
                    priceDiscount += CostCalculation.CalculateDiscountedPrice(item.Price, coupon.Percent);
                }
                else
                    priceDiscount += item.PriceDiscount;
            }

            if (appliedDiscountProducts == 0)
            {
                var error = new BaseResponse
                {
                    Success = false,
                    Message = "Купон нельзя применить к данным товарам",
                    ErrorCode = 403,
                    Error = "Forbidden"
                };
        
                return StatusCode(error.ErrorCode, error);
            }
            
            order.PriceDatails.PercentTheCoupon = coupon.Percent;
            order.PriceDatails.TotalDiscountTheCoupon = priceDiscount;
            var shippingCost = order.PriceDatails.ShippingCost ?? 0;
            order.PriceDatails.TotalCost = priceDiscount + shippingCost;
        }
        else if (coupon.Action == CouponAction.CATEGORY)
        {
            double priceDiscount = 0;
            
            foreach (var item in order.Items)
            {
                if (item.ProductType == coupon.CategoryType)
                {
                    appliedDiscountProducts++;
                    priceDiscount += CostCalculation.CalculateDiscountedPrice(item.Price, coupon.Percent);
                }
                else
                    priceDiscount += item.PriceDiscount;
            }

            if (appliedDiscountProducts == 0)
            {
                var error = new BaseResponse
                {
                    Success = false,
                    Message = "Купон нельзя применить к данным товарам",
                    ErrorCode = 403,
                    Error = "Forbidden"
                };
        
                return StatusCode(error.ErrorCode, error);
            }
            
            order.PriceDatails.PercentTheCoupon = coupon.Percent;
            order.PriceDatails.TotalDiscountTheCoupon = priceDiscount;
            var shippingCost = order.PriceDatails.ShippingCost ?? 0;
            order.PriceDatails.TotalCost = priceDiscount + shippingCost;
        }

        order.couponData = couponOrderData;
        
        jsonData = JsonSerializer.Serialize(order);
        TimeSpan expiration = TimeSpan.FromMinutes(TimeExpiredMinute);
        await database.StringSetAsync(key, jsonData, expiration);
        
        return Ok(order);
    }
    
    // Удаляет купон для заказа
    [Authorize]
    [HttpDelete("coupon")]
    [ServiceFilter(typeof(ValidateUserIpFilter))]
    [ServiceFilter(typeof(ValidateJwtAccessTokenFilter))]
    public async Task<IActionResult> DeleteCoupon()
    {
        var authHeader = HttpContext.Request.Headers["Authorization"].ToString();
        var token = authHeader.Substring("Bearer ".Length);
        var dataToken = JwtController.GetJwtTokenData(token);
        
        var database = redis.GetDatabase();
        string key = "order:" + dataToken.UserId;

        if (!await database.KeyExistsAsync(key))
        {
            var error = new BaseResponse
            {
                Success = false,
                Message = "Заказ не найден",
                ErrorCode = 404,
                Error = "NotFound"
            };
        
            return StatusCode(error.ErrorCode, error);
        }
        
        string jsonData = await database.StringGetAsync(key);
        OrderData order = JsonSerializer.Deserialize<OrderData>(jsonData);
        
        if (order.couponData == null)
        {
            var error = new BaseResponse
            {
                Success = false,
                Message = "Купон не найден в заказе",
                ErrorCode = 404,
                Error = "NotFound"
            };

            return StatusCode(error.ErrorCode, error);
        }
        
        order.couponData = null;
        
        order.PriceDatails.PercentTheCoupon = null;
        order.PriceDatails.TotalDiscountTheCoupon = null;
        
        order.PriceDatails.TotalCost = order.PriceDatails.TotalPriceDiscount + (order.PriceDatails.ShippingCost ?? 0);
        
        jsonData = JsonSerializer.Serialize(order);
        TimeSpan expiration = TimeSpan.FromMinutes(TimeExpiredMinute);
        await database.StringSetAsync(key, jsonData, expiration);

        return Ok(order);
    }
    
    // Рассчитывает стоимость доставки
    [Authorize]
    [HttpPost("shipping-price")]
    [ServiceFilter(typeof(ValidateUserIpFilter))]
    [ServiceFilter(typeof(ValidateJwtAccessTokenFilter))]
    public async Task<IActionResult> ShippingPrice([Required][FromQuery] string addressId, [Required][FromQuery] DeliveryType deliveryType)
    {
        var authHeader = HttpContext.Request.Headers["Authorization"].ToString();
        var token = authHeader.Substring("Bearer ".Length);
        var dataToken = JwtController.GetJwtTokenData(token);
        
        var database = redis.GetDatabase();
        string key = "order:" + dataToken.UserId;

        if (!await database.KeyExistsAsync(key))
        {
            var error = new BaseResponse
            {
                Success = false,
                Message = "Заказ не найден",
                ErrorCode = 404,
                Error = "NotFound"
            };
        
            return StatusCode(error.ErrorCode, error);
        }
        
        string jsonData = await database.StringGetAsync(key);
        OrderData order = JsonSerializer.Deserialize<OrderData>(jsonData);
        
        var address = await context.Address.FirstOrDefaultAsync(a => a.PersonId == dataToken.UserId && a.AddressId == addressId);
        string lonStart = address.lon;
        string latStart = address.lat;
        
        var warehousesAddress = await context.Warehouses.ToListAsync();
        var warehouseAddress = warehousesAddress.First();
        
        var coordinate =
            await GeolocationService.GetGeolocateDistanceAsync(lonStart, latStart, warehouseAddress.lon, warehouseAddress.lat);

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
        
        var result = DeliveryCalculator.CalculateDeliveryCost(coordinate.routes.First().legs.First().distance / 1000, deliveryType);
        
        var couponPrice = order.PriceDatails.TotalDiscountTheCoupon ?? order.PriceDatails.TotalPriceDiscount;
        
        order.PriceDatails.ShippingCost = result.TotalCost;
        order.PriceDatails.TotalCost = couponPrice + result.TotalCost;

        ShippingOrderData shippingOrderData = new ShippingOrderData
        {
            WarehouseAddress = warehouseAddress.Address,
            UserAddress = address.City + " " + address.AddressLine1 + " " + address.AddressLine2,
            UserPostalCode = address.PostalCode,
            ShippingCost = result.TotalCost,
            EstimatedDeliveryTime = Convert.ToInt32(coordinate.routes.First().legs.First().duration),
            ShippingMethod = deliveryType,
            Details = result
        };
        
        order.shippingData = shippingOrderData;
        
        jsonData = JsonSerializer.Serialize(order);
        TimeSpan expiration = TimeSpan.FromMinutes(TimeExpiredMinute);
        await database.StringSetAsync(key, jsonData, expiration);
            
        return Ok(order);
    }
    
    // Удаляет доставку 
    [Authorize]
    [HttpDelete("shipping-price")]
    [ServiceFilter(typeof(ValidateUserIpFilter))]
    [ServiceFilter(typeof(ValidateJwtAccessTokenFilter))]
    public async Task<IActionResult> DeleteShipping()
    {
        return Ok();
    }
}