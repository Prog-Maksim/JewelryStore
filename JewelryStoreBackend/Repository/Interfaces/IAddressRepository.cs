using JewelryStoreBackend.Models.DB;
using JewelryStoreBackend.Models.DB.User;

namespace JewelryStoreBackend.Repository.Interfaces;

public interface IAddressRepository
{
    /// <summary>
    /// Возвращает информацию об адресе пользователя
    /// </summary>
    /// <param name="userId">Идентификатор пользователя</param>
    /// <param name="addressId">Идентификатор адреса</param>
    /// <returns></returns>
    Task<Address?> GetAddressByIdAsync(string userId, string addressId);
    
    /// <summary>
    /// Возвращает информацию об адресах складов
    /// </summary>
    /// <param name="warehouseId">Идентификатор адреса</param>
    /// <returns></returns>
    Task<Warehouses?> GetWarehouseByIdAsync(string warehouseId);
}