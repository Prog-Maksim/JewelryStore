using System.Text.Json.Serialization;
using JewelryStoreBackend.Script;

namespace JewelryStoreBackend.Models.Response.Order;

public class OrderData
{
    /// <summary>
    /// Языковой код
    /// </summary>
    public string languageCode { get; set; }
    
    /// <summary>
    /// Список товаров 
    /// </summary>
    public List<ProductOrderData> Items { get; set; }
    
    /// <summary>
    /// Детализация цены заказа
    /// </summary>
    public PriceOrderData PriceDatails { get; set; }
    
    /// <summary>
    /// Данные о заказчике 
    /// </summary>
    public UserOrderData userData { get; set; }
    
    /// <summary>
    /// Данные о доставки
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public ShippingOrderData? shippingData { get; set; }
    
    /// <summary>
    /// Данные о купоне
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] 
    public CouponOrderData? couponData { get; set; }
}

public class ProductOrderData
{
    /// <summary>
    /// Идентификатор товара
    /// </summary>
    public string SKU { get; set; }
    
    /// <summary>
    /// Название товара
    /// </summary>
    public string Title { get; set; }
    
    /// <summary>
    /// Цена за все товары
    /// </summary>
    public double Price { get; set; }
    
    /// <summary>
    /// Есть ли скидка на товар
    /// </summary>
    public bool Discount { get; set; }
    
    /// <summary>
    /// Цена со скидкой на все товары
    /// </summary>
    public double PriceDiscount { get; set; }
    
    /// <summary>
    /// Скидка на товар
    /// </summary>
    public int Percent { get; set; }
    
    /// <summary>
    /// Кол-во товаров
    /// </summary>
    public int Quantity { get; set; }
    
    /// <summary>
    /// Идентификатор изображения
    /// </summary>
    public string ProductImage { get; set; }
    
    /// <summary>
    /// Тип товара
    /// </summary>
    public string ProductType { get; set; }
    
    /// <summary>
    /// Дата добавления товара
    /// </summary>
    public DateTime ProductAddedData { get; set; }
}

public class PriceOrderData
{
    /// <summary>
    /// Общая цена всех товаров
    /// </summary>
    public double TotalPrice { get; set; }
    
    /// <summary>
    /// Общая цена всех товаров со скидкой
    /// </summary>
    public double TotalPriceDiscount { get; set; }
    
    /// <summary>
    /// Общая скидка на товары в корзине
    /// </summary>
    public int TotalPercentInProduct { get; set; }
    
    /// <summary>
    /// Скидка на товары по купону
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public int? PercentTheCoupon { get; set; }
    
    /// <summary>
    /// Новая цена товаров со скидкой по купону
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public double? TotalDiscountTheCoupon { get; set; }
    
    /// <summary>
    /// Цена за доставку товара
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public double? ShippingCost { get; set; }
    
    /// <summary>
    /// Итоговая цена заказа
    /// </summary>
    public double TotalCost { get; set; }
}

public class UserOrderData
{
    /// <summary>
    /// ФИО пользователя
    /// </summary>
    public string Name { get; set; }
    
    /// <summary>
    /// Почта пользователя
    /// </summary>
    public string Email { get; set; }
}

public class ShippingOrderData
{
    /// <summary>
    /// Адрес склада
    /// </summary>
    public string WarehouseAddress { get; set; }
    
    /// <summary>
    /// Выбранный адрес пользователя
    /// </summary>
    public string UserAddress { get; set; }
    
    /// <summary>
    /// Почтовый индекс пользователя
    /// </summary>
    public string? UserPostalCode { get; set; }
    
    /// <summary>
    /// Стоимоть доставки
    /// </summary>
    public double ShippingCost { get; set; }
    
    /// <summary>
    /// Рассчетное время доставки заказа в днях
    /// </summary>
    public int EstimatedDeliveryTime { get; set; }
    
    /// <summary>
    /// Выбранный метод доставки
    /// </summary>
    public DeliveryType ShippingMethod { get; set; }
    
    /// <summary>
    /// Детализация стоимости доставки
    /// </summary>
    public DetailsShippingOrderData Details { get; set; }
}

public class DetailsShippingOrderData
{
    /// <summary>
    /// Дистанция доставки, км
    /// </summary>
    public double Distance { get; set; }
    
    /// <summary>
    /// Цена за дистанцию
    /// </summary>
    public double DistanceCost { get; set; }
    
    /// <summary>
    /// Базовая стоимость доставки
    /// </summary>
    public double ShippingBasePrice { get; set; }
    
    /// <summary>
    /// Коэффицент за выбранный метод доставки
    /// </summary>
    public double DeliveryRate { get; set; }
    
    /// <summary>
    /// Коэффицент за расстояние доставки
    /// </summary>
    public double KilometerRate { get; set; }
    
    /// <summary>
    /// Итоговая стоимость доставки
    /// </summary>
    public double TotalCost { get; set; }
}

public class CouponOrderData
{
    /// <summary>
    /// Код купона
    /// </summary>
    public string CouponCode { get; set; }
    
    /// <summary>
    /// Название купона
    /// </summary>
    public string Title { get; set; }
    
    /// <summary>
    /// Описание купона
    /// </summary>
    public string Description { get; set; }
    
    /// <summary>
    /// Процент скидки купона
    /// </summary>
    public int Percent { get; set; }
}