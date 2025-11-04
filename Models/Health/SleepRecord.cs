using SeviceSmartHopitail.Models.Infomation;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SeviceSmartHopitail.Models.Health
{
    public class SleepRecord
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        public int UserProfileId { get; set; }

        [ForeignKey(nameof(UserProfileId))]
        public UserProfile UserProfile { get; set; } = null!;

        // Giờ bắt đầu ngủ
        [Required]
        public DateTime SleepTime { get; set; }

        // Giờ thức dậy
        [Required]
        public DateTime WakeTime { get; set; }

        // Giờ ngủ (được lưu trong DB, nhưng tính tự động)
        [Required]
        public decimal HoursSleep { get; set; }

        public string? Note { get; set; }

        public DateTime RecordedAt { get; set; } = DateTime.UtcNow;

        // Cảnh báo giấc ngủ
        [NotMapped]
        public string SleepAlert
        {
            get
            {
                var pri = UserProfile?.PriWarning;
                decimal min = pri?.MinSleep ?? 6;
                decimal max = pri?.MaxSleep ?? 10;

                if (HoursSleep < min) return "Ngủ ít (cảnh báo)";
                if (HoursSleep > max) return "Ngủ quá nhiều (cảnh báo)";
                return "Giấc ngủ bình thường";
            }
        }
    }
}
