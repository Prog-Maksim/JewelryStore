using JewelryStoreBackend.Models.DB;
using JewelryStoreBackend.Repository.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace JewelryStoreBackend.Repository;

public class BasketRepository: IBasketRepository
{
    private readonly ApplicationContext _context;

    public BasketRepository(ApplicationContext context)
    {
        _context = context;
    }

    public async Task<List<Basket>> GetUserBasketAsync(string userId)
    {
        var baskets = await _context.Basket.Where(b => b.PersonId == userId).ToListAsync();
        return baskets;
    }

    public async Task<Basket?> GetProductBasketAsync(string userId, string productId)
    {
        var productInBasket = await _context.Basket.FirstOrDefaultAsync(p => p.PersonId == userId && p.ProductId == productId);
        return productInBasket;
    }

    public async Task RemoveBasketItemsAsync(string userId)
    {
        var basketItems = await _context.Basket.Where(b => b.PersonId == userId).ToListAsync();
        _context.Basket.RemoveRange(basketItems);
    }

    public async Task AddProductToBasketAsync(Basket basket)
    {
        await _context.Basket.AddAsync(basket);
    }
    
    public void DeleteProductToBasketAsync(Basket basket)
    {
        _context.Basket.Remove(basket);
    }

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }
}