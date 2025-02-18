namespace JewelryStoreBackend.Models.Other;

public class MinMaxPrice
{
    /// <summary>
    /// Минимальная цена
    /// </summary>
    public double MinPrice { get; set; }
    
    /// <summary>
    /// Максимальная цена
    /// </summary>
    public double MaxPrice { get; set; }
    
    /// <summary>
    /// Валюта
    /// </summary>
    public required string Currency  { get; set; }
}