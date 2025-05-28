namespace RagWebScraper.Models
{
    public class NamedEntity
    {
        public string Text { get; set; } = string.Empty;
        public string Label { get; set; } = string.Empty; // e.g., "ORG", "PER", "DEVICE"
        public int Start { get; set; }     // Token index or character start
        public int End { get; set; }       // Token index or character end
    }

}
