namespace RagWebScraper.Services;

using RagWebScraper.Models;

/// <summary>
/// Downloads documents from a remote source.
/// </summary>
public interface IDocumentPullerService
{
    /// <summary>
    /// Retrieves documents matching the provided query.
    /// </summary>
    /// <param name="query">Search query to send to the API.</param>
    /// <param name="token">Cancellation token.</param>
    /// <returns>Async sequence of documents.</returns>
    IAsyncEnumerable<CourtOpinion> GetDocumentsAsync(string query, CancellationToken token = default);
}
