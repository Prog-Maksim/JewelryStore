namespace JewelryStoreBackend.Models.Response.Rating;

public class SuccessfulCreateMessage: BaseResponse
{
    /// <summary>
    /// Id созданного комментария
    /// </summary>
    public required string CommentId { get; set; }  
}