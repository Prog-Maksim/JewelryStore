using JewelryStoreBackend.Enums;

namespace JewelryStoreBackend.Models.Response;

public class PreviewOrder
{
    /// <summary>
    /// Идентификатор заказа
    /// </summary>
    public required string OrderId { get; set; }
    
    /// <summary>
    /// Дата оформления заказа
    /// </summary>
    public DateTime CreateOrderTimestamp { get; set; }
    
    /// <summary>
    /// Статус заказа
    /// </summary>
    public OrderStatus Status { get; set; }
    
    /// <summary>
    /// Стоимость заказа
    /// </summary>
    public double Cost { get; set; }
    
    /// <summary>
    /// Валюта
    /// </summary>
    public required string Currency { get; set; }
}