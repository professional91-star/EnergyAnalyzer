using EnergyAnalyzer.Models;
using NModbus;
using System.Net.Sockets;

namespace EnergyAnalyzer.Services
{
    public interface IHamalModbusService
    {
        Task<EnergyData> ReadEnergyDataAsync();
        Task<bool> WriteCoilAsync(ushort address, bool value);
    }

    public class HamalModbusService : IHamalModbusService
    {
        private readonly HamalSettings _settings;
        private readonly ILogger<HamalModbusService> _logger;

        public HamalModbusService(ILogger<HamalModbusService> logger)
        {
            _settings = new HamalSettings();
            _logger = logger;
        }

        public async Task<EnergyData> ReadEnergyDataAsync()
        {
            var data = new EnergyData();

            try
            {
                using var client = new TcpClient();
                await client.ConnectAsync(_settings.IpAddress, _settings.Port);
                var factory = new ModbusFactory();
                var master = factory.CreateMaster(client);
                master.Transport.ReadTimeout = 3000;
                master.Transport.WriteTimeout = 3000;

                // Read Currents (UINT16, /10) - Module 16
                var iaRegs = await master.ReadHoldingRegistersAsync(_settings.SlaveId, _settings.IaAddress, 1);
                var ibRegs = await master.ReadHoldingRegistersAsync(_settings.SlaveId, _settings.IbAddress, 1);
                var icRegs = await master.ReadHoldingRegistersAsync(_settings.SlaveId, _settings.IcAddress, 1);
                
                // Frequency (UINT16, /10)
                var freqRegs = await master.ReadHoldingRegistersAsync(_settings.SlaveId, _settings.FreqAddress, 1);

                // Read Voltages (UINT32, /10) - Module 17
                var vabRegs = await master.ReadHoldingRegistersAsync(_settings.SlaveId, _settings.VabAddress, 2);
                var vbcRegs = await master.ReadHoldingRegistersAsync(_settings.SlaveId, _settings.VbcAddress, 2);
                var vcaRegs = await master.ReadHoldingRegistersAsync(_settings.SlaveId, _settings.VcaAddress, 2);

                // Read Power (INT32, /1000) - Module 18
                var kwRegs = await master.ReadHoldingRegistersAsync(_settings.SlaveId, _settings.KwTotAddress, 2);
                var kvarRegs = await master.ReadHoldingRegistersAsync(_settings.SlaveId, _settings.KvarTotAddress, 2);
                var kvaRegs = await master.ReadHoldingRegistersAsync(_settings.SlaveId, _settings.KvaTotAddress, 2);

                // Read Energy (INT32, /1000) - Module 19
                var kwhDelRegs = await master.ReadHoldingRegistersAsync(_settings.SlaveId, _settings.KwhDelAddress, 2);
                var kwhRecRegs = await master.ReadHoldingRegistersAsync(_settings.SlaveId, _settings.KwhRecAddress, 2);
                var kvarhDelRegs = await master.ReadHoldingRegistersAsync(_settings.SlaveId, _settings.KvarhDelAddress, 2);
                var kvarhRecRegs = await master.ReadHoldingRegistersAsync(_settings.SlaveId, _settings.KvarhRecAddress, 2);

                // Read Power Factor (INT16, /100) - Module 20
                var pfRegs = await master.ReadHoldingRegistersAsync(_settings.SlaveId, _settings.PfAddress, 1);

                // Parse values
                // Currents - UINT16, scale /10
                data.Current_A = iaRegs[0] / 10.0f;
                data.Current_B = ibRegs[0] / 10.0f;
                data.Current_C = icRegs[0] / 10.0f;

                // Frequency - UINT16, scale /10
                data.Frequency = freqRegs[0] / 10.0f;

                // Voltages - Use second register (low word contains the value), no scale
                // Raw data shows [0, 31763] pattern - actual voltage is in registers[1]
                data.Voltage_AB = vabRegs[1];
                data.Voltage_BC = vbcRegs[1];
                data.Voltage_CA = vcaRegs[1];

                // Power - Use second register (low word), no scaling (already in kW)
                // Raw data shows [0, 18] pattern
                data.ActivePower = (short)kwRegs[1] / 1.0f;  // kW direct
                data.ReactivePower = ToInt32(kvarRegs) / 1.0f;  // kVAr INT32 signed, no scale
                data.ApparentPower = kvaRegs[1] / 1.0f;  // kVA direct

                // Energy - INT32, use full 32-bit value (high word might be non-zero for large values)
                data.ActiveEnergyImport = ToInt32(kwhDelRegs) / 1.0f;  // kWh
                data.ActiveEnergyExport = ToInt32(kwhRecRegs) / 1.0f;
                data.ReactiveEnergyImport = ToInt32(kvarhDelRegs) / 1.0f;  // kVArh
                data.ReactiveEnergyExport = ToInt32(kvarhRecRegs) / 1.0f;

                // Power Factor - INT16, value is percentage * 100 (1216 = 12.16%)
                // Store as percentage (12.16)
                data.PowerFactorDirect = (short)pfRegs[0] / 100.0f;

                data.Timestamp = DateTime.Now;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "HAMAL Modbus veri okuma hatası");
            }

            return data;
        }

        // ION 7650 uses Big Endian word order (High word first)
        private static uint ToUInt32(ushort[] registers)
        {
            return ((uint)registers[0] << 16) | registers[1];
        }

        private static int ToInt32(ushort[] registers)
        {
            return (int)(((uint)registers[0] << 16) | registers[1]);
        }

        public async Task<bool> WriteCoilAsync(ushort address, bool value)
        {
            try
            {
                using var client = new TcpClient();
                await client.ConnectAsync(_settings.IpAddress, _settings.Port);
                var factory = new ModbusFactory();
                var master = factory.CreateMaster(client);
                master.Transport.ReadTimeout = 3000;
                master.Transport.WriteTimeout = 3000;

                await master.WriteSingleCoilAsync(_settings.SlaveId, address, value);
                
                _logger.LogInformation($"HAMAL Coil yazıldı - Adres: {address}, Değer: {value}");
                
                // Pulse için 500ms bekle ve sıfırla
                await Task.Delay(500);
                await master.WriteSingleCoilAsync(_settings.SlaveId, address, false);
                
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"HAMAL Coil yazma hatası - Adres: {address}");
                return false;
            }
        }
    }
}
