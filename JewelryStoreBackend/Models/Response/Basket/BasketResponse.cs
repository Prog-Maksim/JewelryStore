using JewelryStoreBackend.Models.Response.ProductStructure;

namespace JewelryStoreBackend.Models.Response.Basket;

public class BasketResponse
{
    /// <summary>
    /// Кол-во товаров в корзине
    /// </summary>
    public int Quantity { get; set; }
    
    /// <summary>
    /// Общее число товаров
    /// </summary>
    public int Subtotal { get; set; }
    
    /// <summary>
    /// Стоимость заказа в корзине
    /// </summary>
    public Price TotalPrice { get; set; }
    
    /// <summary>
    /// Список товаров в корзине
    /// </summary>
    public List<Productions> Productions { get; set; }
}