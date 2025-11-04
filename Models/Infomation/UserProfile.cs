using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using SeviceSmartHopitail.Models.Health;

namespace SeviceSmartHopitail.Models.Infomation
{
    public class UserProfile
    {

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int HoSoId { get; set; }

        [Required]
        public int TaiKhoanId { get; set; }

        [ForeignKey(nameof(TaiKhoanId))]
        public TaiKhoan TaiKhoan { get; set; } = null!;

        [MaxLength(100)]
        public string? FullName { get; set; }
        public string? AvatarUrl { get; set; }
        public int? Age { get; set; }
        public string? PhoneNumber { get; set; }
        public DateOnly? Brith { get; set; }
        public string? Gender { get; set; }
        public decimal? Height { get; set; }
        public decimal? Weight { get; set; }
        public string? Address { get; set; }
        public bool Check { get; set; } = false;

        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime UpdatedAt { get; set; } = DateTime.Now;
        // === Thêm navigation tới PriWarning (1–1) ===
        public PriWarning? PriWarning { get; set; }
        // === Liên kết tới các bảng sức khỏe mới ===
        public ICollection<SleepRecord>? SleepRecords { get; set; }   // Giấc ngủ
        public ICollection<BloodPressureRecord>? HealthHeartRates { get; set; } // Nhịp tim
        public ICollection<BloodSugarRecord>? HealthBloodSugars { get; set; } // Đường huyết
        public ICollection<HeartRateRecord>? HealthBloodPressures { get; set; } // Huyết áp
    }
}
