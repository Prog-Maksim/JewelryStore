namespace JewelryStoreBackend.Models.Response.Rating;

public class ProductRating: BaseResponse
{
    /// <summary>
    /// Средний рейтинг товара
    /// </summary>
    public double Rating { get; set; }
    
    /// <summary>
    /// Кол-во комментариев у товара
    /// </summary>
    public int CustomersCount { get; set; }
}