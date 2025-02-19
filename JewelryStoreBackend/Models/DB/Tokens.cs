using System.ComponentModel.DataAnnotations;

namespace JewelryStoreBackend.Models.DB;

public class Tokens
{
    public int Id { get; set; }
    
    /// <summary>
    /// Идентификатор пользователя
    /// </summary>
    [MaxLength(100)]
    public required string PersonId { get; set; }
    
    /// <summary>
    /// Токен доступа
    /// </summary>
    [MaxLength(550)]
    public required string AccessToken { get; set; }
    
    /// <summary>
    /// Токен обновления
    /// </summary>
    [MaxLength(550)]
    public required string RefreshToken { get; set; }
}