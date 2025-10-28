using SeviceSmartHopitail.Models.Infomation;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SeviceSmartHopitail.Models.Health
{
    public class BloodPressureRecord
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        public int UserProfileId { get; set; }

        [ForeignKey(nameof(UserProfileId))]
        public UserProfile UserProfile { get; set; } = null!;

        [Required]
        public int Systolic { get; set; }

        [Required]
        public int Diastolic { get; set; }

        public DateTime RecordedAt { get; set; } = DateTime.UtcNow;

        public string? Note { get; set; }

        [NotMapped]
        public string BloodPressureAlert
        {
            get
            {
                var pri = UserProfile?.PriWarning;
                int minSys = pri?.MinSystolic ?? 90;
                int maxSys = pri?.MaxSystolic ?? 140;
                int minDia = pri?.MinDiastolic ?? 60;
                int maxDia = pri?.MaxDiastolic ?? 90;

                if (Systolic < minSys || Diastolic < minDia)
                    return "Huyết áp thấp (cảnh báo)";
                if (Systolic > maxSys || Diastolic > maxDia)
                    return "Huyết áp cao (cảnh báo)";
                return "Huyết áp bình thường";
            }
        }
    }
}
