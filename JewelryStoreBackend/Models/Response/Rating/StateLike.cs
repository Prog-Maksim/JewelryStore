namespace JewelryStoreBackend.Models.Response.Rating;

public class StateLike: BaseResponse
{
    /// <summary>
    /// Лайкнут или нет
    /// </summary>
    public bool IsLiked { get; set; }
}