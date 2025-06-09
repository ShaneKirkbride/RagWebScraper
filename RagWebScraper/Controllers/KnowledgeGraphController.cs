using Microsoft.AspNetCore.Mvc;
using RagWebScraper.Models;
using RagWebScraper.Services;

namespace RagWebScraper.Controllers;
[ApiController]
[Route("api/KnowledgeGraph")]
public class KnowledgeGraphController : ControllerBase
{
    private readonly IKnowledgeGraphService _graphService;

    public KnowledgeGraphController(IKnowledgeGraphService graphService)
    {
        _graphService = graphService;
    }

    [HttpPost("analyze")]
    public async Task<ActionResult<EntityGraphResult>> AnalyzeText([FromBody] string text)
    {
        if (string.IsNullOrWhiteSpace(text)) return BadRequest("Text is empty");
        var graph = await _graphService.AnalyzeTextAsync(text);
        return Ok(graph);
    }

    [HttpPost("analyze-pdf")]
    public async Task<ActionResult<EntityGraphResult>> AnalyzePdf([FromBody] string fileName)
    {
        if (string.IsNullOrWhiteSpace(fileName))
            return BadRequest("Filename is required.");

        var graph = await _graphService.AnalyzePdfAsync(fileName);
        return Ok(graph);
    }
}
