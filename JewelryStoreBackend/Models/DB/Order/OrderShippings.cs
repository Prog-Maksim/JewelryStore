using JewelryStoreBackend.Script;

namespace JewelryStoreBackend.Models.DB.Order;

public class OrderShippings
{
    public int Id { get; set; }
    
    /// <summary>
    /// Идентификатор заказа
    /// </summary>
    public string OrderId { get; set; }
    
    /// <summary>
    /// Адрес пользователя
    /// </summary>
    public string TargetAddress { get; set; }
    
    /// <summary>
    /// Адрес склада
    /// </summary>
    public string WarehouseAddress { get; set; }
    
    /// <summary>
    /// Тип доставки
    /// </summary>
    public DeliveryType DeliveryType { get; set; }
    
    /// <summary>
    /// Дата доставки товара
    /// </summary>
    public DateTime DateShipping { get; set; }
    
    public Orders Order { get; set; }
}