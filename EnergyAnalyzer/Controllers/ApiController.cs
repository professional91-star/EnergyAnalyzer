using EnergyAnalyzer.Services;
using Microsoft.AspNetCore.Mvc;

namespace EnergyAnalyzer.Controllers
{
    [Route("api")]
    [ApiController]
    public class ApiController : ControllerBase
    {
        private readonly IModbusService _modbusService;
        private readonly IIsikYuvarModbusService _isikYuvarModbusService;

        public ApiController(IModbusService modbusService, IIsikYuvarModbusService isikYuvarModbusService)
        {
            _modbusService = modbusService;
            _isikYuvarModbusService = isikYuvarModbusService;
        }

        [HttpGet("energy")]
        public async Task<IActionResult> GetEnergyData()
        {
            var data = await _modbusService.ReadEnergyDataAsync();
            return Ok(data);
        }

        // ISIKYUVAR Kontrol Bitleri
        // Coil 0: AG şalter aç
        // Coil 1: AG şalter kapat
        // Coil 2: OG kesici aç
        // Coil 3: OG kesici kapat

        [HttpPost("isikyuvar/control")]
        public async Task<IActionResult> IsikYuvarControl([FromBody] ControlRequest request)
        {
            if (request.Address > 3)
            {
                return BadRequest(new { success = false, message = "Geçersiz adres" });
            }

            var result = await _isikYuvarModbusService.WriteCoilAsync(request.Address, request.Value);
            
            if (result)
            {
                return Ok(new { success = true, message = GetControlMessage(request.Address, request.Value) });
            }
            
            return StatusCode(500, new { success = false, message = "Kontrol komutu gönderilemedi" });
        }

        private static string GetControlMessage(ushort address, bool value)
        {
            return address switch
            {
                0 => value ? "AG Şalter AÇ komutu gönderildi" : "AG Şalter AÇ komutu kaldırıldı",
                1 => value ? "AG Şalter KAPAT komutu gönderildi" : "AG Şalter KAPAT komutu kaldırıldı",
                2 => value ? "OG Kesici AÇ komutu gönderildi" : "OG Kesici AÇ komutu kaldırıldı",
                3 => value ? "OG Kesici KAPAT komutu gönderildi" : "OG Kesici KAPAT komutu kaldırıldı",
                _ => "Bilinmeyen komut"
            };
        }

        // ISIKKURE Kontrol Bitleri
        // Coil 0: AG şalter aç
        // Coil 1: AG şalter kapat
        // Coil 2: OG kesici aç
        // Coil 3: OG kesici kapat

        [HttpPost("isikkure/control")]
        public async Task<IActionResult> IsikKureControl([FromBody] ControlRequest request)
        {
            if (request.Address > 3)
            {
                return BadRequest(new { success = false, message = "Geçersiz adres" });
            }

            var result = await _modbusService.WriteCoilAsync(request.Address, request.Value);
            
            if (result)
            {
                return Ok(new { success = true, message = GetIsikKureControlMessage(request.Address, request.Value) });
            }
            
            return StatusCode(500, new { success = false, message = "Kontrol komutu gönderilemedi" });
        }

        private static string GetIsikKureControlMessage(ushort address, bool value)
        {
            return address switch
            {
                0 => value ? "AG Şalter AÇ komutu gönderildi" : "AG Şalter AÇ komutu kaldırıldı",
                1 => value ? "AG Şalter KAPAT komutu gönderildi" : "AG Şalter KAPAT komutu kaldırıldı",
                2 => value ? "OG Kesici AÇ komutu gönderildi" : "OG Kesici AÇ komutu kaldırıldı",
                3 => value ? "OG Kesici KAPAT komutu gönderildi" : "OG Kesici KAPAT komutu kaldırıldı",
                _ => "Bilinmeyen komut"
            };
        }
    }

    public class ControlRequest
    {
        public ushort Address { get; set; }
        public bool Value { get; set; }
    }
}
