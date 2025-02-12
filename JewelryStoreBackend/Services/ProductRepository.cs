using JewelryStoreBackend.Enums;
using JewelryStoreBackend.Models.DB.Product;
using MongoDB.Bson;
using MongoDB.Driver;

namespace JewelryStoreBackend.Services;

public class ProductRepository
{
    private readonly IMongoCollection<Product> _productsCollection;

    public ProductRepository(IMongoClient mongoClient)
    {
        var database = mongoClient.GetDatabase("JewelryStoreDB");
        _productsCollection = database.GetCollection<Product>("Products");
    }
    
    public async Task AddProductAsync(Product product)
    {
        await _productsCollection.InsertOneAsync(product);
    }

    public async Task UpdateProductAsync(string SKU, Product product)
    {
        var filter = Builders<Product>.Filter.ElemMatch(p => p.specifications, spec => spec.sku == SKU);

        // Обновление всех необходимых полей
        var update = Builders<Product>.Update
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

        // Обновляем все записи, которые соответствуют фильтру
        var updateResult = await _productsCollection.UpdateManyAsync(filter, update);

        if (updateResult.ModifiedCount == 0)
        {
            throw new Exception($"No products found with SKU: {SKU} to update.");
        }
    }
    
    // Извлечение всех товаров
    public async Task<List<Product>> GetAllProductsAsync(string languageCode)
    {
        var filter = Builders<Product>.Filter.Eq("language", languageCode);
        
        return await _productsCollection.Find(filter).ToListAsync();
    }

    public async Task<List<Product>> GetProductsInSearchAsync(
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
        var filterBuilder = Builders<Product>.Filter;
        var filters = new List<FilterDefinition<Product>>();
        
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
        var sortBuilder = Builders<Product>.Sort;
        SortDefinition<Product>? sortDefinition = null;

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
    
    // Извлечение всех категорий товаров
    public async Task<List<string>> GetUniqueProductTypesAsync(string languageCode)
    {
        var filter = Builders<Product>.Filter.Eq("language", languageCode);
    
        var productTypes = await _productsCollection
            .Distinct<string>("productType", filter)
            .ToListAsync();

        return productTypes;
    }
    
    
    // Возвращает максимальную и минимальную цену товаров
    public async Task<MinMaxPrice?> GetMinMaxPricesAsync(string languageCode)
    {
        var filter = Builders<Product>.Filter.Eq("language", languageCode);
        
        // Получаем минимальную цену
        var minProduct = await _productsCollection
            .Find(filter)
            .Sort(Builders<Product>.Sort.Ascending(p => p.price.cost))
            .Limit(1)
            .FirstOrDefaultAsync();
        
        var maxProduct = await _productsCollection
            .Find(filter)
            .Sort(Builders<Product>.Sort.Descending(p => p.price.cost))
            .Limit(1)
            .FirstOrDefaultAsync();
        
        var minPrice = minProduct?.price?.cost ?? -1;
        var maxPrice = maxProduct?.price?.cost ?? -1;

        if (maxPrice == -1 || minPrice == -1)
            return null;
        
        return new MinMaxPrice
        {
            minPrice = minPrice,
            maxPrice = maxPrice,
            currency = maxProduct.price.currency
        };
    }
    
    
    // Получение всех товаров по категории и языковому коду
    public async Task<List<Product>> GetProductsByCategoryAsync(string category, string languageCode)
    {
        var filter = Builders<Product>.Filter.Eq(p => p.productType, category) &
            Builders<Product>.Filter.Where(p => languageCode == p.language);

        var products = await _productsCollection
            .Find(filter)
            .ToListAsync();

        return products;
    }
    
    
    // Получение всех новых товаров, добавленных за последние 2 недели
    public async Task<List<Product>> GetNewProductsAsync(string languageCode)
    {
        var twoWeeksAgo = DateTime.UtcNow.AddDays(-14);

        var filter = Builders<Product>.Filter.Gt(p => p.createTimeStamp, twoWeeksAgo) &
                     Builders<Product>.Filter.Eq(p => p.language, languageCode);

        var newProducts = await _productsCollection
            .Find(filter)
            .ToListAsync();

        return newProducts;
    }

    
    // Возвращает товар по id
    public async Task<Product?> GetProductByIdAsync(string languageCode, string sku)
    {
        var filter = Builders<Product>.Filter.Eq(p => p.language, languageCode) &
                     Builders<Product>.Filter.Eq("specifications.sku", sku);

        var product = await _productsCollection
            .Find(filter)
            .FirstOrDefaultAsync();

        if (product == null)
            return null;

        // Проверяем, что объект specifications соответствует SKU
        var specification = product.specifications?
            .FirstOrDefault(spec => spec.sku == sku);

        if (specification != null)
            product.specifications = new List<Specifications> { specification };
        else
            product.specifications = new List<Specifications>();

        return product;
    }
    
    
    // Возвращает все рекоммендованные товары
    public async Task<List<Product>> GetRecommendedProductsAsync(string languageCode, string sku)
    {
        var filter = Builders<Product>.Filter.Eq(p => p.language, languageCode) &
                     Builders<Product>.Filter.Eq("specifications.sku", sku);

        var product = await _productsCollection
            .Find(filter)
            .FirstOrDefaultAsync();
        
        if (product == null)
            return new List<Product>();
        
        
        var productType = product.productType;
        
        var filterRecommended = Builders<Product>.Filter.Eq(p => p.productType, productType) &
                                Builders<Product>.Filter.Eq(p => p.language, languageCode) &
                                Builders<Product>.Filter.Ne(p => p.Id, product.Id);
        
        var recommendedProducts = await _productsCollection
            .Find(filterRecommended)
            .Limit(3)
            .ToListAsync();
        
        return recommendedProducts;
    }
    
    
    // Вовзвращает все популярные товары
    public async Task<List<Product>> GetPopularProductsAsync(string languageCode)
    {
        var filter = Builders<Product>.Filter.Eq(p => p.language, languageCode);
        
        var sort = Builders<Product>.Sort.Descending(p => p.likes);
        var popularProducts = await _productsCollection.Find(filter)
            .Sort(sort)
            .Limit(9)
            .ToListAsync();

        return popularProducts;
    }
    
    
    public class MinMaxPrice
    {
        public double minPrice { get; set; }
        public double maxPrice { get; set; }
        public string currency  { get; set; }
    }
}