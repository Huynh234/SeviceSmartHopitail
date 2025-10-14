namespace SeviceSmartHopitail.Schemas
{
    public class AskDto
    {
        public int TkId { get; set; }
        public string Question { get; set; } = null!;
        public DateOnly? Time { get; set; } = null;
    }
}
