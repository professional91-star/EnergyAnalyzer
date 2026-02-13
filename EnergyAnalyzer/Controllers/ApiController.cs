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
        private readonly IHasanoglanModbusService _hasanoglanModbusService;
        private readonly IHasanoglanIsikYuvarModbusService _hasanoglanIsikYuvarModbusService;
        private readonly IHasanoglanIsikKureModbusService _hasanoglanIsikKureModbusService;
        private readonly IHasanoglanBelardiModbusService _hasanoglanBelardiModbusService;
        private readonly IHasanoglanIpsiModbusService _hasanoglanIpsiModbusService;
        private readonly IHamalModbusService _hamalModbusService;

        public ApiController(
            IModbusService modbusService, 
            IIsikYuvarModbusService isikYuvarModbusService, 
            IHasanoglanModbusService hasanoglanModbusService,
            IHasanoglanIsikYuvarModbusService hasanoglanIsikYuvarModbusService,
            IHasanoglanIsikKureModbusService hasanoglanIsikKureModbusService,
            IHasanoglanBelardiModbusService hasanoglanBelardiModbusService,
            IHasanoglanIpsiModbusService hasanoglanIpsiModbusService,
            IHamalModbusService hamalModbusService)
        {
            _modbusService = modbusService;
            _isikYuvarModbusService = isikYuvarModbusService;
            _hasanoglanModbusService = hasanoglanModbusService;
            _hasanoglanIsikYuvarModbusService = hasanoglanIsikYuvarModbusService;
            _hasanoglanIsikKureModbusService = hasanoglanIsikKureModbusService;
            _hasanoglanBelardiModbusService = hasanoglanBelardiModbusService;
            _hasanoglanIpsiModbusService = hasanoglanIpsiModbusService;
            _hamalModbusService = hamalModbusService;
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

        // HASANOGLAN Kontrol Bitleri
        // Coil 0: AG şalter aç
        // Coil 1: AG şalter kapat
        // Coil 2: OG kesici aç
        // Coil 3: OG kesici kapat

        [HttpPost("hasanoglan/control")]
        public async Task<IActionResult> HasanoglanControl([FromBody] ControlRequest request)
        {
            if (request.Address > 3)
            {
                return BadRequest(new { success = false, message = "Geçersiz adres" });
            }

            var result = await _hasanoglanModbusService.WriteCoilAsync(request.Address, request.Value);
            
            if (result)
            {
                return Ok(new { success = true, message = GetHasanoglanControlMessage(request.Address, request.Value) });
            }
            
            return StatusCode(500, new { success = false, message = "Kontrol komutu gönderilemedi" });
        }

        private static string GetHasanoglanControlMessage(ushort address, bool value)
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

        // HASANOGLAN IŞIKYUVAR Kontrol
        [HttpPost("hasanoglan-isikyuvar/control")]
        public async Task<IActionResult> HasanoglanIsikYuvarControl([FromBody] ControlRequest request)
        {
            if (request.Address > 3)
            {
                return BadRequest(new { success = false, message = "Geçersiz adres" });
            }

            var result = await _hasanoglanIsikYuvarModbusService.WriteCoilAsync(request.Address, request.Value);
            
            if (result)
            {
                return Ok(new { success = true, message = GetHasanoglanControlMessage(request.Address, request.Value) });
            }
            
            return StatusCode(500, new { success = false, message = "Kontrol komutu gönderilemedi" });
        }

        // HASANOGLAN IŞIKKURE Kontrol
        [HttpPost("hasanoglan-isikkure/control")]
        public async Task<IActionResult> HasanoglanIsikKureControl([FromBody] ControlRequest request)
        {
            if (request.Address > 3)
            {
                return BadRequest(new { success = false, message = "Geçersiz adres" });
            }

            var result = await _hasanoglanIsikKureModbusService.WriteCoilAsync(request.Address, request.Value);
            
            if (result)
            {
                return Ok(new { success = true, message = GetHasanoglanControlMessage(request.Address, request.Value) });
            }
            
            return StatusCode(500, new { success = false, message = "Kontrol komutu gönderilemedi" });
        }

        // HASANOGLAN BELARDİ Kontrol
        [HttpPost("hasanoglan-belardi/control")]
        public async Task<IActionResult> HasanoglanBelardiControl([FromBody] ControlRequest request)
        {
            if (request.Address > 3)
            {
                return BadRequest(new { success = false, message = "Geçersiz adres" });
            }

            var result = await _hasanoglanBelardiModbusService.WriteCoilAsync(request.Address, request.Value);
            
            if (result)
            {
                return Ok(new { success = true, message = GetHasanoglanControlMessage(request.Address, request.Value) });
            }
            
            return StatusCode(500, new { success = false, message = "Kontrol komutu gönderilemedi" });
        }

        // HASANOGLAN İPSİ Kontrol
        [HttpPost("hasanoglan-ipsi/control")]
        public async Task<IActionResult> HasanoglanIpsiControl([FromBody] ControlRequest request)
        {
            if (request.Address > 3)
            {
                return BadRequest(new { success = false, message = "Geçersiz adres" });
            }

            var result = await _hasanoglanIpsiModbusService.WriteCoilAsync(request.Address, request.Value);
            
            if (result)
            {
                return Ok(new { success = true, message = GetHasanoglanControlMessage(request.Address, request.Value) });
            }
            
            return StatusCode(500, new { success = false, message = "Kontrol komutu gönderilemedi" });
        }

        // HAMAL Kontrol
        [HttpPost("hamal/control")]
        public async Task<IActionResult> HamalControl([FromBody] ControlRequest request)
        {
            if (request.Address > 3)
            {
                return BadRequest(new { success = false, message = "Geçersiz adres" });
            }

            var result = await _hamalModbusService.WriteCoilAsync(request.Address, request.Value);
            
            if (result)
            {
                return Ok(new { success = true, message = GetHamalControlMessage(request.Address, request.Value) });
            }
            
            return StatusCode(500, new { success = false, message = "Kontrol komutu gönderilemedi" });
        }

        private static string GetHamalControlMessage(ushort address, bool value)
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
