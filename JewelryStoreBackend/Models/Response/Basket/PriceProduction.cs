namespace JewelryStoreBackend.Models.Response.Basket;

public class PriceProduction
{
    /// <summary>
    /// Цена
    /// </summary>
    public double Cost { get; set; }
    
    /// <summary>
    /// Валюта
    /// </summary>
    public required string Currency { get; set; }
    
    /// <summary>
    /// Есть ли скидка
    /// </summary>
    public bool Discount { get; set; }
    
    /// <summary>
    /// Процент скидки
    /// </summary>
    public int Percent { get; set; }
    
    /// <summary>
    /// Цена со скидкой
    /// </summary>
    public double CostDiscount { get; set; }
}