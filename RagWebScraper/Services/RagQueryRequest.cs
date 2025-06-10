namespace RagWebScraper.Services;

public class RagQueryRequest
{
    public required string Query { get; init; }
    public TaskCompletionSource<List<string>> Completion { get; init; } = new();
}
