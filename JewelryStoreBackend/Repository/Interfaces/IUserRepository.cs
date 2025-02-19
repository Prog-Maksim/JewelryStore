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
    Task<Users?> GetUserByIdAsync(string userId);
    
    /// <summary>
    /// Возвращает пользователя по почте 
    /// </summary>
    /// <param name="email">Почта пользователя</param>
    /// <returns></returns>
    Task<Users?> GetUserByEmailAsync(string email);
    
    /// <summary>
    /// Добавляет пользователя в БД
    /// </summary>
    /// <param name="user">Пользователь</param>
    /// <returns></returns>
    Task AddUserAsync(Users user);
    
    /// <summary>
    /// Добавляет токен в БД
    /// </summary>
    /// <param name="tokens">Токен</param>
    /// <returns></returns>
    Task AddTokensAsync(Tokens tokens);
    
    /// <summary>
    /// Возвращает токен 
    /// </summary>
    /// <param name="userId">Идентификатор пользователя</param>
    /// <returns></returns>
    Task<Tokens?> GetTokenByUserIdAsync(string userId);
    
    /// <summary>
    /// Возвращает все токены пользователя
    /// </summary>
    /// <param name="userId">Идентификатор пользователя</param>
    /// <returns></returns>
    Task<List<Tokens>> GetTokensByPersonIdAsync(string userId);
    
    /// <summary>
    /// Сохраняет изменентя в БД
    /// </summary>
    /// <returns></returns>
    Task SaveChangesAsync();

    /// <summary>
    /// Добавляет токены в бан
    /// </summary>
    /// <param name="userId">Идентификатор пользователя</param>
    /// <param name="tokens">Список токенов</param>
    /// <param name="expiresDay">Время бана</param>
    /// <returns></returns>
    Task AddTokensToBanAsync(string userId, List<string> tokens, int expiresDay = JwtController.AccessTokenLifetimeDay);
    
    /// <summary>
    /// Удаляет токен в БД
    /// </summary>
    /// <param name="tokens"></param>
    void DeleteTokensAsync(List<Tokens> tokens);

    /// <summary>
    /// Возвращает список адресов пользователя
    /// </summary>
    /// <param name="userId">Идентификатор пользователя</param>
    /// <returns></returns>
    Task<List<Address>> GetAddressesByUserIdAsync(string userId);
    
    /// <summary>
    /// Возвращает адрес пользователя
    /// </summary>
    /// <param name="userId">Идентификатор пользователя</param>
    /// <param name="addressId">Идентификатор адреса</param>
    /// <returns></returns>
    Task<Address?> GetAddressByIdAsync(string userId, string addressId);
    
    /// <summary>
    /// Удаляет адрес пользователя
    /// </summary>
    /// <param name="address">Адрес</param>
    void DeleteAddress(Address address);

    /// <summary>
    /// Добавляет адрес
    /// </summary>
    /// <param name="address">Адрес</param>
    /// <returns></returns>
    Task AddAddress(Address address);
    
    /// <summary>
    /// Возвращает Пользователя по номеру телефона
    /// </summary>
    /// <param name="phoneNumber">Номер телефона</param>
    /// <returns></returns>
    Task<Users?> GetUserByPhoneNumberAsync(string phoneNumber);
}