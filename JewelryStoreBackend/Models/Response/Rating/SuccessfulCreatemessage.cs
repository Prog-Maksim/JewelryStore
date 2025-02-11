namespace JewelryStoreBackend.Models.Response.Rating;

public class SuccessfulCreatemessage: BaseResponse
{
    /// <summary>
    /// Id созданного комментария
    /// </summary>
    public string CommentId { get; set; }  
}