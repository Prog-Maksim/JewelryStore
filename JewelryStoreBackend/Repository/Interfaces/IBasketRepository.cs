using JewelryStoreBackend.Models.DB;

namespace JewelryStoreBackend.Repository.Interfaces;

public interface IBasketRepository
{
    /// <summary>
    /// Возвращает все товары в корзине пользователя
    /// </summary>
    /// <param name="userId">Идентификатор пользователя</param>
    /// <returns></returns>
    Task<List<Basket>> GetUserBasketAsync(string userId);

    /// <summary>
    /// Возвращает товар в корзине пользователя
    /// </summary>
    /// <param name="userId">Идентификатор пользователя</param>
    /// <param name="productId">Идентификатор товара</param>
    /// <returns></returns>
    Task<Basket?> GetProductBasketAsync(string userId, string productId);
    
    /// <summary>
    /// Удаляет товары в корзине у пользователя
    /// </summary>
    /// <param name="userId">Идентификатор пользователя</param>
    /// <returns></returns>
    Task RemoveBasketItemsAsync(string userId);

    /// <summary>
    /// Добавляет товар в корзину
    /// </summary>
    /// <param name="basket"></param>
    /// <returns></returns>
    Task AddProductToBasketAsync(Basket basket);

    /// <summary>
    /// Удаляет товар в корзине
    /// </summary>
    /// <param name="basket"></param>
    void DeleteProductToBasketAsync(Basket basket);
    
    /// <summary>
    /// Сохраняет изменения в БД
    /// </summary>
    /// <returns></returns>
    Task SaveChangesAsync();
}