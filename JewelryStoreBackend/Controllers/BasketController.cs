using System.ComponentModel.DataAnnotations;
using JewelryStoreBackend.Enums;
using JewelryStoreBackend.Filters;
using JewelryStoreBackend.Models.DB;
using JewelryStoreBackend.Models.Response;
using JewelryStoreBackend.Models.Response.Basket;
using JewelryStoreBackend.Models.Response.ProductStructure;
using JewelryStoreBackend.Script;
using JewelryStoreBackend.Security;
using JewelryStoreBackend.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StackExchange.Redis;

namespace JewelryStoreBackend.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Produces("application/json")]
[Route("api/v{version:apiVersion}/[controller]")]
public class BasketController(ApplicationContext context, ProductRepository repository, IConnectionMultiplexer redis): ControllerBase
{
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
        var authHeader = HttpContext.Request.Headers["Authorization"].ToString();
        var token = authHeader.Substring("Bearer ".Length);

        var dataToken = JwtController.GetJwtTokenData(token);
        var productInBasket = await context.Basket.Where(p => p.PersonId == dataToken.UserId).ToListAsync();
        
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
        
        BasketResponse bakset = new BasketResponse
        {
            Quantity = productInBasket.Count,
            Subtotal = productInBasket.Select(p => p.Count).Sum(),
        };
        
        Price priceBasket = new Price();
        
        List<Productions> products = new List<Productions>();
        
        foreach (var item in productInBasket)
        {
            var product = await repository.GetProductByIdAsync(languageCode, item.ProductId);

            PriceProduction price = new PriceProduction
            {
                cost = product.price.cost,
                currency = product.price.currency,
                discount = product.price.discount,
                costDiscount = product.price.costDiscount,
                percent = product.price.percent,
            };
        
            Productions productModel = new Productions
            {
                languageCode = languageCode,
                ProductId = product.productId,
                SKU = item.ProductId,
                Title = product.title,
                Description = product.description,
                Images = product.images,
                Quantity = item.Count,
                OnSale = product.onSale,
                InStock = product.specifications.First().inStock,
                Likes = product.likes,
                PriceProduction = price,
            };
        
            priceBasket.Currency = productModel.PriceProduction.currency;
            priceBasket.Cost += productModel.PriceProduction.cost;
            priceBasket.CostDiscount += productModel.PriceProduction.costDiscount;
            
            products.Add(productModel);
        }
        
        if (priceBasket.Cost == priceBasket.CostDiscount)
        {
            priceBasket.Discount = false;
            priceBasket.Percent = 0;
        }
        else
        {
            priceBasket.Discount = true;
            int discountPercentage = CostCalculation.CalculateDiscountPercentage(priceBasket.Cost, priceBasket.CostDiscount);
            priceBasket.Percent = discountPercentage;
        }
        
        bakset.TotalPrice = priceBasket;
        bakset.Productions = products;
        
        return Ok(bakset);
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
        var authHeader = HttpContext.Request.Headers["Authorization"].ToString();
        var token = authHeader.Substring("Bearer ".Length);

        var dataToken = JwtController.GetJwtTokenData(token);
        var productInBasket = await context.Basket.FirstOrDefaultAsync(p => p.PersonId == dataToken.UserId && p.ProductId == productId);

        if (productInBasket == null)
        {
            var error = new BaseResponse
            {
                Success = false,
                Message = "Товар в корзине не найден",
                ErrorCode = 404,
                Error = "NotFound"
            };
            return StatusCode(error.ErrorCode, error);
        }
        
        return Ok(productInBasket);
    }
    
    /// <summary>
    /// Добавляет товар(ы) в корзину
    /// </summary>
    /// <param name="productId">Идентификатор товара</param>
    /// <param name="quantity">Кол-во товара</param>
    /// <returns></returns>
    /// <response code="200">Успешно</response>
    /// <response code="400">Невозможно определить IP адрес пользователя</response>
    /// <response code="403">Некорректный токен</response>
    [Authorize]
    [HttpPost("product")]
    [ServiceFilter(typeof(ValidateUserIpFilter))]
    [ServiceFilter(typeof(ValidateJwtAccessTokenFilter))]
    [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(BaseResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(BaseResponse), StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> AddProductInBasket([Required][FromQuery] string productId, [FromQuery] int? quantity = 1)
    {
        var authHeader = HttpContext.Request.Headers["Authorization"].ToString();
        var token = authHeader.Substring("Bearer ".Length);

        var dataToken = JwtController.GetJwtTokenData(token);
        var productInBasket = await context.Basket.FirstOrDefaultAsync(p => p.PersonId == dataToken.UserId && p.ProductId == productId);

        if (productInBasket != null)
            productInBasket.Count += quantity ?? 1;
        else
        {
            Basket basket = new Basket
            {
                PersonId = dataToken.UserId,
                ProductId = productId,
                Count = quantity ?? 1,
                DateAdded = DateTime.Now,
            };

            await context.Basket.AddAsync(basket);
        }
        await context.SaveChangesAsync();
        
        return Ok("Товар(ы) успешно добавлен в корзину");
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
    [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(BaseResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(BaseResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(BaseResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteProductInBasket([Required][FromQuery] string productId, [Required][FromQuery] Count quantity)
    {
        var authHeader = HttpContext.Request.Headers["Authorization"].ToString();
        var token = authHeader.Substring("Bearer ".Length);

        var dataToken = JwtController.GetJwtTokenData(token);
        var productInBasket = await context.Basket.FirstOrDefaultAsync(p => p.PersonId == dataToken.UserId && p.ProductId == productId);

        if (productInBasket == null)
        {
            var error = new BaseResponse
            {
                Success = false,
                Message = "Товар не найден в корзине!",
                ErrorCode = 404,
                Error = "NotFound"
            };

            return StatusCode(error.ErrorCode, error);
        }
        
        if (quantity == Count.One)
            productInBasket.Count -= 1;
        if (quantity == Count.All || productInBasket.Count <= 0)
            context.Basket.Remove(productInBasket);
        
        await context.SaveChangesAsync();
        
        return Ok("Товар успешно удален из корзины");
    }
}