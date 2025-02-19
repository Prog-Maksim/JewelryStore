using System.ComponentModel.DataAnnotations;

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
    [MaxLength(255)]
    public required string Title { get; set; }
    
    /// <summary>
    /// Адрес склада
    /// </summary>
    [MaxLength(255)]
    public required string Address { get; set; }
    
    /// <summary>
    /// Долгота
    /// </summary>
    [MaxLength(100)]
    public required string Lon  { get; set; }
    
    /// <summary>
    /// Широта
    /// </summary>
    [MaxLength(100)]
    public required string Lat { get; set; }
}