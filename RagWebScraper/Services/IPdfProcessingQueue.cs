namespace RagWebScraper.Services;

/// <summary>
/// Queue used for background PDF processing jobs.
/// </summary>
public interface IPdfProcessingQueue : IRequestQueue<PdfProcessingRequest>
{
}
