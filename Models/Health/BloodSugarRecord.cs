using SeviceSmartHopitail.Models.Infomation;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SeviceSmartHopitail.Models.Health
{
    public class BloodSugarRecord
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        public int UserProfileId { get; set; }

        [ForeignKey(nameof(UserProfileId))]
        public UserProfile UserProfile { get; set; } = null!;

        [Required]
        public decimal BloodSugar { get; set; } // mg/dL
        public string? Note { get; set; }
        public DateTime RecordedAt { get; set; } = DateTime.Now;

        [NotMapped]
        public string BloodSugarAlert
        {
            get
            {
                var pri = UserProfile?.PriWarning;
                decimal min = pri?.MinBloodSugar ?? 70;
                decimal max = pri?.MaxBloodSugar ?? 140;

                if (BloodSugar < min) return "Đường huyết thấp (cảnh báo)";
                if (BloodSugar > max) return "Đường huyết cao (cảnh báo)";
                return "Đường huyết bình thường";
            }
        }
    }
}
