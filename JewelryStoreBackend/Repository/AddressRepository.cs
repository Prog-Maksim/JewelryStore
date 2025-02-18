using JewelryStoreBackend.Models.DB;
using JewelryStoreBackend.Models.DB.User;
using JewelryStoreBackend.Repository.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace JewelryStoreBackend.Repository;

public class AddressRepository: IAddressRepository
{
    private readonly ApplicationContext _context;

    public AddressRepository(ApplicationContext context)
    {
        _context = context;
    }


    public async Task<Address?> GetAddressByIdAsync(string userId, string addressId)
    {
        var address = await _context.Address.FirstOrDefaultAsync(a => a.PersonId == userId && a.AddressId == addressId);
        return address;
    }

    public async Task<Warehouses?> GetWarehouseByIdAsync(string warehouseId)
    {
        var warehousesAddress = await _context.Warehouses.ToListAsync();
        var warehouseAddress = warehousesAddress.First();
        
        return warehouseAddress;
    }
}