using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace RagWebScraper.Services;

public abstract class ChannelBackgroundWorker<TRequest> : BackgroundService
{
    private readonly IRequestQueue<TRequest> _queue;
    private readonly ILogger _logger;

    protected ChannelBackgroundWorker(IRequestQueue<TRequest> queue, ILogger logger)
    {
        _queue = queue;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await foreach (var request in _queue.ReadAllAsync(stoppingToken))
        {
            try
            {
                await ProcessRequestAsync(request, stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to process request");
            }
        }
    }

    protected abstract Task ProcessRequestAsync(TRequest request, CancellationToken cancellationToken);
}
