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

        [Required]
        public bool Status { get; set; } = false; // Pending / Active / Locked

        public UserProfile? UserProfile { get; set; }
    }
}
