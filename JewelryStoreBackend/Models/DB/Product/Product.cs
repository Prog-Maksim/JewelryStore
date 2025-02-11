using System.Text.Json.Serialization;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace JewelryStoreBackend.Models.DB.Product;

public class Product
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    [JsonIgnore(Condition = JsonIgnoreCondition.Always)]
    public string Id { get; set; } = ObjectId.GenerateNewId().ToString();
    
    public string productId { get; set; }
    
    public string language { get; set; }
    
    public string title { get; set; }
    
    public bool onSale { get; set; }
    
    public string categories { get; set; }
    
    public string productType { get; set; }
    
    public string? productSubType { get; set; }

    public string description { get; set; }
    
    public int likes { get; set; }
    
    public Price price { get; set; }
    
    public List<string>? images { get; set; }

    public Dictionary<string, string> baseAdditionalInformation { get; set; }
    
    public List<Specifications>? specifications { get; set; }
    
    public DateTime createTimeStamp { get; set; }
}

public class Price
{
    public double cost { get; set; }
    
    public string currency { get; set; }
    
    public bool discount { get; set; }
    
    public int percent { get; set; }
    
    public double costDiscount { get; set; }
}

public class Specifications
{
    public string Name { get; set; }
    
    public string specificationId { get; set; }
    
    public string sku { get; set; }
    
    public string item { get; set; }
    
    public bool inStock { get; set; }
    
    [JsonIgnore(Condition = JsonIgnoreCondition.Always)]
    public int stockCount { get; set; }
}