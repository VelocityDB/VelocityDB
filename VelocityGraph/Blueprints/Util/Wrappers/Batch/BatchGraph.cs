using System;
using System.Collections.Generic;
using System.Diagnostics;
using Frontenac.Blueprints.Util.Wrappers.Batch.Cache;
using System.Linq;
using Frontenac.Blueprints.Contracts;

namespace Frontenac.Blueprints.Util.Wrappers.Batch
{
    /// <summary>
    ///     BatchGraph is a wrapper that enables batch loading of a large number of edges and vertices by chunking the entire
    ///     load into smaller batches and maintaining a memory-efficient vertex cache so that the entire transactional state can
    ///     be flushed after each chunk is loaded.
    ///     <br />
    ///     BatchGraph is ONLY meant for loading data and does not support any retrieval or removal operations.
    ///     That is, BatchGraph only supports the following methods:
    ///     - addVertex for adding vertices
    ///     - addEdge for adding edges
    ///     - getVertex to be used when adding edges
    ///     - Property getter, setter and removal methods for vertices and edges.
    ///     <br />
    ///     An important limitation of BatchGraph is that edge properties can only be set immediately after the edge has been added.
    ///     If other vertices or edges have been created in the meantime, setting, getting or removing properties will throw
    ///     exceptions. This is done to avoid caching of edges which would require a great amount of memory.
    ///     <br />
    ///     BatchGraph wraps TransactionalGraph. To wrap arbitrary graphs, use wrap which will additionally wrap non-transactional.
    ///     <br />
    ///     BatchGraph can also automatically set the provided element ids as properties on the respective element. Use
    ///     setVertexIdKey and setEdgeIdKey to set the keys for the vertex and edge properties
    ///     respectively. This allows to make the loaded baseGraph compatible for later wrapping with idInnerTinkerGrapĥ.
    /// </summary>
    public class BatchGraph : ITransactionalGraph, IWrapperGraph
    {
        /// <summary>
        ///     Default buffer size
        /// </summary>
        public const long DefaultBufferSize = 100000;

        private readonly ITransactionalGraph _baseGraph;
        private readonly long _bufferSize = DefaultBufferSize;
        private readonly IVertexCache _cache;
        private BatchEdge _currentEdge;
        private IEdge _currentEdgeCached;
        private string _edgeIdKey;
        private bool _loadingFromScratch = true;
        private object _previousOutVertexId;
        private long _remainingBufferSize;
        private string _vertexIdKey;

        /// <summary>
        ///     Constructs a BatchGraph wrapping the provided baseGraph, using the specified buffer size and expecting vertex ids of
        ///     the specified IdType. Supplying vertex ids which do not match this type will throw exceptions.
        /// </summary>
        /// <param name="graph">Graph to be wrapped</param>
        /// <param name="type"> Type of vertex id expected. This information is used to optimize the vertex cache memory footprint.</param>
        /// <param name="bufferSize">Defines the number of vertices and edges loaded before starting a new transaction. The larger this value, 
        /// the more memory is required but the faster the loading process.</param>
        public BatchGraph(ITransactionalGraph graph, VertexIdType type, long bufferSize)
        {
            if (graph == null)
                throw new ArgumentNullException(nameof(graph));
            if (bufferSize <= 0)
                throw new ArgumentException("bufferSize must be greater than zero");

            _baseGraph = graph;
            _bufferSize = bufferSize;
            _vertexIdKey = null;
            _edgeIdKey = null;
            _cache = type.GetVertexCache();
            _remainingBufferSize = _bufferSize;
        }

        /// <summary>
        ///     Constructs a BatchGraph wrapping the provided baseGraph.
        /// </summary>
        /// <param name="graph">Graph to be wrapped</param>
        public BatchGraph(ITransactionalGraph graph)
            : this(graph, VertexIdType.Object, DefaultBufferSize)
        {
            if (graph == null)
                throw new ArgumentNullException(nameof(graph));
        }

        /// <summary>
        ///     Should only be invoked after loading is complete. Committing the transaction before will cause the loading to fail.
        /// </summary>
        public void Commit()
        {
            _currentEdge = null;
            _currentEdgeCached = null;
            _remainingBufferSize = 0;
            _baseGraph.Commit();
        }

        /// <summary>
        ///     Not supported for batch loading, since data may have already been partially persisted.
        /// </summary>
        public void Rollback()
        {
            throw new InvalidOperationException();
        }

        public void Shutdown()
        {
            _baseGraph.Commit();
            _baseGraph.Shutdown();
            _currentEdge = null;
            _currentEdgeCached = null;
        }

        public Features Features
        {
            get
            {
                var features = _baseGraph.Features.CopyFeatures();
                features.IgnoresSuppliedIds = false;
                features.IsWrapper = true;
                features.SupportsEdgeIteration = false;
                features.SupportsThreadedTransactions = false;
                features.SupportsVertexIteration = false;
                return features;
            }
        }

        /// <note>
        ///     If the input data are sorted, then out vertex will be repeated for several edges in a row.
        ///     In this case, bypass cache and instead immediately return a new vertex using the known id.
        ///     This gives a modest performance boost, especially when the cache is large or there are
        ///     on average many edges per vertex.
        /// </note>
        public IVertex GetVertex(object id)
        {
            GraphContract.ValidateGetVertex(id);
            if ((_previousOutVertexId != null) && (_previousOutVertexId == id))
                return new BatchVertex(_previousOutVertexId, this);
            var v = RetrieveFromCache(id);
            if (v == null)
            {
                if (_loadingFromScratch) return null;
                if (_baseGraph.Features.IgnoresSuppliedIds)
                {
                    Debug.Assert(_vertexIdKey != null);
                    var iter = _baseGraph.GetVertices(_vertexIdKey, id).GetEnumerator();
                    if (!iter.MoveNext()) return null;
                    v = iter.Current;
                    if (iter.MoveNext())
                        throw new ArgumentException(
                            string.Concat("There are multiple vertices with the provided id in the database: ", id));
                }
                else
                {
                    v = _baseGraph.GetVertex(id);
                    if (v == null) return null;
                }
                _cache.Set(v, id);
            }
            return new BatchVertex(id, this);
        }

        public IVertex AddVertex(object id)
        {
            if (RetrieveFromCache(id) != null) throw ExceptionFactory.VertexWithIdAlreadyExists(id);
            NextElement();

            var v = _baseGraph.AddVertex(id);
            if (_vertexIdKey != null)
                v.SetProperty(_vertexIdKey, id);

            _cache.Set(v, id);
            return new BatchVertex(id, this);
        }

        public IEdge AddEdge(object id, IVertex outVertex, IVertex inVertex, string label)
        {
            GraphContract.ValidateAddEdge(id, outVertex, inVertex, label);

            if (!(outVertex is BatchVertex) || !(inVertex is BatchVertex))
                throw new ArgumentException("Given element was not created in this baseGraph");
            NextElement();

            var ov = GetCachedVertex(outVertex.Id);
            var iv = GetCachedVertex(inVertex.Id);

            _previousOutVertexId = outVertex.Id; //keep track of the previous out vertex id

            if (ov != null && iv != null)
            {
                _currentEdgeCached = _baseGraph.AddEdge(id, ov, iv, label);
                if (_edgeIdKey != null && id != null)
                    _currentEdgeCached.SetProperty(_edgeIdKey, id);
            }

            _currentEdge = new BatchEdge(this);
            return _currentEdge;
        }

        // ################### Unsupported Graph Methods ####################

        public IEdge GetEdge(object id)
        {
            throw RetrievalNotSupported();
        }

        public void RemoveVertex(IVertex vertex)
        {
            GraphContract.ValidateRemoveVertex(vertex);
            throw RetrievalNotSupported();
        }

        public IEnumerable<IVertex> GetVertices()
        {
            throw RetrievalNotSupported();
        }

        public IEnumerable<IVertex> GetVertices(string key, object value)
        {
            GraphContract.ValidateGetVertices(key, value);
            throw RetrievalNotSupported();
        }

        public void RemoveEdge(IEdge edge)
        {
            throw RetrievalNotSupported();
        }

        public IEnumerable<IEdge> GetEdges()
        {
            throw RetrievalNotSupported();
        }

        public IEnumerable<IEdge> GetEdges(string key, object value)
        {
            throw RetrievalNotSupported();
        }

        public IQuery Query()
        {
            throw RetrievalNotSupported();
        }

        public IGraph GetBaseGraph()
        {
            return _baseGraph;
        }

        /// <summary>
        ///     Constructs a BatchGraph wrapping the provided baseGraph. Immediately returns the baseGraph if its a BatchGraph
        ///     and wraps non-transactional graphs in an additional WritethroughGraph.
        /// </summary>
        /// <param name="graph">Graph to be wrapped</param>
        /// <returns>a BatchGraph wrapping the provided baseGraph</returns>
        public static BatchGraph Wrap(IGraph graph)
        {
            if (graph == null)
                throw new ArgumentNullException(nameof(graph));

            var wrap = graph as BatchGraph;
            if (wrap != null) return wrap;
            var transactionalGraph = graph as ITransactionalGraph;
            return transactionalGraph != null
                       ? new BatchGraph(transactionalGraph)
                       : new BatchGraph(new WritethroughGraph(graph));
        }

        /// <summary>
        ///     Constructs a BatchGraph wrapping the provided baseGraph. Immediately returns the baseGraph if its a BatchGraph
        ///     and wraps non-transactional graphs in an additional WritethroughGraph.
        /// </summary>
        /// <param name="graph">Graph to be wrapped</param>
        /// <param name="buffer">Size of the buffer</param>
        /// <returns>a BatchGraph wrapping the provided baseGraph</returns>
        public static BatchGraph Wrap(IGraph graph, long buffer)
        {
            if (graph == null)
                throw new ArgumentNullException(nameof(graph));
            if (buffer <= 0)
                throw new ArgumentException("buffer must be greater than zero");

            var wrap = graph as BatchGraph;
            if (wrap != null) return wrap;
            return graph is ITransactionalGraph
                       ? new BatchGraph((ITransactionalGraph) graph, VertexIdType.Object, buffer)
                       : new BatchGraph(new WritethroughGraph(graph), VertexIdType.Object, buffer);
        }

        /// <summary>
        ///     Sets the key to be used when setting the vertex id as a property on the respective vertex.
        ///     If the key is null, then no property will be set.
        ///     If the loaded baseGraph should later be wrapped with idInnerTinkerGrapĥ use idInnerTinkerGrapĥ.ID.
        /// </summary>
        /// <param name="key">key Key to be used.</param>
        public void SetVertexIdKey(string key)
        {
            bool? ignoresSuppliedIds = _baseGraph.Features.IgnoresSuppliedIds;
            if (ignoresSuppliedIds != null && (!_loadingFromScratch && key == null && ignoresSuppliedIds.Value))
                throw new InvalidOperationException(
                    "Cannot set vertex id key to null when not loading from scratch while ids are ignored.");
            _vertexIdKey = key;
        }

        /// <summary>
        ///     Returns the key used to set the id on the vertices or null if such has not been set
        ///     via setVertexIdKey
        /// </summary>
        /// <returns>The key used to set the id on the vertices or null if such has not been set</returns>
        public string GetVertexIdKey()
        {
            return _vertexIdKey;
        }

        /// <summary>
        ///     Sets the key to be used when setting the edge id as a property on the respective edge.
        ///     If the key is null, then no property will be set.
        ///     If the loaded baseGraph should later be wrapped with IdGraphuse idInnerTinkerGrapĥ.ID.
        /// </summary>
        /// <param name="key">Key to be used.</param>
        public void SetEdgeIdKey(string key)
        {
            _edgeIdKey = key;
        }

        /// <summary>
        ///     Returns the key used to set the id on the edges or null if such has not been set
        ///     via setEdgeIdKey
        /// </summary>
        /// <returns>The key used to set the id on the edges or null if such has not been set</returns>
        public string GetEdgeIdKey()
        {
            return _edgeIdKey;
        }

        /// <summary>
        ///     Sets whether the Graph loaded through this instance of BatchGraph is loaded from scratch
        ///     (i.e. the wrapped Graph is initially empty) or whether Graph is loaded incrementally into an
        ///     existing Graph.
        ///     In the former case, BatchGraph does not need to check for the existence of vertices with the wrapped
        ///     Graph but only needs to consult its own cache which can be significantly faster. In the latter case,
        ///     the cache is checked first but an additional check against the wrapped Graph may be necessary if
        ///     the vertex does not exist.
        ///     By default, BatchGraph assumes that the data is loaded from scratch.
        ///     When setting loading from scratch to false, a vertex id key must be specified first using
        ///     setVertexIdKey - otherwise an exception is thrown.
        /// </summary>
        /// <param name="fromScratch">Sets whether the Graph loaded through this instance of BatchGraph is loaded from scratch</param>
        public void SetLoadingFromScratch(bool fromScratch)
        {
            if (_baseGraph.Features.IgnoresSuppliedIds && (fromScratch == false && _vertexIdKey == null))
                throw new InvalidOperationException(
                    "Vertex id key is required to query existing vertices in wrapped Graph.");
            _loadingFromScratch = fromScratch;
        }

        /// <summary>
        ///     Whether this BatchGraph is loading data from scratch or incrementally into an existing Graph.
        ///     By default, this returns true.
        ///     see setLoadingFromScratch
        /// </summary>
        /// <returns>Whether this BatchGraph is loading data from scratch or incrementally into an existing Graph.</returns>
        public bool IsLoadingFromScratch()
        {
            return _loadingFromScratch;
        }

        private void NextElement()
        {
            _currentEdge = null;
            _currentEdgeCached = null;
            if (_remainingBufferSize <= 0)
            {
                _baseGraph.Commit();
                _cache.NewTransaction();
                _remainingBufferSize = _bufferSize;
            }
            _remainingBufferSize--;
        }

        private IVertex RetrieveFromCache(object externalId)
        {
            if (externalId == null)
                throw new ArgumentNullException(nameof(externalId));

            var internal_ = _cache.GetEntry(externalId);
            var cache = internal_ as IVertex;
            if (cache != null)
                return cache;
            if (internal_ != null)
            {
                //its an internal id
                var v = _baseGraph.GetVertex(internal_);
                _cache.Set(v, externalId);
                return v;
            }
            return null;
        }

        private IVertex GetCachedVertex(object externalId)
        {
            if (externalId == null)
                throw new ArgumentNullException(nameof(externalId));

            var v = RetrieveFromCache(externalId);

            return v;
        }

        protected IEdge AddEdgeSupport(IVertex outVertex, IVertex inVertex, string label)
        {
            if (outVertex == null)
                throw new ArgumentNullException(nameof(outVertex));
            if (inVertex == null)
                throw new ArgumentNullException(nameof(inVertex));

            return AddEdge(null, outVertex, inVertex, label);
        }

        public override string ToString()
        {
            return this.GraphString(_baseGraph.ToString());
        }

        private static InvalidOperationException RetrievalNotSupported()
        {
            return new InvalidOperationException("Retrieval operations are not supported during batch loading");
        }

        private class BatchEdge : DictionaryElement, IEdge
        {
            private readonly BatchGraph _batchInnerTinkerGrapĥ;

            public BatchEdge(BatchGraph batchInnerTinkerGrapĥ):base(batchInnerTinkerGrapĥ)
            {
                if (batchInnerTinkerGrapĥ == null)
                    throw new ArgumentNullException(nameof(batchInnerTinkerGrapĥ));

                _batchInnerTinkerGrapĥ = batchInnerTinkerGrapĥ;
            }

            public IVertex GetVertex(Direction direction)
            {
                EdgeContract.ValidateGetVertex(direction);
                return GetWrappedEdge().GetVertex(direction);
            }

            public string Label
            {
                get { return GetWrappedEdge().Label; }
            }

            public override void SetProperty(string key, object value)
            {
                ElementContract.ValidateSetProperty(key, value);
                GetWrappedEdge().SetProperty(key, value);
            }

            public override object Id
            {
                get { return GetWrappedEdge().Id; }
            }

            public override object GetProperty(string key)
            {
                ElementContract.ValidateGetProperty(key);

                return GetWrappedEdge().GetProperty(key);
            }

            public override IEnumerable<string> GetPropertyKeys()
            {
                return GetWrappedEdge().GetPropertyKeys();
            }

            public override object RemoveProperty(string key)
            {
                ElementContract.ValidateRemoveProperty(key);
                return GetWrappedEdge().RemoveProperty(key);
            }

            public override void Remove()
            {
                _batchInnerTinkerGrapĥ.RemoveEdge(this);
            }

            private IEdge GetWrappedEdge()
            {
                return _batchInnerTinkerGrapĥ._currentEdgeCached;
            }

            public override string ToString()
            {
                return GetWrappedEdge().ToString();
            }
        }

        private class BatchVertex : DictionaryElement, IVertex
        {
            private readonly BatchGraph _batchInnerTinkerGrapĥ;
            private readonly object _externalId;

            public BatchVertex(object id, BatchGraph batchInnerTinkerGrapĥ):base(batchInnerTinkerGrapĥ)
            {
                if (id == null)
                    throw new ArgumentNullException(nameof(id));
                if (batchInnerTinkerGrapĥ == null)
                    throw new ArgumentNullException(nameof(batchInnerTinkerGrapĥ));

                if (id == null) throw new ArgumentNullException("id");
                _externalId = id;
                _batchInnerTinkerGrapĥ = batchInnerTinkerGrapĥ;
            }

            public IEnumerable<IEdge> GetEdges(Direction direction, params string[] labels)
            {
                throw RetrievalNotSupported();
            }

            public IEnumerable<IVertex> GetVertices(Direction direction, params string[] labels)
            {
                throw RetrievalNotSupported();
            }

            public IVertexQuery Query()
            {
                throw RetrievalNotSupported();
            }

            public IEdge AddEdge(object id, string label, IVertex inVertex)
            {
                VertexContract.ValidateAddEdge(id, label, inVertex);

                return _batchInnerTinkerGrapĥ.AddEdgeSupport(this, inVertex, label);
            }

            public override void SetProperty(string key, object value)
            {
                ElementContract.ValidateSetProperty(key, value);
                var cachedVertex = _batchInnerTinkerGrapĥ.GetCachedVertex(_externalId);
                if(cachedVertex != null)
                    cachedVertex.SetProperty(key, value);
            }

            public override object Id
            {
                get { return _externalId; }
            }

            public override object GetProperty(string key)
            {
                ElementContract.ValidateGetProperty(key);
                object result  = null;
                var chachedVertex = _batchInnerTinkerGrapĥ.GetCachedVertex(_externalId);
                if(chachedVertex != null)
                    result = chachedVertex.GetProperty(key);
                return result;
            }

            public override IEnumerable<string> GetPropertyKeys()
            {
                var chachedVertex = _batchInnerTinkerGrapĥ.GetCachedVertex(_externalId);
                return chachedVertex != null ? chachedVertex.GetPropertyKeys() : Enumerable.Empty<string>();
            }

            public override object RemoveProperty(string key)
            {
                ElementContract.ValidateRemoveProperty(key);
                object result = null;
                var cachedVertex = _batchInnerTinkerGrapĥ.GetCachedVertex(_externalId);
                if(cachedVertex != null)
                    result = cachedVertex.RemoveProperty(key);
                return result;
            }

            public override void Remove()
            {
                _batchInnerTinkerGrapĥ.RemoveVertex(this);
            }

            public override string ToString()
            {
                return string.Concat("v[", _externalId, "]");
            }
        }
    }
}