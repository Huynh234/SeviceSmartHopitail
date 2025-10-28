using SeviceSmartHopitail.Models.Infomation;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace SeviceSmartHopitail.Models.Reminds
{
    public class RemindExercise
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [ForeignKey(nameof(TaiKhoan))]
        public int TkId { get; set; }
        public decimal? TimeRemind { get; set; }
        public string? Title { get; set; }            // Tiêu đề nhắc nhở
        public DateTime? LastSent { get; set; }       // Lần gửi gần nhất

        public TaiKhoan TaiKhoan { get; set; } = null!;
    }
}
