namespace EnergyAnalyzer.Models
{
    public class HasanoglanBelardiSettings
    {
        public string IpAddress { get; set; } = "5.26.159.175";
        public int Port { get; set; } = 1502;
        public byte SlaveId { get; set; } = 1;
        public int ReadIntervalMs { get; set; } = 1000;
    }
}
