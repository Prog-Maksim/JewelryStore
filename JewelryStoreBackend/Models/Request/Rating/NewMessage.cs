using System.ComponentModel.DataAnnotations;

namespace JewelryStoreBackend.Models.Request.Rating;

public class NewMessage
{
    /// <summary>
    /// Текст комментария
    /// </summary>
    [Required(AllowEmptyStrings = false, ErrorMessage = "Поле Text обязательно к заполнению")]
    [StringLength(1000, MinimumLength = 50, ErrorMessage = "Text должен быть от 50 до 1000 символов")]
    public string text { get; set; }
    
    /// <summary>
    /// Рейтинг
    /// </summary>
    [Required(ErrorMessage = "Поле Rating обязательно к заполнению")]
    [Range(1, 5, ErrorMessage = "Rating должен быть от 1 до 5")]
    public int rating { get; set; }
    
    /// <summary>
    /// Идентификатор товара
    /// </summary>
    [Required(AllowEmptyStrings = false, ErrorMessage = "Поле ProductId обязательно к заполнению")]
    [StringLength(100, MinimumLength = 5, ErrorMessage = "ProductId должен быть от 5 до 100 символов")]
    public string SKU { get; set; }
    
    /// <summary>
    /// Идентификатор сообщения на которое отвечаете
    /// </summary>
    [StringLength(100, MinimumLength = 5, ErrorMessage = "ReplyMessageId должен быть от 5 до 100 символов")]
    public string? replyMessageId { get; set; }
}