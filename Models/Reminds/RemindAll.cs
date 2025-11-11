using SeviceSmartHopitail.Models.Infomation;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace SeviceSmartHopitail.Models.Reminds
{
    public class RemindAll
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [ForeignKey(nameof(TaiKhoan))]
        public int TkId { get; set; }
        public string? Content { get; set; }
        public decimal? TimeRemind { get; set; }
        public string? Title { get; set; }
        public int loop { get; set; } = 0;
        public string? DayOfWkeek { get; set; }
        public DateTime? LastSent { get; set; }

        public TaiKhoan TaiKhoan { get; set; } = null!;
    }
}
