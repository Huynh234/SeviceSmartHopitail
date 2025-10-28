namespace SeviceSmartHopitail.Models.AI
{
    public class IndexTerm
    {
        public int Id { get; set; }
        public string Term { get; set; } = null!;
        public int? IcdCodeId { get; set; }
        public IcdCode? IcdCode { get; set; }
    }
}
