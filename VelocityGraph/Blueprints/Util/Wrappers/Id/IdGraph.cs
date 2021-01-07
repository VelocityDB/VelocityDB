using System;
using System.Collections.Generic;
using System.Linq;
using Frontenac.Blueprints.Contracts;

namespace Frontenac.Blueprints.Util.Wrappers.Id
{
    /// <summary>
    ///     A Graph implementation which wraps another Graph implementation,
    ///     enabling custom element IDs even for those graphs which don't otherwise support them.
    ///     The base Graph must be an instance of KeyIndexableGraph.
    ///     It *may* be an instance of IIndexableGraph, in which case its indices will be wrapped appropriately.
    ///     It *may* be an instance of TransactionalGraph, in which case transaction operations will be passed through.
    ///     For those graphs which support vertex indices but not edge indices (or vice versa),
    ///     you may configure idInnerTinkerGrapĥ to use custom IDs only for vertices or only for edges.
    /// </summary>
    public class IdGraph : IKeyIndexableGraph, IWrapperGraph, IIndexableGraph, ITransactionalGraph
    {
        // Note: using "__id" instead of "_id" avoids collision with Rexster's "_id"
        public const string Id = "__id";

        private readonly IKeyIndexableGraph _baseGraph;
        private readonly Features _features;
        private readonly bool _supportEdgeIds;
        private readonly bool _supportVertexIds;
        private IIdFactory _edgeIdFactory;
        private bool _uniqueIds = true;
        private IIdFactory _vertexIdFactory;

        /// <summary>
        ///     Adds custom ID functionality to the given graph,
        ///     supporting both custom vertex IDs and custom edge IDs.
        /// </summary>
        /// <param name="baseGraph">the base graph which does not necessarily support custom IDs</param>
        public IdGraph(IKeyIndexableGraph baseGraph)
            : this(baseGraph, true, true)
        {
            if (baseGraph == null)
                throw new ArgumentNullException(nameof(baseGraph));
        }

        /// <summary>
        ///     Adds custom ID functionality to the given graph,
        ///     supporting either custom vertex IDs, custom edge IDs, or both.
        /// </summary>
        /// <param name="baseGraph">the base graph which does not necessarily support custom IDs</param>
        /// <param name="supportVertexIds">whether to support custom vertex IDs</param>
        /// <param name="supportEdgeIds">whether to support custom edge IDs</param>
        public IdGraph(IKeyIndexableGraph baseGraph, bool supportVertexIds, bool supportEdgeIds)
        {
            if (baseGraph == null)
                throw new ArgumentNullException(nameof(baseGraph));

            if(!(supportVertexIds || supportEdgeIds))
                throw new ArgumentException("Edge or Vertex Ids support must be on");

            _baseGraph = baseGraph;
            _features = _baseGraph.Features.CopyFeatures();
            _features.IsWrapper = true;
            _features.IgnoresSuppliedIds = false;

            _supportVertexIds = supportVertexIds;
            _supportEdgeIds = supportEdgeIds;

            CreateIndices();

            _vertexIdFactory = new DefaultIdFactory();
            _edgeIdFactory = new DefaultIdFactory();
        }

        /// <summary>
        ///     When edges are created using null IDs, the actual IDs are chosen based on this factory.
        /// </summary>
        /// <value>the factory for new vertex IDs.</value>
        public IIdFactory VertexIdFactory
        {
            get
            {
                return _vertexIdFactory;
            }
            set
            {
                if (value == null)
                    throw new ArgumentNullException(nameof(value));
                _vertexIdFactory = value;
            }
        }


        /// <summary>
        ///     When edges are created using null IDs, the actual IDs are chosen based on this factory.
        /// </summary>
        /// <value>the factory for new edge IDs.</value>
        public IIdFactory EdgeIdFactory
        {
            get
            {
                return _edgeIdFactory;
            }
            set
            {
                if (value == null)
                    throw new ArgumentNullException(nameof(value));
                _edgeIdFactory = value;
            }
        }

        public IIndex CreateIndex(string indexName, Type indexClass, params Parameter[] indexParameters)
        {
            IndexableGraphContract.ValidateCreateIndex(indexName, indexClass, indexParameters);

            VerifyBaseGraphIsIndexableGraph();

            if (IsVertex(indexClass))
                return
                    new IdVertexIndex(
                        ((IIndexableGraph) _baseGraph).CreateIndex(indexName, indexClass, indexParameters), this);
            return new IdEdgeIndex(((IIndexableGraph) _baseGraph).CreateIndex(indexName, indexClass, indexParameters),
                                   this);
        }

        public IIndex GetIndex(string indexName, Type indexClass)
        {
            IndexableGraphContract.ValidateGetIndex(indexName, indexClass);

            VerifyBaseGraphIsIndexableGraph();

            var baseIndex = ((IIndexableGraph) _baseGraph).GetIndex(indexName, indexClass);
            return null == baseIndex ? null : new IdVertexIndex(baseIndex, this);
        }

        public IEnumerable<IIndex> GetIndices()
        {
            throw new NotImplementedException("sorry, you currently can't get a list of indexes through idInnerTinkerGrapĥ");
        }

        public void DropIndex(string indexName)
        {
            IndexableGraphContract.ValidateDropIndex(indexName);

            VerifyBaseGraphIsIndexableGraph();

            ((IIndexableGraph) _baseGraph).DropIndex(indexName);
        }

        public Features Features
        {
            get { return _features; }
        }

        public IVertex AddVertex(object id)
        {
            if (_uniqueIds && null != id && null != GetVertex(id))
                throw new ArgumentException(string.Concat("vertex with given id already exists: '", id, "'"));

            var base_ = _baseGraph.AddVertex(id);

            if (_supportVertexIds)
            {
                var v = id ?? _vertexIdFactory.CreateId();

                if (null != v)
                    base_.SetProperty(Id, v);
            }

            return new IdVertex(base_, this);
        }

        public IVertex GetVertex(object id)
        {
            GraphContract.ValidateGetVertex(id);
            if (null == id)
                throw new ArgumentNullException("id");

            if (_supportVertexIds)
            {
                var i = _baseGraph.GetVertices(Id, id);
                var iter = i.GetEnumerator();
                if (!iter.MoveNext())
                    return null;
                var e = iter.Current;

                if (iter.MoveNext())
                    throw new InvalidOperationException(string.Concat("multiple vertices exist with id '", id, "'"));

                return new IdVertex(e, this);
            }
            var base_ = _baseGraph.GetVertex(id);
            return null == base_ ? null : new IdVertex(base_, this);
        }

        public void RemoveVertex(IVertex vertex)
        {
            GraphContract.ValidateRemoveVertex(vertex);
            VerifyNativeElement(vertex);
            _baseGraph.RemoveVertex(((IdVertex) vertex).GetBaseVertex());
        }

        public IEnumerable<IVertex> GetVertices()
        {
            return new IdVertexIterable(_baseGraph.GetVertices(), this);
        }

        public IEnumerable<IVertex> GetVertices(string key, object value)
        {
            GraphContract.ValidateGetVertices(key, value);
            if (_supportVertexIds && key == Id)
                throw new ArgumentException(string.Concat("index key ", Id, " is reserved by idInnerTinkerGrapĥ"));
            return new IdVertexIterable(_baseGraph.GetVertices(key, value), this);
        }

        public IEdge AddEdge(object id, IVertex outVertex, IVertex inVertex, string label)
        {
            GraphContract.ValidateAddEdge(id, outVertex, inVertex, label);
            if (_uniqueIds && null != id && null != GetEdge(id))
                throw new ArgumentException(string.Concat("edge with given id already exists: ", id));

            VerifyNativeElement(outVertex);
            VerifyNativeElement(inVertex);

            var base_ = _baseGraph.AddEdge(id, ((IdVertex) outVertex).GetBaseVertex(),
                                           ((IdVertex) inVertex).GetBaseVertex(), label);

            if (_supportEdgeIds)
            {
                var v = id ?? _edgeIdFactory.CreateId();

                if (null != v)
                    base_.SetProperty(Id, v);
            }

            return new IdEdge(base_, this);
        }

        public IEdge GetEdge(object id)
        {
            if (_supportEdgeIds)
            {
                var i = _baseGraph.GetEdges(Id, id);
                var iter = i.GetEnumerator();
                if (!iter.MoveNext())
                    return null;
                var e = iter.Current;

                if (iter.MoveNext())
                    throw new InvalidOperationException(string.Concat("multiple edges exist with id ", id));

                return new IdEdge(e, this);
            }
            var base_ = _baseGraph.GetEdge(id);
            return null == base_ ? null : new IdEdge(base_, this);
        }

        public void RemoveEdge(IEdge edge)
        {
            VerifyNativeElement(edge);
            _baseGraph.RemoveEdge(((IdEdge) edge).GetBaseEdge());
        }

        public IEnumerable<IEdge> GetEdges()
        {
            return new IdEdgeIterable(_baseGraph.GetEdges(), this);
        }

        public IEnumerable<IEdge> GetEdges(string key, object value)
        {
            if (_supportEdgeIds && key == Id)
                throw new ArgumentException(string.Concat("index key ", Id, " is reserved by idInnerTinkerGrapĥ"));
            return new IdEdgeIterable(_baseGraph.GetEdges(key, value), this);
        }

        public void DropKeyIndex(string key, Type elementClass)
        {
            KeyIndexableGraphContract.ValidateDropKeyIndex(key, elementClass);

            _baseGraph.DropKeyIndex(key, elementClass);
        }

        public void CreateKeyIndex(string key, Type elementClass, params Parameter[] indexParameters)
        {
            KeyIndexableGraphContract.ValidateCreateKeyIndex(key, elementClass, indexParameters);

            _baseGraph.CreateKeyIndex(key, elementClass, indexParameters);
        }

        public IEnumerable<string> GetIndexedKeys(Type elementClass)
        {
            KeyIndexableGraphContract.ValidateGetIndexedKeys(elementClass);

            var v = IsVertex(elementClass);
            var supported = ((v && _supportVertexIds) || (!v && _supportEdgeIds));

            if (supported)
            {
                ISet<string> keys = new HashSet<string>(_baseGraph.GetIndexedKeys(elementClass));
                keys.Remove(Id);
                return keys;
            }
            return _baseGraph.GetIndexedKeys(elementClass);
        }

        public IQuery Query()
        {
            return new WrappedQuery(_baseGraph.Query(),
                                    t => new IdEdgeIterable(t.Edges(), this),
                                    t => new IdVertexIterable(t.Vertices(), this));
        }

        public virtual void Shutdown()
        {
            _baseGraph.Shutdown();
        }

        public void Rollback()
        {
            if (_baseGraph is ITransactionalGraph)
                (_baseGraph as ITransactionalGraph).Rollback();
        }

        public void Commit()
        {
            if (_baseGraph is ITransactionalGraph)
                (_baseGraph as ITransactionalGraph).Commit();
        }

        public IGraph GetBaseGraph()
        {
            return _baseGraph;
        }

        public override string ToString()
        {
            return this.GraphString(_baseGraph.ToString());
        }

        public void EnforceUniqueIds(bool enforceUniqueIds)
        {
            _uniqueIds = enforceUniqueIds;
        }

        public bool GetSupportVertexIds()
        {
            return _supportVertexIds;
        }

        public bool GetSupportEdgeIds()
        {
            return _supportEdgeIds;
        }

        private void VerifyBaseGraphIsIndexableGraph()
        {
            if (!(_baseGraph is IIndexableGraph))
                throw new ArgumentException("_baseGraph must be of type IIndexableGraph");
        }

        private static bool IsVertex(Type c)
        {
            return typeof (IVertex).IsAssignableFrom(c);
        }

        private static bool IsEdge(Type c)
        {
            return typeof (IEdge).IsAssignableFrom(c);
        }

        private void CreateIndices()
        {
            if (_supportVertexIds && !_baseGraph.GetIndexedKeys(typeof(IVertex)).Contains(Id))
                _baseGraph.CreateKeyIndex(Id, typeof(IVertex));

            if (_supportEdgeIds && !_baseGraph.GetIndexedKeys(typeof(IEdge)).Contains(Id))
                _baseGraph.CreateKeyIndex(Id, typeof(IEdge));
        }

// ReSharper disable UnusedParameter.Local
        private static void VerifyNativeElement(IElement e)
// ReSharper restore UnusedParameter.Local
        {
            if(!(e is IdElement))
                throw new ArgumentException("e must be of type IdElement");
        }

        private class DefaultIdFactory : IIdFactory
        {
            public object CreateId()
            {
                return Guid.NewGuid().ToString();
            }
        }

        /// <summary>
        ///     A factory for IDs of newly-created vertices and edges (where an ID is not otherwise specified).
        /// </summary>
        public interface IIdFactory
        {
            object CreateId();
        }
    }
}