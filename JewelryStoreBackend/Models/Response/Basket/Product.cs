namespace JewelryStoreBackend.Models.Response.Basket;

public class Productions
{
    /// <summary>
    /// Языковой код
    /// </summary>
    public required string LanguageCode { get; set; }
    
    /// <summary>
    /// Внутренний идентификатор товара
    /// </summary>
    public required string ProductId { get; set; }
    
    /// <summary>
    /// Идентификатор товара
    /// </summary>
    public required string Sku { get; set; }
    
    /// <summary>
    /// Название товара
    /// </summary>
    public required string Title { get; set; }
    
    /// <summary>
    /// Описание товара
    /// </summary>
    public required string Description { get; set; }
    
    /// <summary>
    /// Идентификаторы картинок
    /// </summary>
    public required List<string> Images { get; set; }
    
    /// <summary>
    /// Цена товара
    /// </summary>
    public required PriceProduction PriceProduction { get; set; }
    
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