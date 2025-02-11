namespace JewelryStoreBackend.Models.DB.Rating;

public class UsersLike
{
    public int Id { get; set; }
    public string PersonId { get; set; }
    public string ProductId { get; set; }
}