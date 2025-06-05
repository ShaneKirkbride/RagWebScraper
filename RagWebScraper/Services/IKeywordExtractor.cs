namespace RagWebScraper.Services
{
    public interface IKeywordExtractor
    {
        Dictionary<string, int> ExtractKeywords(string text, List<string> keywords);
    }
}