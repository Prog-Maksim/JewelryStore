using System.Text.Json.Serialization;

namespace JewelryStoreBackend.Models.DB.User;

public class Address
{
    [JsonIgnore(Condition = JsonIgnoreCondition.Always)]
    public int Id { get; set; }
    
    /// <summary>
    /// Идентификатор адреса
    /// </summary>
    public string AddressId { get; set; }
    
    /// <summary>
    /// Идентификатор пользователя
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.Always)]
    public string PersonId { get; set; }
    
    /// <summary>
    /// Страна
    /// </summary>
    public string Country { get; set; }
    
    /// <summary>
    /// Город
    /// </summary>
    public string City { get; set; }
    
    /// <summary>
    /// Адрес
    /// </summary>
    public string AddressLine1 { get; set; }
    
    /// <summary>
    /// Дополнительный адрес
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? AddressLine2 { get; set; }
    
    /// <summary>
    /// Почтовый индекс
    /// </summary>
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
    [JsonIgnore(Condition = JsonIgnoreCondition.Always)]
    public string lat { get; set; }
    
    /// <summary>
    /// Долгота
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.Always)]
    public string lon { get; set; }
}