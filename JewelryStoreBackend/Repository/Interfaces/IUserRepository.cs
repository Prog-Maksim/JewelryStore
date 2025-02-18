using JewelryStoreBackend.Models.DB;
using JewelryStoreBackend.Models.DB.User;
using JewelryStoreBackend.Security;

namespace JewelryStoreBackend.Repository.Interfaces;

public interface IUserRepository
{
    /// <summary>
    /// Возвращает объект пользователя
    /// </summary>
    /// <param name="userId">Идентификатор пользователя</param>
    /// <returns></returns>
    Task<Person?> GetUserByIdAsync(string userId);
    
    Task<Person?> GetUserByEmailAsync(string email);
    Task AddUserAsync(Person user);
    Task AddTokensAsync(Tokens tokens);
    Task<Tokens?> GetTokenByUserIdAsync(string userId);
    Task<List<Tokens>> GetTokensByPersonIdAsync(string personId);
    Task SaveChangesAsync();

    Task AddTokensToBanAsync(string userId, List<string> tokens, int expiresDay = JwtController.AccessTokenLifetimeDay);
    void DeleteTokensAsync(List<Tokens> tokens);

    Task<List<Address>> GetAddressesByUserIdAsync(string userId);
    
    Task<Address?> GetAddressByIdAsync(string userId, string addressId);
    void DeleteAddresses(Address addresses);

    Task AddAddress(Address address);
    
    Task<Person?> GetUserByPhoneNumberAsync(string phoneNumber);
}