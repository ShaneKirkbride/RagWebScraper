namespace RagWebScraper.Services
{
    public interface IWebScraperService
    {
        Task<string> ScrapeTextAsync(string url);
    }
}
