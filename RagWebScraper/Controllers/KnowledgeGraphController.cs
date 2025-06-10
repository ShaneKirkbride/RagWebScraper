using Microsoft.AspNetCore.Mvc;
using RagWebScraper.Models;
using RagWebScraper.Services;
using RagWebScraper.Factories;

namespace RagWebScraper.Controllers;
[ApiController]
[Route("api/KnowledgeGraph")]
public class KnowledgeGraphController : ControllerBase, IAnalyzerController
{
    private readonly IKnowledgeGraphService _graphService;

    public KnowledgeGraphController(IKnowledgeGraphService graphService)
    {
        _graphService = graphService;
    }

    [HttpPost("analyze")]
    public Task<IActionResult> AnalyzeText([FromBody] string text) =>
        AnalyzeTextInternal(text);

    private async Task<IActionResult> AnalyzeTextInternal(string text)
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

    async Task<IActionResult> IAnalyzerController.AnalyzeAsync(object request)
    {
        if (request is not string text)
            return new BadRequestObjectResult("Invalid request type.");

        return await AnalyzeTextInternal(text);
    }
}
