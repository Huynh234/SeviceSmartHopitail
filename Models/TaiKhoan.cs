using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.CompilerServices;

namespace SeviceSmartHopitail.Models
{
    public class TaiKhoan
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required, MaxLength(100)]
        public string UserName { get; set; } = "";

        [Required, MaxLength(100)]
        public string Email { get; set; } = "";

        [Required]
        public string PasswordHash { get; set; } = "";

        public string OtpHash { get; set; } = "";
        public DateTime? OtpExpireAt { get; set; } = DateTime.MinValue;
        public DateTime? CreatAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdateAt { get; set; } = null;

        [Required]
        public bool Status { get; set; } = false; // Pending / Active / Locked
        public bool LockStatus { get; set; } = false;

        public UserProfile? UserProfile { get; set; }
    }
}
