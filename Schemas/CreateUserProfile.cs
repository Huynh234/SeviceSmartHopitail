using System.ComponentModel.DataAnnotations;

namespace SeviceSmartHopitail.Schemas
{
    public class CreateUserProfile
    {
        public int TaiKhoanId { get; set; }

        public string? FullName { get; set; }

        public int? Age { get; set; }
        public string? Gender { get; set; }

        public decimal? Height { get; set; }
        public decimal? Weight { get; set; }

        public string? Adress { get; set; }

    }
}
