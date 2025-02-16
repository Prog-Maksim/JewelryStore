using JewelryStoreBackend.Enums;
using JewelryStoreBackend.Models.Request;

namespace JewelryStoreBackend.Models.DB.Order;

public class OrderPayments
{
    public int Id { get; set; }
    
    /// <summary>
    /// Идентификатор заказа
    /// </summary>
    public string OrderId { get; set; }
    
    /// <summary>
    /// Метод оплаты
    /// </summary>
    public PaymentType PaymentMethod { get; set; }
    
    /// <summary>
    /// Статус оплаты
    /// </summary>
    public PaymentStatus PaymentStatus { get; set; }
    
    /// <summary>
    /// Дата оплаты
    /// </summary>
    public DateTime? DatePayment { get; set; }
    

    public Orders Order { get; set; }
}