using JewelryStoreBackend.Enums;

namespace JewelryStoreBackend.Models.Other;

public class JwtTokenData
{
    /// <summary>
    /// Идентификатор пользователя
    /// </summary>
    public string UserId { get; set; }
    
    /// <summary>
    /// Тип токена
    /// </summary>
    public TokenType TokenType { get; set; }
    
    /// <summary>
    /// Роль пользователя
    /// </summary>
    public Roles Role { get; set; }
    
    /// <summary>
    /// Версия пароля
    /// </summary>
    public int Version { get; set; }
    

    public string Jti { get; set; }
    public string Token { get; set; }
}