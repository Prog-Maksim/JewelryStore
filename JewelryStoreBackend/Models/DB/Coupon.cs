using System.ComponentModel.DataAnnotations;
using JewelryStoreBackend.Enums;

namespace JewelryStoreBackend.Models.DB;

public class Coupon
{
    public int Id { get; set; }
    
    /// <summary>
    /// Идентификатор купона
    /// </summary>
    [MaxLength(150)]
    public required string CouponId { get; set; }
    
    /// <summary>
    /// Код купона
    /// </summary>
    [MaxLength(10)]
    public required string CouponCode { get; set; }
    
    /// <summary>
    /// Название купона
    /// </summary>
    [MaxLength(100)]
    public required string Title { get; set; }
    
    /// <summary>
    /// Описание купона
    /// </summary>
    [MaxLength(255)]
    public string? Description { get; set; }
    
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
    [MaxLength(100)]
    public string? CategoryType { get; set; }
    
    /// <summary>
    /// Языковой код купона
    /// </summary>
    [MaxLength(10)]
    public required string LanguageCode { get; set; }
}