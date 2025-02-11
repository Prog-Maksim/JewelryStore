using System.ComponentModel.DataAnnotations;

namespace JewelryStoreBackend.Models.Request;

public class RegistrationUser
{
    /// <summary>
    /// Имя
    /// </summary>
    [Required(AllowEmptyStrings = false, ErrorMessage = "Поле Name обязательно к заполнению")]
    [StringLength(100, MinimumLength = 5, ErrorMessage = "Name должен быть от 5 до 100 символов")]
    public string Name { get; set; }
    
    /// <summary>
    /// Фамилия
    /// </summary>
    [Required(AllowEmptyStrings = false, ErrorMessage = "Поле Surname обязательно к заполнению")]
    [StringLength(100, MinimumLength = 5, ErrorMessage = "Surname должен быть от 5 до 100 символов")]
    public string Surname { get; set; }
    
    /// <summary>
    /// Отчество, если есть
    /// </summary>
    [StringLength(100, MinimumLength = 5, ErrorMessage = "Patronymic должен быть от 5 до 100 символов")]
    public string? Patronymic { get; set; }
    
    /// <summary>
    /// Почта
    /// </summary>
    [EmailAddress(ErrorMessage = "Некорректный адрес электронной почты")]
    [Required(AllowEmptyStrings = false, ErrorMessage = "Поле Email обязательно к заполнению")]
    [StringLength(70, MinimumLength = 10, ErrorMessage = "Email должен быть от 10 до 70 символов")]
    public string Email { get; set; }
    
    /// <summary>
    /// Пароль
    /// </summary>
    [Required(AllowEmptyStrings = false, ErrorMessage = "Поле Password обязательно к заполнению")]
    [StringLength(100, MinimumLength = 10, ErrorMessage = "Пароль должен быть не менее 10 символов.")]
    public string Password { get; set; }
}