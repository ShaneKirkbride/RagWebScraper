namespace RagWebScraper.Services;

/// <summary>
/// Queue for RAG query execution requests.
/// </summary>
public interface IRagQueryQueue : IRequestQueue<RagQueryRequest>
{
}
