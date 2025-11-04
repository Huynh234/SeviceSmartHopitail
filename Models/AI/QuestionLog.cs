using SeviceSmartHopitail.Models.Infomation;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace SeviceSmartHopitail.Models.AI
{
    public class QuestionLog
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        [ForeignKey(nameof(TkId))]
        public int TkId { get; set; }
        public string Question { get; set; } = null!;
        public string Answer { get; set; } = null!;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public TaiKhoan TaiKhoan { get; set; } = null!;
    }
}
