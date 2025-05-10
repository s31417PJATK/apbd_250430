using System.Data;
using System.Data.Common;
using Microsoft.Data.SqlClient;
using Tutorial9.Model;

namespace Tutorial9.Services;

public class WarehouseService : IWarehouseService
{
    private readonly IConfiguration _configuration;
    public WarehouseService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public async Task<object> PostWarehouse(WarehouseDTO warehouseDTO)
    {
        string command1 = "Select Price from Product where IdProduct = @idProduct";
        float resCom1;
        string command2 = "Select count(*) from Warehouse where IdWarehouse = @idWarehouse";
        string command3 = "SELECT TOP 1 o.IdOrder  FROM [Order] o LEFT JOIN Product_Warehouse pw ON o.IdOrder=pw.IdOrder WHERE o.IdProduct=@idProduct AND o.Amount=@amount AND pw.IdProductWarehouse IS NULL AND o.CreatedAt<@createdAt";
        int? resCom3 = null;
        string command4 = "Update [Order] set FulfilledAt = @fulfilledAt where IdOrder = @idOrder";
        string command5 = "Insert into Product_Warehouse Values(@idWarehouse, @idProduct, @idOrder, @amount, @price, @createdAt); Select SCOPE_IDENTITY()";
        int resCom5;
        
        using (SqlConnection conn = new SqlConnection(_configuration.GetConnectionString("Default")))
        {
            await conn.OpenAsync();
            using (SqlCommand cmd1 = new SqlCommand(command1, conn))
            {
                cmd1.Parameters.AddWithValue("@idProduct", warehouseDTO.IdProduct);
                var res = await cmd1.ExecuteScalarAsync();
                if (res is null) return "Product not found";
                resCom1 = (float)Convert.ToDecimal(res);
            }

            using (SqlCommand cmd2 = new SqlCommand(command2, conn))
            {
                cmd2.Parameters.AddWithValue("@idWarehouse", warehouseDTO.IdWarehouse);
                var res = (int) await cmd2.ExecuteScalarAsync();
                if (res == 0) return "Warehouse not found";
            }
            
            if (warehouseDTO.Amount <= 0) return "Wrong Amount";

            using (SqlCommand cmd3 = new SqlCommand(command3, conn))
            {
                cmd3.Parameters.AddWithValue("@idProduct", warehouseDTO.IdProduct);
                cmd3.Parameters.AddWithValue("@amount", warehouseDTO.Amount);
                cmd3.Parameters.AddWithValue("@createdAt", warehouseDTO.CreatedAt);
                
                resCom3 = (int?)await cmd3.ExecuteScalarAsync();
                if (resCom3 == null) return "Order not found";
            }

            DbTransaction transaction = await conn.BeginTransactionAsync();

            try
            {
                using (SqlCommand cmd4 = new SqlCommand(command4, conn))
                {
                    cmd4.Transaction = transaction as SqlTransaction;
                    cmd4.Parameters.AddWithValue("@fulfilledAt", DateTime.Now);
                    cmd4.Parameters.AddWithValue("@idOrder", resCom3);
                    
                    await cmd4.ExecuteNonQueryAsync();
                }

                using (SqlCommand cmd5 = new SqlCommand(command5, conn))
                {
                    cmd5.Transaction = transaction as SqlTransaction;
                    cmd5.Parameters.AddWithValue("@idWarehouse", warehouseDTO.IdWarehouse);
                    cmd5.Parameters.AddWithValue("@idProduct", warehouseDTO.IdProduct);
                    cmd5.Parameters.AddWithValue("@idOrder", resCom3);
                    cmd5.Parameters.AddWithValue("@amount", warehouseDTO.Amount);
                    float? price = warehouseDTO.Amount*resCom1;
                    cmd5.Parameters.AddWithValue("@price", price);
                    cmd5.Parameters.AddWithValue("@createdAt", warehouseDTO.CreatedAt);
                    
                    resCom5 = await cmd5.ExecuteNonQueryAsync();
                }
                
                await transaction.CommitAsync();
            }
            catch (Exception e)
            {
                await transaction.RollbackAsync();
                return "Transaction failed";
            }
            
        }

        return resCom5;
    }

    public async Task<object> PostWarehouseProc(WarehouseDTO warehouseDTO)
    {
        string command = "EXEC AddProductToWarehouse @idProduct, @idWarehouse, @amount, @createdAt";

        using (SqlConnection conn = new SqlConnection(_configuration.GetConnectionString("Default")))
        {
            await conn.OpenAsync();
            using (SqlCommand cmd = new SqlCommand(command,conn))
            {
                cmd.Parameters.AddWithValue("@idProduct", warehouseDTO.IdProduct);
                cmd.Parameters.AddWithValue("@idWarehouse", warehouseDTO.IdWarehouse);
                cmd.Parameters.AddWithValue("@amount", warehouseDTO.Amount);
                cmd.Parameters.AddWithValue("@createdAt", warehouseDTO.CreatedAt);
                
                var res = await cmd.ExecuteScalarAsync();
                if (res == null) return "Procedure failed";
                return res;
            }
        }
    }
}