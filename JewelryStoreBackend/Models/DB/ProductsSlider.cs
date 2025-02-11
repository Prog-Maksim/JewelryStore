namespace JewelryStoreBackend.Models.DB;

public class ProductsSlider
{
    public int Id { get; set; } 
    public string SliderProductId { get; set; }
    public DateTime DateAdded { get; set; }

    public string SliderImageId { get; set; }
    public DateTime DateEnd { get; set; }
}