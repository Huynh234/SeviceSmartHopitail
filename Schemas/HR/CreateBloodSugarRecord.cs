using System.ComponentModel.DataAnnotations;

namespace SeviceSmartHopitail.Schemas.HR
{
    public class CreateBloodSugarRecord
    {
        public int UserProfileId { get; set; }

        [Required]
        public decimal BloodSugar { get; set; } // mg/dL

        public string? Note { get; set; }
    }
}
