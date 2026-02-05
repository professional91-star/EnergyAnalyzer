using System.Net.Sockets;
using EnergyAnalyzer.Models;
using NModbus;

namespace EnergyAnalyzer.Services
{
    public interface IIsikYuvarModbusService
    {
        Task<EnergyData> ReadEnergyDataAsync();
        bool IsConnected { get; }
    }

    public class IsikYuvarModbusService : IIsikYuvarModbusService, IDisposable
    {
        private readonly IsikYuvarSettings _settings;
        private readonly ILogger<IsikYuvarModbusService> _logger;
        private TcpClient? _tcpClient;
        private IModbusMaster? _master;
        private readonly object _lock = new();

        public bool IsConnected => _tcpClient?.Connected ?? false;

        public IsikYuvarModbusService(ILogger<IsikYuvarModbusService> logger)
        {
            _logger = logger;
            _settings = new IsikYuvarSettings();
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

                _logger.LogInformation("ISIKYUVAR Modbus bağlantısı başarılı: {Ip}:{Port}", _settings.IpAddress, _settings.Port);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "ISIKYUVAR Modbus bağlantı hatası");
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

                    // Registerleri oku (Address 2-33 arası = 32 register)
                    var registers = _master!.ReadHoldingRegisters(_settings.SlaveId, 2, 32);

                    // Float32 değerlerini dönüştür
                    data.Voltage_AB = GetFloat32(registers, 0);
                    data.Voltage_BC = GetFloat32(registers, 2);
                    data.Voltage_CA = GetFloat32(registers, 10);
                    
                    data.Current_A = GetFloat32(registers, 6);
                    data.Current_B = GetFloat32(registers, 8);
                    data.Current_C = GetFloat32(registers, 4);
                    
                    data.ActivePower = GetFloat32(registers, 16);
                    data.ApparentPower = GetFloat32(registers, 20);
                    data.ReactivePower = GetFloat32(registers, 12);
                    
                    // Enerji değerleri (yeni adresler)
                    // AG_import_kWh: 26, AG_export_kWh: 28, AG_inductive_kVArh: 30, AG_capacitive_kVArh: 32
                    data.ActiveEnergyImport = GetFloat32(registers, 24);    // Address 26 -> index 24
                    data.ActiveEnergyExport = GetFloat32(registers, 26);    // Address 28 -> index 26
                    data.ReactiveEnergyImport = GetFloat32(registers, 28);  // Address 30 -> index 28
                    data.ReactiveEnergyExport = GetFloat32(registers, 30);  // Address 32 -> index 30

                    data.IsConnected = true;
                    data.Timestamp = DateTime.Now;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "ISIKYUVAR Modbus okuma hatası");
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

        public void Dispose()
        {
            _tcpClient?.Dispose();
        }
    }
}
