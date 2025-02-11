namespace JewelryStoreBackend.Models.Response.Basket;

public class PriceProduction
{
    /// <summary>
    /// Цена
    /// </summary>
    public double cost { get; set; }
    
    /// <summary>
    /// Валюта
    /// </summary>
    public string currency { get; set; }
    
    /// <summary>
    /// Есть ли скидка
    /// </summary>
    public bool discount { get; set; }
    
    /// <summary>
    /// Процент скидки
    /// </summary>
    public int percent { get; set; }
    
    /// <summary>
    /// Цена со скидкой
    /// </summary>
    public double costDiscount { get; set; }
}