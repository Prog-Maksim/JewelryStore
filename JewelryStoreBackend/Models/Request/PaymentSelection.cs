using System.ComponentModel.DataAnnotations;

namespace JewelryStoreBackend.Models.Request;

public class PaymentSelection
{
    /// <summary>
    /// Метод оплаты
    /// </summary>
    public PaymentType PaymentType { get; set; }
    
    /// <summary>
    /// Данные карты (необязательно)
    /// </summary>
    public Card? Card { get; set; }
}

public enum PaymentType
{
    /// <summary>
    /// При получении
    /// </summary>
    Reception,
    
    /// <summary>
    /// Картой
    /// </summary>
    Card,
    
    /// <summary>
    /// Наличкой
    /// </summary>
    Cash
}

public class Card
{
    /// <summary>
    /// Номер карты
    /// </summary>
    [Required(ErrorMessage = "Номер карты обязателен для заполнения.")]
    [StringLength(16, MinimumLength = 16, ErrorMessage = "Номер карты должен состоять из 16 цифр.")]
    [RegularExpression(@"^\d{16}$", ErrorMessage = "Номер карты должен содержать только цифры.")]
    public required string CardNumber { get; set; }
    
    /// <summary>
    /// Срок годности
    /// </summary>
    [Required(ErrorMessage = "Срок годности обязателен для заполнения.")]
    [RegularExpression(@"^(0[1-9]|1[0-2])/\d{2}$", ErrorMessage = "Неверный формат срока годности. Формат должен быть MM/YY.")]
    public required string DateExpiration { get; set; }
    
    /// <summary>
    /// 3-значный код (CVV)
    /// </summary>
    [Required(ErrorMessage = "CVV код обязателен для заполнения.")]
    [StringLength(3, MinimumLength = 3, ErrorMessage = "CVV код должен состоять из 3 цифр.")]
    [RegularExpression(@"^\d{3}$", ErrorMessage = "CVV код должен содержать только цифры.")]
    public required string Cvv { get; set; }
}