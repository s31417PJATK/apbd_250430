using Tutorial9.Model;

namespace Tutorial9.Services;

public interface IWarehouseService
{
    Task<object> PostWarehouse(WarehouseDTO warehouseDTO);
    
    Task<object> PostWarehouseProc(WarehouseDTO warehouseDTO);
}