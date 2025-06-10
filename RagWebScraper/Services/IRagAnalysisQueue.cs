namespace RagWebScraper.Services;

/// <summary>
/// Queue for RAG (Retrieval-Augmented Generation) analysis requests.
/// </summary>
public interface IRagAnalysisQueue : IRequestQueue<RagAnalysisRequest>
{
}
