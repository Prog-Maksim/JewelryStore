using System.ComponentModel.DataAnnotations;
using JewelryStoreBackend.Enums;
using JewelryStoreBackend.Models.Other;
using JewelryStoreBackend.Models.Response;
using JewelryStoreBackend.Repository.Interfaces;
using JewelryStoreBackend.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace JewelryStoreBackend.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Produces("application/json")]
[Route("api/v{version:apiVersion}/[controller]")]
public class ProductController(IProductRepository productRepository, ProductService productService): ControllerBase
{
    /// <summary>
    /// Возвращает товары для слайдера
    /// </summary>
    /// <param name="languageCode">Языковой код</param>
    /// <returns></returns>
    /// <response code="200">Успешное выполнение запроса</response>
    /// <response code="404">Товары не найдены</response>
    [AllowAnonymous]
    [HttpGet("get-slider-info")]
    [ProducesResponseType(typeof(List<Product>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(BaseResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetSliderInfo([Required] [FromQuery] string languageCode)
    {
        var (response, products) = await productService.GetSliderInfo(languageCode);
        
        if (!response.Success)
            return StatusCode(response.StatusCode, response);
        
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
        var (response, products) = await productService.GetProductsInSearchAsync(search, productType, minPrice, maxPrice, isSale,
            isStock, isDiscount, sortOrder, sortField, languageCode);
        
        if (!response.Success)
            return StatusCode(response.StatusCode, response);
        
        return Ok(products);
    }
    
    /// <summary>
    /// Выдает новые товары в системе
    /// </summary>
    /// <param name="languageCode">Языковой код</param>
    /// <returns></returns>
    /// <response code="200">Успешное выполнение запроса</response>
    /// <response code="404">Товары не найдены</response>
    [AllowAnonymous]
    [HttpGet("get-new-product")]
    [ProducesResponseType(typeof(List<Product>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(BaseResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetNewProduct(
        [Required][FromQuery] string languageCode)
    {
        var (response, products) = await productService.GetNewProductsAsync(languageCode);
        
        if (!response.Success)
            return StatusCode(response.StatusCode, response);
        
        return Ok(products);
    }
    
    /// <summary>
    /// Выдает полную информацию о товаре
    /// </summary>
    /// <param name="languageCode">Языковой код</param>
    /// <param name="sku">id товара со спецификацией</param>
    /// <returns></returns>
    /// <response code="200">Успешное выполнение запроса</response>
    /// <response code="404">Товар не найден</response>
    [AllowAnonymous]
    [HttpGet("get-detail-product")]
    [ProducesResponseType(typeof(BaseResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetDetailProduct(
        [Required][FromQuery] string languageCode,
        [Required][FromQuery] string sku
    )
    {
        var product = await productRepository.GetProductByIdAllAsync(languageCode, sku);

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
    /// <param name="targetSku">Id товара со спецификацией</param>
    /// <param name="languageCode">Языковой код</param>
    /// <returns></returns>
    /// <response code="200">Успешное выполнение запроса</response>
    /// <response code="404">Товары не найдены</response>
    [AllowAnonymous]
    [HttpGet("recommend-products")]
    [ProducesResponseType(typeof(BaseResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetRecommendProducts(
        [Required][FromQuery] string targetSku, 
        [Required][FromQuery] string languageCode)
    {
        var products = await productRepository.GetRecommendedProductsAsync(languageCode, targetSku);
        
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
    /// <response code="200">Успешное выполнение запроса</response>
    /// <response code="404">Товары не найдены</response>
    [AllowAnonymous]
    [HttpGet("popular-products")]
    [ProducesResponseType(typeof(List<Product>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(BaseResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetPopularProducts([Required][FromQuery] string languageCode)
    {
        var (response, products) = await productService.GetPopularProductsAsync(languageCode);
        
        if (!response.Success)
            return StatusCode(response.StatusCode, response);
        
        return Ok(products);
    }
    
    /// <summary>
    /// Возвращает абсолютно все товары в системе
    /// </summary>
    /// <param name="languageCode">Языковой код</param>
    /// <returns></returns>
    /// <response code="200">Успешное выполнение запроса</response>
    /// <response code="404">Товары не найдены</response>
    [AllowAnonymous]
    [HttpGet("products")]
    [ProducesResponseType(typeof(List<Product>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(BaseResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetAllProductsAsync([Required][FromQuery] string languageCode)
    {
        var (response, products) = await productService.GetAllProductsSingleSpecificationsAsync(languageCode);
        
        if (!response.Success)
            return StatusCode(response.StatusCode, response);
        
        return Ok(products);
    }
    
    
    /// <summary>
    /// Возвращает все товары в системе по категориям
    /// </summary>
    /// <param name="languageCode">Языковой код</param>
    /// <param name="category">Категория товара</param>
    /// <returns></returns>
    /// <response code="200">Успешное выполнение запроса</response>
    /// <response code="404">Товары не найдены</response>
    [AllowAnonymous]
    [HttpGet("products-category")]
    [ProducesResponseType(typeof(List<Product>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(BaseResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetAllProductsCategoryAsync(
        [Required][FromQuery] string languageCode,
        [Required][FromQuery] string category)
    {
        var (response, products) = await productService.GetProductsByCategorySingleSpecificationsAsync(category, languageCode);
        
        if (!response.Success)
            return StatusCode(response.StatusCode, response);
        
        return Ok(products);
    }

    
    /// <summary>
    /// Возвращает минимальную и максимальную цену товара 
    /// </summary>
    /// <returns></returns>
    /// <response code="200">Успешное выполнение запроса</response>
    /// <response code="404">Цены не найдены</response>
    [AllowAnonymous]
    [ProducesResponseType(typeof(BaseResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(MinMaxPrice), StatusCodes.Status200OK)]
    [HttpGet("get-min-max-price")]
    public async Task<IActionResult> GetMinMaxPriceAsync(string languageCode)
    {
        var (response, products) = await productService.GetMinMaxPricesAsync(languageCode);
        
        if (!response.Success)
            return StatusCode(response.StatusCode, response);
        
        return Ok(products);
    }
    
    
    /// <summary>
    /// Возвращает товар по ID
    /// </summary>
    /// <param name="languageCode">Языковой код</param>
    /// <param name="sku">Id товара со спецификацией</param>
    /// <returns></returns>
    /// <response code="200">Успешное выполнение запроса</response>
    /// <response code="404">Товар не найден</response>
    [AllowAnonymous]
    [HttpGet("get-product-inform-id")]
    [ProducesResponseType(typeof(Product), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(BaseResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetProductInIdAsync(
        [Required][FromQuery] string languageCode,
        [Required][FromQuery] string sku)
    {
        var (response, products) = await productService.GetProductByIdAsync(languageCode, sku);
        
        if (!response.Success)
            return StatusCode(response.StatusCode, response);
        
        return Ok(products);
    }

    /// <summary>
    /// Возвращает список доступных категорий
    /// </summary>
    /// <param name="languageCode">Языковой код</param>
    /// <returns></returns>
    /// <response code="200">Успешное выполнение запроса</response>
    /// <response code="404">Категории не найдены</response>
    [AllowAnonymous]
    [HttpGet("get-all-category")]
    [ProducesResponseType(typeof(List<string>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(BaseResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetAllCategoryAsync([Required] [FromQuery] string languageCode)
    {
        var productTypes = await productRepository.GetUniqueProductTypesAsync(languageCode);
        
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