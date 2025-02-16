using JewelryStoreBackend.Enums;

namespace JewelryStoreBackend.Models.DB;

public class Coupon
{
    public int Id { get; set; }
    public string CouponId { get; set; }
    public string CouponCode { get; set; }
    public string Title { get; set; }
    public string Description { get; set; }
    public int Percent { get; set; }
    public DateTime DateExpired { get; set; }
    public DateTime DateCreated { get; set; }
    public CouponAction Action { get; set; }
    public string? CategoryType { get; set; }
    public string LanguageCode { get; set; }
}