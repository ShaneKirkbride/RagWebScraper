using Microsoft.AspNetCore.Mvc;
using RagWebScraper.Models;
using RagWebScraper.Services;


// Controller using SRP and DIP
[ApiController]
[Route("api/rag")]
public class RAGAnalyzerController : ControllerBase
{
    private readonly IRagAnalysisQueue _queue;

    public RAGAnalyzerController(IRagAnalysisQueue queue)
    {
        _queue = queue;
    }

    [HttpPost("analyze")]
    public Task<IActionResult> Analyze([FromBody] UrlAnalysisRequest request) =>
        AnalyzeInternal(request);

    private async Task<IActionResult> AnalyzeInternal(UrlAnalysisRequest request)
    {
        var tasks = new List<Task<AnalysisResult?>>();

        foreach (var url in request.Urls)
        {
            var queueRequest = new RagAnalysisRequest
            {
                Url = url,
                Keywords = request.Keywords
            };

            _queue.Enqueue(queueRequest);
            tasks.Add(queueRequest.Completion.Task);
        }

        var results = (await Task.WhenAll(tasks)).Where(r => r != null).ToList()!;
        return Ok(results);
    }

}
