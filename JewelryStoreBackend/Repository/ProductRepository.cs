using JewelryStoreBackend.Enums;
using JewelryStoreBackend.Models.DB;
using JewelryStoreBackend.Models.DB.Product;
using JewelryStoreBackend.Models.DB.Rating;
using JewelryStoreBackend.Repository.Interfaces;
using Microsoft.EntityFrameworkCore;
using MongoDB.Bson;
using MongoDB.Driver;
using Newtonsoft.Json;
using StackExchange.Redis;

namespace JewelryStoreBackend.Repository;

public class ProductRepository: IProductRepository
{
    private readonly IMongoCollection<ProductDb> _productsCollection;
    private readonly ApplicationContext _context;
    private readonly IDatabase _database;

    public ProductRepository(IMongoClient mongoClient, IConnectionMultiplexer redis, ApplicationContext context)
    {
        var database = mongoClient.GetDatabase("JewelryStoreDB");
        _productsCollection = database.GetCollection<ProductDb>("Products");
        
        _database = redis.GetDatabase();
        _context = context;
    }
    
    public async Task AddProductAsync(ProductDb productDb)
    {
        await _productsCollection.InsertOneAsync(productDb);
    }
    
    public async Task<List<ProductDb>> GetAllProductsAsync(string languageCode)
    {
        var filter = Builders<ProductDb>.Filter.Eq("language", languageCode);
        return await _productsCollection.Find(filter).ToListAsync();
    }

    public async Task UpdateProductAsync(string sku, ProductDb product)
    {
        var filter = Builders<ProductDb>.Filter.ElemMatch(p => p.Specifications, spec => spec.Sku == sku);
        
        var update = Builders<ProductDb>.Update
            .Set(p => p.Title, product.Title)
            .Set(p => p.OnSale, product.OnSale)
            .Set(p => p.Categories, product.Categories)
            .Set(p => p.ProductType, product.ProductType)
            .Set(p => p.ProductSubType, product.ProductSubType)
            .Set(p => p.Description, product.Description)
            .Set(p => p.Likes, product.Likes)
            .Set(p => p.Price, product.Price)
            .Set(p => p.Images, product.Images)
            .Set(p => p.BaseAdditionalInformation, product.BaseAdditionalInformation)
            .Set(p => p.Specifications, product.Specifications)
            .Set(p => p.CreateTimeStamp, product.CreateTimeStamp);
        
        var updateResult = await _productsCollection.UpdateManyAsync(filter, update);
    
        if (updateResult.ModifiedCount == 0)
            throw new Exception($"Не найдено товаров с артикулом: {sku} для обновления");
    }

    public async Task<List<ProductsSlider>?> GetProductInSliderAsync(string languageCode)
    {
        string cacheKey = $"slider-products:{languageCode}";
        
        var cachedData = await _database.StringGetAsync(cacheKey);
        List<ProductsSlider>? sliderItem;
        
        if (!cachedData.IsNullOrEmpty && cachedData.HasValue)
            sliderItem = JsonConvert.DeserializeObject<List<ProductsSlider>>(cachedData.ToString());
        else
        {
            sliderItem = _context.ProductsSlider.ToList();
            var jsonData = JsonConvert.SerializeObject(sliderItem);
            await _database.StringSetAsync(cacheKey, jsonData, TimeSpan.FromMinutes(10));
        }

        return sliderItem;
    }
    
    public async Task<List<ProductDb>> GetProductsInSearchAsync(
        string? search,
        string? productType,
        double? minPrice,
        double? maxPrice,
        bool? isSale,
        bool? isStock,
        bool? isDiscount,
        Sorted? sortOrder,
        SortedParameter? sortField,
        string languageCode)
    {
        var filterBuilder = Builders<ProductDb>.Filter;
        var filters = new List<FilterDefinition<ProductDb>>();
        
        filters.Add(filterBuilder.Eq(p => p.Language, languageCode));
        
        if (!string.IsNullOrEmpty(search))
        {
            filters.Add(filterBuilder.Or(
                filterBuilder.Regex(p => p.Title, new BsonRegularExpression(search, "i"))
            ));
        }
    
        // Фильтр по типу продукта
        if (!string.IsNullOrEmpty(productType))
            filters.Add(filterBuilder.Eq(p => p.ProductType, productType));
    
        // Фильтр по цене
        if (minPrice.HasValue)
            filters.Add(filterBuilder.Gte(p => p.Price.Cost, minPrice.Value));
        if (maxPrice.HasValue)
            filters.Add(filterBuilder.Lte(p => p.Price.Cost, maxPrice.Value));
    
        // Фильтр по распродаже
        if (isSale.HasValue)
            filters.Add(filterBuilder.Eq(p => p.OnSale, isSale.Value));
    
        // Фильтр по наличию
        if (isStock.HasValue && isStock.Value)
            filters.Add(filterBuilder.ElemMatch(p => p.Specifications, s => s.InStock));
    
        // Фильтр по скидке
        if (isDiscount.HasValue && isDiscount.Value)
            filters.Add(filterBuilder.Eq(p => p.Price.Discount, true));
    
        var combinedFilter = filters.Count > 0 ? filterBuilder.And(filters) : filterBuilder.Empty;
    
        // Сортировка
        var sortBuilder = Builders<ProductDb>.Sort;
        SortDefinition<ProductDb>? sortDefinition = null;
    
        if (sortField.HasValue)
        {
            var isAscending = sortOrder == Sorted.asc;
    
            sortDefinition = sortField switch
            {
                SortedParameter.price => isAscending ? sortBuilder.Ascending(p => p.Price.Cost) : sortBuilder.Descending(p => p.Price.Cost),
                SortedParameter.date => isAscending ? sortBuilder.Ascending(p => p.CreateTimeStamp) : sortBuilder.Descending(p => p.CreateTimeStamp),
                _ => null
            };
        }
    
        // Применение фильтров и сортировки
        var query = _productsCollection.Find(combinedFilter);
        if (sortDefinition != null)
        {
            query = query.Sort(sortDefinition);
        }
    
        var products = await query.ToListAsync();
        return products;
    }
    
    
    public async Task<List<string>> GetUniqueProductTypesAsync(string languageCode)
    {
        var filter = Builders<ProductDb>.Filter.Eq("language", languageCode);
    
        var productTypes = await _productsCollection
            .Distinct<string>("productType", filter)
            .ToListAsync();
    
        return productTypes;
    }
    
    
    public async Task<(ProductDb minProduct, ProductDb maxProduct)> GetMinMaxPricesAsync(string languageCode)
    {
        var filter = Builders<ProductDb>.Filter.Eq("language", languageCode);
        
        // Получаем минимальную цену
        var minProduct = await _productsCollection
            .Find(filter)
            .Sort(Builders<ProductDb>.Sort.Ascending(p => p.Price.Cost))
            .Limit(1)
            .FirstOrDefaultAsync();
        
        var maxProduct = await _productsCollection
            .Find(filter)
            .Sort(Builders<ProductDb>.Sort.Descending(p => p.Price.Cost))
            .Limit(1)
            .FirstOrDefaultAsync();

        return (minProduct, maxProduct);
    }
    
    public async Task<List<ProductDb>> GetProductsByCategoryAsync(string category, string languageCode)
    {
        var filter = Builders<ProductDb>.Filter.Eq(p => p.ProductType, category) &
            Builders<ProductDb>.Filter.Where(p => languageCode == p.Language);
    
        var products = await _productsCollection
            .Find(filter)
            .ToListAsync();
    
        return products;
    }
    
    
    public async Task<List<ProductDb>> GetNewProductsAsync(string languageCode)
    {
        var twoWeeksAgo = DateTime.UtcNow.AddDays(-14);
    
        var filter = Builders<ProductDb>.Filter.Gt(p => p.CreateTimeStamp, twoWeeksAgo) &
                     Builders<ProductDb>.Filter.Eq(p => p.Language, languageCode);
    
        var newProducts = await _productsCollection
            .Find(filter)
            .ToListAsync();
    
        return newProducts;
    }
    
    
    public async Task<ProductDb?> GetProductByIdAsync(string languageCode, string sku)
    {
        var filter = Builders<ProductDb>.Filter.Eq(p => p.Language, languageCode) &
                     Builders<ProductDb>.Filter.Eq("specifications.sku", sku);
    
        var product = await _productsCollection
            .Find(filter)
            .FirstOrDefaultAsync();
    
        if (product == null)
            return null;
        
        var specification = product.Specifications?
            .FirstOrDefault(spec => spec.Sku == sku);
    
        if (specification != null)
            product.Specifications = [specification];
        else
            product.Specifications = [];
    
        return product;
    }
    
    public async Task<ProductDb?> GetProductByIdAllAsync(string languageCode, string sku)
    {
        var filter = Builders<ProductDb>.Filter.Eq(p => p.Language, languageCode) &
                     Builders<ProductDb>.Filter.Eq("specifications.sku", sku);
    
        var product = await _productsCollection
            .Find(filter)
            .FirstOrDefaultAsync();
    
        if (product == null)
            return null;
    
        return product;
    }
    
    public async Task<List<ProductDb>> GetRecommendedProductsAsync(string languageCode, string sku)
    {
        var filter = Builders<ProductDb>.Filter.Eq(p => p.Language, languageCode) &
                     Builders<ProductDb>.Filter.Eq("specifications.sku", sku);
    
        var product = await _productsCollection
            .Find(filter)
            .FirstOrDefaultAsync();
        
        if (product == null)
            return new List<ProductDb>();
        
        
        var productType = product.ProductType;
        
        var filterRecommended = Builders<ProductDb>.Filter.Eq(p => p.ProductType, productType) &
                                Builders<ProductDb>.Filter.Eq(p => p.Language, languageCode) &
                                Builders<ProductDb>.Filter.Ne(p => p.Id, product.Id);
        
        var recommendedProducts = await _productsCollection
            .Find(filterRecommended)
            .Limit(3)
            .ToListAsync();
        
        return recommendedProducts;
    }
    
    
    public async Task<List<ProductDb>> GetPopularProductsAsync(string languageCode)
    {
        var filter = Builders<ProductDb>.Filter.Eq(p => p.Language, languageCode);
        
        var sort = Builders<ProductDb>.Sort.Descending(p => p.Likes);
        var popularProducts = await _productsCollection.Find(filter)
            .Sort(sort)
            .Limit(9)
            .ToListAsync();
    
        return popularProducts;
    }


    public async Task<UsersLike?> GetLikesAsync(string userId, string sku)
    {
        var result = await _context.UsersLike.FirstOrDefaultAsync(l => l.ProductId == sku && l.PersonId == userId);
        return result;
    }

    public async Task AddLikeAsync(UsersLike like)
    {
        await _context.UsersLike.AddAsync(like);
    }

    public void RemoveLikeAsync(UsersLike like)
    {
        _context.Remove(like);
    }

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }
}