namespace SeviceSmartHopitail.Schemas.RAG
{
    public class AskDto
    {
        public int TkId { get; set; }
        public string Question { get; set; } = null!;
        public DateOnly? Time { get; set; } = null;
    }
}
