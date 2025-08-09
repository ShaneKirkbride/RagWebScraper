namespace RagWebScraper.Models;

/// <summary>
/// Represents a single retrieved passage and its source.
/// </summary>
public class RAGQueryResult
{
    public string ChunkText { get; set; } = string.Empty;
    public string? Source { get; set; }
}

/// <summary>
/// Response returned from a RAG query, including passages and AI commentary.
/// </summary>
public class RAGQueryResponse
{
    public List<RAGQueryResult> Results { get; set; } = new();
    public string Commentary { get; set; } = string.Empty;
}
