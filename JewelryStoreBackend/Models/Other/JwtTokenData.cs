using JewelryStoreBackend.Enums;

namespace JewelryStoreBackend.Models.Other;

public class JwtTokenData
{
    /// <summary>
    /// Идентификатор пользователя
    /// </summary>
    public required string UserId { get; set; }
    
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
    
    /// <summary>
    /// Уникальный идентификатор токена
    /// </summary>
    public required string Jti { get; set; }
    
    public required string Token { get; set; }
}