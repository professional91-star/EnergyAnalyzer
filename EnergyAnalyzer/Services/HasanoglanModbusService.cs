using System.Net.Sockets;
using EnergyAnalyzer.Models;
using NModbus;

namespace EnergyAnalyzer.Services
{
    public interface IHasanoglanModbusService
    {
        Task<EnergyData> ReadEnergyDataAsync();
        Task<bool> WriteCoilAsync(ushort address, bool value);
        bool IsConnected { get; }
    }

    public class HasanoglanModbusService : IHasanoglanModbusService, IDisposable
    {
        private readonly HasanoglanSettings _settings;
        private readonly ILogger<HasanoglanModbusService> _logger;
        private TcpClient? _tcpClient;
        private IModbusMaster? _master;
        private readonly object _lock = new();

        public bool IsConnected => _tcpClient?.Connected ?? false;

        public HasanoglanModbusService(ILogger<HasanoglanModbusService> logger)
        {
            _logger = logger;
            _settings = new HasanoglanSettings();
        }

        private async Task<bool> EnsureConnectedAsync()
        {
            if (_tcpClient?.Connected == true)
                return true;

            try
            {
                _tcpClient?.Dispose();
                _tcpClient = new TcpClient();
                
                var connectTask = _tcpClient.ConnectAsync(_settings.IpAddress, _settings.Port);
                if (await Task.WhenAny(connectTask, Task.Delay(5000)) != connectTask)
                {
                    throw new TimeoutException("Modbus bağlantı zaman aşımı");
                }

                var factory = new ModbusFactory();
                _master = factory.CreateMaster(_tcpClient);
                _master.Transport.ReadTimeout = 3000;
                _master.Transport.WriteTimeout = 3000;

                _logger.LogInformation("Hasanoğlan Modbus bağlantısı başarılı: {Ip}:{Port}", _settings.IpAddress, _settings.Port);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Hasanoğlan Modbus bağlantı hatası");
                return false;
            }
        }

        public async Task<EnergyData> ReadEnergyDataAsync()
        {
            var data = new EnergyData();

            try
            {
                lock (_lock)
                {
                    if (!EnsureConnectedAsync().Result)
                    {
                        data.IsConnected = false;
                        data.ErrorMessage = "Cihaza bağlanılamadı";
                        return data;
                    }

                    // Registerleri oku (Address 10-38 arası = 30 register)
                    // H3_PQ_Vab=10, H3_PQ_Vbc=12, H3_PQ_Vca=14, H3_PQ_Ia=16, H3_PQ_Ib=18, H3_PQ_Ic=20
                    // H3_PQ_Active_Power=22, H3_PQ_reactive_power=24, H3_PQ_apparent_power=26
                    // H3_PQ_PF=28, H3_PQ_frequency=30
                    // H3_PQ_import_kWh=32, H3_PQ_export_kWh=34, H3_PQ_inductive_kVArh=36, H3_PQ_capacitive_kVArh=38
                    var registers = _master!.ReadHoldingRegisters(_settings.SlaveId, 10, 30);

                    // Float32 değerlerini dönüştür (offset 10'dan başladığı için index'leri ayarla)
                    // H3_PQ_Vab (Address 10) -> index 0
                    data.Voltage_AB = GetFloat32(registers, 0);
                    // H3_PQ_Vbc (Address 12) -> index 2
                    data.Voltage_BC = GetFloat32(registers, 2);
                    // H3_PQ_Vca (Address 14) -> index 4
                    data.Voltage_CA = GetFloat32(registers, 4);
                    
                    // H3_PQ_Ia (Address 16) -> index 6
                    data.Current_A = GetFloat32(registers, 6);
                    // H3_PQ_Ib (Address 18) -> index 8
                    data.Current_B = GetFloat32(registers, 8);
                    // H3_PQ_Ic (Address 20) -> index 10
                    data.Current_C = GetFloat32(registers, 10);
                    
                    // H3_PQ_Active_Power (Address 22) -> index 12
                    data.ActivePower = GetFloat32(registers, 12);
                    // H3_PQ_reactive_power (Address 24) -> index 14
                    data.ReactivePower = GetFloat32(registers, 14);
                    // H3_PQ_apparent_power (Address 26) -> index 16
                    data.ApparentPower = GetFloat32(registers, 16);
                    
                    // H3_PQ_PF (Address 28) -> index 18
                    data.PowerFactorDirect = GetFloat32(registers, 18);
                    // H3_PQ_frequency (Address 30) -> index 20
                    data.Frequency = GetFloat32(registers, 20);
                    
                    // H3_PQ_import_kWh (Address 32) -> index 22
                    data.ActiveEnergyImport = GetFloat32(registers, 22);
                    // H3_PQ_export_kWh (Address 34) -> index 24
                    data.ActiveEnergyExport = GetFloat32(registers, 24);
                    // H3_PQ_inductive_kVArh (Address 36) -> index 26
                    data.ReactiveEnergyImport = GetFloat32(registers, 26);
                    // H3_PQ_capacitive_kVArh (Address 38) -> index 28
                    data.ReactiveEnergyExport = GetFloat32(registers, 28);

                    data.IsConnected = true;
                    data.Timestamp = DateTime.Now;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Hasanoğlan Modbus okuma hatası");
                data.IsConnected = false;
                data.ErrorMessage = ex.Message;
                
                // Bağlantıyı sıfırla
                _tcpClient?.Dispose();
                _tcpClient = null;
            }

            return data;
        }

        private static float GetFloat32(ushort[] registers, int startIndex)
        {
            // Big Endian Float32: High word first
            var bytes = new byte[4];
            bytes[0] = (byte)(registers[startIndex + 1] & 0xFF);
            bytes[1] = (byte)(registers[startIndex + 1] >> 8);
            bytes[2] = (byte)(registers[startIndex] & 0xFF);
            bytes[3] = (byte)(registers[startIndex] >> 8);
            
            return BitConverter.ToSingle(bytes, 0);
        }

        public async Task<bool> WriteCoilAsync(ushort address, bool value)
        {
            try
            {
                lock (_lock)
                {
                    if (!EnsureConnectedAsync().Result)
                    {
                        _logger.LogError("Hasanoğlan Coil yazma hatası: Bağlantı yok");
                        return false;
                    }

                    _master!.WriteSingleCoil(_settings.SlaveId, address, value);
                    _logger.LogInformation("Hasanoğlan Coil yazıldı: Address={Address}, Value={Value}", address, value);
                    return true;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Hasanoğlan Coil yazma hatası: Address={Address}", address);
                _tcpClient?.Dispose();
                _tcpClient = null;
                return false;
            }
        }

        public void Dispose()
        {
            _tcpClient?.Dispose();
        }
    }
}
