using System.ComponentModel.DataAnnotations;
using JewelryStoreBackend.Script;

namespace JewelryStoreBackend.Models.DB.Order;

public class OrderShippings
{
    public int Id { get; set; }
    
    /// <summary>
    /// Идентификатор заказа
    /// </summary>
    [MaxLength(11)]
    public required string OrderId { get; set; }
    
    /// <summary>
    /// Адрес пользователя
    /// </summary>
    [MaxLength(500)]
    public required string TargetAddress { get; set; }
    
    /// <summary>
    /// Адрес склада
    /// </summary>
    [MaxLength(500)]
    public required string WarehouseAddress { get; set; }
    
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