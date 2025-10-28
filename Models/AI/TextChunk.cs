namespace SeviceSmartHopitail.Models.AI
{
    public class TextChunk
    {
        public int Id { get; set; }
        public int? IcdCodeId { get; set; }
        public IcdCode? IcdCode { get; set; }
        public string Text { get; set; } = null!;
        public byte[]? Embedding { get; set; }
        public DateTime? EmbeddingCreatedAt { get; set; }
    }
}
