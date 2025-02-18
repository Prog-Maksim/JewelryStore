using JewelryStoreBackend.Models.DB;
using JewelryStoreBackend.Repository.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace JewelryStoreBackend.Repository;

public class CouponRepository: ICouponRepository
{
    private readonly ApplicationContext _context;

    public CouponRepository(ApplicationContext context)
    {
        _context = context;
    }

    public async Task<Coupon?> GetCoupon(string couponCode, string languageCode)
    {
        var coupon = await _context.Coupon.FirstOrDefaultAsync(c => c.CouponCode == couponCode && c.LanguageCode == languageCode);
        return coupon;
    }
}