namespace JewelryStoreBackend.Models.Response;

public class PersonInform
{
    /// <summary>
    /// Имя пользователя
    /// </summary>
    public required string Name { get; set; }
    
    /// <summary>
    /// Фамилия пользователя
    /// </summary>
    public required string Surname { get; set; }
    
    /// <summary>
    /// Отчество пользователя
    /// </summary>
    public string? Patronymic { get; set; }
    
    /// <summary>
    /// Почта пользователя
    /// </summary>
    public required string Email { get; set; }
    
    /// <summary>
    /// Адрес пользователя
    /// </summary>
    public string? Adress { get; set; }
    
    /// <summary>
    /// Номер телефона
    /// </summary>
    public string? PhoneNumber { get; set; }
}