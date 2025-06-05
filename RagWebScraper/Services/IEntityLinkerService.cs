using RagWebScraper.Models;

namespace RagWebScraper.Services
{
    public interface IEntityLinkerService
    {
        IEnumerable<LinkedEntity> LinkEntities(IEnumerable<NamedEntity> entities);
    }

    public interface IRelationDetectorService
    {
        IEnumerable<EntityRelation> DetectRelations(DocumentChunk chunk, IEnumerable<NamedEntity> entities);
    }

    public interface IKnowledgeGraphBuilderService
    {
        KnowledgeGraph Build(IEnumerable<LinkedEntity> entities, IEnumerable<EntityRelation> relations);
    }

    public interface IGraphExportService
    {
        string ExportToJson(KnowledgeGraph graph);
        string ExportToDot(KnowledgeGraph graph);
    }
}
