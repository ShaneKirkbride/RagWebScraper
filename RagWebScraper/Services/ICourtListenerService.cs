namespace RagWebScraper.Services;

using RagWebScraper.Models;

/// <summary>
/// Downloads court opinions from the CourtListener API.
/// </summary>
public interface ICourtListenerService
{
    /// <summary>
    /// Retrieves court opinions matching the provided query.
    /// </summary>
    /// <param name="query">Search query to send to the API.</param>
    /// <param name="token">Cancellation token.</param>
    /// <returns>Async sequence of court opinions.</returns>
    IAsyncEnumerable<CourtOpinion> GetOpinionsAsync(string query, CancellationToken token = default);
}
