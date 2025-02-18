using JewelryStoreBackend.Models.DB;

namespace JewelryStoreBackend.Repository.Interfaces;

public interface ICouponRepository
{
    /// <summary>
    /// Фозвращает купон
    /// </summary>
    /// <param name="couponCode">Идентификатор купона</param>
    /// <param name="languageCode">Языковой код</param>
    /// <returns></returns>
    Task<Coupon?> GetCoupon(string couponCode, string languageCode);
}