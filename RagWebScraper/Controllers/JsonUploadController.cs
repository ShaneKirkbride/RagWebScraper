using Microsoft.AspNetCore.Mvc;
using RagWebScraper.Services;

namespace RagWebScraper.Controllers;

/// <summary>
/// API controller for uploading JSON files and ingesting their content into the RAG vector store.
/// </summary>
[ApiController]
[Route("api/json-ingest")]
public class JsonUploadController : ControllerBase
{
    private readonly IJsonIngestService _ingestService;

    /// <summary>
    /// Initializes a new instance of the <see cref="JsonUploadController"/> class.
    /// </summary>
    public JsonUploadController(IJsonIngestService ingestService)
    {
        _ingestService = ingestService;
    }

    /// <summary>
    /// Uploads one or more JSON files and ingests their content.
    /// </summary>
    /// <param name="files">The uploaded JSON files.</param>
    /// <param name="token">Cancellation token.</param>
    [HttpPost("upload")]
    public async Task<IActionResult> Upload([FromForm] IFormFileCollection files, CancellationToken token)
    {
        if (files == null || files.Count == 0)
            return BadRequest("No files uploaded.");

        foreach (var file in files)
        {
            var safeName = Path.GetFileName(file.FileName);
            var path = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid()}_{safeName}");
            await using (var fs = new FileStream(path, FileMode.Create, FileAccess.Write))
            {
                await file.CopyToAsync(fs, token);
            }

            await _ingestService.IngestAsync(path, token);
            System.IO.File.Delete(path);
        }

        return Ok(new { Message = "Files ingested." });
    }
}
