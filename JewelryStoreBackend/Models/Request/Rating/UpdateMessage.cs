using System.ComponentModel.DataAnnotations;

namespace JewelryStoreBackend.Models.Request.Rating;

public class UpdateMessage
{
    /// <summary>
    /// Новый текст сообщения
    /// </summary>
    [Required(AllowEmptyStrings = false, ErrorMessage = "Поле Text обязательно к заполнению")]
    [StringLength(1000, MinimumLength = 50, ErrorMessage = "Text должен быть от 50 до 1000 символов")]
    public string newText { get; set; }
    
    /// <summary>
    /// Новый рейтинг
    /// </summary>
    [Required(ErrorMessage = "Поле Rating обязательно к заполнению")]
    [Range(1, 5, ErrorMessage = "Rating должен быть от 1 до 5")]
    public int newRating { get; set; }
    
    /// <summary>
    /// Идентификатор товара
    /// </summary>
    [Required(AllowEmptyStrings = false, ErrorMessage = "Поле ProductId обязательно к заполнению")]
    [StringLength(100, MinimumLength = 5, ErrorMessage = "ProductId должен быть от 5 до 100 символов")]
    public string SKU { get; set; }
    
    /// <summary>
    /// Идентификатор изменяемого сообщения
    /// </summary>
    [Required(AllowEmptyStrings = false, ErrorMessage = "Поле MessageId обязательно к заполнению")]
    [StringLength(100, MinimumLength = 5, ErrorMessage = "MessageId должен быть от 5 до 100 символов")]
    public string messageId { get; set; }
}