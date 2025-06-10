using Microsoft.ML.OnnxRuntime;

namespace RagWebScraper.Services;

public interface IOnnxSession : IDisposable
{
    IDisposableReadOnlyCollection<DisposableNamedOnnxValue> Run(IEnumerable<NamedOnnxValue> inputs);
}
