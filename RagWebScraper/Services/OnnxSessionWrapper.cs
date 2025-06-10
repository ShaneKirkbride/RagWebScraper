using Microsoft.ML.OnnxRuntime;

namespace RagWebScraper.Services;

public class OnnxSessionWrapper : IOnnxSession
{
    private readonly InferenceSession _session;

    public OnnxSessionWrapper(string modelPath)
    {
        _session = new InferenceSession(modelPath);
    }

    public IDisposableReadOnlyCollection<DisposableNamedOnnxValue> Run(IEnumerable<NamedOnnxValue> inputs)
        => _session.Run(inputs.ToList());

    public void Dispose() => _session.Dispose();
}
