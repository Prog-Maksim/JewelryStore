namespace JewelryStoreBackend.Models.Response;

public class PersonInform
{
    /// <summary>
    /// Имя пользователя
    /// </summary>
    public string Name { get; set; }
    
    /// <summary>
    /// Фамилия пользователя
    /// </summary>
    public string Surname { get; set; }
    
    /// <summary>
    /// Отчество пользователя
    /// </summary>
    public string? Patronymic { get; set; }
    
    /// <summary>
    /// Почта пользователя
    /// </summary>
    public string Email { get; set; }
    
    /// <summary>
    /// Адрес пользователя
    /// </summary>
    public string? Adress { get; set; }
    
    /// <summary>
    /// Номер телефона
    /// </summary>
    public string? PhoneNumber { get; set; }
}