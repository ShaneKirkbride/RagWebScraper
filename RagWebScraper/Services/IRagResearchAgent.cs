namespace RagWebScraper.Services;

/// <summary>
/// Provides expert RAG analysis capabilities.
/// </summary>
public interface IRagResearchAgent
{
    /// <summary>
    /// Executes a research query across ingested documents and highlights nuanced differences.
    /// </summary>
    /// <param name="query">The user's research question.</param>
    /// <returns>A natural language summary of findings.</returns>
    Task<string> ResearchAsync(string query);
}
