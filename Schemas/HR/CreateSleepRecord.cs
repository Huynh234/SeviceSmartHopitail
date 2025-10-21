using System.ComponentModel.DataAnnotations;

namespace SeviceSmartHopitail.Schemas.HR
{
    public class CreateSleepRecord
    {
        public int UserProfileId { get; set; }

        [Required]
        public decimal HoursSleep { get; set; } // hours

        public string? Note { get; set; }
    }
}
