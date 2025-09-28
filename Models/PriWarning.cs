using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SeviceSmartHopitail.Models
{
    public class PriWarning
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int WarningId { get; set; }

        [Required]
        public int UserProfileId { get; set; }

        [ForeignKey(nameof(UserProfileId))]
        public UserProfile UserProfile { get; set; } = null!;

        // Ngưỡng cảnh báo cá nhân hóa
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

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}
