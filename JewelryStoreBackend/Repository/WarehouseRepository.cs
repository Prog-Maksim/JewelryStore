using JewelryStoreBackend.Models.DB.Product;
using MongoDB.Driver;

namespace JewelryStoreBackend.Repository;

public class WarehouseRepository
{
    private readonly IMongoCollection<Warehouse> _warehousesCollection;

    public WarehouseRepository(IMongoClient mongoClient)
    {
        var database = mongoClient.GetDatabase("JewelryStoreDB");
        _warehousesCollection = database.GetCollection<Warehouse>("Warehouses");
    }
    
    public async Task AddMessageAsync(Warehouse product)
    {
        await _warehousesCollection.InsertOneAsync(product);
    }
}