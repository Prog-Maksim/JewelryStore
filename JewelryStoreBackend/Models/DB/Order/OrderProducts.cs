namespace JewelryStoreBackend.Models.DB.Order;

public class OrderProducts
{
    public int Id { get; set; }
    
    /// <summary>
    /// Идентификатор заказа
    /// </summary>
    public string OrderId { get; set; }
    
    /// <summary>
    /// Идентификатор товара
    /// </summary>
    public string SKU { get; set; }
    
    /// <summary>
    /// Стоимость товаров
    /// </summary>
    public double Cost { get; set; }
    
    /// <summary>
    /// Кол-во товаров
    /// </summary>
    public int Quantity { get; set; }
    

    public Orders Order { get; set; }
}