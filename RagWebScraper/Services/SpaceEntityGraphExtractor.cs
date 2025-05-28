using RagWebScraper.Models;
using System.CodeDom;

namespace RagWebScraper.Services
{
    public sealed class SpacyEntityGraphExtractor : IEntityGraphExtractor
    {
        private readonly INerService _nerService;

        public SpacyEntityGraphExtractor(INerService nerService)
        {
            _nerService = nerService;
        }

        public EntityGraph Extract(string text, string sourceId)
        {
            List<NamedEntity> namedEntities = _nerService.RecognizeEntities(text);

            var nodes = namedEntities
                .DistinctBy(e => e.Text.ToLowerInvariant()) // Avoid duplicates
                .Select(e => new EntityNode(
                    Id: e.Text.ToLowerInvariant().Replace(" ", "_"),
                    Label: e.Text,
                    Type: e.Label
                ))
                .ToList();

            var edges = new List<EntityEdge>();

            // Simple co-occurrence-based edge generation
            for (int i = 0; i < namedEntities.Count - 1; i++)
            {
                for (int j = i + 1; j < Math.Min(i + 3, namedEntities.Count); j++)
                {
                    var a = namedEntities[i];
                    var b = namedEntities[j];

                    if (a.Text != b.Text)
                    {
                        edges.Add(new EntityEdge(
                            SourceId: a.Text.ToLowerInvariant().Replace(" ", "_"),
                            TargetId: b.Text.ToLowerInvariant().Replace(" ", "_"),
                            Relation: "related_to"
                        ));
                    }
                }
            }

            return new EntityGraph(sourceId, nodes, edges);
        }
    }
}
