using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using JewelryStoreBackend.Enums;
using JewelryStoreBackend.Filters;
using JewelryStoreBackend.Models.DB.Order;
using JewelryStoreBackend.Models.Request;
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
                StatusCode = 404,
                Error = "NotFound"
            };
        
            return StatusCode(error.StatusCode, error);
        }
        
        var database = redis.GetDatabase();
        string key = "order:" + dataToken.UserId;

        if (await database.KeyExistsAsync(key))
        {
            var error = new BaseResponse
            {
                Success = false,
                Message = "Заказ уже оформляется",
                StatusCode = 400,
                Error = "BadRequest"
            };
        
            return StatusCode(error.StatusCode, error);
        }

        OrderData orderData = new OrderData { languageCode = languageCode };

        UserOrderData userData = new UserOrderData
        {
            Name = user.Surname + " " + user.Name + " " + user.Patronymic,
            Email = user.Email,
            NumberPhone = user.PhoneNumber,
        };
        orderData.userData = userData;
        
        List<ProductOrderData> productOrderData = new List<ProductOrderData>();

        string currency = "";

        foreach (var item in productInBasket)
        {
            var product = await repository.GetProductByIdAsync(languageCode, item.ProductId);

            if (product.specifications.First().inStock || product.onSale)
            {
                currency = product.price.currency;

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
                    ProductAddedData = product.createTimeStamp
                };
                productOrderData.Add(productData);
            }
        }
        orderData.Items= productOrderData;

        var priceItems = productOrderData.Select(p => p.Price).Sum();
        var priceDiscount = productOrderData.Select(p => p.PriceDiscount).Sum();
        
        PriceOrderData priceOrderData = new PriceOrderData
        {
            TotalPrice = priceItems,
            TotalPriceDiscount = priceDiscount,
            TotalPercentInProduct = CostCalculation.CalculateDiscountPercentage(priceItems, priceDiscount),
            TotalCost = priceDiscount,
            Currency = currency
        };
        orderData.PriceDatails = priceOrderData;

        string jsonData = JsonSerializer.Serialize(orderData);
        TimeSpan expiration = TimeSpan.FromMinutes(TimeExpiredMinute);
        
        await database.StringSetAsync(key, jsonData, expiration);
        
        return Ok(orderData);
    }
    
    /// <summary>
    /// Отменяет оформление заказа
    /// </summary>
    /// <returns></returns>
    /// <response code="200">Успешная отмена заказа</response>
    /// <response code="404">не найден заказ для отмены</response>
    /// <response code="500">Ошибка при отмене заказа</response>
    [Authorize]
    [HttpDelete("cancel")]
    [ServiceFilter(typeof(ValidateUserIpFilter))]
    [ServiceFilter(typeof(ValidateJwtAccessTokenFilter))]
    [ProducesResponseType(typeof(BaseResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(BaseResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(BaseResponse), StatusCodes.Status500InternalServerError)]
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
                StatusCode = 404,
                Error = "NotFound"
            };
        
            return StatusCode(error.StatusCode, error);
        }
        
        var isRemoved = await database.KeyDeleteAsync(key);

        if (!isRemoved)
        {
            var error = new BaseResponse
            {
                Success = false,
                Message = "Произошла ошибка при отмене товара",
                StatusCode = 500,
                Error = "InternalServerError"
            };
        
            return StatusCode(error.StatusCode, error);
        }
        
        return Ok(new BaseResponse
        {
            Message = "Оформление заказа успешно отменено",
            Success = true
        });
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
                StatusCode = 404,
                Error = "NotFound"
            };
        
            return StatusCode(error.StatusCode, error);
        }
        if (!await database.KeyExistsAsync(key))
        {
            var error = new BaseResponse
            {
                Success = false,
                Message = "Заказ не найден",
                StatusCode = 404,
                Error = "NotFound"
            };
        
            return StatusCode(error.StatusCode, error);
        }
        
        string jsonData = await database.StringGetAsync(key);
        OrderData order = JsonSerializer.Deserialize<OrderData>(jsonData);

        if (order.couponData != null)
        {
            var error = new BaseResponse
            {
                Success = false,
                Message = "Купон уже применен!",
                StatusCode = 400,
                Error = "BadRequest"
            };
        
            return StatusCode(error.StatusCode, error);
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
                    StatusCode = 403,
                    Error = "Forbidden"
                };
        
                return StatusCode(error.StatusCode, error);
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
                    StatusCode = 403,
                    Error = "Forbidden"
                };
        
                return StatusCode(error.StatusCode, error);
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
                    StatusCode = 403,
                    Error = "Forbidden"
                };
        
                return StatusCode(error.StatusCode, error);
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
                StatusCode = 404,
                Error = "NotFound"
            };
        
            return StatusCode(error.StatusCode, error);
        }
        
        string jsonData = await database.StringGetAsync(key);
        OrderData order = JsonSerializer.Deserialize<OrderData>(jsonData);
        
        if (order.couponData == null)
        {
            var error = new BaseResponse
            {
                Success = false,
                Message = "Купон не найден в заказе",
                StatusCode = 404,
                Error = "NotFound"
            };

            return StatusCode(error.StatusCode, error);
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
                StatusCode = 404,
                Error = "NotFound"
            };
        
            return StatusCode(error.StatusCode, error);
        }
        
        string jsonData = await database.StringGetAsync(key);
        OrderData order = JsonSerializer.Deserialize<OrderData>(jsonData);
        
        var address = await context.Address.FirstOrDefaultAsync(a => a.PersonId == dataToken.UserId && a.AddressId == addressId);

        if (address == null)
        {
            var error = new BaseResponse
            {
                Success = false,
                Message = "Данный адрес не найден!",
                StatusCode = 404,
                Error = "NotFound"
            };
        
            return StatusCode(error.StatusCode, error);
        }
        
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
                StatusCode = 408,
                Error = "Request Timeout"
            };

            return StatusCode(error.StatusCode, error);
        }
        
        var result = DeliveryCalculator.CalculateDeliveryCost(coordinate.routes.First().legs.First().distance / 1000, deliveryType);
        
        var couponPrice = order.PriceDatails.TotalDiscountTheCoupon ?? order.PriceDatails.TotalPriceDiscount;
        
        order.PriceDatails.ShippingCost = result.TotalCost;
        order.PriceDatails.TotalCost = Math.Round(couponPrice + result.TotalCost, 2);
        
        var duration = Convert.ToInt32(coordinate.routes.First().legs.First().duration);
        DateTime dateShipping = DateTime.Now + TimeSpan.FromDays(duration);

        ShippingOrderData shippingOrderData = new ShippingOrderData
        {
            WarehouseAddress = warehouseAddress.Address,
            UserAddress = address.City + " " + address.AddressLine1 + " " + address.AddressLine2,
            UserPostalCode = address.PostalCode,
            ShippingCost = result.TotalCost,
            EstimatedDeliveryTime = dateShipping,
            ShippingMethod = deliveryType,
            Details = result
        };
        
        order.shippingData = shippingOrderData;
        
        jsonData = JsonSerializer.Serialize(order);
        TimeSpan expiration = TimeSpan.FromMinutes(TimeExpiredMinute);
        await database.StringSetAsync(key, jsonData, expiration);
            
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
                StatusCode = 404,
                Error = "NotFound"
            };
    
            return StatusCode(error.StatusCode, error);
        }
        
        string jsonData = await database.StringGetAsync(key);
        OrderData order = JsonSerializer.Deserialize<OrderData>(jsonData);
        
        if (order.shippingData == null)
        {
            var error = new BaseResponse
            {
                Success = false,
                Message = "Информация о доставке отсутствует",
                StatusCode = 404,
                Error = "NotFound"
            };

            return StatusCode(error.StatusCode, error);
        }
        
        // Удаляем данные о доставке
        order.shippingData = null;
        order.PriceDatails.TotalCost -= order.PriceDatails.ShippingCost ?? 0;
        order.PriceDatails.TotalCost = Math.Round(order.PriceDatails.TotalCost, 2);
        order.PriceDatails.ShippingCost = null;
        
        // Сохраняем обновленные данные в кэш
        jsonData = JsonSerializer.Serialize(order);
        TimeSpan expiration = TimeSpan.FromMinutes(TimeExpiredMinute);
        await database.StringSetAsync(key, jsonData, expiration);

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
                StatusCode = 404,
                Error = "NotFound"
            };
    
            return StatusCode(error.StatusCode, error);
        }
        
        string jsonData = await database.StringGetAsync(key);
        OrderData order = JsonSerializer.Deserialize<OrderData>(jsonData);

        return Ok(order);
    }

    /// <summary>
    /// Оформление заказа
    /// </summary>
    /// <param name="payment"></param>
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
                StatusCode = 404,
                Error = "NotFound"
            };
    
            return StatusCode(error.StatusCode, error);
        }
        
        string jsonData = await database.StringGetAsync(key);
        OrderData preOrder = JsonSerializer.Deserialize<OrderData>(jsonData);
        
        Random rnd = new Random();
        string orderId = '#' + rnd.NextInt64(1111111111, 10000000000).ToString();

        Orders order = new Orders
        {
            OrderId = orderId,
            PersonId = dataToken.UserId,
            CreateTimestamp = DateTime.Now,
            Status = OrderStatus.Pending,
            OrderCost = preOrder.PriceDatails.TotalCost,
            Currency = preOrder.PriceDatails.Currency
        };
        await context.Orders.AddAsync(order);

        foreach (var item in preOrder.Items)
        {
            OrderProducts product = new OrderProducts
            {
                OrderId = orderId,
                SKU = item.SKU,
                Cost = item.PriceDiscount,
                Quantity = item.Quantity
            };
            await context.OrderProducts.AddAsync(product);
        }

        OrderPayments payments = new OrderPayments
        {
            OrderId = orderId,
            PaymentMethod = payment.PaymentType,
            PaymentStatus = payment.PaymentType == PaymentType.Card ? PaymentStatus.Paid : PaymentStatus.NotPaid,
            DatePayment = payment.PaymentType == PaymentType.Card ? DateTime.Now : null
        };
        await context.OrderPayments.AddAsync(payments);

        OrderShippings shippings = new OrderShippings
        {
            OrderId = orderId,
            TargetAddress = preOrder.shippingData.UserAddress,
            WarehouseAddress = preOrder.shippingData.WarehouseAddress,
            DeliveryType = preOrder.shippingData.ShippingMethod,
            DateShipping = preOrder.shippingData.EstimatedDeliveryTime
        };
        await context.OrderShippings.AddAsync(shippings);

        var basketProducts = await context.Basket.Where(b => b.PersonId == dataToken.UserId).ToListAsync();
        context.Basket.RemoveRange(basketProducts);
        await database.KeyDeleteAsync(key);
        
        await context.SaveChangesAsync();
        
        return Ok(new OrderCompleted
        {
            Message = "Заказ успешно оформлен",
            OrderId = orderId,
            Success = true
        });
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
        var authHeader = HttpContext.Request.Headers["Authorization"].ToString();
        var token = authHeader.Substring("Bearer ".Length);
        var dataToken = JwtController.GetJwtTokenData(token);

        var order = await context.Orders.FirstOrDefaultAsync(p => p.OrderId == orderId && p.PersonId == dataToken.UserId);

        if (order == null)
        {
            var error = new BaseResponse
            {
                Success = false,
                Message = "Заказ не найден",
                StatusCode = 404,
                Error = "NotFound"
            };
    
            return StatusCode(error.StatusCode, error);
        }

        if (order.Status == OrderStatus.Refund || order.Status == OrderStatus.Cancelled ||
            order.Status == OrderStatus.Completed || order.Status == OrderStatus.Received)
        {
            var error = new BaseResponse
            {
                Success = false,
                Message = "Данный заказ нельзя отменить!",
                StatusCode = 403,
                Error = "Forbidden"
            };
    
            return StatusCode(error.StatusCode, error);
        }
        
        order.Status = OrderStatus.Cancelled;
        order.CompletedTimestamp = DateTime.Now;
        
        await context.SaveChangesAsync();
        
        return Ok(new OrderCompleted
        {
            OrderId = orderId,
            Message = "Заказ успешно отменен"
        });
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
        var authHeader = HttpContext.Request.Headers["Authorization"].ToString();
        var token = authHeader.Substring("Bearer ".Length);
        var dataToken = JwtController.GetJwtTokenData(token);
        
        var orders = await context.Orders.Where(o => o.PersonId == dataToken.UserId).ToListAsync();

        if (orders.Count == 0)
        {
            var error = new BaseResponse
            {
                Success = false,
                Message = "Заказы не найдены",
                StatusCode = 404,
                Error = "NotFound"
            };
    
            return StatusCode(error.StatusCode, error);
        }

        List<PreviewOrder> preOrders = new List<PreviewOrder>();

        foreach (var item in orders)
        {
            PreviewOrder order = new PreviewOrder
            {
                OrderId = item.OrderId,
                CreateOrderTimestamp = item.CreateTimestamp,
                Status = item.Status,
                Cost = item.OrderCost,
                Currency = item.Currency,
            };
            preOrders.Add(order);
        }

        return Ok(preOrders);
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
        var authHeader = HttpContext.Request.Headers["Authorization"].ToString();
        var token = authHeader.Substring("Bearer ".Length);
        var dataToken = JwtController.GetJwtTokenData(token);

        var order = await context.Orders
            .Include(o => o.Products)
            .Include(o => o.Payment)
            .Include(o => o.Shipping)
            .Include(o => o.Users)
            .FirstOrDefaultAsync(o => o.PersonId == dataToken.UserId && o.OrderId == orderId);

        if (order == null)
        {
            var error = new BaseResponse
            {
                Success = false,
                Message = "Заказ не найден",
                StatusCode = 404,
                Error = "NotFound"
            };
    
            return StatusCode(error.StatusCode, error);
        }

        OrderDetailShipping shipping = new OrderDetailShipping
        {
            TargetAddress = order.Shipping.TargetAddress,
            WarehouseAddress = order.Shipping.WarehouseAddress,
            DeliveryType = order.Shipping.DeliveryType,
            DateShipping = order.Shipping.DateShipping
        };

        OrderDetailPayment payment = new OrderDetailPayment
        {
            paymentMethod = order.Payment.PaymentMethod,
            paymentStatus = order.Payment.PaymentStatus,
            datePayment = order.Payment.DatePayment
        };
        
        List<OrderDetailProduct> products = new List<OrderDetailProduct>();

        foreach (var item in order.Products)
        {
            OrderDetailProduct product = new OrderDetailProduct
            {
                sku = item.SKU,
                cost = item.Cost,
                quantity = item.Quantity
            };
            products.Add(product);
        }

        OrderDetail orderDetail = new OrderDetail
        {
            orderId = order.OrderId,
            name = order.Users.Surname + " " + order.Users.Name,
            email = order.Users.Email,
            phoneNumber = order.Users.PhoneNumber,
            createTimestamp = order.CreateTimestamp,
            completedTimestamp = order.CompletedTimestamp,
            status = order.Status,
            orderCost = order.OrderCost,
            currency = order.Currency,
            products = products,
            shipping = shipping,
            payment = payment,
        };
        
        return Ok(orderDetail);
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
        var authHeader = HttpContext.Request.Headers["Authorization"].ToString();
        var token = authHeader.Substring("Bearer ".Length);
        var dataToken = JwtController.GetJwtTokenData(token);

        var order = await context.Orders.FirstOrDefaultAsync(p => p.OrderId == orderId && p.PersonId == dataToken.UserId);

        if (order == null)
        {
            var error = new BaseResponse
            {
                Success = false,
                Message = "Заказ не найден",
                StatusCode = 404,
                Error = "NotFound"
            };
    
            return StatusCode(error.StatusCode, error);
        }

        bool result = !(order.Status == OrderStatus.Refund || order.Status == OrderStatus.Cancelled ||
                        order.Status == OrderStatus.Completed || order.Status == OrderStatus.Received);
        
        return Ok(result);
    }
}