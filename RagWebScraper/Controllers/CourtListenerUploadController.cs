using Microsoft.AspNetCore.Mvc;
using RagWebScraper.Models;
using RagWebScraper.Services;

namespace RagWebScraper.Controllers;

/// <summary>
/// Accepts CourtListener JSON uploads and returns analysis results.
/// </summary>
[ApiController]
[Route("api/courtlistener")]
public class CourtListenerUploadController : ControllerBase
{
    private readonly ICourtListenerService _loader;
    private readonly ICourtOpinionAnalyzerService _analyzer;

    public CourtListenerUploadController(ICourtListenerService loader, ICourtOpinionAnalyzerService analyzer)
    {
        _loader = loader;
        _analyzer = analyzer;
    }

    [HttpPost("analyze")]
    public async Task<ActionResult<List<AnalysisResult>>> Analyze([FromForm] IFormFileCollection files, [FromForm] string keywords, CancellationToken token)
    {
        if (files == null || files.Count == 0)
            return BadRequest("No files uploaded.");

        var keywordList = keywords?.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries) ?? Array.Empty<string>();
        var results = new List<AnalysisResult>();

        foreach (var file in files)
        {
            var tempPath = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid()}_{file.FileName}");
            await using (var fs = new FileStream(tempPath, FileMode.Create, FileAccess.Write))
            {
                await file.CopyToAsync(fs, token);
            }

            await foreach (var opinion in _loader.GetOpinionsAsync(tempPath, token))
            {
                var result = await _analyzer.AnalyzeOpinionAsync(opinion, keywordList);
                result.FileName = $"{opinion.CaseName} ({opinion.Id})";
                results.Add(result);
            }

            System.IO.File.Delete(tempPath);
        }

        return Ok(results);
    }
}
