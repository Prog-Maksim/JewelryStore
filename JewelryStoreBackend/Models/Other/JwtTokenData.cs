using JewelryStoreBackend.Enums;

namespace JewelryStoreBackend.Models.Other;

public class JwtTokenData
{
    public string UserId { get; set; }
    public TokenType TokenType { get; set; }
    public Roles Role { get; set; }
    public int Version { get; set; }
    public string Jti { get; set; }
        
    public string Token { get; set; }
}