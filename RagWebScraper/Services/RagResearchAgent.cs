

namespace RagWebScraper.Services;

/// <summary>
/// Default implementation of <see cref="IRagResearchAgent"/>.
/// </summary>
public sealed class RagResearchAgent : IRagResearchAgent
{
    private readonly IEmbeddingService _embedding;
    private readonly IVectorStoreService _vectorStore;
    private readonly IChatCompletionService _chat;

    public RagResearchAgent(
        IEmbeddingService embedding,
        IVectorStoreService vectorStore,
        IChatCompletionService chat)
    {
        _embedding = embedding;
        _vectorStore = vectorStore;
        _chat = chat;
    }

    /// <inheritdoc />
    public async Task<string> ResearchAsync(string query)
    {
        if (string.IsNullOrWhiteSpace(query))
            return "Query cannot be empty.";

        var embedding = await _embedding.GetEmbeddingAsync(query);
        var results = await _vectorStore.QueryAsync(embedding, topK: 5);
        var passages = results
            .Select(r => r.payload != null && r.payload.ContainsKey("ChunkText")
                ? r.payload["ChunkText"]?.ToString()
                : string.Empty)
            .Where(t => !string.IsNullOrWhiteSpace(t))
            .ToList();

        if (passages.Count == 0)
            return "No relevant documents found.";

        var context = string.Join("\n---\n", passages);
        var prompt = $"{context}\n\nProvide a concise comparison highlighting any differing rules or interpretations.";
        var systemPrompt = "You are an expert researcher skilled in finding nuanced rules and differences across documents.";

        var result = await _chat.GetCompletionAsync(systemPrompt, prompt);
        return string.IsNullOrWhiteSpace(result) ? "No response." : result;
    }
}
