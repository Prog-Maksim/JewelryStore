namespace JewelryStoreBackend.Enums;

public enum OrderStatus
{
    /// <summary>
    /// В обработке
    /// </summary>
    Pending,
    
    /// <summary>
    /// Оформлен
    /// </summary>
    Decorated,
    
    /// <summary>
    /// В обработке
    /// </summary>
    Processing,
    
    /// <summary>
    /// Сборка
    /// </summary>
    Build,
    
    /// <summary>
    /// Доставка
    /// </summary>
    Delivery,
    
    /// <summary>
    /// Оплата
    /// </summary>
    Payment,
    
    /// <summary>
    /// Получен
    /// </summary>
    Received,
    
    /// <summary>
    /// Завершен
    /// </summary>
    Completed,
    
    /// <summary>
    /// Отменен
    /// </summary>
    Cancelled,
    
    /// <summary>
    /// Возврат
    /// </summary>
    Refund
}