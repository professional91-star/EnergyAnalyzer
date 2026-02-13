namespace EnergyAnalyzer.Models
{
    public class HamalSettings
    {
        public string IpAddress { get; set; } = "5.26.245.211";
        public int Port { get; set; } = 502;
        public byte SlaveId { get; set; } = 1;
        public int PollingIntervalMs { get; set; } = 1000;
        
        // Register Addresses (Modbus Holding Register - subtract 40001 from document address)
        // Currents - Module 16 (UINT16, Scale /10)
        public ushort IaAddress { get; set; } = 149;      // 40150
        public ushort IbAddress { get; set; } = 150;      // 40151
        public ushort IcAddress { get; set; } = 151;      // 40152
        public ushort FreqAddress { get; set; } = 158;    // 40159
        
        // Voltages - Module 17 (Read 2 registers but use second one - UINT16/100)
        public ushort VabAddress { get; set; } = 177;     // 40178
        public ushort VbcAddress { get; set; } = 179;     // 40180
        public ushort VcaAddress { get; set; } = 181;     // 40182
        
        // Power - Module 18 (INT32)
        public ushort KwTotAddress { get; set; } = 203;   // 40204
        public ushort KvarTotAddress { get; set; } = 213; // 40214
        public ushort KvaTotAddress { get; set; } = 223;  // 40224
        
        // Energy - Module 19 (INT32)
        public ushort KwhDelAddress { get; set; } = 229;  // 40230
        public ushort KwhRecAddress { get; set; } = 231;  // 40232
        public ushort KvarhDelAddress { get; set; } = 233; // 40234
        public ushort KvarhRecAddress { get; set; } = 235; // 40236
        
        // Power Factor - Module 20 (INT16, Scale /1000)
        public ushort PfAddress { get; set; } = 264;      // 40265
    }
}
