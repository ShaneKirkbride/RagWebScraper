using Microsoft.Extensions.DependencyInjection;
using RagWebScraper.Controllers;

namespace RagWebScraper.Factories;

/// <summary>
/// Represents the supported data mediums.
/// </summary>
public enum DataMedium
{
    Web,
    Pdf,
    Text
}

/// <summary>
/// Factory contract for resolving controllers based on the data medium.
/// </summary>
public interface IAnalyzerControllerFactory
{
    IAnalyzerController Create(DataMedium medium);
}

/// <summary>
/// Default implementation that resolves controllers from the DI container.
/// </summary>
public class AnalyzerControllerFactory : IAnalyzerControllerFactory
{
    private readonly IServiceProvider _provider;

    public AnalyzerControllerFactory(IServiceProvider provider)
    {
        _provider = provider;
    }

    public IAnalyzerController Create(DataMedium medium) => medium switch
    {
        DataMedium.Web => _provider.GetRequiredService<RAGAnalyzerController>(),
        DataMedium.Pdf => _provider.GetRequiredService<PdfUploadController>(),
        DataMedium.Text => _provider.GetRequiredService<KnowledgeGraphController>(),
        _ => throw new ArgumentOutOfRangeException(nameof(medium), medium, null)
    };
}
