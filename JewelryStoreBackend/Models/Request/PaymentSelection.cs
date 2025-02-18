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
    public required string CardNumber { get; set; }
    
    /// <summary>
    /// Срок годности
    /// </summary>
    public required string DateExpiration { get; set; }
    
    /// <summary>
    /// 3 значный код
    /// </summary>
    public required string Cvv { get; set; }
}