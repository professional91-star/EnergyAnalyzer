using EnergyAnalyzer.Services;
using Microsoft.AspNetCore.Mvc;

namespace EnergyAnalyzer.Controllers
{
    [Route("api")]
    [ApiController]
    public class ApiController : ControllerBase
    {
        private readonly IModbusService _modbusService;

        public ApiController(IModbusService modbusService)
        {
            _modbusService = modbusService;
        }

        [HttpGet("energy")]
        public async Task<IActionResult> GetEnergyData()
        {
            var data = await _modbusService.ReadEnergyDataAsync();
            return Ok(data);
        }
    }
}
