using System.ComponentModel.DataAnnotations;

namespace JewelryStoreBackend.Models.Request.Profile;

public class AddPhoneNumberRequest
{
    /// <summary>
    /// Номер телефона
    /// </summary>
    [Required(AllowEmptyStrings = false, ErrorMessage = "Поле PhoneNumber обязательно к заполнению")]
    [StringLength(12, MinimumLength = 11, ErrorMessage = "PhoneNumber должен быть не менее 11 символов и не более 12 символов.")]
    [RegularExpression(@"^(\+7|8)\d{10}$", ErrorMessage = "Номер телефона должен начинаться с +7 или 8 и содержать 10 цифр после этого.")]
    public required string PhoneNumber { get; set; }
}