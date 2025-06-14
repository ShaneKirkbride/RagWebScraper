using System.IO;
using System.Linq;
using Microsoft.ML.OnnxRuntime.Tensors;
using Microsoft.ML.OnnxRuntime;
using RagWebScraper.Services;
using Xunit;

namespace RagWebScraper.Tests;

public class OnnxSessionWrapperTests
{
    [Fact]
    public void Run_ReturnsExpectedOutput()
    {
        var modelPath = Path.Combine(AppContext.BaseDirectory, "identity.onnx");
        using var session = new OnnxSessionWrapper(modelPath);
        var tensor = new DenseTensor<float>(new float[]{1f}, new []{1});
        var inputs = new[] { NamedOnnxValue.CreateFromTensor("input", tensor) };

        using var outputs = session.Run(inputs);
        var value = outputs.First().AsTensor<float>()[0];

        Assert.Equal(1f, value);
    }
}
