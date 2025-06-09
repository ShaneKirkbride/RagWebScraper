using RagWebScraper.Models;

namespace RagWebScraper.Services
{
    public interface IEntityLinkerService
    {
        IEnumerable<LinkedEntity> LinkEntities(IEnumerable<NamedEntity> entities);
    }

    public interface IRelationDetectorService
    {
        IEnumerable<EntityGraphResult> DetectRelations(DocumentChunk chunk, IEnumerable<NamedEntity> entities);
    }

    public interface IKnowledgeGraphBuilderService
    {
        KnowledgeGraphService Build(IEnumerable<LinkedEntity> entities, IEnumerable<EntityGraphResult> relations);
    }

    public interface IGraphExportService
    {
        string ExportToJson(KnowledgeGraphService graph);
        string ExportToDot(KnowledgeGraphService graph);
    }
}
