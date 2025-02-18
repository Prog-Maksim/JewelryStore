using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;

namespace JewelryStoreBackend.Models.DB.Product;

public class Warehouse
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = ObjectId.GenerateNewId().ToString();
    
    /// <summary>
    /// Идентификатор товара
    /// </summary>
    [BsonElement("SKU")]
    public required string Sku { get; set; }
    
    /// <summary>
    /// Идентификатор склада
    /// </summary>
    public int WarehouseId { get; set; }
    
    /// <summary>
    /// Кол-во товаров
    /// </summary>
    public int Count { get; set; }
}