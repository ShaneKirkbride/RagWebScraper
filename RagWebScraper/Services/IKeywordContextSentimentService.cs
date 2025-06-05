namespace RagWebScraper.Services
{
    public interface IKeywordContextSentimentService
    {
        Dictionary<string, float> ExtractKeywordSentiments(string text, IEnumerable<string> keywords);
    }

}