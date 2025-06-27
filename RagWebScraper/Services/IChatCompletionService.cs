namespace RagWebScraper.Services;

/// <summary>
/// Provides LLM chat completion functionality.
/// </summary>
public interface IChatCompletionService
{
    Task<string> GetCompletionAsync(string systemPrompt, string userPrompt);
}
