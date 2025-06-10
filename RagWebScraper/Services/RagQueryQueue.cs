using System.Threading.Channels;

namespace RagWebScraper.Services;

public class RagQueryQueue : IRagQueryQueue
{
    private readonly Channel<RagQueryRequest> _channel = Channel.CreateUnbounded<RagQueryRequest>();

    public void Enqueue(RagQueryRequest request) => _channel.Writer.TryWrite(request);

    public IAsyncEnumerable<RagQueryRequest> ReadAllAsync(CancellationToken token) => _channel.Reader.ReadAllAsync(token);
}
