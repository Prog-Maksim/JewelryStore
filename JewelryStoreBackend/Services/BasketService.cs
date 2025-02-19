using JewelryStoreBackend.Enums;
using JewelryStoreBackend.Models.DB;
using JewelryStoreBackend.Models.Response;
using JewelryStoreBackend.Models.Response.Basket;
using JewelryStoreBackend.Models.Response.ProductStructure;
using JewelryStoreBackend.Repository.Interfaces;
using JewelryStoreBackend.Script;

namespace JewelryStoreBackend.Services;

public class BasketService
{
    private readonly IBasketRepository _basketRepository;
    private readonly IProductRepository _productRepository;

    public BasketService(IBasketRepository basketRepository, IProductRepository productRepository)
    {
        _basketRepository = basketRepository;
        _productRepository = productRepository;
    }

    public async Task<(BaseResponse Response, BasketResponse? Baslets)> GetProductInBasket(string userId, string languageCode)
    {
        var productInBasket = await _basketRepository.GetUserBasketAsync(userId);
        
        if (productInBasket.Count == 0)
            return (new BaseResponse { Success = false, Message = "Товары не найдены в корзине", StatusCode = 404, Error = "NotFound" }, null);
        
        BasketResponse bakset = new BasketResponse
        { Quantity = productInBasket.Count, Subtotal = productInBasket.Select(p => p.Count).Sum() };
        
        Price priceBasket = new Price();
        List<Productions> products = new List<Productions>();
        
        foreach (var item in productInBasket)
        {
            var product = await _productRepository.GetProductByIdAsync(languageCode, item.ProductId);
            
            if (product == null)
                return (new BaseResponse { Success = false, Message = "Товар не найден", StatusCode = 404, Error = "NotFound" }, null);

            PriceProduction price = new PriceProduction
            {
                Cost = product.Price.Cost,
                Currency = product.Price.Currency,
                Discount = product.Price.Discount,
                CostDiscount = product.Price.CostDiscount,
                Percent = product.Price.Percent,
            };
        
            Productions productModel = new Productions
            {
                LanguageCode = languageCode,
                ProductId = product.ProductId,
                Sku = item.ProductId,
                Title = product.Title,
                Description = product.Description,
                Images = product.Images,
                Quantity = item.Count,
                OnSale = product.OnSale,
                InStock = product.Specifications.First().InStock,
                Likes = product.Likes,
                PriceProduction = price,
            };
        
            priceBasket.Currency = productModel.PriceProduction.Currency;
            priceBasket.Cost += productModel.PriceProduction.Cost;
            priceBasket.CostDiscount += productModel.PriceProduction.CostDiscount;
            
            products.Add(productModel);
        }
        
        if (priceBasket.Cost == priceBasket.CostDiscount)
        {
            priceBasket.Discount = false;
            priceBasket.Percent = 0;
        }
        else
        {
            priceBasket.Discount = true;
            int discountPercentage = CostCalculation.CalculateDiscountPercentage(priceBasket.Cost, priceBasket.CostDiscount);
            priceBasket.Percent = discountPercentage;
        }
        
        bakset.TotalPrice = priceBasket;
        bakset.Productions = products;
        
        return (new BaseResponse { Success = true, Message = "Успешно" }, bakset);
    }

    public async Task<(BaseResponse Response, Basket? Baslet)> CheckProductInBasket(string userId,
        string productId)
    {
        var productInBasket = await _basketRepository.GetProductBasketAsync(userId, productId);
        
        if (productInBasket == null)
            return (new BaseResponse { Success = false, Message = "Товар в корзине не найден", StatusCode = 404, Error = "NotFound" }, null);
        
        return (new BaseResponse { Success = true, Message = "Успешно" }, productInBasket);
    }

    public async Task<BaseResponse> AddProductInBasket(string userId, string productId, int quantity)
    {
        if (quantity <= 0)
            return new BaseResponse { Success = false, Message = "Добавляемое кол-во товаров не может быть меньше или равно 0", StatusCode = 400, Error = "Bad Request" };
        
        var productInBasket = await _basketRepository.GetProductBasketAsync(userId, productId);
        
        if (productInBasket != null)
            productInBasket.Count += quantity;
        else
        {
            Basket basket = new Basket { PersonId = userId, ProductId = productId, Count = quantity, DateAdded = DateTime.Now, };
            await _basketRepository.AddProductToBasketAsync(basket);
        }

        await _basketRepository.SaveChangesAsync();
        return new BaseResponse { Success = true, Message = "Товар(ы) успешно добавлен в корзину", StatusCode = 200 };
    }

    public async Task<BaseResponse> DeleteProductInBasket(string userId, string productId, Count count)
    {
        var productInBasket = await _basketRepository.GetProductBasketAsync(userId, productId);
        
        if (productInBasket == null)
            return new BaseResponse { Success = false, Message = "Товар не найден в корзине!", StatusCode = 404, Error = "NotFound" };
        
        if (count == Count.One)
            productInBasket.Count -= 1;
        if (count == Count.All || productInBasket.Count <= 0)
            _basketRepository.DeleteProductToBasketAsync(productInBasket);
        
        await _basketRepository.SaveChangesAsync();
        return new BaseResponse { Success = true, Message = "Товар успешно удален из корзины", StatusCode = 200 };
    }
}