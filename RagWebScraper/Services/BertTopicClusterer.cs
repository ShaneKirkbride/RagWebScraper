using System.Diagnostics;
using System.Text.Json;
using RagWebScraper.Models;

namespace RagWebScraper.Services;

/// <summary>
/// Clusters documents using the Python BERTopic library.
/// </summary>
public class BertTopicClusterer : IDocumentClusterer
{
    private readonly string _scriptPath;

    public BertTopicClusterer()
    {
        _scriptPath = Path.Combine(AppContext.BaseDirectory, "Python", "bertopic_cluster.py");
    }

    /// <inheritdoc />
    public async Task<DocumentClusteringResult> ClusterAsync(IEnumerable<Document> documents, int numberOfClusters = 5)
    {
        if (documents is null)
            throw new ArgumentNullException(nameof(documents));

        var docs = documents.ToList();
        if (docs.Count == 0)
        {
            return new DocumentClusteringResult(
                new Dictionary<Guid, int>(),
                new ClusterMetrics(0, 0, 0),
                new List<ClusterDescriptor>());
        }

        var psi = new ProcessStartInfo
        {
            FileName = "python",
            Arguments = $"\"{_scriptPath}\" {numberOfClusters}",
            RedirectStandardInput = true,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
        };

        using var process = new Process { StartInfo = psi };
        process.Start();

        await process.StandardInput.WriteAsync(JsonSerializer.Serialize(docs.Select(d => d.Text).ToList()));
        process.StandardInput.Close();

        var output = await process.StandardOutput.ReadToEndAsync();
        var error = await process.StandardError.ReadToEndAsync();
        await process.WaitForExitAsync();

        if (process.ExitCode != 0)
            throw new InvalidOperationException($"BERTopic process failed: {error}");

        using var json = JsonDocument.Parse(output);
        var assignmentsJson = json.RootElement.GetProperty("assignments");
        var descriptorsJson = json.RootElement.GetProperty("descriptors");

        var assignments = new Dictionary<Guid, int>(docs.Count);
        for (int i = 0; i < docs.Count; i++)
        {
            assignments[docs[i].Id] = assignmentsJson[i].GetInt32();
        }

        var descriptors = descriptorsJson
            .EnumerateArray()
            .Select(el =>
            {
                var id = el.GetProperty("cluster_id").GetInt32();
                var words = el.GetProperty("top_words")
                    .EnumerateArray()
                    .Select(w => w.GetString()!)
                    .ToList();
                var reason = words.Count > 0
                    ? $"Documents discuss: {string.Join(", ", words)}"
                    : "No keywords available";
                return new ClusterDescriptor(id, words, reason);
            })
            .OrderBy(d => d.ClusterId)
            .ToList();

        return new DocumentClusteringResult(
            assignments,
            new ClusterMetrics(0, 0, 0),
            descriptors);
    }
}
