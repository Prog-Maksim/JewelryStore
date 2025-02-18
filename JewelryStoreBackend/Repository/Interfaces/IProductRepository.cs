using JewelryStoreBackend.Enums;
using JewelryStoreBackend.Models.DB;
using JewelryStoreBackend.Models.DB.Product;
using JewelryStoreBackend.Models.DB.Rating;

namespace JewelryStoreBackend.Repository.Interfaces;

public interface IProductRepository
{
    /// <summary>
    /// Добавляет продукт
    /// </summary>
    /// <param name="productDb">Данные товара</param>
    /// <returns></returns>
    Task AddProductAsync(ProductDb productDb);
    
    /// <summary>
    /// Обновляет продукт
    /// </summary>
    /// <param name="sku">Артикул товара</param>
    /// <param name="product">Новые данные товара</param>
    /// <returns></returns>
    Task UpdateProductAsync(string sku, ProductDb product);
    
    /// <summary>
    /// Извлечение всех товаров
    /// </summary>
    /// <param name="languageCode">Языковой код</param>
    /// <returns></returns>
    Task<List<ProductDb>> GetAllProductsAsync(string languageCode);

    /// <summary>
    /// Возвращает товары для слайдера
    /// </summary>
    /// <param name="languageCode"></param>
    /// <returns></returns>
    Task<List<ProductsSlider>?> GetProductInSliderAsync(string languageCode);
    
    /// <summary>
    /// Производит поиск товаров
    /// </summary>
    /// <param name="search">Строка поиска</param>
    /// <param name="productType">Тип продукта</param>
    /// <param name="minPrice">Минимальная цена</param>
    /// <param name="maxPrice">Максимальная цена</param>
    /// <param name="isSale"></param>
    /// <param name="isStock"></param>
    /// <param name="isDiscount">Есть ли скидка</param>
    /// <param name="sortOrder">Сортирока по убыванию, возрастанию</param>
    /// <param name="sortField">Поле по которому производится поиск</param>
    /// <param name="languageCode">Языковой код</param>
    /// <returns></returns>
    Task<List<ProductDb>> GetProductsInSearchAsync(
        string? search,
        string? productType,
        double? minPrice,
        double? maxPrice,
        bool? isSale,
        bool? isStock,
        bool? isDiscount,
        Sorted? sortOrder,
        SortedParameter? sortField,
        string languageCode);
    
    /// <summary>
    /// Извлечение всех категорий товаров
    /// </summary>
    /// <param name="languageCode">Языковой код</param>
    /// <returns></returns>
    Task<List<string>> GetUniqueProductTypesAsync(string languageCode);
    
    /// <summary>
    /// Возвращает максимальную и минимальную цену товаров
    /// </summary>
    /// <param name="languageCode">Языковой код</param>
    /// <returns></returns>
    Task<(ProductDb minProduct, ProductDb maxProduct)> GetMinMaxPricesAsync(string languageCode);
    
    /// <summary>
    /// Получение всех товаров по категории и языковому коду
    /// </summary>
    /// <param name="category">Категория товара</param>
    /// <param name="languageCode">Языковой код</param>
    /// <returns></returns>
    Task<List<ProductDb>> GetProductsByCategoryAsync(string category, string languageCode);
    
    /// <summary>
    /// Получение всех новых товаров, добавленных за последние 2 недели
    /// </summary>
    /// <param name="languageCode">Языковой код</param>
    /// <returns></returns>
    Task<List<ProductDb>> GetNewProductsAsync(string languageCode);
    //
    /// <summary>
    /// Возвращает товар по id
    /// </summary>
    /// <param name="languageCode">Языковой код</param>
    /// <param name="sku">Артикул товара</param>
    /// <returns></returns>
    Task<ProductDb?> GetProductByIdAsync(string languageCode, string sku);
    
    /// <summary>
    /// Возвращает товар по id
    /// </summary>
    /// <param name="languageCode">Языковой код</param>
    /// <param name="sku">Артикул товара</param>
    /// <returns></returns>
    Task<ProductDb?> GetProductByIdAllAsync(string languageCode, string sku);
    
    /// <summary>
    /// Возвращает все рекоммендованные товары
    /// </summary>
    /// <param name="languageCode">Языковой код</param>
    /// <param name="sku">Артикул товара</param>
    /// <returns></returns>
    Task<List<ProductDb>> GetRecommendedProductsAsync(string languageCode, string sku);
    
    /// <summary>
    /// Вовзвращает все популярные товары
    /// </summary>
    /// <param name="languageCode">Языковой код</param>
    /// <returns></returns>
    Task<List<ProductDb>> GetPopularProductsAsync(string languageCode);

    /// <summary>
    /// Возвращает лайк пользователя на товар
    /// </summary>
    /// <param name="userId">Идентификатор пользователя</param>
    /// <param name="sku">Артикул</param>
    /// <returns></returns>
    Task<UsersLike?> GetLikesAsync(string userId, string sku);

    /// <summary>
    /// Добавляет лайк пользователя в БД
    /// </summary>
    /// <param name="like">Информация о лайке</param>
    /// <returns></returns>
    Task AddLikeAsync(UsersLike like);
    
    /// <summary>
    /// Удаляет лайк пользователя из БД
    /// </summary>
    /// <param name="like">Информация о лайке</param>
    void RemoveLikeAsync(UsersLike like);
    
    /// <summary>
    /// Сохраняет изменения в БД
    /// </summary>
    /// <returns></returns>
    Task SaveChangesAsync();
}