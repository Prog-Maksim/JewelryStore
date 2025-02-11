namespace JewelryStoreBackend.Models.Response.Basket;

public class Productions
{
    /// <summary>
    /// Языковой код
    /// </summary>
    public string languageCode { get; set; }
    
    /// <summary>
    /// Внутренний идентификатор товара
    /// </summary>
    public string ProductId { get; set; }
    
    /// <summary>
    /// Идентификатор товара
    /// </summary>
    public string SKU { get; set; }
    
    /// <summary>
    /// Название товара
    /// </summary>
    public string Title { get; set; }
    
    /// <summary>
    /// Описание товара
    /// </summary>
    public string Description { get; set; }
    
    /// <summary>
    /// Идентификаторы картинок
    /// </summary>
    public List<string> Images { get; set; }
    
    /// <summary>
    /// Цена товара
    /// </summary>
    public PriceProduction PriceProduction { get; set; }
    
    /// <summary>
    /// Кол-во товаров в корзине
    /// </summary>
    public int Quantity { get; set; }
    
    /// <summary>
    /// Статус товара (продается/не продается)
    /// </summary>
    public bool OnSale { get; set; }
    
    /// <summary>
    /// Наличие товара
    /// </summary>
    public bool InStock { get; set; }
    
    /// <summary>
    /// Кол-во лайков товара
    /// </summary>
    public int Likes { get; set; }
}