namespace JewelryStoreBackend.Models;

public class Warehouses
{
    /// <summary>
    /// Идентификатор
    /// </summary>
    public int Id { get; set; }
    
    /// <summary>
    /// Идентификатор склада
    /// </summary>
    public int WarehouseId { get; set; }
    
    /// <summary>
    /// Название
    /// </summary>
    public required  string Title { get; set; }
    
    /// <summary>
    /// Адрес склада
    /// </summary>
    public required  string Address { get; set; }
    
    /// <summary>
    /// Долгота
    /// </summary>
    public required  string Lon  { get; set; }
    
    /// <summary>
    /// Широта
    /// </summary>
    public required  string Lat { get; set; }
}