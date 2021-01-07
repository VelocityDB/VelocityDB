using System;
using System.Collections.Generic;
using System.Linq;

namespace Frontenac.Blueprints.Util.Wrappers.Batch.Cache
{
    public class StringIdVertexCache : IVertexCache
    {
        private const int InitialCapacity = 1000;

        private readonly StringCompression _compression;
        private Dictionary<string, object> _map;

        public StringIdVertexCache(StringCompression compression)
        {
            if (compression == null)
                throw new ArgumentNullException(nameof(compression));

            _compression = compression;
            _map = new Dictionary<string, object>(InitialCapacity);
        }

        public StringIdVertexCache()
            : this(StringCompression.NoCompression)
        {
        }

        public object GetEntry(object externalId)
        {
            VertexCacheContract.ValidateGetEntry(externalId);

            var id = _compression.Compress(externalId.ToString());
            return _map.Get(id);
        }

        public void Set(IVertex vertex, object externalId)
        {
            VertexCacheContract.ValidateSet(vertex, externalId);

            SetId(vertex, externalId);
        }

        public void SetId(object vertexId, object externalId)
        {
            VertexCacheContract.ValidateSetId(vertexId, externalId);

            var id = _compression.Compress(externalId.ToString());
            _map[id] = vertexId;
        }

        public bool Contains(object externalId)
        {
            VertexCacheContract.ValidateContains(externalId);

            return _map.ContainsKey(_compression.Compress(externalId.ToString()));
        }

        public void NewTransaction()
        {
            _map = _map.ToDictionary(t => t.Key, t => t.Value is IVertex ? (t.Value as IVertex).Id : t.Value);
        }
    }
}