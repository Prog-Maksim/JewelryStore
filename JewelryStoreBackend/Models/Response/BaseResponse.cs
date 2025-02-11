using System.ComponentModel;
using System.Text.Json.Serialization;

namespace JewelryStoreBackend.Models.Response;

/// <summary>
/// Класс с данными об ошибке
/// </summary>
public class BaseResponse
{
    /// <summary>
    /// Описание ответа
    /// </summary>
    public string Message { get; set; }
    
    /// <summary>
    /// Статус выполнения
    /// </summary>
    [DefaultValue(false)]
    public bool Success { get; set; }
    
    /// <summary>
    /// Код статуса ошибки
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public int ErrorCode { get; set; }
    
    /// <summary>
    /// Название ошибки
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string Error { get; set; }
}