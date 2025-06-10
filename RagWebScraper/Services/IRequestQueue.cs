namespace RagWebScraper.Services;

/// <summary>
/// Basic asynchronous queue abstraction.
/// </summary>
public interface IRequestQueue<TRequest>
{
    /// <summary>
    /// Adds a request to the queue.
    /// </summary>
    /// <param name="request">The request item.</param>
    void Enqueue(TRequest request);

    /// <summary>
    /// Reads queued items as an asynchronous stream.
    /// </summary>
    /// <param name="cancellationToken">Token to cancel the enumeration.</param>
    IAsyncEnumerable<TRequest> ReadAllAsync(CancellationToken cancellationToken);
}
