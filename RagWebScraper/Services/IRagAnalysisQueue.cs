namespace RagWebScraper.Services;

public interface IRagAnalysisQueue
{
    void Enqueue(RagAnalysisRequest request);
    IAsyncEnumerable<RagAnalysisRequest> ReadAllAsync(CancellationToken cancellationToken);
}
