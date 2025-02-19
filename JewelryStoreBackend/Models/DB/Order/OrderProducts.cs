using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace JewelryStoreBackend.Models.DB.Order;

public class OrderProducts
{
    public int Id { get; set; }
    
    /// <summary>
    /// Идентификатор заказа
    /// </summary>
    [MaxLength(11)]
    public required string OrderId { get; set; }
    
    /// <summary>
    /// Идентификатор товара
    /// </summary>
    [MaxLength(20)]
    [Column("SKU")]
    public required string Sku { get; set; }
    
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