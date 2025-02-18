using System.ComponentModel.DataAnnotations;
using JewelryStoreBackend.Enums;
using JewelryStoreBackend.Filters;
using JewelryStoreBackend.Models.DB.Product;
using JewelryStoreBackend.Models.Request;
using JewelryStoreBackend.Models.Response;
using JewelryStoreBackend.Security;
using JewelryStoreBackend.Repository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StackExchange.Redis;

namespace JewelryStoreBackend.Controllers;

[Authorize]
[ApiController]
[ApiVersion("1.0")]
[Produces("application/json")]
[Route("api/v{version:apiVersion}/[controller]")]
public class AdminController(ApplicationContext context, ProductRepository repository, IConnectionMultiplexer redis): ControllerBase
{
    /// <summary>
    /// Блокирует пользователя в системе
    /// </summary>
    /// <param name="personId">Идентификатор пользователя</param>
    /// <returns></returns>
    [Authorize(Roles = "admin")]
    [HttpPost("ban-user")]
    [ServiceFilter(typeof(ValidateUserIpFilter))]
    public async Task<IActionResult> BanUser([Required][FromQuery] string personId)
    {
        var authHeader = HttpContext.Request.Headers["Authorization"].ToString();
        var token = authHeader.Substring("Bearer ".Length);

        var dataToken = JwtController.GetJwtTokenData(token);
        var database = redis.GetDatabase();

        if (await JwtController.ValidateAccessJwtToken(database, dataToken))
        {
            var error = new BaseResponse
            {
                Success = false,
                Message = "отказано в доступе",
                StatusCode = 403,
                Error = "Forbidden"
            };

            return StatusCode(error.StatusCode, error);
        }
        
        var user = await context.Users.FirstOrDefaultAsync(u => u.PersonId == personId);

        if (user == null)
        {
            var error = new BaseResponse
            {
                Success = false,
                Message = "Пользователь не найден",
                StatusCode = 404,
                Error = "NotFound"
            };

            return StatusCode(error.StatusCode, error);
        }

        if (user.Role == Roles.admin)
        {
            var error = new BaseResponse
            {
                Success = false,
                Message = "Данного пользователя нельзя заблокировать",
                StatusCode = 403,
                Error = "Forbidden"
            };

            return StatusCode(error.StatusCode, error);
        }
        
        user.State = false;

        var tokens = await context.Tokens.Where(t => t.PersonId == personId).ToListAsync();
        List<string> tokensToBan = new List<string>();

        foreach (var item in tokens)
            tokensToBan.Add(item.AccessToken);
        await JwtController.AddTokensToBan(database, dataToken.UserId, tokensToBan);
            
        tokens.Clear();
        foreach (var item in tokens)
            tokensToBan.Add(item.RefreshToken);
        await JwtController.AddTokensToBan(database, dataToken.UserId, tokensToBan, JwtController.RefreshTokenLifetimeDay);
        
        context.Tokens.RemoveRange(tokens);
        await context.SaveChangesAsync();
        
        return Ok("Пользователь успешно заблокирован!");
    }
    
    /// <summary>
    /// Разблокирует пользователя в системе
    /// </summary>
    /// <param name="personId">Идентификатор пользователя</param>
    /// <returns></returns>
    [Authorize(Roles = "admin")]
    [HttpPost("unban-user")]
    [ServiceFilter(typeof(ValidateUserIpFilter))]
    public async Task<IActionResult> UnBanUser([Required] [FromQuery] string personId)
    {
        var authHeader = HttpContext.Request.Headers["Authorization"].ToString();
        var token = authHeader.Substring("Bearer ".Length);

        var dataToken = JwtController.GetJwtTokenData(token);
        var database = redis.GetDatabase();

        if (await JwtController.ValidateAccessJwtToken(database, dataToken))
        {
            var error = new BaseResponse
            {
                Success = false,
                Message = "отказано в доступе",
                StatusCode = 403,
                Error = "Forbidden"
            };

            return StatusCode(error.StatusCode, error);
        }
        
        var user = await context.Users.FirstOrDefaultAsync(u => u.PersonId == personId);
        
        if (user == null)
        {
            var error = new BaseResponse
            {
                Success = false,
                Message = "Пользователь не найден",
                StatusCode = 404,
                Error = "NotFound"
            };

            return StatusCode(error.StatusCode, error);
        }
        
        user.State = true;

        await context.SaveChangesAsync();
        
        return Ok("Пользователь успешно разблокирован!");
    }
    
    
    // добавляет новый продукт в систему
    [Authorize(Roles = "manager,admin")]
    [HttpPost("product")]
    public async Task<IActionResult> AddProduct(AddProduct addProduct)
    {
        Price price = new Price
        {
            cost = addProduct.Price.Cost,
            currency = addProduct.Price.Currency,
            discount = addProduct.Price.Discount,
            percent = addProduct.Price.Percent,
            costDiscount = addProduct.Price.CostDiscount,
        };
        
        Random rnd = new Random();
        List<Specifications> specs = new ();
        
        string productId = rnd.Next(111111111, 999999999).ToString();
        
        ProductDB productDb = new ProductDB
        {
            productId = productId,
            language = addProduct.Language,
            title = addProduct.Title,
            onSale = true,
            categories = addProduct.Categories,
            productType = addProduct.ProductType,
            productSubType = addProduct.ProductSubType,
            description = addProduct.Description,
            likes = 0,
            price = price,
            images = addProduct.Images,
            baseAdditionalInformation = addProduct.BaseAdditionalInformation,
            createTimeStamp = DateTime.Now
        };

        if (addProduct.Specifications != null)
        {
            foreach (var item in addProduct.Specifications)
            {
                string specificationId = rnd.Next(1111, 9999).ToString();

                Specifications specifications = new Specifications
                {
                    Name = item.Name,
                    specificationId = specificationId,
                    sku = productId + "-" + specificationId,
                    item = item.Item,
                    inStock = true,
                    stockCount = 100
                };

                specs.Add(specifications);
            }
            
            productDb.specifications = specs;
        }
        else
            productDb.specifications = null;
        
        await repository.AddProductAsync(productDb);
        
        return Ok();
    }
}