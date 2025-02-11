using JewelryStoreBackend.Models.Other;
using JewelryStoreBackend.Security;
using StackExchange.Redis;

namespace JewelryStoreBackend.Services;

public class AuthorizationService
{
    private readonly IConnectionMultiplexer _redis;

    public AuthorizationService(IConnectionMultiplexer redis)
    {
        _redis = redis;
    }

    public IDatabase GetRedisDatabase() => _redis.GetDatabase();

    public async Task<bool> ValidateAccessJwtTokenAsync(IDatabase database, JwtTokenData dataToken)
    {
        return await JwtController.ValidateAccessJwtToken(database, dataToken);
    }
}