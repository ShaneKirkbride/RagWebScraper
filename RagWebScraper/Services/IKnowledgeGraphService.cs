namespace RagWebScraper.Services;
using RagWebScraper.Models;

public interface IKnowledgeGraphService
{
    Task<EntityGraphResult> AnalyzeTextAsync(string text);

    // Optional future extension ideas:
    Task<EntityGraphResult> AnalyzePdfAsync(string pdfBytes);
    //Task<EntityGraphResult> AnalyzeUrlAsync(string url);
}