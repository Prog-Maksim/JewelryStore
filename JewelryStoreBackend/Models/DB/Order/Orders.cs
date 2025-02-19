using System.ComponentModel.DataAnnotations;
using JewelryStoreBackend.Enums;
using JewelryStoreBackend.Models.DB.User;

namespace JewelryStoreBackend.Models.DB.Order;

public class Orders
{
    public int Id { get; set; }
    
    /// <summary>
    /// Идентификатор заказа
    /// </summary>
    [MaxLength(11)]
    public required string OrderId { get; set; }
    
    /// <summary>
    /// Идентификатор пользователя
    /// </summary>
    [MaxLength(100)]
    public required string PersonId { get; set; }
    
    /// <summary>
    /// Дата создания заказа
    /// </summary>
    public DateTime CreateTimestamp { get; set; }
    
    /// <summary>
    /// Дата завершения заказа
    /// </summary>
    public DateTime? CompletedTimestamp { get; set; }
    
    /// <summary>
    /// Статус заказа
    /// </summary>
    public OrderStatus Status { get; set; }
    
    /// <summary>
    /// Итоговая цена заказа
    /// </summary>
    public double OrderCost { get; set; }
    
    /// <summary>
    /// Валюта
    /// </summary>
    [MaxLength(1)]
    public required string Currency { get; set; } 
    
    
    public ICollection<OrderProducts> Products { get; set; }
    public OrderPayments Payment { get; set; }
    public OrderShippings Shipping { get; set; }
    public Users Users { get; set; }
}