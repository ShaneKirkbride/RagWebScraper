namespace RagWebScraper.Models
{
    public class NerSettings
    {
        public string ModelPath { get; set; } = string.Empty;
        public string VocabPath { get; set; } = string.Empty;
        public string LabelMapPath { get; set; } = string.Empty;
    }
}
