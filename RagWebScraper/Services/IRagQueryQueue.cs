namespace RagWebScraper.Services;

public interface IRagQueryQueue
{
    void Enqueue(RagQueryRequest request);
    IAsyncEnumerable<RagQueryRequest> ReadAllAsync(CancellationToken cancellationToken);
}
