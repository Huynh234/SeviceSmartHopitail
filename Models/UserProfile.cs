using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace SeviceSmartHopitail.Models
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
        public DateOnly? Brith { get; set; }
        public string? Gender { get; set; }
        public decimal? Height { get; set; }
        public decimal? Weight { get; set; }
        public string? Address { get; set; }
        public bool Check { get; set; } = false;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        // === Thêm navigation tới PriWarning (1–1) ===
        public PriWarning? PriWarning { get; set; }

        // === Navigation tới HealthRecord (1–n) ===
        public ICollection<HealthRecord> HealthRecords { get; set; } = new List<HealthRecord>();
    }
}
