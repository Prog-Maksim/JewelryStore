using JewelryStoreBackend.Enums;
using JewelryStoreBackend.Models.DB.Rating;
using JewelryStoreBackend.Repository.Interfaces;
using MongoDB.Driver;

namespace JewelryStoreBackend.Repository;

public class MessageRepository: IMessageRepository
{
    private readonly IMongoCollection<Message> _messagesCollection;

    public MessageRepository(IMongoClient mongoClient)
    {
        var database = mongoClient.GetDatabase("JewelryStoreDB");
        _messagesCollection = database.GetCollection<Message>("Messages");
    }

    public async Task<string> AddMessageAsync(Message message)
    {
        await _messagesCollection.InsertOneAsync(message);
        return message.Id;
    }
    
    public async Task UpdateMessageAsync(string produсtId, string messageId, string newText, int newRating)
    {
        var filter = Builders<Message>.Filter.And(
            Builders<Message>.Filter.Eq(m => m.ProdutId, produсtId),
            Builders<Message>.Filter.Eq(m => m.MessageId, messageId)
        );

        var update = Builders<Message>.Update
            .Set(m => m.Text, newText)
            .Set(m => m.Rating, newRating)
            .Set(m => m.UpdatedTimestamp, DateTime.Now)
            .Set(m => m.Status, MessageStatus.Updated);

        await _messagesCollection.UpdateOneAsync(filter, update);
    }
    
    public async Task DeleteMessageAsync(string productId, string messageId)
    {
        var filter = Builders<Message>.Filter.And(
            Builders<Message>.Filter.Eq(m => m.ProdutId, productId),
            Builders<Message>.Filter.Eq(m => m.MessageId, messageId)
        );

        var update = Builders<Message>.Update
            .Set(m => m.UpdatedTimestamp, DateTime.Now)
            .Set(m => m.Status, MessageStatus.Deleted);

        await _messagesCollection.UpdateOneAsync(filter, update);
    }
    
    public async Task<Message?> GetMessageByIdAsync(string productId, string messageId)
    {
        var filter = Builders<Message>.Filter.And(
            Builders<Message>.Filter.Eq(m => m.ProdutId, productId),
            Builders<Message>.Filter.Eq(m => m.MessageId, messageId),
            Builders<Message>.Filter.Ne(m => m.Status, MessageStatus.Deleted)
        );

        return await _messagesCollection.Find(filter).FirstOrDefaultAsync();
    }
    
    public async Task<IEnumerable<Message>> GetMessagesByProductIdAsync(string productId)
    {
        var filter = Builders<Message>.Filter.And(
            Builders<Message>.Filter.Eq(m => m.ProdutId, productId),
            Builders<Message>.Filter.Ne(m => m.Status, MessageStatus.Deleted)
        );
        
        return await _messagesCollection.Find(filter).ToListAsync();
    }
    
    public async Task<long> GetMessageCountByProductIdAsync(string productId)
    {
        var filter = Builders<Message>.Filter.And(
            Builders<Message>.Filter.Eq(m => m.ProdutId, productId),
            Builders<Message>.Filter.Ne(m => m.Status, MessageStatus.Deleted)
        );

        return await _messagesCollection.CountDocumentsAsync(filter);
    }
    
    public async Task<List<int>> GetAllRatingsAsync(string productId)
    {
        var filter = Builders<Message>.Filter.And(
            Builders<Message>.Filter.Eq(m => m.ProdutId, productId),
            Builders<Message>.Filter.Ne(m => m.Status, MessageStatus.Deleted)
        );
        var projection = Builders<Message>.Projection.Expression(m => m.Rating);

        return await _messagesCollection
            .Find(filter)
            .Project(projection)
            .ToListAsync();
    }
}