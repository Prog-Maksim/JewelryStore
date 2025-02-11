using System.Text.Json.Serialization;
using JewelryStoreBackend.Enums;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace JewelryStoreBackend.Models.DB.Rating;

public class Message
{
    /// <summary>
    /// Уникальный идентификатор записи
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.Always)]
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = ObjectId.GenerateNewId().ToString();
    
    /// <summary>
    /// Идентификатор продукта
    /// </summary>
    [BsonElement("produtId")]
    public string ProdutId { get; set; }

    /// <summary>
    /// Идентификатор пользователя
    /// </summary>
    [BsonElement("personId")]
    public string PersonId { get; set; }

    /// <summary>
    /// Идентификатор сообщения
    /// </summary>
    [BsonElement("messageId")]
    public string MessageId { get; set; }
    
    /// <summary>
    /// Идентификатор сообщения на который отвечает
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [BsonElement("reply-messageId")]
    public string? ReplyMessageId { get; set; }

    /// <summary>
    /// Текст комментария
    /// </summary>
    [BsonElement("text")]
    public string Text { get; set; }
    
    /// <summary>
    /// Райтинг комментария
    /// </summary>
    [BsonElement("rating")]
    public int Rating { get; set; }

    /// <summary>
    /// Время создания комментария
    /// </summary>
    [BsonElement("timestamp")]
    public DateTime Timestamp { get; set; } = DateTime.Now;

    /// <summary>
    /// Время обновления комментария
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    [BsonElement("updatedTimestamp")]
    public DateTime? UpdatedTimestamp { get; set; }
    
    /// <summary>
    /// Статус комментария
    /// </summary>
    [BsonElement("status")]
    public MessageStatus Status { get; set; } = MessageStatus.Sent;
    
    /// <summary>
    /// Кем отправлен комментарий
    /// </summary>
    [BsonIgnore]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public PersonStatus? SendBy { get; set; }
}