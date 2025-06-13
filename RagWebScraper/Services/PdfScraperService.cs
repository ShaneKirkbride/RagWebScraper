using HtmlAgilityPack;
using System.Net.Http.Headers;

/// <summary>
/// Service for scraping PDF links from web pages and downloading them.
/// </summary>
public class PdfScraperService : IPdfScraperService
{
    private readonly HttpClient _httpClient;

    /// <summary>
    /// Initializes a new instance of the <see cref="PdfScraperService"/> class.
    /// </summary>
    /// <param name="httpClient">HTTP client used for making requests.</param>
    public PdfScraperService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    /// <summary>
    /// Gets all PDF links on the specified page.
    /// </summary>
    /// <param name="url">URL of the page to scrape.</param>
    /// <returns>Enumerable of absolute PDF URLs.</returns>
    public async Task<IEnumerable<string>> GetPdfLinksAsync(string url)
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, url);
        request.Headers.UserAgent.ParseAdd(
            "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 " +
            "(KHTML, like Gecko) Chrome/124.0.0.0 Safari/537.36");

        using var response = await _httpClient.SendAsync(request);
        response.EnsureSuccessStatusCode(); // throws on 403, 404, etc.

        var html = await response.Content.ReadAsStringAsync();

        var doc = new HtmlDocument();
        doc.LoadHtml(html);

        var baseUri = new Uri(url);
        var links = doc.DocumentNode.SelectNodes("//a[@href]")
            ?.Select(node => node.GetAttributeValue("href", null))
            .Where(href => href != null && href.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase))
            .Select(href => new Uri(baseUri, href).ToString())
            .Distinct();

        return links ?? Enumerable.Empty<string>();
    }

    /// <summary>
    /// Downloads the PDFs to the given directory.
    /// </summary>
    /// <param name="pdfUrls">PDF URLs to download.</param>
    /// <param name="outputDirectory">Destination directory.</param>
    public async Task DownloadPdfsAsync(IEnumerable<string> pdfUrls, string outputDirectory)
    {
        Directory.CreateDirectory(outputDirectory);

        foreach (var pdfUrl in pdfUrls)
        {
            var fileName = Path.GetFileName(new Uri(pdfUrl).AbsolutePath);
            var filePath = Path.Combine(outputDirectory, fileName);

            using var request = new HttpRequestMessage(HttpMethod.Get, pdfUrl);
            request.Headers.UserAgent.ParseAdd(
                "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 " +
                "(KHTML, like Gecko) Chrome/124.0.0.0 Safari/537.36");

            using var response = await _httpClient.SendAsync(request);
            response.EnsureSuccessStatusCode();

            var pdfBytes = await response.Content.ReadAsByteArrayAsync();
            await File.WriteAllBytesAsync(filePath, pdfBytes);
        }
    }
}
