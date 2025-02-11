namespace JewelryStoreBackend.Models.DB;

public class Basket
{
    public int Id { get; set; }
    public string PersonId { get; set; }
    public string ProductId { get; set; }
    public int Count { get; set; }
    public DateTime DateAdded { get; set; }
}