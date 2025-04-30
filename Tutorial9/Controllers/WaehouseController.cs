using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
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

        [HttpGet]
        public async Task<IActionResult> GetTrips()
        {
            var trips = await _warehouseService.GetTrips();
            return Ok(trips);
        }
    }
}
