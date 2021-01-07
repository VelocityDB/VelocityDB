using System;

namespace Frontenac.Blueprints.Util.Wrappers.Batch.Cache
{
    public abstract class VertexCacheContract
    {
        public static void ValidateGetEntry(object externalId)
        {
            if (externalId == null)
                throw new ArgumentNullException(nameof(externalId));
        }

        public static void ValidateSet(IVertex vertex, object externalId)
        {
            if (vertex == null)
                throw new ArgumentNullException(nameof(vertex));
            if (externalId == null)
                throw new ArgumentNullException(nameof(externalId));
        }

        public static void ValidateSetId(object vertexId, object externalId)
        {
            if (vertexId == null)
                throw new ArgumentNullException(nameof(vertexId));
            if (externalId == null)
                throw new ArgumentNullException(nameof(externalId));
        }

        public static void ValidateContains(object externalId)
        {
            if (externalId == null)
                throw new ArgumentNullException(nameof(externalId));
        }
    }
}