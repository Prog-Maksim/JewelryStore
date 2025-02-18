using JewelryStoreBackend.Models.DB.Order;
using JewelryStoreBackend.Models.Response.Order;

namespace JewelryStoreBackend.Repository.Interfaces;

public interface IOrderRepository
{
    /// <summary>
    /// Проверяет есть ли данный ключ
    /// </summary>
    /// <param name="userId">Идентификатор пользователя</param>
    /// <returns></returns>
    Task<bool> IsOrderInProgressAsync(string userId);
    
    /// <summary>
    /// Сохраняет данные о заказе
    /// </summary>
    /// <param name="userId">Идентификатор пользователя</param>
    /// <param name="orderData">Данные заказа</param>
    /// <param name="expiration">Время жизни заказа</param>
    Task StartOrderAsync(string userId, OrderData orderData, TimeSpan expiration);
    
    /// <summary>
    /// Отменяет заказ
    /// </summary>
    /// <param name="userId">Идентификатор пользователя</param>
    /// <returns></returns>
    Task<bool> CancelOrderAsync(string userId);
    
    /// <summary>
    /// Возвращает данныео о заказе
    /// </summary>
    /// <param name="userId"></param>
    /// <returns></returns>
    Task<OrderData?> GetOrderDataByIdAsync(string userId);
    
    /// <summary>
    /// Возвращает заказ пользователя
    /// </summary>
    /// <param name="userId">Идентификатор пользователя</param>
    /// <param name="orderId">Идентификатор заказа</param>
    /// <returns></returns>
    Task<Orders?> GetOrderByIdAsync(string userId, string orderId);
    
    /// <summary>
    /// Возвращает все заказы пользователя
    /// </summary>
    /// <param name="userId">Идентификатор пользователя</param>
    /// <returns></returns>
    Task<List<Orders>> GetAllOrdersByUserIdAsync(string userId);
    
    /// <summary>
    /// Возвращает полную информацию о заказе
    /// </summary>
    /// <param name="userId">Идентификатор пользователя</param>
    /// <param name="orderId">Идентификатор заказа</param>
    /// <returns></returns>
    Task<Orders?> GetDetailOrderByIdAsync(string userId, string orderId);
    
    /// <summary>
    /// Добавляет заказ в БД
    /// </summary>
    /// <param name="order">Информация о заказе</param>
    /// <returns></returns>
    Task AddOrderAsync(Orders order);
    
    /// <summary>
    /// Добавляет Продукты заказа в БД
    /// </summary>
    /// <param name="products">Информация о товаре</param>
    /// <returns></returns>
    Task AddOrderProductsAsync(OrderProducts products);
    
    /// <summary>
    /// Добавляет Оплату в БД
    /// </summary>
    /// <param name="payments">Информация о платеже</param>
    /// <returns></returns>
    Task AddOrderPaymentsAsync(OrderPayments payments);
    
    /// <summary>
    /// Добавляет Доставку в БД
    /// </summary>
    /// <param name="shippings">Информация о доставке</param>
    /// <returns></returns>
    Task AddOrderShippingsAsync(OrderShippings shippings);
    
    /// <summary>
    /// Сохраняет изменения в БД
    /// </summary>
    /// <returns></returns>
    Task SaveChangesAsync();
}