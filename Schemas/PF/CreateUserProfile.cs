using System.ComponentModel.DataAnnotations;

namespace SeviceSmartHopitail.Schemas.PF
{
    public class CreateUserProfile
    {
        public int TaiKhoanId { get; set; }

        public string? FullName { get; set; }

        public DateOnly? Birth { get; set; }
        public string? Gender { get; set; }

        [MaxLength(10)] // Correctly applying the MaxLength attribute
        public string? PhoneNumber { get; set; }

        public decimal? Height { get; set; }
        public decimal? Weight { get; set; }

        public string? Adress { get; set; }
    }
}
