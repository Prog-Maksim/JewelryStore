namespace JewelryStoreBackend.Models.DB;

public class ProductsSlider
{
    public int Id { get; set; } 
    
    /// <summary>
    /// Идентификатор продукта для слайдера
    /// </summary>
    public string SliderProductId { get; set; }
    
    /// <summary>
    /// Дата добавления
    /// </summary>
    public DateTime DateAdded { get; set; }

    /// <summary>
    /// Идентификатор изображения для слайдера
    /// </summary>
    public string SliderImageId { get; set; }
    
    /// <summary>
    /// Дата окончания слайдера
    /// </summary>
    public DateTime DateEnd { get; set; }
}