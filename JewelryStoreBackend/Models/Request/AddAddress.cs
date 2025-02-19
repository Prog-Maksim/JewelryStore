using System.ComponentModel.DataAnnotations;

namespace JewelryStoreBackend.Models.Request;

public class AddAddress
{
    /// <summary>
    /// Страна пользователя
    /// </summary>
    [Required(ErrorMessage = "Поле 'Страна' обязательно для заполнения.")]
    [MaxLength(150, ErrorMessage = "Длина страны не может превышать 150 символов.")]
    public required string Country { get; set; }

    /// <summary>
    /// Город пользователя
    /// </summary>
    [Required(ErrorMessage = "Поле 'Город' обязательно для заполнения.")]
    [MaxLength(150, ErrorMessage = "Длина города не может превышать 150 символов.")]
    public required string City { get; set; }

    /// <summary>
    /// Адрес пользователя (Улица, дом)
    /// </summary>
    [Required(ErrorMessage = "Поле 'Адрес' обязательно для заполнения.")]
    [MaxLength(250, ErrorMessage = "Длина адреса не может превышать 250 символов.")]
    public required string AddressLine1 { get; set; }

    /// <summary>
    /// Адрес пользователя (квартира, корпус) (опционально)
    /// </summary>
    [MaxLength(100, ErrorMessage = "Длина дополнительного адреса не может превышать 100 символов.")]
    public string? AddressLine2 { get; set; }
}