using Microsoft.AspNetCore.Mvc;
using RagWebScraper.Models;
using RagWebScraper.Services;

[ApiController]
[Route("api/rag")]
public class RAGQueryController : ControllerBase
{
    private readonly EmbeddingService _embedding;
    private readonly VectorStoreService _vectorStore;

    public RAGQueryController(EmbeddingService embedding, VectorStoreService vectorStore)
    {
        _embedding = embedding;
        _vectorStore = vectorStore;
    }

    [HttpPost("query")]
    public async Task<ActionResult<List<string>>> QueryRag([FromBody] RAGQueryRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Query))
            return BadRequest("Query cannot be empty.");

        var embedding = await _embedding.GetEmbeddingAsync(request.Query);
        var results = await _vectorStore.QueryAsync(embedding);

        return Ok(results.Select(r =>
            r.payload != null && r.payload.ContainsKey("ChunkText")
                ? r.payload["ChunkText"]?.ToString()
                : "[Missing ChunkText]").ToList());
    }
}