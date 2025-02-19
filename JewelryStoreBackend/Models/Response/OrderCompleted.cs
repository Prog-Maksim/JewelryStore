namespace JewelryStoreBackend.Models.Response;

public class OrderCompleted: BaseResponse
{
    /// <summary>
    /// Идентификатор заказа
    /// </summary>
    public required string OrderId { get; set; }
}