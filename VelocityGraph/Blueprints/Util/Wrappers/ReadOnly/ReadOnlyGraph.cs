using System;
using System.Collections.Generic;
using Frontenac.Blueprints.Contracts;

namespace Frontenac.Blueprints.Util.Wrappers.ReadOnly
{
    /// <summary>
    ///     A ReadOnlyInnerTinkerGrapĥ wraps a Graph and overrides the underlying Graph's mutating methods.
    ///     In this way, a ReadOnlyInnerTinkerGrapĥ can only be read from, not written to.
    /// </summary>
    public class ReadOnlyGraph : IGraph, IWrapperGraph
    {
        private readonly Features _features;
        protected IGraph BaseGraph;

        public ReadOnlyGraph(IGraph baseGraph)
        {
            if (baseGraph == null)
                throw new ArgumentNullException(nameof(baseGraph));

            BaseGraph = baseGraph;
            _features = BaseGraph.Features.CopyFeatures();
            _features.IsWrapper = true;
        }

        public void RemoveVertex(IVertex vertex)
        {
            GraphContract.ValidateRemoveVertex(vertex);
            throw new InvalidOperationException(ReadOnlyTokens.MutateErrorMessage);
        }

        public IEdge AddEdge(object id, IVertex outVertex, IVertex inVertex, string label)
        {
            throw new InvalidOperationException(ReadOnlyTokens.MutateErrorMessage);
        }

        public IVertex GetVertex(object id)
        {
            GraphContract.ValidateGetVertex(id);
            var vertex = BaseGraph.GetVertex(id);
            return null == vertex ? null : new ReadOnlyVertex(this, vertex);
        }

        public void RemoveEdge(IEdge edge)
        {
            throw new InvalidOperationException(ReadOnlyTokens.MutateErrorMessage);
        }

        public IEnumerable<IEdge> GetEdges()
        {
            return new ReadOnlyEdgeIterable(this, BaseGraph.GetEdges());
        }

        public IEnumerable<IEdge> GetEdges(string key, object value)
        {
            return new ReadOnlyEdgeIterable(this, BaseGraph.GetEdges(key, value));
        }

        public IEdge GetEdge(object id)
        {
            var edge = BaseGraph.GetEdge(id);
            return null == edge ? null : new ReadOnlyEdge(this, edge);
        }

        public IEnumerable<IVertex> GetVertices()
        {
            return new ReadOnlyVertexIterable(this, BaseGraph.GetVertices());
        }

        public IEnumerable<IVertex> GetVertices(string key, object value)
        {
            GraphContract.ValidateGetVertices(key, value);
            return new ReadOnlyVertexIterable(this, BaseGraph.GetVertices(key, value));
        }

        public IVertex AddVertex(object id)
        {
            throw new InvalidOperationException(ReadOnlyTokens.MutateErrorMessage);
        }

        public IQuery Query()
        {
            return new WrappedQuery(BaseGraph.Query(),
                                    t => new ReadOnlyEdgeIterable(this, t.Edges()),
                                    t => new ReadOnlyVertexIterable(this, t.Vertices()));
        }

        public Features Features
        {
            get { return _features; }
        }

        public void Shutdown()
        {
            BaseGraph.Shutdown();
        }

        public IGraph GetBaseGraph()
        {
            return BaseGraph;
        }

        public override string ToString()
        {
            return this.GraphString(BaseGraph.ToString());
        }
    }
}