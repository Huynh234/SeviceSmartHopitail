namespace SeviceSmartHopitail.Schemas
{
    public class UpdateHealthRecord
    {
        public int HeartRate { get; set; } // bpm

        public decimal? BloodSugar { get; set; } // mg/dL

        public int? Systolic { get; set; }   // mmHg
        public int? Diastolic { get; set; }  // mmHg

        public decimal? TimeSleep { get; set; } // hours

        public string? Note { get; set; }
    }
}
