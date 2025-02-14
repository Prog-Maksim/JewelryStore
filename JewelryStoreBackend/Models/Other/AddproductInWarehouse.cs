namespace JewelryStoreBackend.Models.Other;

public class AddproductInWarehouse
{
    public string SKU { get; set; }
    public int WarehouseId { get; set; }
    public int Count { get; set; }
}