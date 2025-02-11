namespace JewelryStoreBackend.Models.DB;

public class Tokens
{
    public int Id { get; set; }
    public string PersonId { get; set; }
    public string AccessToken { get; set; }
    public string RefreshToken { get; set; }
}