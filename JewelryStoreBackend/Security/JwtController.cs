using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using JewelryStoreBackend.Enums;
using JewelryStoreBackend.Models.DB.User;
using JewelryStoreBackend.Models.Other;
using Microsoft.IdentityModel.Tokens;
using StackExchange.Redis;

namespace JewelryStoreBackend.Security;

public class JwtController
{
    public const int AccessTokenLifetimeDay = 1;
    public const int RefreshTokenLifetimeDay = 30;
    
    public static string GenerateJwtAccessToken(string userId, Roles role)
    {
        string jti = Guid.NewGuid().ToString();
        
        var claims = new List<Claim>
        {
            new (ClaimTypes.Name, userId),
            new (ClaimTypes.Role, role.ToString()),
            new ("token_type", TokenType.access.ToString()),
            new (JwtRegisteredClaimNames.Jti, jti),
        };

        var jwt = new JwtSecurityToken(
            issuer: AuthOptions.ISSUER,
            audience: AuthOptions.AUDIENCE,
            claims: claims,
            expires: DateTime.UtcNow.AddDays(AccessTokenLifetimeDay),
            signingCredentials: new SigningCredentials(AuthOptions.GetSymmetricSecurityKey(), SecurityAlgorithms.HmacSha256)
        );

        return new JwtSecurityTokenHandler().WriteToken(jwt);
    }
    
    public static string GenerateJwtRefreshToken(string userId, int passwordVersion, Roles role)
    {
        string jti = Guid.NewGuid().ToString();
        
        var claims = new List<Claim>
        {
            new (ClaimTypes.Name, userId),
            new (ClaimTypes.Role, role.ToString()),
            new ("token_type", TokenType.refresh.ToString()),
            new ("version", passwordVersion.ToString()),
            new (JwtRegisteredClaimNames.Jti, jti)
        };
        
        var jwt = new JwtSecurityToken(
            issuer: AuthOptions.ISSUER,
            audience: AuthOptions.AUDIENCE,
            claims: claims,
            expires: DateTime.UtcNow.AddDays(RefreshTokenLifetimeDay),
            signingCredentials: new SigningCredentials(AuthOptions.GetSymmetricSecurityKey(), SecurityAlgorithms.HmacSha256)
        );

        return new JwtSecurityTokenHandler().WriteToken(jwt);
    }
    
    public static JwtTokenData GetJwtTokenData(string token)
    {
        var handler = new JwtSecurityTokenHandler();
        
        if (!handler.CanReadToken(token))
            throw new ArgumentException("Неверный jwt токен");
        
        var jwtToken = handler.ReadJwtToken(token);
        
        var userId = jwtToken.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Name)?.Value;
        var tokenType = jwtToken.Claims.FirstOrDefault(c => c.Type == "token_type")?.Value;
        var tokenTypeEnum = Enum.Parse<TokenType>(tokenType);
        var roles = Enum.Parse<Roles>(jwtToken.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role)?.Value);
        var versionClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == "version")?.Value;
        var jti = jwtToken.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Jti)?.Value;
        
        int version = 0;
        if (int.TryParse(versionClaim, out var parsedVersion))
            version = parsedVersion;
        
        return new JwtTokenData
        {
            UserId = userId,
            TokenType = tokenTypeEnum,
            Version = version,
            Jti = jti,
            Token = token,
            Role = roles
        };
    }
    
    public static bool ValidateRefreshJwtToken(JwtTokenData token, Person user)
    {
        if (token.TokenType != TokenType.refresh)
            return false;

        if (token.Version != user.PasswordVersion)
            return false;
        
        return true;
    }
    
    public static async Task<bool> ValidateAccessJwtToken(IDatabase database, JwtTokenData token)
    {
        if (token.TokenType != TokenType.access)
            return true;
        
        return await IsTokenBannedAsync(database, token.UserId, token.Token);
    }
    
    public static async Task AddTokensToBan(IDatabase database, string userId, List<string> tokens, int expiresDay = AccessTokenLifetimeDay)
    {
        var tag = $"ban:{userId}";

        if (tokens.Any())
        {
            foreach (var item in tokens)
            {
                await database.SetAddAsync(tag, item);
            }
            
            await database.KeyExpireAsync(tag, TimeSpan.FromDays(expiresDay));
        }
    }

    public static async Task<bool> IsTokenBannedAsync(IDatabase database, string userId, string token)
    {
        var tag = $"ban:{userId}";
        bool isBanned = await database.SetContainsAsync(tag, token);

        return isBanned;
    }
}