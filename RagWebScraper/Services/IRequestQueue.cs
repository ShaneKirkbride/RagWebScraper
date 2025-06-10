namespace RagWebScraper.Services;

public interface IRequestQueue<TRequest>
{
    void Enqueue(TRequest request);
    IAsyncEnumerable<TRequest> ReadAllAsync(CancellationToken cancellationToken);
}
