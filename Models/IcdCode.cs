namespace SeviceSmartHopitail.Models
{
    public class IcdCode
    {
        public int Id { get; set; }
        public string Code { get; set; } = null!;
        public string? Title { get; set; }
        public string? Chapter { get; set; }
        public string? Description { get; set; }
        public string SourceVolume { get; set; } = "V1";
        public List<IndexTerm> IndexTerms { get; set; } = new();
        public List<TextChunk> TextChunks { get; set; } = new();
    }
}
