using System.ComponentModel.DataAnnotations;

namespace JewelryStoreBackend.Models.DB;

public class Basket
{
    public int Id { get; set; }
    
    /// <summary>
    /// Идентификатор пользователя
    /// </summary>
    [MaxLength(100)]
    public required string PersonId { get; set; }
    
    /// <summary>
    /// Идентификатор товара
    /// </summary>
    [MaxLength(20)]
    public required string ProductId { get; set; }
    
    /// <summary>
    /// Кол-во товаров
    /// </summary>
    public int Count { get; set; }
    
    /// <summary>
    /// Дата добавления товара
    /// </summary>
    public DateTime DateAdded { get; set; }
}