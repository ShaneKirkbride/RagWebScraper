using RagWebScraper.Models;

namespace RagWebScraper.Services;

public class RagAnalysisRequest
{
    public required string Url { get; init; }
    public List<string> Keywords { get; init; } = new();
    public TaskCompletionSource<AnalysisResult?> Completion { get; init; } = new();
}
