using System.ComponentModel.DataAnnotations;
using JewelryStoreBackend.Enums;
using JewelryStoreBackend.Models.DB;
using JewelryStoreBackend.Models.Response;
using JewelryStoreBackend.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using StackExchange.Redis;
using Convert = JewelryStoreBackend.Script.Convert;
using Product = JewelryStoreBackend.Models.Response.Product;

namespace JewelryStoreBackend.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Produces("application/json")]
[Route("api/v{version:apiVersion}/[controller]")]
public class ProductController(ApplicationContext context, ProductRepository repository, IConnectionMultiplexer redis): ControllerBase
{
    /// <summary>
    /// Возвращает товары для слайдера
    /// </summary>
    /// <param name="languageCode">Языковой код</param>
    /// <returns></returns>
    ///  <response code="200">Успешное выполнение запроса</response>
    ///  <response code="404">Товары не найдены</response>
    [AllowAnonymous]
    [HttpGet("get-slider-info")]
    [ProducesResponseType(typeof(List<Product>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(BaseResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetSliderInfo([Required][FromQuery] string languageCode)
    {
        var database = redis.GetDatabase();
        string cacheKey = $"slider-products:{languageCode}";
        
        var cachedData = await database.StringGetAsync(cacheKey);
        List<ProductsSlider> sliderItem = new List<ProductsSlider>();
        
        if (!cachedData.IsNullOrEmpty)
            sliderItem = JsonConvert.DeserializeObject<List<ProductsSlider>>(cachedData);
        else
        {
            sliderItem = context.ProductsSlider.ToList();
            var jsonData = JsonConvert.SerializeObject(sliderItem);
            await database.StringSetAsync(cacheKey, jsonData, TimeSpan.FromMinutes(10));
        }
        
        List<Product> products = new ();
        
        foreach (var item in sliderItem)
        {
            var result = await repository.GetProductByIdAsync(languageCode, item.SliderProductId);

            if (result != null)
            {
                Product product = Convert.ConvertToSimpleModel(result);
                product.ProductImageId = new List<string> { item.SliderImageId };
            
                products.Add(product);
            }
        }
        
        if (products.Count == 0)
        {
            var error = new BaseResponse
            {
                Success = false,
                Message = "Товары не найдены",
                StatusCode = 404,
                Error = "NotFound"
            };

            return StatusCode(error.StatusCode, error);
        }
        
        return Ok(products);
    }

    /// <summary>
    /// Производит поиск по товарам
    /// </summary>
    /// <param name="search">Строка поиска</param>
    /// <param name="productType">Тип продукта</param>
    /// <param name="minPrice">Минимальная цена</param>
    /// <param name="maxPrice">Максимальная цена</param>
    /// <param name="isSale">Продается ли товар</param>
    /// <param name="isStock">Есть ли товар в наличие</param>
    /// <param name="isDiscount">Есть ли скидка на товар</param>
    /// <param name="sortOrder">Сортировать по возрастанию или убыванию</param>
    /// <param name="sortField">Поле по которому сортировать данные</param>
    /// <param name="languageCode">Языковой код</param>
    /// <returns></returns>
    ///  <response code="200">Успешное выполнение запроса</response>
    ///  <response code="404">Товары не найдены</response>
    [AllowAnonymous]
    [HttpGet("search-products")]
    [ProducesResponseType(typeof(List<Product>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(BaseResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetSearchProducts(
        [FromQuery] string? search,
        [FromQuery] string? productType,
        [FromQuery] double? minPrice,
        [FromQuery] double? maxPrice,
        [FromQuery] bool? isSale,
        [FromQuery] bool? isStock,
        [FromQuery] bool? isDiscount,
        [FromQuery] SortedParameter? sortField,
        [FromQuery] Sorted? sortOrder,
        [Required][FromQuery] string languageCode)
    {
        var resultSearch = await repository.GetProductsInSearchAsync(search, productType, minPrice, maxPrice, isSale,
            isStock, isDiscount, sortOrder, sortField, languageCode);
        
        if (resultSearch.Count == 0)
        {
            var error = new BaseResponse
            {
                Success = false,
                Message = "Товары не найдены",
                StatusCode = 404,
                Error = "NotFound"
            };

            return StatusCode(error.StatusCode, error);
        }
        
        return Ok(Convert.ConvertProductWithSingleSpecification(resultSearch));
    }
    
    /// <summary>
    /// Выдает новые товары в системе
    /// </summary>
    /// <param name="languageCode">Языковой код</param>
    /// <returns></returns>
    ///  <response code="200">Успешное выполнение запроса</response>
    ///  <response code="404">Товары не найдены</response>
    [AllowAnonymous]
    [HttpGet("get-new-product")]
    [ProducesResponseType(typeof(List<Product>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(BaseResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetNewProduct(
        [Required][FromQuery] string languageCode)
    {
        var products = await repository.GetNewProductsAsync(languageCode);
        
        if (products.Count == 0)
        {
            var error = new BaseResponse
            {
                Success = false,
                Message = "Товары не найдены",
                StatusCode = 404,
                Error = "NotFound"
            };

            return StatusCode(error.StatusCode, error);
        }
        
        return Ok(Convert.ConvertProductWithSingleSpecification(products));
    }
    
    /// <summary>
    /// Выдает полную информацию о товаре
    /// </summary>
    /// <param name="languageCode">Языковой код</param>
    /// <param name="SKU">id товара со спецификацией</param>
    /// <returns></returns>
    ///  <response code="200">Успешное выполнение запроса</response>
    ///  <response code="404">Товар не найден</response>
    [AllowAnonymous]
    [HttpGet("get-detail-product")]
    [ProducesResponseType(typeof(BaseResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetDetailProduct(
        [Required][FromQuery] string languageCode,
        [Required][FromQuery] string SKU
    )
    {
        var product = await repository.GetProductByIdAllAsync(languageCode, SKU);

        if (product == null)
        {
            var error = new BaseResponse
            {
                Success = false,
                Message = "Товары не найдены",
                StatusCode = 404,
                Error = "NotFound"
            };

            return StatusCode(error.StatusCode, error);
        }
        
        return Ok(product);
    }

    /// <summary>
    /// Выдает рекомендованные товары к товару
    /// </summary>
    /// <param name="targetSKU">id товара со спецификацией</param>
    /// <param name="languageCode">Языковой код</param>
    /// <returns></returns>
    ///  <response code="200">Успешное выполнение запроса</response>
    ///  <response code="404">Товары не найдены</response>
    [AllowAnonymous]
    [HttpGet("recommend-products")]
    [ProducesResponseType(typeof(BaseResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetRecommendProducts(
        [Required][FromQuery] string targetSKU, 
        [Required][FromQuery] string languageCode)
    {
        var products = await repository.GetRecommendedProductsAsync(languageCode, targetSKU);
        
        if (products.Count == 0)
        {
            var error = new BaseResponse
            {
                Success = false,
                Message = "Товары не найдены",
                StatusCode = 404,
                Error = "NotFound"
            };

            return StatusCode(error.StatusCode, error);
        }
        
        return Ok(products);
    }
    
    /// <summary>
    /// Выдает самые залайканые товары, до 9 штук
    /// </summary>
    /// <param name="languageCode">Языковой код</param>
    /// <returns></returns>
    ///  <response code="200">Успешное выполнение запроса</response>
    ///  <response code="404">Товары не найдены</response>
    [AllowAnonymous]
    [HttpGet("popular-products")]
    [ProducesResponseType(typeof(List<Product>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(BaseResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetPopularProducts([Required][FromQuery] string languageCode)
    {
        var products = await repository.GetPopularProductsAsync(languageCode);
        
        if (products.Count == 0)
        {
            var error = new BaseResponse
            {
                Success = false,
                Message = "Товары не найдены",
                StatusCode = 404,
                Error = "NotFound"
            };

            return StatusCode(error.StatusCode, error);
        }
        
        return Ok(Convert.ConvertProductWithSingleSpecification(products));
    }
    
    /// <summary>
    /// Возвращает абсолютно все товары в системе
    /// </summary>
    /// <param name="languageCode">Языковой код</param>
    /// <returns></returns>
    ///  <response code="200">Успешное выполнение запроса</response>
    ///  <response code="404">Товары не найдены</response>
    [AllowAnonymous]
    [HttpGet("products")]
    [ProducesResponseType(typeof(List<Product>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(BaseResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetAllProductsAsync([Required][FromQuery] string languageCode)
    {
        var products = await repository.GetAllProductsAsync(languageCode);
        
        if (products.Count == 0)
        {
            var error = new BaseResponse
            {
                Success = false,
                Message = "Товары не найдены",
                StatusCode = 404,
                Error = "NotFound"
            };

            return StatusCode(error.StatusCode, error);
        }
        
        return Ok(Convert.ConvertProductWithSingleSpecification(products));
    }
    
    /// <summary>
    /// Возвращает все товары в системе по категориям
    /// </summary>
    /// <param name="languageCode">Языковой код</param>
    /// <param name="category">Категория товара</param>
    /// <returns></returns>
    ///  <response code="200">Успешное выполнение запроса</response>
    ///  <response code="404">Товары не найдены</response>
    [AllowAnonymous]
    [HttpGet("products-category")]
    [ProducesResponseType(typeof(List<Product>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(BaseResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetAllProductsCategoryAsync(
        [Required][FromQuery] string languageCode,
        [Required][FromQuery] string category)
    {
        var products = await repository.GetProductsByCategoryAsync(category, languageCode);
        
        if (products.Count == 0)
        {
            var error = new BaseResponse
            {
                Success = false,
                Message = "Товары не найдены",
                StatusCode = 404,
                Error = "NotFound"
            };

            return StatusCode(error.StatusCode, error);
        }
        
        return Ok(Convert.ConvertProductWithSingleSpecification(products));
    }

    /// <summary>
    /// Возвращает минимальную и максимальную цену товара 
    /// </summary>
    /// <returns></returns>
    ///  <response code="200">Успешное выполнение запроса</response>
    ///  <response code="404">Цены не найдены</response>
    [AllowAnonymous]
    [ProducesResponseType(typeof(BaseResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProductRepository.MinMaxPrice), StatusCodes.Status200OK)]
    [HttpGet("get-min-max-price")]
    public async Task<IActionResult> GetMinMaxPriceAsync(string languageCode)
    {
        var result = await repository.GetMinMaxPricesAsync(languageCode);
        
        if (result == null)
        {
            var error = new BaseResponse
            {
                Success = false,
                Message = "Цены не найдены",
                StatusCode = 404,
                Error = "NotFound"
            };

            return StatusCode(error.StatusCode, error);
        }
        
        return Ok(result);
    }
    
    /// <summary>
    /// Возвращает товар по ID
    /// </summary>
    /// <param name="languageCode">Языковой код</param>
    /// <param name="SKU">id товара со спецификацией</param>
    /// <returns></returns>
    ///  <response code="200">Успешное выполнение запроса</response>
    ///  <response code="404">Товар не найден</response>
    [AllowAnonymous]
    [HttpGet("get-product-inform-in-id")]
    [ProducesResponseType(typeof(Product), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(BaseResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetProductInIdAsync(
        [Required][FromQuery] string languageCode,
        [Required][FromQuery] string SKU)
    {
        var product = await repository.GetProductByIdAsync(languageCode, SKU);
        
        if (product == null)
        {
            var error = new BaseResponse
            {
                Success = false,
                Message = "Товары не найдены",
                StatusCode = 404,
                Error = "NotFound"
            };

            return StatusCode(error.StatusCode, error);
        }
        
        return Ok(Convert.ConvertToSimpleModel(product));
    }
    
    /// <summary>
    /// Возвращает список доступных категорий
    /// </summary>
    /// <param name="languageCode">Языковой код</param>
    /// <returns></returns>
    ///  <response code="200">Успешное выполнение запроса</response>
    ///  <response code="404">Категории не найдены</response>
    [AllowAnonymous]
    [HttpGet("get-all-category")]
    [ProducesResponseType(typeof(List<string>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(BaseResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetAllCategoryAsync([Required][FromQuery] string languageCode)
    {
        var productTypes = await repository.GetUniqueProductTypesAsync(languageCode);
        
        if (productTypes.Count == 0)
        {
            var error = new BaseResponse
            {
                Success = false,
                Message = "Категории не найдены",
                StatusCode = 404,
                Error = "NotFound"
            };

            return StatusCode(error.StatusCode, error);
        }
        
        return Ok(productTypes);
    }
}