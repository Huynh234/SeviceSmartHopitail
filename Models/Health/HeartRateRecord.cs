using SeviceSmartHopitail.Models.Infomation;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SeviceSmartHopitail.Models.Health
{
    public class HeartRateRecord
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        public int UserProfileId { get; set; }

        [ForeignKey(nameof(UserProfileId))]
        public UserProfile UserProfile { get; set; } = null!;

        [Required]
        public int HeartRate { get; set; } // bpm
        public string? Note { get; set; }
        public DateTime RecordedAt { get; set; } = DateTime.UtcNow;

        [NotMapped]
        public string HeartRateAlert
        {
            get
            {
                var pri = UserProfile?.PriWarning;
                int min = pri?.MinHeartRate ?? 60;
                int max = pri?.MaxHeartRate ?? 100;

                if (HeartRate < min) return "Nhịp tim thấp (cảnh báo)";
                if (HeartRate > max) return "Nhịp tim cao (cảnh báo)";
                return "Nhịp tim bình thường";
            }
        }
    }
}
