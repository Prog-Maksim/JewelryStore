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
    private readonly IMongoCollection<ProductDB> _productsCollection;
    private readonly ApplicationContext _context;
    private readonly IDatabase _database;

    public ProductRepository(IMongoClient mongoClient, IConnectionMultiplexer redis, ApplicationContext context)
    {
        var database = mongoClient.GetDatabase("JewelryStoreDB");
        _productsCollection = database.GetCollection<ProductDB>("Products");
        
        _database = redis.GetDatabase();
        _context = context;
    }
    
    public async Task AddProductAsync(ProductDB productDb)
    {
        await _productsCollection.InsertOneAsync(productDb);
    }
    
    public async Task<List<ProductDB>> GetAllProductsAsync(string languageCode)
    {
        var filter = Builders<ProductDB>.Filter.Eq("language", languageCode);
        return await _productsCollection.Find(filter).ToListAsync();
    }

    public async Task UpdateProductAsync(string SKU, ProductDB product)
    {
        var filter = Builders<ProductDB>.Filter.ElemMatch(p => p.specifications, spec => spec.sku == SKU);
        
        var update = Builders<ProductDB>.Update
            .Set(p => p.title, product.title)
            .Set(p => p.onSale, product.onSale)
            .Set(p => p.categories, product.categories)
            .Set(p => p.productType, product.productType)
            .Set(p => p.productSubType, product.productSubType)
            .Set(p => p.description, product.description)
            .Set(p => p.likes, product.likes)
            .Set(p => p.price, product.price)
            .Set(p => p.images, product.images)
            .Set(p => p.baseAdditionalInformation, product.baseAdditionalInformation)
            .Set(p => p.specifications, product.specifications)
            .Set(p => p.createTimeStamp, product.createTimeStamp);
        
        var updateResult = await _productsCollection.UpdateManyAsync(filter, update);
    
        if (updateResult.ModifiedCount == 0)
            throw new Exception($"Не найдено товаров с артикулом: {SKU} для обновления");
    }

    public async Task<List<ProductsSlider>> GetProductInSliderAsync(string languageCode)
    {
        string cacheKey = $"slider-products:{languageCode}";
        
        var cachedData = await _database.StringGetAsync(cacheKey);
        List<ProductsSlider> sliderItem = new List<ProductsSlider>();
        
        if (!cachedData.IsNullOrEmpty)
            sliderItem = JsonConvert.DeserializeObject<List<ProductsSlider>>(cachedData);
        else
        {
            sliderItem = _context.ProductsSlider.ToList();
            var jsonData = JsonConvert.SerializeObject(sliderItem);
            await _database.StringSetAsync(cacheKey, jsonData, TimeSpan.FromMinutes(10));
        }

        return sliderItem;
    }
    
    public async Task<List<ProductDB>> GetProductsInSearchAsync(
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
        var filterBuilder = Builders<ProductDB>.Filter;
        var filters = new List<FilterDefinition<ProductDB>>();
        
        filters.Add(filterBuilder.Eq(p => p.language, languageCode));
        
        if (!string.IsNullOrEmpty(search))
        {
            filters.Add(filterBuilder.Or(
                filterBuilder.Regex(p => p.title, new BsonRegularExpression(search, "i"))
            ));
        }
    
        // Фильтр по типу продукта
        if (!string.IsNullOrEmpty(productType))
            filters.Add(filterBuilder.Eq(p => p.productType, productType));
    
        // Фильтр по цене
        if (minPrice.HasValue)
            filters.Add(filterBuilder.Gte(p => p.price.cost, minPrice.Value));
        if (maxPrice.HasValue)
            filters.Add(filterBuilder.Lte(p => p.price.cost, maxPrice.Value));
    
        // Фильтр по распродаже
        if (isSale.HasValue)
            filters.Add(filterBuilder.Eq(p => p.onSale, isSale.Value));
    
        // Фильтр по наличию
        if (isStock.HasValue && isStock.Value)
            filters.Add(filterBuilder.ElemMatch(p => p.specifications, s => s.inStock));
    
        // Фильтр по скидке
        if (isDiscount.HasValue && isDiscount.Value)
            filters.Add(filterBuilder.Eq(p => p.price.discount, true));
    
        var combinedFilter = filters.Count > 0 ? filterBuilder.And(filters) : filterBuilder.Empty;
    
        // Сортировка
        var sortBuilder = Builders<ProductDB>.Sort;
        SortDefinition<ProductDB>? sortDefinition = null;
    
        if (sortField.HasValue)
        {
            var isAscending = sortOrder == Sorted.asc;
    
            sortDefinition = sortField switch
            {
                SortedParameter.price => isAscending ? sortBuilder.Ascending(p => p.price.cost) : sortBuilder.Descending(p => p.price.cost),
                SortedParameter.date => isAscending ? sortBuilder.Ascending(p => p.createTimeStamp) : sortBuilder.Descending(p => p.createTimeStamp),
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
        var filter = Builders<ProductDB>.Filter.Eq("language", languageCode);
    
        var productTypes = await _productsCollection
            .Distinct<string>("productType", filter)
            .ToListAsync();
    
        return productTypes;
    }
    
    
    public async Task<(ProductDB minProduct, ProductDB maxProduct)> GetMinMaxPricesAsync(string languageCode)
    {
        var filter = Builders<ProductDB>.Filter.Eq("language", languageCode);
        
        // Получаем минимальную цену
        var minProduct = await _productsCollection
            .Find(filter)
            .Sort(Builders<ProductDB>.Sort.Ascending(p => p.price.cost))
            .Limit(1)
            .FirstOrDefaultAsync();
        
        var maxProduct = await _productsCollection
            .Find(filter)
            .Sort(Builders<ProductDB>.Sort.Descending(p => p.price.cost))
            .Limit(1)
            .FirstOrDefaultAsync();

        return (minProduct, maxProduct);
    }
    
    public async Task<List<ProductDB>> GetProductsByCategoryAsync(string category, string languageCode)
    {
        var filter = Builders<ProductDB>.Filter.Eq(p => p.productType, category) &
            Builders<ProductDB>.Filter.Where(p => languageCode == p.language);
    
        var products = await _productsCollection
            .Find(filter)
            .ToListAsync();
    
        return products;
    }
    
    
    public async Task<List<ProductDB>> GetNewProductsAsync(string languageCode)
    {
        var twoWeeksAgo = DateTime.UtcNow.AddDays(-14);
    
        var filter = Builders<ProductDB>.Filter.Gt(p => p.createTimeStamp, twoWeeksAgo) &
                     Builders<ProductDB>.Filter.Eq(p => p.language, languageCode);
    
        var newProducts = await _productsCollection
            .Find(filter)
            .ToListAsync();
    
        return newProducts;
    }
    
    
    public async Task<ProductDB?> GetProductByIdAsync(string languageCode, string sku)
    {
        var filter = Builders<ProductDB>.Filter.Eq(p => p.language, languageCode) &
                     Builders<ProductDB>.Filter.Eq("specifications.sku", sku);
    
        var product = await _productsCollection
            .Find(filter)
            .FirstOrDefaultAsync();
    
        if (product == null)
            return null;
        
        var specification = product.specifications?
            .FirstOrDefault(spec => spec.sku == sku);
    
        if (specification != null)
            product.specifications = new List<Specifications> { specification };
        else
            product.specifications = new List<Specifications>();
    
        return product;
    }
    
    public async Task<ProductDB?> GetProductByIdAllAsync(string languageCode, string sku)
    {
        var filter = Builders<ProductDB>.Filter.Eq(p => p.language, languageCode) &
                     Builders<ProductDB>.Filter.Eq("specifications.sku", sku);
    
        var product = await _productsCollection
            .Find(filter)
            .FirstOrDefaultAsync();
    
        if (product == null)
            return null;
    
        return product;
    }
    
    public async Task<List<ProductDB>> GetRecommendedProductsAsync(string languageCode, string sku)
    {
        var filter = Builders<ProductDB>.Filter.Eq(p => p.language, languageCode) &
                     Builders<ProductDB>.Filter.Eq("specifications.sku", sku);
    
        var product = await _productsCollection
            .Find(filter)
            .FirstOrDefaultAsync();
        
        if (product == null)
            return new List<ProductDB>();
        
        
        var productType = product.productType;
        
        var filterRecommended = Builders<ProductDB>.Filter.Eq(p => p.productType, productType) &
                                Builders<ProductDB>.Filter.Eq(p => p.language, languageCode) &
                                Builders<ProductDB>.Filter.Ne(p => p.Id, product.Id);
        
        var recommendedProducts = await _productsCollection
            .Find(filterRecommended)
            .Limit(3)
            .ToListAsync();
        
        return recommendedProducts;
    }
    
    
    public async Task<List<ProductDB>> GetPopularProductsAsync(string languageCode)
    {
        var filter = Builders<ProductDB>.Filter.Eq(p => p.language, languageCode);
        
        var sort = Builders<ProductDB>.Sort.Descending(p => p.likes);
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


    public class MinMaxPrice
    {
        public double minPrice { get; set; }
        public double maxPrice { get; set; }
        public string currency  { get; set; }
    }
}