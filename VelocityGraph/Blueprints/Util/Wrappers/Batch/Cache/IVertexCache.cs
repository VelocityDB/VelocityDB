namespace Frontenac.Blueprints.Util.Wrappers.Batch.Cache
{
    public interface IVertexCache
    {
        object GetEntry(object externalId);

        void Set(IVertex vertex, object externalId);

        void SetId(object vertexId, object externalId);

        bool Contains(object externalId);

        void NewTransaction();
    }
}