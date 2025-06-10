using Microsoft.AspNetCore.Mvc;
using RagWebScraper.Models;
using RagWebScraper.Services;

[ApiController]
[Route("api/rag")]
public class RAGQueryController : ControllerBase
{
    private readonly IRagQueryQueue _queue;

    public RAGQueryController(IRagQueryQueue queue)
    {
        _queue = queue;
    }

    [HttpPost("query")]
    public async Task<ActionResult<List<string>>> QueryRag([FromBody] RAGQueryRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Query))
            return BadRequest("Query cannot be empty.");

        var queueRequest = new RagQueryRequest { Query = request.Query };
        _queue.Enqueue(queueRequest);
        var results = await queueRequest.Completion.Task;
        return Ok(results);
    }
}