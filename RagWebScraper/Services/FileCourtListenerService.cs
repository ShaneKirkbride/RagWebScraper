using System.Text.Json;
using RagWebScraper.Models;

namespace RagWebScraper.Services;

/// <summary>
/// Loads <see cref="CourtOpinion"/> instances from local CourtListener JSON files.
/// </summary>
public sealed class FileCourtListenerService : ICourtListenerService
{
    /// <inheritdoc />
    public async IAsyncEnumerable<CourtOpinion> GetOpinionsAsync(string filePath, [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken token = default)
    {
        if (string.IsNullOrWhiteSpace(filePath) || !File.Exists(filePath))
            yield break;

        await using var stream = File.OpenRead(filePath);
        using var doc = await JsonDocument.ParseAsync(stream, cancellationToken: token);

        if (!doc.RootElement.TryGetProperty("results", out var results))
            yield break;

        foreach (var element in results.EnumerateArray())
        {
            token.ThrowIfCancellationRequested();
            yield return new CourtOpinion
            {
                Id = element.GetProperty("id").GetInt32(),
                CaseName = element.GetProperty("case_name").GetString() ?? string.Empty,
                PlainText = element.GetProperty("plain_text").GetString() ?? string.Empty
            };
        }
    }
}
