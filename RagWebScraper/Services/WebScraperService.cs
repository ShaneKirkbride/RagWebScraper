using HtmlAgilityPack;
using Polly;
using Polly.Retry;

namespace RagWebScraper.Services
{
    public class WebScraperService : IWebScraperService
    {
        private readonly HttpClient _httpClient;
        private readonly AsyncRetryPolicy<HttpResponseMessage> _retryPolicy;

        public WebScraperService(HttpClient httpClient)
        {
            _httpClient = httpClient;

            _retryPolicy = Policy
                .HandleResult<HttpResponseMessage>(r => !r.IsSuccessStatusCode)
                .Or<HttpRequestException>()
                .WaitAndRetryAsync(
                    retryCount: 3,
                    sleepDurationProvider: attempt => TimeSpan.FromSeconds(Math.Pow(2, attempt)),
                    onRetry: (outcome, timespan, attempt, context) =>
                    {
                        Console.WriteLine($"[Retry {attempt}] Request failed. Waiting {timespan} before next try.");
                    });
        }

        public async Task<string> ScrapeTextAsync(string url)
        {
            try
            {
                var response = await _retryPolicy.ExecuteAsync(() => _httpClient.GetAsync(url));

                if (!response.IsSuccessStatusCode)
                {
                    Console.WriteLine($"Failed to get {url} after retries.");
                    return string.Empty;
                }

                var html = await response.Content.ReadAsStringAsync();
                return ExtractTextFromHtml(html);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Critical failure scraping {url}: {ex.Message}");
                return string.Empty;
            }
        }

        private string ExtractTextFromHtml(string html)
        {
            var doc = new HtmlDocument();
            doc.LoadHtml(html);

            foreach (var node in doc.DocumentNode.SelectNodes("//script|//style") ?? Enumerable.Empty<HtmlNode>())
            {
                node.Remove();
            }

            return doc.DocumentNode.InnerText
                .Replace("\n", " ")
                .Replace("\r", " ")
                .Replace("\t", " ")
                .Trim();
        }
    }
}
