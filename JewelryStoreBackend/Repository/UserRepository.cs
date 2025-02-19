using JewelryStoreBackend.Models.DB;
using JewelryStoreBackend.Models.DB.User;
using JewelryStoreBackend.Repository.Interfaces;
using JewelryStoreBackend.Security;
using Microsoft.EntityFrameworkCore;
using StackExchange.Redis;

namespace JewelryStoreBackend.Repository;

public class UserRepository: IUserRepository
{
    private readonly ApplicationContext _context;
    private readonly IDatabase _database;

    public UserRepository(ApplicationContext context, IConnectionMultiplexer redis)
    {
        _context = context;
        _database = redis.GetDatabase();
    }

    public async Task<Users?> GetUserByIdAsync(string userId)
    {
        var user = await _context.Users.FirstOrDefaultAsync(p => p.PersonId == userId);
        return user;
    }
    
    public async Task<Users?> GetUserByEmailAsync(string email)
    {
        return await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
    }

    public async Task AddUserAsync(Users user)
    {
        await _context.Users.AddAsync(user);
    }

    public async Task AddTokensAsync(Tokens tokens)
    {
        await _context.Tokens.AddAsync(tokens);
    }

    public async Task<Tokens?> GetTokenByUserIdAsync(string userId)
    {
        return await _context.Tokens.FirstOrDefaultAsync(t => t.PersonId == userId);
    }

    public async Task<List<Tokens>> GetTokensByPersonIdAsync(string userId)
    {
        return await _context.Tokens.Where(t => t.PersonId == userId).ToListAsync();
    }

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }
    
    public async Task AddTokensToBanAsync(string userId, List<string> tokens, int expiresDay = JwtController.AccessTokenLifetimeDay)
    {
        var tag = $"ban:{userId}";

        if (tokens.Any())
        {
            foreach (var item in tokens)
                await _database.SetAddAsync(tag, item);
            await _database.KeyExpireAsync(tag, TimeSpan.FromDays(expiresDay));
        }
    }

    public void DeleteTokensAsync(List<Tokens> tokens)
    {
        _context.Tokens.RemoveRange(tokens);
    }

    public async Task<List<Address>> GetAddressesByUserIdAsync(string userId)
    {
        List<Address> adresses = await _context.Address.Where(a => a.PersonId == userId).ToListAsync();
        return adresses;
    }

    public async Task<Address?> GetAddressByIdAsync(string userId, string addressId)
    {
        var address = await _context.Address.FirstOrDefaultAsync(p => p.PersonId ==userId && p.AddressId == addressId);
        return address;
    }

    public void DeleteAddress(Address address)
    {
        _context.Remove(address);
    }

    public async Task AddAddress(Address address)
    {
        await _context.Address.AddAsync(address);
    }
    
    public async Task<Users?> GetUserByPhoneNumberAsync(string phoneNumber)
    {
        return await _context.Users.FirstOrDefaultAsync(u => u.PhoneNumber == phoneNumber);
    }
}