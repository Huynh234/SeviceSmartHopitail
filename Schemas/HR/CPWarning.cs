namespace SeviceSmartHopitail.Schemas.HR
{
    public class CPWarning
    {
        public int? UserProfileId { get; set; }
        public int? MinHeartRate { get; set; }
        public int? MaxHeartRate { get; set; }

        public decimal? MinBloodSugar { get; set; }
        public decimal? MaxBloodSugar { get; set; }

        public int? MinSystolic { get; set; }
        public int? MaxSystolic { get; set; }
        public int? MinDiastolic { get; set; }
        public int? MaxDiastolic { get; set; }

        public decimal? MinSleep { get; set; }
        public decimal? MaxSleep { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}
