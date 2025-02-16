namespace JewelryStoreBackend.Models.DB;

public class Basket
{
    public int Id { get; set; }
    
    /// <summary>
    /// Идентификатор пользователя
    /// </summary>
    public string PersonId { get; set; }
    
    /// <summary>
    /// Идентификатор товара
    /// </summary>
    public string ProductId { get; set; }
    
    /// <summary>
    /// Кол-во товаров
    /// </summary>
    public int Count { get; set; }
    
    /// <summary>
    /// Дата добавления товара
    /// </summary>
    public DateTime DateAdded { get; set; }
}