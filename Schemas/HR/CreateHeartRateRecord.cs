using System.ComponentModel.DataAnnotations;

namespace SeviceSmartHopitail.Schemas.HR
{
    public class CreateHeartRateRecord
    {
        public int UserProfileId { get; set; }

        [Required]
        public int HeartRate { get; set; } // bpm

        public string? Note { get; set; }
    }
}
