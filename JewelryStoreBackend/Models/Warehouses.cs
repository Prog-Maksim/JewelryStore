namespace JewelryStoreBackend.Models;

public class Warehouses
{
    public int Id { get; set; }
    public int WarehouseId { get; set; }
    public required  string Title { get; set; }
    public required  string Address { get; set; }
    public required  string Lon  { get; set; }
    public required  string Lat { get; set; }
}