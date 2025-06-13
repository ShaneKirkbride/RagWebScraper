using Microsoft.ML.OnnxRuntime;

namespace RagWebScraper.Services;

/// <summary>
/// Abstraction over an ONNX inference session.
/// </summary>
public interface IOnnxSession : IDisposable
{
    /// <summary>
    /// Runs inference with the supplied inputs.
    /// </summary>
    /// <param name="inputs">Input tensors for the model.</param>
    /// <returns>The model outputs.</returns>
    IDisposableReadOnlyCollection<NamedOnnxValue> Run(IEnumerable<NamedOnnxValue> inputs);
}
