namespace RagWebScraper.Services
{
    public interface ISentimentAnalyzer
    {
        float AnalyzeSentiment(string text); // e.g., "Positive", "Neutral", "Negative"
    }

}