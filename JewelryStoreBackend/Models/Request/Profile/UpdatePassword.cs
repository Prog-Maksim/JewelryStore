using System.ComponentModel.DataAnnotations;

namespace JewelryStoreBackend.Models.Request.Profile;

public class UpdatePassword
{
    /// <summary>
    /// Старый пароль
    /// </summary>
    [Required(AllowEmptyStrings = false, ErrorMessage = "Поле NewPassword обязательно к заполнению")]
    [StringLength(100, MinimumLength = 10, ErrorMessage = "NewPassword должен быть не менее 10 символов.")]
    public string OldPassword { get; set; }
    
    /// <summary>
    /// Новый пароль
    /// </summary>
    [Required(AllowEmptyStrings = false, ErrorMessage = "Поле NewPassword обязательно к заполнению")]
    [StringLength(100, MinimumLength = 10, ErrorMessage = "NewPassword должен быть не менее 10 символов.")]
    public string NewPassword { get; set; }
}