namespace RagWebScraper.Services
{
    public interface IPdfProcessingQueue
    {
        void Enqueue(PdfProcessingRequest request);
        IAsyncEnumerable<PdfProcessingRequest> ReadAllAsync(CancellationToken cancellationToken);
    }
}