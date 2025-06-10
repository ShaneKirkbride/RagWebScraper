using Microsoft.AspNetCore.Http;

namespace RagWebScraper.Models;

public class PdfUploadRequest
{
    public IFormFileCollection Files { get; set; } = default!;
    public string Keywords { get; set; } = string.Empty;
}
