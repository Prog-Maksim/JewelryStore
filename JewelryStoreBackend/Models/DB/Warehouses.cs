namespace JewelryStoreBackend.Models.DB;

public class Warehouses
{
    public int Id { get; set; }
    
    /// <summary>
    /// Идентификатор склада
    /// </summary>
    public int WarehouseId { get; set; }
    
    /// <summary>
    /// Название склада
    /// </summary>
    public string Title { get; set; }
    
    /// <summary>
    /// Адрес склада
    /// </summary>
    public string Address { get; set; }
    
    /// <summary>
    /// Долгота
    /// </summary>
    public string lon  { get; set; }
    
    /// <summary>
    /// Широта
    /// </summary>
    public string lat { get; set; }
}