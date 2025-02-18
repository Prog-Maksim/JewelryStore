using JewelryStoreBackend.Enums;
using JewelryStoreBackend.Models.DB;
using JewelryStoreBackend.Models.DB.Order;
using JewelryStoreBackend.Models.DB.User;
using JewelryStoreBackend.Models.Request;
using JewelryStoreBackend.Models.Response;
using JewelryStoreBackend.Models.Response.Order;
using JewelryStoreBackend.Repository.Interfaces;
using JewelryStoreBackend.Script;
using Convert = System.Convert;

namespace JewelryStoreBackend.Services;

public class OrderService
{
    private readonly int TimeExpiredMinute = 30;
    
    private readonly IOrderRepository _orderRepository;
    private readonly IBasketRepository _basketRepository;
    private readonly IProductRepository _productRepository;
    private readonly IUserRepository _userRepository;
    private readonly ICouponRepository _couponRepository;
    private readonly IAddressRepository _addressRepository;

    public OrderService(
        IOrderRepository orderRepository, 
        IBasketRepository basketRepository, 
        IProductRepository productRepository,
        IUserRepository userRepository,
        ICouponRepository couponRepository,
        IAddressRepository addressRepository)
    {
        _orderRepository = orderRepository;
        _basketRepository = basketRepository;
        _productRepository = productRepository;
        _userRepository = userRepository;
        _couponRepository = couponRepository;
        _addressRepository = addressRepository;
    }
    
    private (BaseResponse Response, OrderData? Order) CreateResponse(string message, int statusCode, string error)
    {
        return (new BaseResponse { Success = false, Message = message, StatusCode = statusCode, Error = error }, null);
    }
    
    public async Task<(bool Success, BaseResponse Response, OrderData? Order)> InitiateOrderAsync(string userId, string languageCode)
    {
        var basketItems = await _basketRepository.GetUserBasketAsync(userId);
        
        if (!basketItems.Any())
            return (false, new BaseResponse { Success = false, Message = "Товары не найдены в корзине", StatusCode = 404 }, null);

        if (await _orderRepository.IsOrderInProgressAsync(userId))
            return (false, new BaseResponse { Success = false, Message = "Заказ уже оформляется", StatusCode = 400 }, null);

        var products = new List<ProductOrderData>();
        string currency = "";

        foreach (var item in basketItems)
        {
            var product = await _productRepository.GetProductByIdAsync(languageCode, item.ProductId);
            if (product.specifications.First().inStock || product.onSale)
            {
                currency = product.price.currency;
                products.Add(new ProductOrderData
                {
                    SKU = product.specifications.First().sku,
                    Title = product.title,
                    Price = item.Count * product.price.cost,
                    PriceDiscount = item.Count * product.price.costDiscount,
                    Quantity = item.Count,
                    ProductImage = product.images.First(),
                    ProductType = product.productType
                });
            }
        }

        var totalPrice = products.Sum(p => p.Price);
        var totalDiscountPrice = products.Sum(p => p.PriceDiscount);
        
        var user = await _userRepository.GetUserByIdAsync(userId);

        var orderData = new OrderData
        {
            languageCode = languageCode,
            userData = new UserOrderData
            {
                Name = user.Surname + " " + user.Name,
                Email = user.Email,
                NumberPhone = user.PhoneNumber
            },
            Items = products,
            PriceDatails = new PriceOrderData
            {
                TotalPrice = totalPrice,
                TotalPriceDiscount = totalDiscountPrice,
                TotalCost = totalDiscountPrice,
                TotalPercentInProduct = CostCalculation.CalculateDiscountPercentage(totalPrice, totalDiscountPrice),
                Currency = currency
            }
        };
        
        await _orderRepository.StartOrderAsync(userId, orderData, TimeSpan.FromMinutes(TimeExpiredMinute));

        return (true, new BaseResponse { Success = true, Message = "Заказ успешно создан" }, orderData);
    }
    
    public async Task<BaseResponse> CancelOrderAsync(string userId)
    {
        var isCanceled = await _orderRepository.CancelOrderAsync(userId);
        
        if (!isCanceled)
            return new BaseResponse { Success = false, Message = "Ошибка при отмене заказа", StatusCode = 500 };

        return new BaseResponse { Success = true, Message = "Заказ успешно отменен" };
    }

    public async Task<(BaseResponse Response, OrderData? Order)> AddCouponAsync(string userId, string couponCode, string languageCode)
    {
        var coupon = await _couponRepository.GetCoupon(couponCode, languageCode);

        if (coupon == null)
            return CreateResponse("Данный купон не найден!", 404, "Not Found");

        OrderData? order = await _orderRepository.GetOrderDataByIdAsync(userId);
        
        if (!await _orderRepository.IsOrderInProgressAsync(userId) || order == null)
            return CreateResponse("Заказ не найден", 404, "Not Found");
        
        if (order.couponData != null)
            return CreateResponse("Купон уже применен!", 400, "Bad Request");
        
        
        var couponOrderData = MapCouponToOrderData(coupon);
        var appliedDiscountProducts = ApplyCouponDiscount(order, coupon);

        if (appliedDiscountProducts == 0)
            return CreateResponse("Купон нельзя применить к данным товарам", 403, "Forbidden");

        order.couponData = couponOrderData;
        
        await _orderRepository.StartOrderAsync(userId, order, TimeSpan.FromMinutes(TimeExpiredMinute));

        return (new BaseResponse { Success = true, Message = "Купон успешно применен" }, order);
    }

    private CouponOrderData MapCouponToOrderData(Coupon coupon)
    {
        return new()
        {
            CouponCode = coupon.CouponCode,
            Description = coupon.Description,
            Percent = coupon.Percent,
            Title = coupon.Title
        };
    }
    private int ApplyCouponDiscount(OrderData order, Coupon coupon)
    {
        int appliedDiscountProducts = 0;
        double priceDiscount = 0;

        foreach (var item in order.Items)
        {
            if (IsCouponApplicable(item, coupon))
            {
                appliedDiscountProducts++;
                priceDiscount += CostCalculation.CalculateDiscountedPrice(item.Price, coupon.Percent);
            }
            else
                priceDiscount += item.PriceDiscount;
        }

        UpdateOrderPriceDetails(order, priceDiscount, coupon.Percent);

        return appliedDiscountProducts;
    }
    private bool IsCouponApplicable(ProductOrderData item, Coupon coupon)
    {
        return coupon.Action switch
        {
            CouponAction.ALL => !item.Discount,
            CouponAction.NEW => item.ProductAddedData > DateTime.UtcNow.AddDays(-14),
            CouponAction.CATEGORY => item.ProductType == coupon.CategoryType,
            _ => false,
        };
    }
    private void UpdateOrderPriceDetails(OrderData order, double priceDiscount, int? couponPercent)
    {
        order.PriceDatails.PercentTheCoupon = couponPercent;
        order.PriceDatails.TotalDiscountTheCoupon = priceDiscount;
        var shippingCost = order.PriceDatails.ShippingCost ?? 0;
        order.PriceDatails.TotalCost = priceDiscount + shippingCost;
    }

    public async Task<(BaseResponse Response, OrderData? Order)> DeleteCouponAsync(string userId)
    {
        OrderData? order = await _orderRepository.GetOrderDataByIdAsync(userId);
        
        if (!await _orderRepository.IsOrderInProgressAsync(userId) || order == null)
            return CreateResponse("Заказ не найден", 404, "Not Found");
        
        if (order.couponData == null)
            return CreateResponse("Купон не найден!", 404, "Not Found");
        
        order.couponData = null;
        
        order.PriceDatails.PercentTheCoupon = null;
        order.PriceDatails.TotalDiscountTheCoupon = null;
        
        order.PriceDatails.TotalCost = order.PriceDatails.TotalPriceDiscount + (order.PriceDatails.ShippingCost ?? 0);
        
        await _orderRepository.StartOrderAsync(userId, order, TimeSpan.FromMinutes(TimeExpiredMinute));
        
        return (new BaseResponse { Success = true, Message = "Купон успешно удален!" }, order);
    }

    public async Task<(BaseResponse Response, OrderData? Order)> AddShippingAsync(string userId, string addressId, DeliveryType deliveryType)
    {
        OrderData? order = await _orderRepository.GetOrderDataByIdAsync(userId);
        
        if (!await _orderRepository.IsOrderInProgressAsync(userId) || order == null)
            return CreateResponse("Заказ не найден", 404, "Not Found");
        
        if (order.shippingData != null)
            return CreateResponse("Доставка уже применена!", 400, "Bad Request");

        Address? address = await _addressRepository.GetAddressByIdAsync(userId, addressId);
        
        if (address == null)
            return CreateResponse("Данный адрес не найден", 404, "Not Found");
        
        string lonStart = address.lon;
        string latStart = address.lat;
        
        var warehouseAddress = await _addressRepository.GetWarehouseByIdAsync("");
        
        var coordinate =
            await GeolocationService.GetGeolocateDistanceAsync(lonStart, latStart, warehouseAddress.lon, warehouseAddress.lat);
        
        if (coordinate == null)
            return CreateResponse("Не удалось рассчитать стоимость, попробуйте позже", 408, "Request Timeout");
        
        var result = DeliveryCalculator.CalculateDeliveryCost(coordinate.routes.First().legs.First().distance / 1000, deliveryType);
        
        var couponPrice = order.PriceDatails.TotalDiscountTheCoupon ?? order.PriceDatails.TotalPriceDiscount;
        
        order.PriceDatails.ShippingCost = result.TotalCost;
        order.PriceDatails.TotalCost = Math.Round(couponPrice + result.TotalCost, 2);
        
        var duration = Convert.ToInt32(coordinate.routes.First().legs.First().duration);
        DateTime dateShipping = DateTime.Now + TimeSpan.FromDays(duration);

        ShippingOrderData shippingOrderData = new ShippingOrderData
        {
            WarehouseAddress = warehouseAddress.Address,
            UserAddress = address.City + " " + address.AddressLine1 + " " + address.AddressLine2,
            UserPostalCode = address.PostalCode,
            ShippingCost = result.TotalCost,
            EstimatedDeliveryTime = dateShipping,
            ShippingMethod = deliveryType,
            Details = result
        };
        
        order.shippingData = shippingOrderData;
        
        await _orderRepository.StartOrderAsync(userId, order, TimeSpan.FromMinutes(TimeExpiredMinute));
        return (new BaseResponse { Success = true, Message = "Доставка успешно применена!" }, order);
    }

    public async Task<(BaseResponse Response, OrderData? Order)> DeleteShippingAsync(string userId)
    {
        OrderData? order = await _orderRepository.GetOrderDataByIdAsync(userId);
        
        if (!await _orderRepository.IsOrderInProgressAsync(userId) || order == null)
            return CreateResponse("Заказ не найден", 404, "Not Found");
        
        if (order.shippingData == null)
            return CreateResponse("Доставка не найдена!", 404, "Not Found");
        
        order.shippingData = null;
        order.PriceDatails.TotalCost -= order.PriceDatails.ShippingCost ?? 0;
        order.PriceDatails.TotalCost = Math.Round(order.PriceDatails.TotalCost, 2);
        order.PriceDatails.ShippingCost = null;
        
        await _orderRepository.StartOrderAsync(userId, order, TimeSpan.FromMinutes(TimeExpiredMinute));
        return (new BaseResponse { Success = true, Message = "Доставка успешно удалена!" }, order);
    }

    public async Task<(BaseResponse Response, OrderData? Order)> RegisterOrderAsync(string userId, PaymentSelection payment)
    {
        OrderData? preOrder = await _orderRepository.GetOrderDataByIdAsync(userId);
        
        if (!await _orderRepository.IsOrderInProgressAsync(userId) || preOrder == null)
            return CreateResponse("Заказ не найден", 404, "Not Found");
        
        Random rnd = new Random();
        string orderId = '#' + rnd.NextInt64(1111111111, 10000000000).ToString();

        Orders order = new Orders
        {
            OrderId = orderId,
            PersonId = userId,
            CreateTimestamp = DateTime.Now,
            Status = OrderStatus.Pending,
            OrderCost = preOrder.PriceDatails.TotalCost,
            Currency = preOrder.PriceDatails.Currency
        };
        await _orderRepository.AddOrderAsync(order);

        foreach (var item in preOrder.Items)
        {
            OrderProducts product = new OrderProducts
            {
                OrderId = orderId,
                SKU = item.SKU,
                Cost = item.PriceDiscount,
                Quantity = item.Quantity
            };
            await _orderRepository.AddOrderProductsAsync(product);
        }

        OrderPayments payments = new OrderPayments
        {
            OrderId = orderId,
            PaymentMethod = payment.PaymentType,
            PaymentStatus = payment.PaymentType == PaymentType.Card ? PaymentStatus.Paid : PaymentStatus.NotPaid,
            DatePayment = payment.PaymentType == PaymentType.Card ? DateTime.Now : null
        };
        await _orderRepository.AddOrderPaymentsAsync(payments);

        OrderShippings shippings = new OrderShippings
        {
            OrderId = orderId,
            TargetAddress = preOrder.shippingData.UserAddress,
            WarehouseAddress = preOrder.shippingData.WarehouseAddress,
            DeliveryType = preOrder.shippingData.ShippingMethod,
            DateShipping = preOrder.shippingData.EstimatedDeliveryTime
        };
        await _orderRepository.AddOrderShippingsAsync(shippings);
        await _orderRepository.SaveChangesAsync();

        await _basketRepository.RemoveBasketItemsAsync(userId);
        await _basketRepository.SaveChangesAsync();
        
        return (new OrderCompleted
        {
            Message = "Заказ успешно оформлен",
            OrderId = orderId,
            Success = true
        }, preOrder);
    }

    public async Task<BaseResponse> CancelledOrderAsync(string userId, string orderId)
    {
        Orders? order = await _orderRepository.GetOrderByIdAsync(userId, orderId);
        
        if (order == null)
            return new BaseResponse { Success = false, Message = "Заказ не найден!", StatusCode = 404, Error = "Not Found" };
        
        if (await GetCheckCancelledOrderAsync(userId, orderId))
            return new BaseResponse { Success = false, Message = "Данный заказ нельзя отменить!", StatusCode = 403, Error = "Forbidden" };
        
        order.Status = OrderStatus.Cancelled;
        order.CompletedTimestamp = DateTime.Now;

        await _orderRepository.SaveChangesAsync();
        
        return new BaseResponse { Success = false, Message = "Заказ успешно отменен", StatusCode = 200 };
    }

    public async Task<(BaseResponse Response, List<PreviewOrder> order)> GetPreviewOrderAsync(string userId)
    {
        List<Orders> orders = await _orderRepository.GetAllOrdersByUserIdAsync(userId);
        
        if (orders.Count == 0)
            CreateResponse("Заказы не найдены", 404, "Not Found");
        
        List<PreviewOrder> preOrders = new List<PreviewOrder>();

        foreach (var item in orders)
        {
            PreviewOrder order = new PreviewOrder
            {
                OrderId = item.OrderId,
                CreateOrderTimestamp = item.CreateTimestamp,
                Status = item.Status,
                Cost = item.OrderCost,
                Currency = item.Currency,
            };
            preOrders.Add(order);
        }

        return (new BaseResponse { Message = "Список заказов", StatusCode = 200 }, preOrders);
    }

    public async Task<(BaseResponse Response, OrderDetail order)> GetDetailOrderAsync(string userId, string orderId)
    {
        Orders? order = await _orderRepository.GetDetailOrderByIdAsync(userId, orderId);
        
        if (order == null)
            CreateResponse("Заказ не найден", 404, "Not Found");
        
        OrderDetailShipping shipping = new OrderDetailShipping
        {
            TargetAddress = order.Shipping.TargetAddress,
            WarehouseAddress = order.Shipping.WarehouseAddress,
            DeliveryType = order.Shipping.DeliveryType,
            DateShipping = order.Shipping.DateShipping
        };

        OrderDetailPayment payment = new OrderDetailPayment
        {
            paymentMethod = order.Payment.PaymentMethod,
            paymentStatus = order.Payment.PaymentStatus,
            datePayment = order.Payment.DatePayment
        };
        
        List<OrderDetailProduct> products = new List<OrderDetailProduct>();

        foreach (var item in order.Products)
        {
            OrderDetailProduct product = new OrderDetailProduct
            {
                sku = item.SKU,
                cost = item.Cost,
                quantity = item.Quantity
            };
            products.Add(product);
        }

        OrderDetail orderDetail = new OrderDetail
        {
            orderId = order.OrderId,
            name = order.Users.Surname + " " + order.Users.Name,
            email = order.Users.Email,
            phoneNumber = order.Users.PhoneNumber,
            createTimestamp = order.CreateTimestamp,
            completedTimestamp = order.CompletedTimestamp,
            status = order.Status,
            orderCost = order.OrderCost,
            currency = order.Currency,
            products = products,
            shipping = shipping,
            payment = payment,
        };
        
        return (new BaseResponse{ Message = "Успешно", StatusCode = 200 }, orderDetail);
    }

    public async Task<bool> GetCheckCancelledOrderAsync(string userId, string orderId)
    {
        Orders? order = await _orderRepository.GetOrderByIdAsync(userId, orderId);

        if (order == null)
            return false;

        return order.Status == OrderStatus.Refund || order.Status == OrderStatus.Cancelled ||
               order.Status == OrderStatus.Completed || order.Status == OrderStatus.Received;
    }
}