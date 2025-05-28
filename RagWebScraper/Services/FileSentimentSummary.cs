namespace RagWebScraper.Services;
public class FileSentimentSummary
{
    public string FileName { get; set; }
    public float Sentiment { get; set; }
    public Dictionary<string, int>? KeywordFrequencies { get; set; }
    public Dictionary<string, float>? KeywordSentiments { get; set; }
    public string? RawText { get; set; } // Add this
}