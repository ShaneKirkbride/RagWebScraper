using System.Threading.Channels;

namespace RagWebScraper.Services
{
    public class PdfProcessingQueue : IPdfProcessingQueue
    {
        private readonly Channel<PdfProcessingRequest> _channel = Channel.CreateUnbounded<PdfProcessingRequest>();

        public void Enqueue(PdfProcessingRequest request) => _channel.Writer.TryWrite(request);

        public IAsyncEnumerable<PdfProcessingRequest> ReadAllAsync(CancellationToken token) => _channel.Reader.ReadAllAsync(token);
    }

}
