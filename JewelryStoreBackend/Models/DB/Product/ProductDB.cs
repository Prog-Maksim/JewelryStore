using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace JewelryStoreBackend.Models.DB.Product;

public class ProductDb
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    [JsonIgnore(Condition = JsonIgnoreCondition.Always)]
    public string Id { get; set; } = ObjectId.GenerateNewId().ToString();
    
    /// <summary>
    /// Идентификатор продукта
    /// </summary>
    [MaxLength(10)]
    [BsonElement("productId")]
    public required string ProductId { get; set; }
    
    /// <summary>
    /// Языковой код
    /// </summary>
    [MaxLength(5)]
    [BsonElement("language")]
    public required string Language { get; set; }
    
    /// <summary>
    /// Название
    /// </summary>
    [MaxLength(100)]
    [BsonElement("title")]
    public required string Title { get; set; }
    
    /// <summary>
    /// Продается?
    /// </summary>
    [BsonElement("onSale")]
    public bool OnSale { get; set; }
    
    /// <summary>
    /// Категория
    /// </summary>
    [BsonElement("categories")]
    public required string Categories { get; set; }
    
    /// <summary>
    /// Тип товара
    /// </summary>
    [BsonElement("productType")]
    public required string ProductType { get; set; }
    
    /// <summary>
    /// Под тип товара
    /// </summary>
    [BsonElement("productSubType")]
    public string? ProductSubType { get; set; }

    /// <summary>
    /// Описание
    /// </summary>
    [BsonElement("description")]
    public required string Description { get; set; }
    
    /// <summary>
    /// Лайки
    /// </summary>
    [BsonElement("likes")]
    public int Likes { get; set; }
    
    /// <summary>
    /// Цена
    /// </summary>
    [BsonElement("price")]
    public Price Price { get; set; }
    
    /// <summary>
    /// Идентификаторы изображений
    /// </summary>
    [BsonElement("images")]
    public required List<string> Images { get; set; }

    /// <summary>
    /// Дополнительная информация
    /// </summary>
    [BsonElement("baseAdditionalInformation")]
    public required Dictionary<string, string> BaseAdditionalInformation { get; set; }
    
    /// <summary>
    /// Спецификации товара
    /// </summary>
    [BsonElement("specifications")]
    public required List<Specifications> Specifications { get; set; }
    
    /// <summary>
    /// Дата создания
    /// </summary>
    [BsonElement("createTimeStamp")]
    public DateTime CreateTimeStamp { get; set; }
}

public class Price
{
    /// <summary>
    /// Цена товара
    /// </summary>
    [BsonElement("cost")]
    public double Cost { get; set; }
    
    /// <summary>
    /// Значек валюты
    /// </summary>
    [BsonElement("currency")]
    public required string Currency { get; set; }
    
    /// <summary>
    /// Есть ли скидка на товар
    /// </summary>
    [BsonElement("discount")]
    public bool Discount { get; set; }
    
    /// <summary>
    /// Процент скидки
    /// </summary>
    [BsonElement("percent")]
    public int Percent { get; set; }
    
    /// <summary>
    /// Цена со скидкой
    /// </summary>
    [BsonElement("costDiscount")]
    public double CostDiscount { get; set; }
}

public class Specifications
{
    /// <summary>
    /// Имя характеристики
    /// </summary>
    [BsonElement("Name")]
    public required string Name { get; set; }
    
    /// <summary>
    /// Идентификатор характеристики
    /// </summary>
    [BsonElement("specificationId")]
    public required string SpecificationId { get; set; }
    
    /// <summary>
    /// Артикул товара
    /// </summary>
    [BsonElement("sku")]
    public required string Sku { get; set; }
    
    /// <summary>
    /// Название характеристики
    /// </summary>
    [BsonElement("item")]
    public required string Item { get; set; }
    
    /// <summary>
    /// Есть в наличие?
    /// </summary>
    [BsonElement("inStock")]
    public bool InStock { get; set; }
    
    /// <summary>
    /// Кол-во товарав на складе
    /// </summary>
    [BsonElement("stockCount")]
    [JsonIgnore(Condition = JsonIgnoreCondition.Always)]
    public int StockCount { get; set; }
}