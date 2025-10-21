using System.ComponentModel.DataAnnotations;

namespace SeviceSmartHopitail.Schemas.HR
{
    public class CreateBloodPressureRecord
    {
        public int UserProfileId { get; set; }

        [Required]
        public int Systolic { get; set; }   // mmHg

        [Required]
        public int Diastolic { get; set; }  // mmHg

        public string? Note { get; set; }
    }
}
