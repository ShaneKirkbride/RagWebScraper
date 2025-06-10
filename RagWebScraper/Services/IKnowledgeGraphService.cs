namespace RagWebScraper.Services;
using RagWebScraper.Models;

/// <summary>
/// Builds entity graphs from text or previously processed PDFs.
/// </summary>
public interface IKnowledgeGraphService
{
    /// <summary>
    /// Creates a knowledge graph from raw text.
    /// </summary>
    /// <param name="text">The text to analyze.</param>
    Task<EntityGraphResult> AnalyzeTextAsync(string text);

    /// <summary>
    /// Creates a knowledge graph using text extracted from a processed PDF.
    /// </summary>
    /// <param name="fileName">The name of the processed PDF.</param>
    Task<EntityGraphResult> AnalyzePdfAsync(string fileName);
}
