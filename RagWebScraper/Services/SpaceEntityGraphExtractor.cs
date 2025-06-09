using System.Text.RegularExpressions;
using RagWebScraper.Models;

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
            var nodes = new Dictionary<string, EntityNode>(StringComparer.OrdinalIgnoreCase);
            var edges = new List<EntityEdge>();
            var sentences = SplitIntoSentences(text);

            foreach (var sentence in sentences)
            {
                var tokenLabels = _nerService.RecognizeTokensWithLabels(sentence);
                var namedEntities = GroupEntities(tokenLabels).ToList();

                foreach (var entity in namedEntities)
                {
                    string id = null;
                    if (entity.Text == null)
                    {
                        id = "0";
                    }
                    else
                    {
                        id = NormalizeId(entity.Text);
                    }

                    if (!nodes.ContainsKey(id))
                    {
                        nodes[id] = new EntityNode(id, entity.Text, entity.Label);
                    }
                }

                // Rule-Based: Look for [EntityA] [Verb/O] [EntityB]
                for (int i = 0; i < tokenLabels.Count - 2; i++)
                {
                    var (token1, label1) = tokenLabels[i];
                    var (token2, label2) = tokenLabels[i + 1];
                    var (token3, label3) = tokenLabels[i + 2];

                    bool isEntityA = label1.StartsWith("B-") || label1.StartsWith("I-");
                    bool isEntityB = label3.StartsWith("B-") || label3.StartsWith("I-");
                    bool isPotentialVerb = label2 == "O"; //TODO: make this better && IsLikelyVerb(token2);

                    if (isEntityA && isPotentialVerb && isEntityB)
                    {
                        var entityA = namedEntities.FirstOrDefault(e => e.Start <= i && e.End > i);
                        var entityB = namedEntities.FirstOrDefault(e => e.Start <= i + 2 && e.End > i + 2);

                        if (entityA != null && entityB != null)
                        {
                            edges.Add(new EntityEdge(
                                SourceId: NormalizeId(entityA.Text),
                                TargetId: NormalizeId(entityB.Text),
                                Relation: token2.ToLowerInvariant()
                            ));
                        }
                    }
                }

                // Fallback: Co-occurrence edge
                for (int i = 0; i < namedEntities.Count - 1; i++)
                {
                    for (int j = i + 1; j < Math.Min(i + 3, namedEntities.Count); j++)
                    {
                        var a = namedEntities[i];
                        var b = namedEntities[j];

                        if (!string.Equals(a.Text, b.Text, StringComparison.OrdinalIgnoreCase))
                        {
                            edges.Add(new EntityEdge(
                                SourceId: NormalizeId(a.Text),
                                TargetId: NormalizeId(b.Text),
                                Relation: "related_to"
                            ));
                        }
                    }
                }
            }

            return new EntityGraph(sourceId, nodes.Values.ToList(), edges);
        }

        private static IEnumerable<NamedEntity> GroupEntities(List<(string Token, string Label)> tokenLabels)
        {
            var entities = new List<NamedEntity>();
            int n = tokenLabels.Count;

            var group = new List<(string Token, string Label, int Index)>();

            for (int i = 0; i < n; i++)
            {
                var (token, label) = tokenLabels[i];

                // Add every token to a group
                group.Add((token, label, i));

                bool isLast = i == n - 1;

                // When label changes or it's the end → process group
                if (isLast || label != tokenLabels[i + 1].Label)
                {
                    // Extract actual entities (skip pure 'O' spans)
                    var hasEntityTag = group.Any(t => t.Label.StartsWith("B-") || t.Label.StartsWith("I-"));

                    if (hasEntityTag)
                    {
                        var entityText = string.Join(" ", group.Select(g => g.Token));
                        var entityLabel = group.First(g => g.Label.StartsWith("B-") || g.Label.StartsWith("I-")).Label[2..];
                        var start = group.First().Index;
                        var end = group.Last().Index + 1;

                        entities.Add(new NamedEntity
                        {
                            Text = entityText,
                            Label = entityLabel,
                            Start = start,
                            End = end
                        });
                    }

                    group.Clear();
                }
            }

            return entities;
        }

        private static string NormalizeId(string text)
        {
            return text.ToLowerInvariant().Replace(" ", "_");
        }

        private static List<string> SplitIntoSentences(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return new List<string>();

            string abbrev = @"(?:Mr|Mrs|Ms|Dr|Prof|Sr|Jr|vs|etc|e\.g|i\.e|U\.S|U\.K|Inc|Ltd|St|Mt|No)\.";
            string pattern = $@"(?<!\b{abbrev})(?<=[.!?])(?=\s?[A-Z])";

            return Regex.Split(text, pattern, RegexOptions.IgnoreCase)
                        .Where(s => !string.IsNullOrWhiteSpace(s))
                        .Select(s => s.Trim())
                        .ToList();
        }
    }
}
