using System.ComponentModel.DataAnnotations;
using JewelryStoreBackend.Enums;
using JewelryStoreBackend.Models.DB.Order;
using JewelryStoreBackend.Models.DB.Rating;

namespace JewelryStoreBackend.Models.DB.User;

public class Users
{
    /// <summary>
    /// Идентификатор пользователя
    /// </summary>
    public int Id { get; set; }
    
    /// <summary>
    /// Внешний идентификатор пользователя
    /// </summary>
    [MaxLength(100)]
    public required string PersonId { get; set; }
    
    /// <summary>
    /// Имя
    /// </summary>
    [MaxLength(50)]
    public required string Name { get; set; }
    
    /// <summary>
    /// Фамилия
    /// </summary>
    [MaxLength(50)]
    public required string Surname { get; set; }
    
    /// <summary>
    /// Отчество, если есть
    /// </summary>
    [MaxLength(50)]
    public string? Patronymic { get; set; }
    
    /// <summary>
    /// Почта
    /// </summary>
    [MaxLength(100)]
    public required string Email { get; set; }
    
    /// <summary>
    /// Номер телефона
    /// </summary>
    [MaxLength(12)]
    public string? PhoneNumber { get; set; }
    
    /// <summary>
    /// Пароль
    /// </summary>
    [MaxLength(250)]
    public string Password { get; set; }
    
    /// <summary>
    /// Версия пароля
    /// </summary>
    public int PasswordVersion { get; set; }
    
    /// <summary>
    /// Адрес
    /// </summary>
    [MaxLength(200)]
    public string? Adress { get; set; }
    
    /// <summary>
    /// Дата регистрации пользователя
    /// </summary>
    public DateTime DateRegistration { get; set; }
    
    /// <summary>
    /// Адрес регистрации пользователя
    /// </summary>
    [MaxLength(100)]
    public required string AddressRegistration { get; set; }
    
    /// <summary>
    /// Ip адрес регистрации
    /// </summary>
    [MaxLength(50)]
    public required string IpAdressRegistration { get; set; }
    
    /// <summary>
    /// Состояние пользователя
    /// </summary>
    public bool State { get; set; }
    
    /// <summary>
    /// Роль пользователя
    /// </summary>
    public Roles Role { get; set; }
    
    public ICollection<UsersLike> UsersLike { get; set; }
    public ICollection<Orders> Orders { get; set; }
}