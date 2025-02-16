using JewelryStoreBackend.Enums;

namespace JewelryStoreBackend.Models.DB;

public class Coupon
{
    public int Id { get; set; }
    
    /// <summary>
    /// Идентификатор купона
    /// </summary>
    public string CouponId { get; set; }
    
    /// <summary>
    /// Код купона
    /// </summary>
    public string CouponCode { get; set; }
    
    /// <summary>
    /// Название купона
    /// </summary>
    public string Title { get; set; }
    
    /// <summary>
    /// Описание купона
    /// </summary>
    public string Description { get; set; }
    
    /// <summary>
    /// Скидка купона
    /// </summary>
    public int Percent { get; set; }
    
    /// <summary>
    /// Срок годность купона
    /// </summary>
    public DateTime DateExpired { get; set; }
    
    /// <summary>
    /// Дата добавление купона
    /// </summary>
    public DateTime DateCreated { get; set; }
    
    /// <summary>
    /// Распространение купона
    /// </summary>
    public CouponAction Action { get; set; }
    
    /// <summary>
    /// Категория товаров
    /// </summary>
    public string? CategoryType { get; set; }
    
    /// <summary>
    /// Языковой код купона
    /// </summary>
    public string LanguageCode { get; set; }
}