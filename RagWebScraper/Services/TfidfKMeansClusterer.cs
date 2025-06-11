using Microsoft.ML;
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

        public Task<Dictionary<Guid, int>> ClusterAsync(IEnumerable<Document> documents, int numberOfClusters = 5)
        {
            if (documents == null)
                throw new ArgumentNullException(nameof(documents));

            var documentList = documents.ToList();
            if (documentList.Count == 0)
                return Task.FromResult(new Dictionary<Guid, int>());

            var data = documentList.Select(d => new DocumentData { Text = d.Text });
            var dataView = _mlContext.Data.LoadFromEnumerable(data);

            var pipeline = _mlContext.Transforms.Text.FeaturizeText("Features", nameof(DocumentData.Text))
                .Append(_mlContext.Transforms.NormalizeLpNorm("Features"))
                .AppendCacheCheckpoint(_mlContext)
                .Append(_mlContext.Clustering.Trainers.KMeans(featureColumnName: "Features", numberOfClusters: numberOfClusters));

            var model = pipeline.Fit(dataView);
            var predictions = model.Transform(dataView);

            var predictedClusters = _mlContext.Data.CreateEnumerable<ClusterPrediction>(predictions, reuseRowObject: false).ToList();

            var result = new Dictionary<Guid, int>(documentList.Count);
            for (int i = 0; i < documentList.Count; i++)
            {
                result[documentList[i].Id] = (int)predictedClusters[i].PredictedLabel;
            }

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
