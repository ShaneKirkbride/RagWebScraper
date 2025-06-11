using System.Linq;
using RagWebScraper.Models;

namespace RagWebScraper.Services;

/// <summary>
/// Entity extractor backed by an ONNX NER model.
/// </summary>
public class OnnxEntityExtractor : IEntityExtractor
{
    private readonly INerService _nerService;

    public OnnxEntityExtractor(string modelPath, string vocabPath, string mergesPath, string dictionaryPath)
    {
        _nerService = new ONNXNerService(modelPath, vocabPath, mergesPath, dictionaryPath);
    }

    public OnnxEntityExtractor(INerService nerService)
    {
        _nerService = nerService;
    }

    public IEnumerable<Entity> ExtractEntities(string text)
    {
        var entities = _nerService.RecognizeEntities(text);
        return entities.Select(e => new Entity(e.Label, e.Text, e.Start, e.End));
    }
}
