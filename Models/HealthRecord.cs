using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SeviceSmartHopitail.Models
{
    public class HealthRecord
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int RecordId { get; set; }

        [Required]
        public int UserProfileId { get; set; }

        [ForeignKey(nameof(UserProfileId))]
        public UserProfile UserProfile { get; set; } = null!;

        [Required]
        public int HeartRate { get; set; } // bpm

        public decimal? BloodSugar { get; set; } // mg/dL

        public int? Systolic { get; set; }   // mmHg
        public int? Diastolic { get; set; }  // mmHg

        public decimal? TimeSleep { get; set; } // hours

        public string? Note { get; set; }

        public DateTime RecordedAt { get; set; } = DateTime.UtcNow;

        // ========== Alerts (sử dụng PriWarning nếu có) ==========
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

        [NotMapped]
        public string BloodSugarAlert
        {
            get
            {
                if (!BloodSugar.HasValue) return "Không có dữ liệu";

                var pri = UserProfile?.PriWarning;
                decimal min = pri?.MinBloodSugar ?? 70;
                decimal max = pri?.MaxBloodSugar ?? 140;

                if (BloodSugar < min) return "Đường huyết thấp (cảnh báo)";
                if (BloodSugar > max) return "Đường huyết cao (cảnh báo)";
                return "Đường huyết bình thường";
            }
        }

        [NotMapped]
        public string BloodPressureAlert
        {
            get
            {
                if (!Systolic.HasValue || !Diastolic.HasValue)
                    return "Không có dữ liệu";

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

        [NotMapped]
        public string SleepAlert
        {
            get
            {
                if (!TimeSleep.HasValue) return "Không có dữ liệu";

                var pri = UserProfile?.PriWarning;
                decimal min = pri?.MinSleep ?? 6;
                decimal max = pri?.MaxSleep ?? 10;

                if (TimeSleep < min) return "Ngủ ít (cảnh báo)";
                if (TimeSleep > max) return "Ngủ quá nhiều (cảnh báo)";
                return "Giấc ngủ bình thường";
            }
        }
    }
}
