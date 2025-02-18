using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace JewelryStoreBackend;

public class AuthOptions
{
    public static readonly string ISSUER = Environment.GetEnvironmentVariable("ISSUER");
    public static readonly string AUDIENCE = Environment.GetEnvironmentVariable("AUDIENCE");
    private static readonly string KEY = Environment.GetEnvironmentVariable("JWT_KEY");
    public static SymmetricSecurityKey GetSymmetricSecurityKey() => new (Encoding.UTF8.GetBytes(KEY));
}