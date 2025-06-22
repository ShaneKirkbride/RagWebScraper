using System.Text.Json;

namespace RagWebScraper.Services;

/// <summary>
/// Service that parses uploaded JSON files and ingests the contained text into the vector database.
/// </summary>
public interface IJsonIngestService
{
    /// <summary>
    /// Parses the JSON file and ingests its text content.
    /// Each element should contain a <c>text</c> property and optional <c>id</c>.
    /// </summary>
    /// <param name="filePath">Path to the JSON file.</param>
    /// <param name="token">Cancellation token.</param>
    Task IngestAsync(string filePath, CancellationToken token = default);
}

/// <inheritdoc cref="IJsonIngestService"/>
public sealed class JsonIngestService : IJsonIngestService
{
    private readonly IChunkIngestorService _ingestor;

    /// <summary>
    /// Initializes a new instance of the <see cref="JsonIngestService"/> class.
    /// </summary>
    public JsonIngestService(IChunkIngestorService ingestor)
    {
        _ingestor = ingestor;
    }

    /// <inheritdoc />
    public async Task IngestAsync(string filePath, CancellationToken token = default)
    {
        if (string.IsNullOrWhiteSpace(filePath) || !File.Exists(filePath))
            return;

        await using var stream = File.OpenRead(filePath);
        using var doc = await JsonDocument.ParseAsync(stream, cancellationToken: token);

        if (doc.RootElement.ValueKind != JsonValueKind.Array)
            return;

        foreach (var element in doc.RootElement.EnumerateArray())
        {
            token.ThrowIfCancellationRequested();

            if (!element.TryGetProperty("text", out var textProp))
                continue;

            var text = textProp.GetString();
            if (string.IsNullOrWhiteSpace(text))
                continue;

            var id = element.TryGetProperty("id", out var idProp)
                ? idProp.GetString() ?? Guid.NewGuid().ToString()
                : Guid.NewGuid().ToString();

            await _ingestor.IngestChunksAsync(id, text);
        }
    }
}
