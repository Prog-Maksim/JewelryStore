namespace JewelryStoreBackend.Models.Response.ProductStructure;

public class Price
{
    /// <summary>
    /// Цена товара
    /// </summary>
    public double Cost { get; set; }
    
    /// <summary>
    /// Символ валюты 
    /// </summary>
    public string? Currency  { get; set; }
    
    /// <summary>
    /// Есть ли скидка на товар
    /// </summary>
    public bool Discount { get; set; }
    
    /// <summary>
    /// Скидка на товар
    /// </summary>
    public int Percent { get; set; }
    
    /// <summary>
    /// Цена на товар со скидкой
    /// </summary>
    public double CostDiscount { get; set; }
}