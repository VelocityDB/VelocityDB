using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using Frontenac.Blueprints.Contracts;
using Frontenac.Blueprints.Util;

namespace Frontenac.Blueprints.Impls.TG
{
    /// <summary>
    ///     An in-memory, reference implementation of the property TinkerGrapĥ interfaces provided by Blueprints.
    /// </summary>
    [Serializable]
    public class TinkerGrapĥ : IIndexableGraph, IKeyIndexableGraph
    {
        public enum FileType
        {
            DotNet,
            Gml,
            Graphml,
            Graphson
        }

        private static readonly Features TinkerGraphFeatures = new Features();
        private static readonly Features PersistentFeatures;
        private readonly string _directory;
        private readonly FileType _fileType;

        internal long CurrentId = 0;
        internal TinkerKeyIndex EdgeKeyIndex;
        protected ConcurrentDictionary<string, IEdge> Edges = new ConcurrentDictionary<string, IEdge>();
        internal ConcurrentDictionary<string, TinkerIndex> Indices = new ConcurrentDictionary<string, TinkerIndex>();
        protected ConcurrentDictionary<string, IVertex> InnerVertices = new ConcurrentDictionary<string, IVertex>();

        internal TinkerKeyIndex VertexKeyIndex;

        static TinkerGrapĥ()
        {
            TinkerGraphFeatures.SupportsDuplicateEdges = true;
            TinkerGraphFeatures.SupportsSelfLoops = true;
            TinkerGraphFeatures.SupportsSerializableObjectProperty = true;
            TinkerGraphFeatures.SupportsBooleanProperty = true;
            TinkerGraphFeatures.SupportsDoubleProperty = true;
            TinkerGraphFeatures.SupportsFloatProperty = true;
            TinkerGraphFeatures.SupportsIntegerProperty = true;
            TinkerGraphFeatures.SupportsPrimitiveArrayProperty = true;
            TinkerGraphFeatures.SupportsUniformListProperty = true;
            TinkerGraphFeatures.SupportsMixedListProperty = true;
            TinkerGraphFeatures.SupportsLongProperty = true;
            TinkerGraphFeatures.SupportsMapProperty = true;
            TinkerGraphFeatures.SupportsStringProperty = true;

            TinkerGraphFeatures.IgnoresSuppliedIds = false;
            TinkerGraphFeatures.IsPersistent = false;
            TinkerGraphFeatures.IsRdfModel = false;
            TinkerGraphFeatures.IsWrapper = false;

            TinkerGraphFeatures.SupportsIndices = true;
            TinkerGraphFeatures.SupportsKeyIndices = true;
            TinkerGraphFeatures.SupportsVertexKeyIndex = true;
            TinkerGraphFeatures.SupportsEdgeKeyIndex = true;
            TinkerGraphFeatures.SupportsVertexIndex = true;
            TinkerGraphFeatures.SupportsEdgeIndex = true;
            TinkerGraphFeatures.SupportsTransactions = false;
            TinkerGraphFeatures.SupportsVertexIteration = true;
            TinkerGraphFeatures.SupportsEdgeIteration = true;
            TinkerGraphFeatures.SupportsEdgeRetrieval = true;
            TinkerGraphFeatures.SupportsVertexProperties = true;
            TinkerGraphFeatures.SupportsEdgeProperties = true;
            TinkerGraphFeatures.SupportsThreadedTransactions = false;
            TinkerGraphFeatures.SupportsIdProperty = false;
            TinkerGraphFeatures.SupportsLabelProperty = false;

            PersistentFeatures = TinkerGraphFeatures.CopyFeatures();
            PersistentFeatures.IsPersistent = true;
        }

        public TinkerGrapĥ(string directory, FileType fileType)
        {
            if (string.IsNullOrWhiteSpace(directory))
                throw new ArgumentNullException(nameof(directory));

            VertexKeyIndex = new TinkerKeyIndex(typeof (TinkerVertex), this);
            EdgeKeyIndex = new TinkerKeyIndex(typeof (TinkerEdge), this);

            _directory = directory;
            _fileType = fileType;

            if (!Directory.Exists(_directory))
                Directory.CreateDirectory(_directory);
            else
            {
                var tinkerStorage = TinkerStorageFactory.GetInstance().GetTinkerStorage(fileType);
                var graph = tinkerStorage.Load(directory);

                InnerVertices = graph.InnerVertices;
                Edges = graph.Edges;
                CurrentId = graph.CurrentId;
                Indices = graph.Indices;
                VertexKeyIndex = graph.VertexKeyIndex;
                EdgeKeyIndex = graph.EdgeKeyIndex;
            }
        }

        public TinkerGrapĥ(string directory)
            : this(directory, FileType.DotNet)
        {
        }

        public TinkerGrapĥ()
        {
            VertexKeyIndex = new TinkerKeyIndex(typeof (TinkerVertex), this);
            EdgeKeyIndex = new TinkerKeyIndex(typeof (TinkerEdge), this);
            _directory = null;
            _fileType = FileType.DotNet;
        }

        public virtual IEnumerable<IVertex> GetVertices(string key, object value)
        {
            GraphContract.ValidateGetVertices(key, value);
            return VertexKeyIndex.GetIndexedKeys().Contains(key)
                       ? VertexKeyIndex.Get(key, value).Cast<IVertex>()
                       : new PropertyFilteredIterable<IVertex>(key, value, GetVertices());
        }

        public virtual IEnumerable<IEdge> GetEdges(string key, object value)
        {
            GraphContract.ValidateGetEdges(key, value);
            if (EdgeKeyIndex.GetIndexedKeys().Contains(key))
                return EdgeKeyIndex.Get(key, value).Cast<IEdge>();
            return new PropertyFilteredIterable<IEdge>(key, value, GetEdges());
        }

        public virtual IIndex CreateIndex(string indexName, Type indexClass, params Parameter[] indexParameters)
        {
            IndexableGraphContract.ValidateCreateIndex(indexName, indexClass, indexParameters);

            if (Indices.ContainsKey(indexName))
                throw ExceptionFactory.IndexAlreadyExists(indexName);

            var index = new TinkerIndex(indexName, indexClass);
            Indices.Put(index.Name, index);
            return index;
        }

        public virtual IIndex GetIndex(string indexName, Type indexClass)
        {
            IndexableGraphContract.ValidateGetIndex(indexName, indexClass);

            var index = Indices.Get(indexName);
            if (null == index)
                return null;
            if (!indexClass.IsAssignableFrom(index.Type))
                throw ExceptionFactory.IndexDoesNotSupportClass(indexName, indexClass);
            return index;
        }

        public virtual IEnumerable<IIndex> GetIndices()
        {
            return Indices.Values.ToList();
        }

        public virtual void DropIndex(string indexName)
        {
            IndexableGraphContract.ValidateDropIndex(indexName);

            TinkerIndex value;
            Indices.TryRemove(indexName, out value);
        }

        public virtual IVertex AddVertex(object id)
        {
            string idString = null;
            IVertex vertex;
            if (null != id)
            {
                idString = id.ToString();
                vertex = InnerVertices.Get(idString);
                if (null != vertex)
                    throw ExceptionFactory.VertexWithIdAlreadyExists(id);
            }
            else
            {
                var done = false;
                while (!done)
                {
                    idString = GetNextId();
                    vertex = InnerVertices.Get(idString);
                    if (null == vertex)
                        done = true;
                }
            }

            vertex = new TinkerVertex(idString, this);
            InnerVertices.Put(vertex.Id.ToString(), vertex);
            return vertex;
        }

        public virtual IVertex GetVertex(object id)
        {
            GraphContract.ValidateGetVertex(id);
            var idString = id.ToString();
            return InnerVertices.Get(idString);
        }

        public virtual IEdge GetEdge(object id)
        {
            GraphContract.ValidateGetEdge(id);
            var idString = id.ToString();
            return Edges.Get(idString);
        }

        public virtual IEnumerable<IVertex> GetVertices()
        {
            return new List<IVertex>(InnerVertices.Values);
        }

        public virtual IEnumerable<IEdge> GetEdges()
        {
            return new List<IEdge>(Edges.Values);
        }

        public virtual void RemoveVertex(IVertex vertex)
        {
            GraphContract.ValidateRemoveVertex(vertex);

            foreach (var edge in vertex.GetEdges(Direction.Both))
                RemoveEdge(edge);

            VertexKeyIndex.RemoveElement(vertex);
            foreach (var idx in GetIndices().Where(t => t.Type == typeof (IVertex)).Cast<TinkerIndex>())
            {
                idx.RemoveElement(vertex);
            }

            IVertex removedVertex;
            InnerVertices.TryRemove(vertex.Id.ToString(), out removedVertex);
        }

        public virtual IEdge AddEdge(object id, IVertex outVertex, IVertex inVertex, string label)
        {
            GraphContract.ValidateAddEdge(id, outVertex, inVertex, label);

            string idString = null;
            IEdge edge;
            if (null != id)
            {
                idString = id.ToString();
                edge = Edges.Get(idString);
                if (null != edge)
                {
                    throw ExceptionFactory.EdgeWithIdAlreadyExist(id);
                }
            }
            else
            {
                var done = false;
                while (!done)
                {
                    idString = GetNextId();
                    edge = Edges.Get(idString);
                    if (null == edge)
                        done = true;
                }
            }

            edge = new TinkerEdge(idString, outVertex, inVertex, label, this);
            Edges.Put(edge.Id.ToString(), edge);
            var out_ = (TinkerVertex) outVertex;
            var in_ = (TinkerVertex) inVertex;
            out_.AddOutEdge(label, edge);
            in_.AddInEdge(label, edge);
            return edge;
        }

        public virtual void RemoveEdge(IEdge edge)
        {
            GraphContract.ValidateRemoveEdge(edge);

            var outVertex = (TinkerVertex) edge.GetVertex(Direction.Out);
            var inVertex = (TinkerVertex) edge.GetVertex(Direction.In);
            IEdge removedEdge;
            if (null != outVertex && null != outVertex.OutEdges)
            {
                var e = outVertex.OutEdges.Get(edge.Label);
                if (null != e)
                {
                    e.TryRemove(edge.Id.ToString(), out removedEdge);
                }
            }
            if (null != inVertex && null != inVertex.InEdges)
            {
                var e = inVertex.InEdges.Get(edge.Label);
                if (null != e)
                {
                    e.TryRemove(edge.Id.ToString(), out removedEdge);
                }
            }


            EdgeKeyIndex.RemoveElement(edge);
            foreach (var idx in GetIndices().Where(t => t.Type == typeof (IEdge)).Cast<TinkerIndex>())
            {
                idx.RemoveElement(edge);
            }

            Edges.TryRemove(edge.Id.ToString(), out removedEdge);
        }

        public virtual IQuery Query()
        {
            return new DefaultGraphQuery(this);
        }

        public void Shutdown()
        {
            if (null == _directory) return;
            var tinkerStorage = TinkerStorageFactory.GetInstance().GetTinkerStorage(_fileType);
            tinkerStorage.Save(this, _directory);
        }

        public virtual Features Features
        {
            get { return null == _directory ? TinkerGraphFeatures : PersistentFeatures; }
        }

        public virtual void CreateKeyIndex(string key, Type elementClass, params Parameter[] indexParameters)
        {
            KeyIndexableGraphContract.ValidateCreateKeyIndex(key, elementClass, indexParameters);

            if (typeof (IVertex).IsAssignableFrom(elementClass))
                VertexKeyIndex.CreateKeyIndex(key);
            else
                EdgeKeyIndex.CreateKeyIndex(key);
        }

        public virtual void DropKeyIndex(string key, Type elementClass)
        {
            KeyIndexableGraphContract.ValidateDropKeyIndex(key, elementClass);

            if (typeof (IVertex).IsAssignableFrom(elementClass))
                VertexKeyIndex.DropKeyIndex(key);
            else
                EdgeKeyIndex.DropKeyIndex(key);
        }

        public virtual IEnumerable<string> GetIndexedKeys(Type elementClass)
        {
            KeyIndexableGraphContract.ValidateGetIndexedKeys(elementClass);

            return typeof (IVertex).IsAssignableFrom(elementClass)
                       ? VertexKeyIndex.GetIndexedKeys()
                       : EdgeKeyIndex.GetIndexedKeys();
        }

        public override string ToString()
        {
            if (null == _directory)
                return this.GraphString(string.Concat("vertices:",
                                               InnerVertices.LongCount().ToString(CultureInfo.InvariantCulture),
                                               " edges:",
                                               Edges.LongCount().ToString(CultureInfo.InvariantCulture)));

            return this.GraphString(string.Concat("vertices:",
                                           InnerVertices.LongCount().ToString(CultureInfo.InvariantCulture),
                                           " edges:",
                                           Edges.LongCount().ToString(CultureInfo.InvariantCulture),
                                           " directory:", _directory));
        }

        public void Clear()
        {
            InnerVertices.Clear();
            Edges.Clear();
            Indices.Clear();
            CurrentId = 0;
            VertexKeyIndex = new TinkerKeyIndex(typeof (TinkerVertex), this);
            EdgeKeyIndex = new TinkerKeyIndex(typeof (TinkerEdge), this);
        }

        private string GetNextId()
        {
            string idString;
            while (true)
            {
                idString = CurrentId.ToString(CultureInfo.InvariantCulture);
                CurrentId++;
                if (null == InnerVertices.Get(idString) || null == Edges.Get(idString) || CurrentId == long.MaxValue)
                    break;
            }
            return idString;
        }

        [Serializable]
        internal class TinkerKeyIndex : TinkerIndex
        {
            private readonly TinkerGrapĥ _tinkerGrapĥ;
            private readonly HashSet<string> _indexedKeys = new HashSet<string>();

            public TinkerKeyIndex(Type indexClass, TinkerGrapĥ tinkerGrapĥ)
                : base(null, indexClass)
            {
                if (indexClass == null)
                    throw new ArgumentNullException(nameof(indexClass));
                if (tinkerGrapĥ == null)
                    throw new ArgumentNullException(nameof(tinkerGrapĥ));

                _tinkerGrapĥ = tinkerGrapĥ;
            }

            public void AutoUpdate(string key, object newValue, object oldValue, TinkerElement element)
            {
                if (string.IsNullOrWhiteSpace(key))
                    throw new ArgumentNullException(nameof(key));

                if (element == null)
                    throw new ArgumentNullException(nameof(element));

                if (!_indexedKeys.Contains(key)) return;
                if (oldValue != null)
                    Remove(key, oldValue, element);
                Put(key, newValue, element);
            }

            public void AutoRemove(string key, object oldValue, TinkerElement element)
            {
                if (string.IsNullOrWhiteSpace(key))
                    throw new ArgumentNullException(nameof(key));

                if (element == null)
                    throw new ArgumentNullException(nameof(element));

                if (_indexedKeys.Contains(key))
                    Remove(key, oldValue, element);
            }

            public void CreateKeyIndex(string key)
            {
                if (string.IsNullOrWhiteSpace(key))
                    throw new ArgumentNullException(nameof(key));

                if (_indexedKeys.Contains(key))
                    return;

                _indexedKeys.Add(key);

                if (typeof (TinkerVertex) == IndexClass)
                    _tinkerGrapĥ.ReIndexElements(_tinkerGrapĥ.GetVertices(), new[] {key});
                else
                    _tinkerGrapĥ.ReIndexElements(_tinkerGrapĥ.GetEdges(), new[] {key});
            }

            public void DropKeyIndex(string key)
            {
                if (string.IsNullOrWhiteSpace(key))
                    throw new ArgumentNullException(nameof(key));

                if (!_indexedKeys.Contains(key))
                    return;

                _indexedKeys.Remove(key);
                ConcurrentDictionary<object, ConcurrentDictionary<string, IElement>> removedIndex;
                Index.TryRemove(key, out removedIndex);
            }

            public IEnumerable<string> GetIndexedKeys()
            {
                return null != _indexedKeys ? _indexedKeys.ToArray() : Enumerable.Empty<string>();
            }
        }
    }
}