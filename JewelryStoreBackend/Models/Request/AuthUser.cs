using System.ComponentModel.DataAnnotations;

namespace JewelryStoreBackend.Models.Request;

public class AuthUser
{
    /// <summary>
    /// Почта
    /// </summary>
    [EmailAddress(ErrorMessage = "Некорректный адрес электронной почты")]
    [Required(AllowEmptyStrings = false, ErrorMessage = "Поле Email обязательно к заполнению")]
    [StringLength(70, MinimumLength = 10, ErrorMessage = "Email должен быть от 10 до 70 символов")]
    public required string Email { get; set; }
    
    /// <summary>
    /// Пароль
    /// </summary>
    [Required(AllowEmptyStrings = false, ErrorMessage = "Поле Password обязательно к заполнению")]
    [StringLength(100, MinimumLength = 10, ErrorMessage = "Пароль должен быть не менее 10 символов.")]
    public required string Password { get; set; }
}