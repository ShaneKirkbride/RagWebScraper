using System.Threading.Channels;

namespace RagWebScraper.Services;

public class RagAnalysisQueue : IRagAnalysisQueue
{
    private readonly Channel<RagAnalysisRequest> _channel = Channel.CreateUnbounded<RagAnalysisRequest>();

    public void Enqueue(RagAnalysisRequest request) => _channel.Writer.TryWrite(request);

    public IAsyncEnumerable<RagAnalysisRequest> ReadAllAsync(CancellationToken token) => _channel.Reader.ReadAllAsync(token);
}
