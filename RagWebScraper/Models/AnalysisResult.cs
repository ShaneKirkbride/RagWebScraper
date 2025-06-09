
namespace RagWebScraper.Models;
public class AnalysisResult
{
    public AnalysisResult(IEnumerable<LinkedPassage> links)
    {
        Links = links;
    }

    public string Url { get; set; }
    public string FileName { get; set; }
    public float PageSentimentScore { get; set; }
    public Dictionary<string, int> KeywordFrequencies { get; set; } = new();
    public Dictionary<string, float> KeywordSentimentScores { get; set; } = new();
    public IEnumerable<LinkedPassage> Links { get; set; }
    public string KeywordSummary { get; set; }
    public string RawText { get; internal set; }
    public List<string>? RawSentences { get; set; }
}