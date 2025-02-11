namespace JewelryStoreBackend.Models.Request;

/// <summary>
/// Основная информация о товаре
/// </summary>
public class AddProduct
{
    /// <summary>
    /// Языковой код
    /// </summary>
    public string Language {get; set;}
    
    /// <summary>
    /// Название товара
    /// </summary>
    public string Title { get; set; }
    
    /// <summary>
    /// Категория товара
    /// </summary>
    public string Categories { get; set; }
    
    /// <summary>
    /// Тип продукта
    /// </summary>
    public string ProductType { get; set; }
    
    /// <summary>
    /// Под тип продукта
    /// </summary>
    public string? ProductSubType { get; set; }
    
    /// <summary>
    /// Описание товара
    /// </summary>
    public string Description { get; set; }
    
    /// <summary>
    /// Цена товара
    /// </summary>
    public PriceCost Price { get; set; }
    
    /// <summary>
    /// Список идентификаторов фотографий
    /// </summary>
    public List<string>? Images { get; set; }
    
    /// <summary>
    /// Дополнительная информация о товаре
    /// </summary>
    public Dictionary<string, string> BaseAdditionalInformation { get; set; }

    /// <summary>
    /// Спецификация товара
    /// </summary>
    public List<SpecificationsCost>? Specifications { get; set; }
}

public class PriceCost
{
    /// <summary>
    /// Цена
    /// </summary>
    public double Cost { get; set; }

    /// <summary>
    /// Валюта
    /// </summary>
    public string Currency { get; set; }

    /// <summary>
    /// Статус скидки
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

public class SpecificationsCost
{
    /// <summary>
    /// Название спецификации
    /// </summary>
    public string Name { get; set; }
    
    /// <summary>
    /// Значение спецификации
    /// </summary>
    public string Item { get; set; }
}