using System.Text.Json.Serialization;

namespace JewelryStoreBackend.Models.Response;

public class RegistrationRequests: BaseResponse
{
    /// <summary>
    /// Время жизни токена в днях
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public int token_expires { get; set; }
    
    /// <summary>
    /// access токен для доступа к сайту
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string access_token { get; set; }
    
    /// <summary>
    /// Refresh токен для обновления токена
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string refresh_token { get; set; }
}