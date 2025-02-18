using System.Text.Json.Serialization;
using JewelryStoreBackend.Models.Response.ProductStructure;

namespace JewelryStoreBackend.Models.Response;

public class Product
{
    /// <summary>
    /// Id товара
    /// </summary>
    public required string ProductId { get; set; }
    
    /// <summary>
    /// Артикул товара
    /// </summary>
    public required string Sku { get; set; }
    
    /// <summary>
    /// Цена товара
    /// </summary>
    public Price? Price { get; set; }
    
    /// <summary>
    /// Заголовок товара
    /// </summary>
    public required string Title { get; set; }
    
    /// <summary>
    /// Описание товара
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public string? Description { get; set; }
    
    /// <summary>
    /// Идентификатор изображения
    /// </summary>
    public List<string>? ProductImageId { get; set; }
    
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