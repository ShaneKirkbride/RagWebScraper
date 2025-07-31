using System.Net.Http.Json;
using System.Text.Json;
using RagWebScraper.Models;

namespace RagWebScraper.Services;

/// <summary>
/// Client for retrieving documents from the CourtListener API.
/// </summary>
public sealed class DocumentPullerService : IDocumentPullerService
{
    private readonly HttpClient _httpClient;
    private const string BaseUrl = "https://www.courtlistener.com/api/rest/v3/opinions/";

    public DocumentPullerService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    /// <inheritdoc />
    public async IAsyncEnumerable<CourtOpinion> GetDocumentsAsync(string query, [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken token = default)
    {
        if (string.IsNullOrWhiteSpace(query))
            yield break;

        var next = $"{BaseUrl}?search={Uri.EscapeDataString(query)}&full_case=true";

        while (!string.IsNullOrEmpty(next))
        {
            using var response = await _httpClient.GetAsync(new Uri(next, UriKind.Absolute), token).ConfigureAwait(false);
            response.EnsureSuccessStatusCode();

            await using var stream = await response.Content.ReadAsStreamAsync(token).ConfigureAwait(false);
            using var doc = await JsonDocument.ParseAsync(stream, cancellationToken: token).ConfigureAwait(false);

            if (doc.RootElement.TryGetProperty("results", out var results))
            {
                foreach (var element in results.EnumerateArray())
                {
                    yield return new CourtOpinion
                    {
                        Id = element.GetProperty("id").GetInt32(),
                        CaseName = element.GetProperty("case_name").GetString() ?? string.Empty,
                        PlainText = element.GetProperty("plain_text").GetString() ?? string.Empty,
                    };
                }
            }

            next = doc.RootElement.TryGetProperty("next", out var n) ? n.GetString() : null;
        }
    }
}
