using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace JewelryStoreBackend;

public class AuthOptions
{
    public static readonly string Issuer = Environment.GetEnvironmentVariable("ISSUER")
                                           ?? throw new InvalidOperationException("Environment variable 'ISSUER' is not set.");
    
    public static readonly string Audience = Environment.GetEnvironmentVariable("AUDIENCE")
                                             ?? throw new InvalidOperationException("Environment variable 'AUDIENCE' is not set.");
    
    private static readonly string Key = Environment.GetEnvironmentVariable("JWT_KEY")
                                         ?? throw new InvalidOperationException("Environment variable 'JWT_KEY' is not set.");
    
    public static SymmetricSecurityKey GetSymmetricSecurityKey() => new (Encoding.UTF8.GetBytes(Key));
}