namespace EnergyAnalyzer.Models
{
    public class ModbusSettings
    {
        public string IpAddress { get; set; } = "5.11.240.244";
        public int Port { get; set; } = 1502;
        public byte SlaveId { get; set; } = 1;
        public int ReadIntervalMs { get; set; } = 1000;
    }
}
