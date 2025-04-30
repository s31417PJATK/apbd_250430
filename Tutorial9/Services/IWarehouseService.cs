using Tutorial9.Model;

namespace Tutorial9.Services;

public interface IWarehouseService
{
    Task<List<WarehouseDTO>> GetWarehouse();
}