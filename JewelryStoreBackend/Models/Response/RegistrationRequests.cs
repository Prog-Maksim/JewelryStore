using System.Text.Json.Serialization;

namespace JewelryStoreBackend.Models.Response;

public class RegistrationRequests: BaseResponse
{
    /// <summary>
    /// Время жизни токена в днях
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public int TokenExpires { get; set; }
    
    /// <summary>
    /// Access токен для доступа к сайту
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? AccessToken { get; set; }
    
    /// <summary>
    /// Refresh токен для обновления токена
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? RefreshToken { get; set; }
}