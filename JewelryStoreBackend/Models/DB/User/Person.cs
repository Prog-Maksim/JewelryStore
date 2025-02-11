using JewelryStoreBackend.Enums;
using JewelryStoreBackend.Models.DB.Rating;
using JewelryStoreBackend.Security;

namespace JewelryStoreBackend.Models.DB.User;

public class Person
{
    /// <summary>
    /// Идентификатор пользователя
    /// </summary>
    public int Id { get; set; }
    
    /// <summary>
    /// Внешний идентификатор пользователя
    /// </summary>
    public string PersonId { get; set; }
    
    /// <summary>
    /// Имя
    /// </summary>
    public string Name { get; set; }
    
    /// <summary>
    /// Фамилия
    /// </summary>
    public string Surname { get; set; }
    
    /// <summary>
    /// Отчество, если есть
    /// </summary>
    public string? Patronymic { get; set; }
    
    /// <summary>
    /// Почта
    /// </summary>
    public string Email { get; set; }
    
    /// <summary>
    /// Пароль
    /// </summary>
    public string Password { get; set; }
    
    /// <summary>
    /// Версия пароля
    /// </summary>
    public int PasswordVersion { get; set; }
    
    /// <summary>
    /// Адрес
    /// </summary>
    public string? Adress { get; set; }
    
    /// <summary>
    /// Дата регистрации пользователя
    /// </summary>
    public DateTime DateRegistration { get; set; }
    
    /// <summary>
    /// Адрес регистрации пользователя
    /// </summary>
    public string AdressRegistration { get; set; }
    
    /// <summary>
    /// Ip адрес регистрации
    /// </summary>
    public string IpAdressRegistration { get; set; }
    
    public bool State { get; set; }
    
    public Roles Role { get; set; }
    
    public ICollection<UsersLike> UsersLike { get; set; }
}