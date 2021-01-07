using System;
using System.Collections.Generic;
using Frontenac.Blueprints.Contracts;

namespace Frontenac.Blueprints.Util.Wrappers.Batch
{
    /// <summary>
    ///     This is a naive wrapper to make a non-transactional graph transactional by simply writing all mutations
    ///     directly through to the wrapped graph and not supporting transactional failures.
    ///     <br />
    ///     Hence, this is not meant as a functional implementation of a TransactionalGraph but rather as a means
    ///     to using non-transactional graphs where transactional graphs are expected and transactional failure can be
    ///     excluded. BatchGraph is one such case.
    ///     <br />
    ///     Note, the constructor will throw an exception if the given graph already supports transactions.
    /// </summary>
    public class WritethroughGraph : IWrapperGraph, ITransactionalGraph
    {
        private readonly IGraph _graph;

        public WritethroughGraph(IGraph graph)
        {
            if (graph == null)
                throw new ArgumentNullException(nameof(graph));

            if (graph is ITransactionalGraph)
                throw new ArgumentException("graph cannot be ITransactionalGraph");

            _graph = graph;
        }

        public void Rollback()
        {
            throw new InvalidOperationException();
        }

        public void Commit()
        {
        }

        /// <summary>
        ///     Returns the features of the underlying graph but with transactions supported.
        /// </summary>
        /// <value>The features of the underlying graph but with transactions supported</value>
        public Features Features
        {
            get
            {
                var f = _graph.Features.CopyFeatures();
                f.IsWrapper = true;
                f.SupportsTransactions = true;
                return f;
            }
        }

        public IVertex AddVertex(object id)
        {
            return _graph.AddVertex(id);
        }

        public IVertex GetVertex(object id)
        {
            GraphContract.ValidateGetVertex(id);
            return _graph.GetVertex(id);
        }

        public void RemoveVertex(IVertex vertex)
        {
            GraphContract.ValidateRemoveVertex(vertex);
            _graph.RemoveVertex(vertex);
        }

        public IEnumerable<IVertex> GetVertices()
        {
            return _graph.GetVertices();
        }

        public IEnumerable<IVertex> GetVertices(string key, object value)
        {
            GraphContract.ValidateGetVertices(key, value);
            return _graph.GetVertices(key, value);
        }

        public IEdge AddEdge(object id, IVertex outVertex, IVertex inVertex, string label)
        {
            GraphContract.ValidateAddEdge(id, outVertex, inVertex, label);

            return _graph.AddEdge(id, outVertex, inVertex, label);
        }

        public IEdge GetEdge(object id)
        {
            return _graph.GetEdge(id);
        }

        public void RemoveEdge(IEdge edge)
        {
            _graph.RemoveEdge(edge);
        }

        public IEnumerable<IEdge> GetEdges()
        {
            return _graph.GetEdges();
        }

        public IQuery Query()
        {
            return _graph.Query();
        }

        public IEnumerable<IEdge> GetEdges(string key, object value)
        {
            return _graph.GetEdges(key, value);
        }

        public virtual void Shutdown()
        {
            _graph.Shutdown();
        }

        public IGraph GetBaseGraph()
        {
            return _graph;
        }

        public override string ToString()
        {
            return this.GraphString(_graph.ToString());
        }
    }
}