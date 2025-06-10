namespace RagWebScraper.Services;

/// <summary>
/// Fetches and extracts textual content from web pages.
/// </summary>
public interface IWebScraperService
{
    /// <summary>
    /// Downloads the page at the given URL and returns plain text.
    /// </summary>
    /// <param name="url">URL of the page to scrape.</param>
    Task<string> ScrapeTextAsync(string url);
}
