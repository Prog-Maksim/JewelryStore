using System.Text.Json;
using JewelryStoreBackend.Models.DB.Order;
using JewelryStoreBackend.Models.Response.Order;
using JewelryStoreBackend.Repository.Interfaces;
using Microsoft.EntityFrameworkCore;
using StackExchange.Redis;

namespace JewelryStoreBackend.Repository;

public class OrderRepository: IOrderRepository
{
    private readonly ApplicationContext _context;
    private readonly IDatabase _database;

    public OrderRepository(ApplicationContext context, IConnectionMultiplexer redis)
    {
        _context = context;
        _database = redis.GetDatabase();
    }
    
    public async Task<bool> IsOrderInProgressAsync(string userId)
    {
        string key = $"order:{userId}";
        return await _database.KeyExistsAsync(key);
    }
    
    public async Task StartOrderAsync(string userId, OrderData orderData, TimeSpan expiration)
    {
        string key = $"order:{userId}";
        string jsonData = JsonSerializer.Serialize(orderData);
        await _database.StringSetAsync(key, jsonData, expiration);
    }
    
    public async Task<bool> CancelOrderAsync(string userId)
    {
        string key = $"order:{userId}";
        return await _database.KeyDeleteAsync(key);
    }

    public async Task<OrderData?> GetOrderDataByIdAsync(string userId)
    {
        string key = $"order:{userId}";
        string? jsonData = await _database.StringGetAsync(key);

        if (jsonData == null)
            return null;
        
        OrderData? order = JsonSerializer.Deserialize<OrderData>(jsonData);

        return order;
    }

    public async Task<Orders?> GetOrderByIdAsync(string userId, string orderId)
    {
        var order = await _context.Orders.FirstOrDefaultAsync(p => p.OrderId == orderId && p.PersonId == userId);
        return order;
    }

    public async Task<List<Orders>> GetAllOrdersByUserIdAsync(string userId)
    {
        var orders = await _context.Orders.Where(o => o.PersonId == userId).ToListAsync();
        return orders;
    }

    public async Task<Orders?> GetDetailOrderByIdAsync(string userId, string orderId)
    {
        var order = await _context.Orders
            .Include(o => o.Products)
            .Include(o => o.Payment)
            .Include(o => o.Shipping)
            .Include(o => o.Users)
            .FirstOrDefaultAsync(o => o.PersonId == userId && o.OrderId == orderId);

        return order;
    }

    public async Task AddOrderAsync(Orders order)
    {
        await _context.Orders.AddAsync(order);
    }

    public async Task AddOrderProductsAsync(OrderProducts products)
    {
        await _context.OrderProducts.AddRangeAsync(products);
    }

    public async Task AddOrderPaymentsAsync(OrderPayments payments)
    {
        await _context.OrderPayments.AddAsync(payments);
    }

    public async Task AddOrderShippingsAsync(OrderShippings shippings)
    {
        await _context.OrderShippings.AddAsync(shippings);
    }
    
    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }
    
    
}