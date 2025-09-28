using SeviceSmartHopitail.Models;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace SeviceSmartHopitail.Schemas
{
    public class CreateHealthRecord
    {
        public int RecordId { get; set; }

        public int UserProfileId { get; set; }

        public int HeartRate { get; set; } // bpm

        public decimal? BloodSugar { get; set; } // mg/dL

        public int? Systolic { get; set; }   // mmHg
        public int? Diastolic { get; set; }  // mmHg

        public decimal? TimeSleep { get; set; } // hours

        public string? Note { get; set; }

        public DateTime RecordedAt { get; set; } = DateTime.UtcNow;
    }
}
