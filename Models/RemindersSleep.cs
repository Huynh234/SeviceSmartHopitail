using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace SeviceSmartHopitail.Models
{
    public class RemindersSleep
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [ForeignKey(nameof(TaiKhoan))] // chỉ định khóa ngoại 1-1
        public int TkId { get; set; }
        public decimal? HoursSleep { get; set; }
        public string? Title { get; set; }
        public TaiKhoan TaiKhoan { get; set; } = null!;
        public DateTime? LastSent { get; set; }   // Ngày gửi lần cuối
    }
}
