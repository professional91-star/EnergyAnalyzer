using System.Text.Json.Serialization;

namespace EnergyAnalyzer.Models
{
    public class EnergyData
    {
        public DateTime Timestamp { get; set; } = DateTime.Now;
        
        // Hat Gerilimleri (V) - H2_Vab, H2_Vbc, H2_Vca
        [JsonPropertyName("vab")]
        public float Voltage_AB { get; set; }
        
        [JsonPropertyName("vbc")]
        public float Voltage_BC { get; set; }
        
        [JsonPropertyName("vca")]
        public float Voltage_CA { get; set; }
        
        // Faz Akımları (A) - H2_Ia, H2_Ib, H2_Ic
        [JsonPropertyName("ia")]
        public float Current_A { get; set; }
        
        [JsonPropertyName("ib")]
        public float Current_B { get; set; }
        
        [JsonPropertyName("ic")]
        public float Current_C { get; set; }
        
        // Güç Değerleri
        [JsonPropertyName("activePower")]
        public float ActivePower { get; set; }      // kW - H2_P
        
        [JsonPropertyName("apparentPower")]
        public float ApparentPower { get; set; }    // kVA - H2_S
        
        [JsonPropertyName("reactivePower")]
        public float ReactivePower { get; set; }    // kVAR - H2_Q
        
        // Enerji Sayaçları (Aktif)
        [JsonPropertyName("wpPlus")]
        public float ActiveEnergyImport { get; set; }   // kWh - H2_WPplus
        
        [JsonPropertyName("wpMinus")]
        public float ActiveEnergyExport { get; set; }   // kWh - H2_WPminus
        
        // Enerji Sayaçları (Reaktif)
        [JsonPropertyName("wqPlus")]
        public float ReactiveEnergyImport { get; set; } // kVARh - H2_WQplus
        
        [JsonPropertyName("wqMinus")]
        public float ReactiveEnergyExport { get; set; } // kVARh - H2_WQminus
        
        // Hesaplanan Değerler
        [JsonPropertyName("powerFactor")]
        public float PowerFactor => ApparentPower > 0 ? ActivePower / ApparentPower : 0;
        
        public float TotalActivePower => ActivePower;
        public float NetEnergy => ActiveEnergyExport - ActiveEnergyImport;
        
        // Bağlantı Durumu
        public bool IsConnected { get; set; }
        public string? ErrorMessage { get; set; }
    }
}
