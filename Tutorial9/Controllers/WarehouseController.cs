using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Tutorial9.Model;
using Tutorial9.Services;

namespace Tutorial9.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class WarehouseController : ControllerBase
    {
        private readonly IWarehouseService _warehouseService;

        public WarehouseController(IWarehouseService warehouseService)
        {
            _warehouseService = warehouseService;
        }

        [HttpPost]
        public async Task<IActionResult> PostWarehouse([FromBody] WarehouseDTO warehouseDTO)
        {
            var res = await _warehouseService.PostWarehouse(warehouseDTO);
            if (res is int) return Ok((int)res);
            res = (String)res;
            if (res == "Product not found") return BadRequest("Product not found");
            if (res == "Warehouse not found") return BadRequest("Warehouse not found");
            if (res == "Wrong Amount") return BadRequest("Amount needs to be greater than 0");
            if (res == "Order not found") return NotFound("Order not found");
            if (res == "Transaction failed") return Conflict("Transaction failed");
            return Problem();
        }

        [HttpPost("proc")]
        public async Task<IActionResult> PostWarehouse2([FromBody] WarehouseDTO warehouseDTO)
        {
            var res = await _warehouseService.PostWarehouseProc(warehouseDTO);
            if (res is not String) return Ok(Convert.ToInt32(res));
            res = (String)res;
            if (res == "Procedure failed") return Conflict("Procedure failed");
            return Problem();
        }
    }
}
