using JewelryStoreBackend.Models.DB.Rating;

namespace JewelryStoreBackend.Repository.Interfaces;

public interface IMessageRepository
{
    /// <summary>
    /// Добавляет сообщение пользователя
    /// </summary>
    /// <param name="message">Информация о товаре</param>
    /// <returns></returns>
    Task<string> AddMessageAsync(Message message);

    /// <summary>
    /// Обнволяет сообщение пользователя
    /// </summary>
    /// <param name="produсtId">Идентификатор продукта</param>
    /// <param name="messageId">Идентификатор сообщения</param>
    /// <param name="newText">Новый текст комментария</param>
    /// <param name="newRating">Новый рейтинг</param>
    /// <returns></returns>
    Task UpdateMessageAsync(string produсtId, string messageId, string newText, int newRating);
    
    /// <summary>
    /// Удаляет сообщение пользователя
    /// </summary>
    /// <param name="productId">Идентификатор продукта</param>
    /// <param name="messageId">Идентификатор сообщения</param>
    /// <returns></returns>
    Task DeleteMessageAsync(string productId, string messageId);
    
    /// <summary>
    /// Возвращает сообщение
    /// </summary>
    /// <param name="productId">Идентификатор продукта</param>
    /// <param name="messageId">Идентификатор сообщения</param>
    /// <returns></returns>
    Task<Message?> GetMessageByIdAsync(string productId, string messageId);
    
    /// <summary>
    /// Возвращает все сообщения
    /// </summary>
    /// <param name="productId">Идентификатор продукта</param>
    /// <returns></returns>
    Task<IEnumerable<Message>> GetMessagesByProductIdAsync(string productId);
    
    /// <summary>
    /// Возвращает кол-во комментариев для товара
    /// </summary>
    /// <param name="productId">Идентификатор продукта</param>
    /// <returns></returns>
    Task<long> GetMessageCountByProductIdAsync(string productId);
    
    /// <summary>
    /// Возвращает все оценки пользователей для товара
    /// </summary>
    /// <param name="productId">Идентификатор продукта</param>
    /// <returns></returns>
    Task<List<int>> GetAllRatingsAsync(string productId);
}