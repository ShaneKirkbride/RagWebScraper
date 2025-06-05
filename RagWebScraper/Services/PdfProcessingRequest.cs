namespace RagWebScraper.Services
{
    public class PdfProcessingRequest
    {
        public string FileName { get; init; }
        public Stream FileStream { get; init; } = default!;
        public List<string> Keywords { get; init; } = new();
    }

}