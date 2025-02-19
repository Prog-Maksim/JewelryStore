using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace JewelryStoreBackend.Models.DB.User;

public class Address
{
    [JsonIgnore(Condition = JsonIgnoreCondition.Always)]
    public int Id { get; set; }
    
    /// <summary>
    /// Идентификатор адреса
    /// </summary>
    [MaxLength(150)]
    public required string AddressId { get; set; }
    
    /// <summary>
    /// Идентификатор пользователя
    /// </summary>
    [MaxLength(100)]
    public required string PersonId { get; set; }
    
    /// <summary>
    /// Страна
    /// </summary>
    [MaxLength(150)]
    public required string Country { get; set; }
    
    /// <summary>
    /// Город
    /// </summary>
    [MaxLength(200)]
    public required string City { get; set; }
    
    /// <summary>
    /// Адрес
    /// </summary>
    [MaxLength(250)]
    public required string AddressLine1 { get; set; }
    
    /// <summary>
    /// Дополнительный адрес
    /// </summary>
    [MaxLength(100)]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? AddressLine2 { get; set; }
    
    /// <summary>
    /// Почтовый индекс
    /// </summary>
    [MaxLength(20)]
    public string? PostalCode { get; set; }
    
    /// <summary>
    /// Дата добавления
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.Always)]
    public DateTime CreateAt { get; set; }
    
    /// <summary>
    /// Дата обновления
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.Always)]
    public DateTime? UpdateAt { get; set; }
    
    /// <summary>
    /// Широта
    /// </summary>
    [MaxLength(100)]
    [JsonIgnore(Condition = JsonIgnoreCondition.Always)]
    public string? Lat { get; set; }
    
    /// <summary>
    /// Долгота
    /// </summary>
    [MaxLength(100)]
    [JsonIgnore(Condition = JsonIgnoreCondition.Always)]
    public string? Lon { get; set; }
}