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
    public string orderId { get; set; }
    
    /// <summary>
    /// Фамилия имя пользователя
    /// </summary>
    public string name { get; set; }
    
    /// <summary>
    /// Поста пользователя
    /// </summary>
    public string email { get; set; }
    
    /// <summary>
    /// Номер телефона пользователя
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? phoneNumber { get; set; }
    
    /// <summary>
    /// Дата и время создания заказа
    /// </summary>
    public DateTime createTimestamp { get; set; }
    
    /// <summary>
    /// Дата и время выполнения заказа
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public DateTime? completedTimestamp { get; set; }
    
    /// <summary>
    /// Статус заказа
    /// </summary>
    public OrderStatus status { get; set; }
    
    /// <summary>
    /// Стоимость заказа
    /// </summary>
    public double orderCost { get; set; }
    
    /// <summary>
    /// Валюта
    /// </summary>
    public string currency { get; set; }
    
    public List<OrderDetailProduct> products { get; set; }
    public OrderDetailPayment payment { get; set; }
    public OrderDetailShipping shipping { get; set; }
}

public class OrderDetailProduct
{
    /// <summary>
    /// Идентификатор товара
    /// </summary>
    public string sku { get; set; }
    
    /// <summary>
    /// Стоимость товара
    /// </summary>
    public double cost { get; set; }
    
    /// <summary>
    /// Кол-во товара
    /// </summary>
    public int quantity { get; set; }
}

public class OrderDetailPayment
{
    /// <summary>
    /// Метод оплаты
    /// </summary>
    public PaymentType paymentMethod { get; set; }
    
    /// <summary>
    /// Статус оплаты
    /// </summary>
    public PaymentStatus paymentStatus { get; set; }
    
    /// <summary>
    /// Дата оплаты заказа
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public DateTime? datePayment { get; set; }
}

public class OrderDetailShipping
{
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
    /// Время доставки заказа
    /// </summary>
    public DateTime DateShipping { get; set; }
}
