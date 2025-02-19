using System.ComponentModel.DataAnnotations;

namespace JewelryStoreBackend.Models.DB;

public class ProductsSlider
{
    public int Id { get; set; } 
    
    /// <summary>
    /// Идентификатор продукта для слайдера
    /// </summary>
    [MaxLength(255)]
    public required string SliderProductId { get; set; }
    
    /// <summary>
    /// Дата добавления
    /// </summary>
    public DateTime DateAdded { get; set; }

    /// <summary>
    /// Идентификатор изображения для слайдера
    /// </summary>
    [MaxLength(100)]
    public required string SliderImageId { get; set; }
    
    /// <summary>
    /// Дата окончания слайдера
    /// </summary>
    public DateTime DateEnd { get; set; }
}