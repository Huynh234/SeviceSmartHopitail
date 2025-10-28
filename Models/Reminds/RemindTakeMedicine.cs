using SeviceSmartHopitail.Models.Infomation;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace SeviceSmartHopitail.Models.Reminds
{
    public class RemindTakeMedicine
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [ForeignKey(nameof(TaiKhoan))]
        public int TkId { get; set; }
        public decimal? TimeRemind { get; set; }      // Liều lượng (ví dụ: 1 viên, 500mg)
        public string? Title { get; set; }
        public DateTime? LastSent { get; set; }

        public TaiKhoan TaiKhoan { get; set; } = null!;
    }
}
