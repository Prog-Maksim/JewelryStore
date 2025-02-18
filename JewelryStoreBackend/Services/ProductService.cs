using JewelryStoreBackend.Enums;
using JewelryStoreBackend.Models.DB.Product;
using JewelryStoreBackend.Models.DB.Rating;
using JewelryStoreBackend.Models.Other;
using JewelryStoreBackend.Models.Request.Rating;
using JewelryStoreBackend.Models.Response;
using JewelryStoreBackend.Models.Response.Rating;
using JewelryStoreBackend.Repository;
using JewelryStoreBackend.Repository.Interfaces;
using JewelryStoreBackend.Security;
using StackExchange.Redis;
using Convert = JewelryStoreBackend.Script.Convert;
using Product = JewelryStoreBackend.Models.Response.Product;

namespace JewelryStoreBackend.Services;

public class ProductService
{
    private readonly IProductRepository _productRepository;
    private readonly IMessageRepository _messageRepository;
    private readonly IDatabase _database;

    public ProductService(IProductRepository productRepository, IMessageRepository messageRepository, IConnectionMultiplexer redis)
    {
        _productRepository = productRepository;
        _messageRepository = messageRepository;
        _database = redis.GetDatabase();
    }

    public async Task<(BaseResponse Response, List<Product>? products)> GetSliderInfo(string languageCode)
    {
        var sliderItem = await _productRepository.GetProductInSliderAsync(languageCode);
        
        List<Product> products = new ();
        
        foreach (var item in sliderItem)
        {
            var result = await _productRepository.GetProductByIdAsync(languageCode, item.SliderProductId);

            if (result != null)
            {
                var product = Convert.ConvertToSimpleModel(result);
                product.ProductImageId = new List<string> { item.SliderImageId };
            
                products.Add(product);
            }
        }

        if (products.Count == 0)
            return (new BaseResponse { Success = false, Message = "Товары не найдены", StatusCode = 404, Error = "NotFound" }, null);
        
        return (new BaseResponse { Success = true, Message = "Успешно", StatusCode = 200 }, products);
    }

    public async Task<(BaseResponse Response, List<ProductDB>? products)> GetProductsInSearchAsync(
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
        var products = await _productRepository.GetProductsInSearchAsync(search, productType, minPrice, maxPrice, isSale,
            isStock, isDiscount, sortOrder, sortField, languageCode);
        
        if (products.Count == 0)
            return (new BaseResponse { Success = false, Message = "Товары не найдены", StatusCode = 404, Error = "NotFound" }, null);

        var result = Convert.ConvertProductWithSingleSpecification(products);
        return (new BaseResponse { Success = true, Message = "Успешно", StatusCode = 200 }, result);
    }

    public async Task<(BaseResponse Response, List<ProductDB>?)> GetNewProductsAsync(string languageCode)
    {
        var products = await _productRepository.GetNewProductsAsync(languageCode);
        
        if (products.Count == 0)
            return (new BaseResponse { Success = false, Message = "Товары не найдены", StatusCode = 404, Error = "NotFound" }, null);

        var result = Convert.ConvertProductWithSingleSpecification(products);
        return (new BaseResponse { Success = true, Message = "Успешно", StatusCode = 200 }, result);
    }
    
    public async Task<(BaseResponse Response, List<ProductDB>? products)> GetPopularProductsAsync(string languageCode)
    {
        var products = await _productRepository.GetPopularProductsAsync(languageCode);
        
        if (products.Count == 0)
            return (new BaseResponse { Success = false, Message = "Товары не найдены", StatusCode = 404, Error = "NotFound" }, null);
        
        var result = Convert.ConvertProductWithSingleSpecification(products);
        return (new BaseResponse { Success = true, Message = "Успешно", StatusCode = 200 }, result);
    }

    public async Task<(BaseResponse Response, List<ProductDB>? products)> GetAllProductsSingleSpecificationsAsync(string languageCode)
    {
        var products = await _productRepository.GetAllProductsAsync(languageCode);
        
        if (products.Count == 0)
            return (new BaseResponse { Success = false, Message = "Товары не найдены", StatusCode = 404, Error = "NotFound" }, null);
        
        var result = Convert.ConvertProductWithSingleSpecification(products);
        return (new BaseResponse { Success = true, Message = "Успешно", StatusCode = 200 }, result);
    }

    public async Task<(BaseResponse Response, List<ProductDB>? products)> GetProductsByCategorySingleSpecificationsAsync(
        string category, string languageCode)
    {
        var products = await _productRepository.GetProductsByCategoryAsync(category, languageCode);
        
        if (products.Count == 0)
            return (new BaseResponse { Success = false, Message = "Товары не найдены", StatusCode = 404, Error = "NotFound" }, null);
        
        var result = Convert.ConvertProductWithSingleSpecification(products);
        return (new BaseResponse { Success = true, Message = "Успешно", StatusCode = 200 }, result);
    }

    public async Task<(BaseResponse Response, ProductRepository.MinMaxPrice? price)> GetMinMaxPricesAsync(
        string languageCode)
    {
        var (minProduct, maxProduct) = await _productRepository.GetMinMaxPricesAsync(languageCode);
        
        double minPrice = minProduct?.price?.cost ?? -1;
        double maxPrice = maxProduct?.price?.cost ?? -1;
        
        if (maxPrice == -1 || minPrice == -1)
            return (new BaseResponse { Success = false, Message = "Товары не найдены", StatusCode = 404, Error = "NotFound" }, null);
        
        return (new BaseResponse { Success = false, Message = "Товары не найдены", StatusCode = 404, Error = "NotFound" }, new ProductRepository.MinMaxPrice
        {
            minPrice = minPrice,
            maxPrice = maxPrice,
            currency = maxProduct.price.currency
        });
    }

    public async Task<(BaseResponse Response, Product? product)> GetProductByIdAsync(string languageCode, string sku)
    {
        var product = await _productRepository.GetProductByIdAsync(languageCode, sku);
        
        if (product == null)
            return (new BaseResponse { Success = false, Message = "Товары не найдены", StatusCode = 404, Error = "NotFound" }, null);
        
        return (new BaseResponse { Success = true, Message = "Успешно", StatusCode = 200 }, Convert.ConvertToSimpleModel(product));
    }

    public async Task<BaseResponse> AddNewCommentAsync(string userId, string languageCode, NewMessage message)
    {
        var product = await _productRepository.GetProductByIdAsync(languageCode, message.SKU);

        if (product == null)
            return new BaseResponse { Success = false, Message = "Товар не найден", StatusCode = 404, Error = "NotFound" };
        
        Message newMessage = new Message
        {
            MessageId = Guid.NewGuid().ToString(),
            PersonId = userId,
            ProdutId = message.SKU,
            ReplyMessageId = message.replyMessageId,

            Text = message.text,
            Rating = message.rating,
            Timestamp = DateTime.Now,
            Status = MessageStatus.Sent
        };
        
        string messageId = await _messageRepository.AddMessageAsync(newMessage);
        
        return new SuccessfulCreatemessage { Success = true, Message = "Комментарий успешно добавлен", CommentId = messageId };
    }

    public async Task<BaseResponse> UpdateCommentAsync(JwtTokenData dataToken, UpdateMessage message)
    {
        var result = await _messageRepository.GetMessageByIdAsync(message.SKU, message.messageId);
        
        if (result == null)
            return new BaseResponse { Success = false, Message = "Комментарий не найден",StatusCode = 404,Error = "NotFound" };
        
        if (result.PersonId == dataToken.UserId || dataToken.Role == Roles.editor || dataToken.Role == Roles.admin)
            await _messageRepository.UpdateMessageAsync(message.SKU, message.messageId, message.newText, message.newRating);
        else
            return new BaseResponse { Success = false, Message = "отказано в доступе", StatusCode = 403, Error = "Forbidden" };
        
        return new SuccessfulCreatemessage { Success = true,Message = "Комментарий успешно обновлен", CommentId = message.messageId };
    }

    public async Task<BaseResponse> DeleteCommentAsync(JwtTokenData dataToken, string sku, string messageId)
    {
        var message = await _messageRepository.GetMessageByIdAsync(sku, messageId);
        
        if (message == null)
            return new BaseResponse { Success = false, Message = "Комментарий не найден",StatusCode = 404,Error = "NotFound" };
        
        if (message.PersonId == dataToken.UserId || dataToken.Role == Roles.editor || dataToken.Role == Roles.admin)
            await _messageRepository.DeleteMessageAsync(sku, messageId);
        else
            return new BaseResponse { Success = false, Message = "отказано в доступе", StatusCode = 403, Error = "Forbidden" };
        
        return new SuccessfulCreatemessage { Success = true, Message = "Комментарий успешно удален", CommentId = messageId };
    }
    
    public async Task<(BaseResponse Response, IEnumerable<Message>? Messages)> GetAllMesageAuthorizeAsync(JwtTokenData dataToken, string sku)
    {
        if (await JwtController.ValidateAccessJwtToken(_database, dataToken))
            return (new BaseResponse { Success = false, Message = "отказано в доступе", StatusCode = 403, Error = "Forbidden" }, null);
        
        var comments = (await _messageRepository.GetMessagesByProductIdAsync(sku)).ToList();

        foreach (var comment in comments)
        {
            if (comment.PersonId == dataToken.UserId || dataToken.Role == Roles.editor || dataToken.Role == Roles.admin)
                comment.SendBy = PersonStatus.SendByYou;
            else
                comment.SendBy = PersonStatus.SendByAnother;
        }
        
        return (new BaseResponse { Success = false, Message = "Успешно", StatusCode = 200 }, comments);
    }

    public async Task<BaseResponse> GetProductRatingAsync(string sku)
    {
        var ratings = await _messageRepository.GetAllRatingsAsync(sku);

        if (ratings.Count == 0)
            return new ProductRating { Message = "Рейтинг товара", Rating = 0.0, CustomersCount = ratings.Count };

        var averageRating = Math.Round(ratings.Average(), 2);
        return new ProductRating { Message = "Рейтинг товара", Rating = averageRating, CustomersCount = ratings.Count };
    }

    public async Task<BaseResponse> ToggleLikeAsync(string userId, string languageCode, string sku)
    {
        var product = await _productRepository.GetProductByIdAsync(languageCode, sku);

        if (product == null)
            return new BaseResponse { Success = false, Message = "Данный товар не найден!", StatusCode = 404, Error = "Not Found" };

        var result = await _productRepository.GetLikesAsync(userId, sku);

        if (result == null)
        {
            product.likes += 1;

            UsersLike like = new UsersLike
            {
                ProductId = sku,
                PersonId = userId
            };
            
            await _productRepository.UpdateProductAsync(sku, product);

            await _productRepository.AddLikeAsync(like);
            await _productRepository.SaveChangesAsync();
            
            return new StateLike { Success = true, Message = "Лайк успешно добавлен", IsLiked = true };
        }
        
        product.likes -= 1;
        await _productRepository.UpdateProductAsync(sku, product);
        
        _productRepository.RemoveLikeAsync(result);
        await _productRepository.SaveChangesAsync();
            
        return new StateLike { Success = true, Message = "Лайк успешно удален", IsLiked = false };
    }

    public async Task<BaseResponse> IsProductLikedByUserAsync(string userId, string languageCode, string sku)
    {
        var product = await _productRepository.GetProductByIdAsync(languageCode, sku);
        
        if (product == null)
            return new BaseResponse { Success = false, Message = "Данный товар не найден!", StatusCode = 404, Error = "Not Found" };
        
        var result = await _productRepository.GetLikesAsync(userId, sku);

        if (result != null)
            return new StateLike { Success = true, Message = "Лайк установлен", IsLiked = true };
        
        return new StateLike { Success = true, Message = "Лайк не установлен", IsLiked = false };
    }
}