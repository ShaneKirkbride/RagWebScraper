using Microsoft.ML;
using Microsoft.ML.Transforms.Text;
using System.Linq;
using RagWebScraper.Models;

namespace RagWebScraper.Services
{
    /// <summary>
    /// Clusters documents using TF-IDF features with a KMeans algorithm.
    /// </summary>
    public class TfidfKMeansClusterer : IDocumentClusterer
    {
        private readonly MLContext _mlContext;

        public TfidfKMeansClusterer()
        {
            // Use a fixed seed for deterministic clustering in tests
            _mlContext = new MLContext(seed: 1);
        }

        public Task<DocumentClusteringResult> ClusterAsync(IEnumerable<Document> documents, int numberOfClusters = 5)
        {
            if (documents == null)
                throw new ArgumentNullException(nameof(documents));

            var documentList = documents.ToList();
            if (documentList.Count == 0)
                return Task.FromResult(
                    new DocumentClusteringResult(new Dictionary<Guid, int>(), new ClusterMetrics(0, 0, 0)));

            if (documentList.Count < numberOfClusters)
                throw new InvalidOperationException(
                    $"At least {numberOfClusters} documents are required to form {numberOfClusters} clusters.");

            var data = documentList.Select(d => new DocumentData { Text = d.Text });
            var dataView = _mlContext.Data.LoadFromEnumerable(data);

            var pipeline = _mlContext.Transforms.Text.ProduceHashedWordBags(
                    outputColumnName: "Features",
                    inputColumnName: nameof(DocumentData.Text),
                    numberOfBits: 16,
                    ngramLength: 2,
                    useAllLengths: true)
                .Append(_mlContext.Transforms.NormalizeLpNorm("Features"))
                .AppendCacheCheckpoint(_mlContext)
                .Append(_mlContext.Clustering.Trainers.KMeans(featureColumnName: "Features", numberOfClusters: numberOfClusters));

            var model = pipeline.Fit(dataView);
            var predictions = model.Transform(dataView);

            var metrics = _mlContext.Clustering.Evaluate(predictions, scoreColumnName: "Score", featureColumnName: "Features");

            var predictedClusters = _mlContext.Data
                .CreateEnumerable<ClusterPrediction>(predictions, reuseRowObject: false)
                .ToList();

            var assignments = new Dictionary<Guid, int>(documentList.Count);
            for (int i = 0; i < documentList.Count; i++)
            {
                assignments[documentList[i].Id] = (int)predictedClusters[i].PredictedLabel;
            }

            var clusterMetrics = new ClusterMetrics(
                metrics.AverageDistance,
                metrics.DaviesBouldinIndex,
                metrics.NormalizedMutualInformation);

            var result = new DocumentClusteringResult(assignments, clusterMetrics);

            return Task.FromResult(result);
        }

        private class ClusterPrediction
        {
            public uint PredictedLabel { get; set; }
        }

        private class DocumentData
        {
            public string Text { get; set; } = string.Empty;
        }
    }
}
