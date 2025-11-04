using System.ComponentModel.DataAnnotations;

namespace SeviceSmartHopitail.Schemas.HR
{
    public class CreateSleepRecord
    {
        public int UserProfileId { get; set; }

        [Required]
        public string TimeSleep { get; set; } = "";// hours
        public string TimeWake { get; set; } = "";

        public string? Note { get; set; }
    }
}
