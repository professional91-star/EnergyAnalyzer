using System.Net.Sockets;
using EnergyAnalyzer.Models;
using NModbus;

namespace EnergyAnalyzer.Services
{
    public interface IHasanoglanBelardiModbusService
    {
        Task<EnergyData> ReadEnergyDataAsync();
        Task<bool> WriteCoilAsync(ushort address, bool value);
        bool IsConnected { get; }
    }

    public class HasanoglanBelardiModbusService : IHasanoglanBelardiModbusService, IDisposable
    {
        private readonly HasanoglanBelardiSettings _settings;
        private readonly ILogger<HasanoglanBelardiModbusService> _logger;
        private TcpClient? _tcpClient;
        private IModbusMaster? _master;
        private readonly object _lock = new();

        public bool IsConnected => _tcpClient?.Connected ?? false;

        public HasanoglanBelardiModbusService(ILogger<HasanoglanBelardiModbusService> logger)
        {
            _logger = logger;
            _settings = new HasanoglanBelardiSettings();
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

                _logger.LogInformation("Hasanoğlan BELARDİ Modbus bağlantısı başarılı: {Ip}:{Port}", _settings.IpAddress, _settings.Port);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Hasanoğlan BELARDİ Modbus bağlantı hatası");
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

                    var registers = _master!.ReadHoldingRegisters(_settings.SlaveId, 10, 30);

                    data.Voltage_AB = GetFloat32(registers, 0);
                    data.Voltage_BC = GetFloat32(registers, 2);
                    data.Voltage_CA = GetFloat32(registers, 4);
                    
                    data.Current_A = GetFloat32(registers, 6);
                    data.Current_B = GetFloat32(registers, 8);
                    data.Current_C = GetFloat32(registers, 10);
                    
                    data.ActivePower = GetFloat32(registers, 12);
                    data.ReactivePower = GetFloat32(registers, 14);
                    data.ApparentPower = GetFloat32(registers, 16);
                    
                    data.PowerFactorDirect = GetFloat32(registers, 18);
                    data.Frequency = GetFloat32(registers, 20);
                    
                    data.ActiveEnergyImport = GetFloat32(registers, 22);
                    data.ActiveEnergyExport = GetFloat32(registers, 24);
                    data.ReactiveEnergyImport = GetFloat32(registers, 26);
                    data.ReactiveEnergyExport = GetFloat32(registers, 28);

                    data.IsConnected = true;
                    data.Timestamp = DateTime.Now;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Hasanoğlan BELARDİ Modbus okuma hatası");
                data.IsConnected = false;
                data.ErrorMessage = ex.Message;
                
                _tcpClient?.Dispose();
                _tcpClient = null;
            }

            return data;
        }

        private static float GetFloat32(ushort[] registers, int startIndex)
        {
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
                        _logger.LogError("Hasanoğlan BELARDİ Coil yazma hatası: Bağlantı yok");
                        return false;
                    }

                    _master!.WriteSingleCoil(_settings.SlaveId, address, value);
                    _logger.LogInformation("Hasanoğlan BELARDİ Coil yazıldı: Address={Address}, Value={Value}", address, value);
                    return true;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Hasanoğlan BELARDİ Coil yazma hatası: Address={Address}", address);
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
