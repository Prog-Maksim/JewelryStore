using System.Text.Json.Serialization;
using JewelryStoreBackend.Enums;
using JewelryStoreBackend.Models.Request;
using JewelryStoreBackend.Script;

namespace JewelryStoreBackend.Models.Response.Order;

public class OrderDetail
{
    /// <summary>
    /// Идентификатор заказа
    /// </summary>
    public required string OrderId { get; set; }
    
    /// <summary>
    /// Фамилия имя пользователя
    /// </summary>
    public required string Name { get; set; }
    
    /// <summary>
    /// Поста пользователя
    /// </summary>
    public required string Email { get; set; }
    
    /// <summary>
    /// Номер телефона пользователя
    /// </summary>
    public required string PhoneNumber { get; set; }
    
    /// <summary>
    /// Дата и время создания заказа
    /// </summary>
    public DateTime CreateTimestamp { get; set; }
    
    /// <summary>
    /// Дата и время выполнения заказа
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public DateTime? CompletedTimestamp { get; set; }
    
    /// <summary>
    /// Статус заказа
    /// </summary>
    public OrderStatus Status { get; set; }
    
    /// <summary>
    /// Стоимость заказа
    /// </summary>
    public double OrderCost { get; set; }
    
    /// <summary>
    /// Валюта
    /// </summary>
    public required string Currency { get; set; }
    
    public List<OrderDetailProduct>? Products { get; set; }
    public OrderDetailPayment? Payment { get; set; }
    public OrderDetailShipping? Shipping { get; set; }
}

public class OrderDetailProduct
{
    /// <summary>
    /// Идентификатор товара
    /// </summary>
    public required string Sku { get; set; }
    
    /// <summary>
    /// Стоимость товара
    /// </summary>
    public double Cost { get; set; }
    
    /// <summary>
    /// Кол-во товара
    /// </summary>
    public int Quantity { get; set; }
}

public class OrderDetailPayment
{
    /// <summary>
    /// Метод оплаты
    /// </summary>
    public PaymentType PaymentMethod { get; set; }
    
    /// <summary>
    /// Статус оплаты
    /// </summary>
    public PaymentStatus PaymentStatus { get; set; }
    
    /// <summary>
    /// Дата оплаты заказа
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public DateTime? DatePayment { get; set; }
}

public class OrderDetailShipping
{
    /// <summary>
    /// Адрес пользователя
    /// </summary>
    public required string TargetAddress { get; set; }
    
    /// <summary>
    /// Адрес склада
    /// </summary>
    public required string WarehouseAddress { get; set; }
    
    /// <summary>
    /// Тип доставки
    /// </summary>
    public DeliveryType DeliveryType { get; set; }
    
    /// <summary>
    /// Время доставки заказа
    /// </summary>
    public DateTime DateShipping { get; set; }
}
