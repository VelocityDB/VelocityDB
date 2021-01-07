using System;
using System.Collections.Generic;
using System.Linq;
using Frontenac.Blueprints.Contracts;

namespace Frontenac.Blueprints.Util.Wrappers.Partition
{
    public class PartitionGraph : IGraph, IWrapperGraph
    {
        private readonly Features _features;
        private readonly ISet<string> _readPartitions;
        protected IGraph BaseGraph;
        private string _partitionKey;
        private string _writePartition;

        public PartitionGraph(IGraph baseGraph, string partitionKey, string writePartition,
                              IEnumerable<string> readPartitions)
        {
            if (baseGraph == null)
                throw new ArgumentNullException(nameof(baseGraph));
            if (string.IsNullOrWhiteSpace(partitionKey))
                throw new ArgumentNullException(nameof(partitionKey));
            if (string.IsNullOrWhiteSpace(writePartition))
                throw new ArgumentNullException(nameof(writePartition));
            if (readPartitions == null)
                throw new ArgumentNullException(nameof(readPartitions));

            BaseGraph = baseGraph;
            _partitionKey = partitionKey;
            _writePartition = writePartition;
            _readPartitions = new HashSet<string>(readPartitions);
            _features = BaseGraph.Features.CopyFeatures();
            _features.IsWrapper = true;
        }

        public PartitionGraph(IGraph baseGraph, string partitionKey, string readWritePartition) :
            this(baseGraph, partitionKey, readWritePartition, new[] {readWritePartition})
        {
            if (baseGraph == null)
                throw new ArgumentNullException(nameof(baseGraph));
            if (string.IsNullOrWhiteSpace(partitionKey))
                throw new ArgumentNullException(nameof(partitionKey));
            if (string.IsNullOrWhiteSpace(readWritePartition))
                throw new ArgumentNullException(nameof(readWritePartition));
        }

        public string WritePartition
        {
            get
            {
                return _writePartition;
            }
            set
            {
                if (value == null)
                    throw new ArgumentNullException(nameof(value));
                _writePartition = value;
            }
        }

        public string PartitionKey
        {
            set
            {
                if (value == null)
                    throw new ArgumentNullException(nameof(value));
                _partitionKey = value;
            }
            get
            {
                return _partitionKey;
            }
        }

        public IVertex AddVertex(object id)
        {
            var vertex = new PartitionVertex(BaseGraph.AddVertex(id), this);
            vertex.SetPartition(_writePartition);
            return vertex;
        }

        public IVertex GetVertex(object id)
        {
            GraphContract.ValidateGetVertex(id);
            IVertex vertex = BaseGraph.GetVertex(id);
            if (null == vertex || !IsInPartition(vertex))
                return null;

            return new PartitionVertex(vertex, this);
        }

        public IEnumerable<IVertex> GetVertices()
        {
            return new PartitionVertexIterable(BaseGraph.GetVertices(), this);
        }

        public IEnumerable<IVertex> GetVertices(string key, object value)
        {
            GraphContract.ValidateGetVertices(key, value);
            return new PartitionVertexIterable(BaseGraph.GetVertices(key, value), this);
        }

        public IEdge AddEdge(object id, IVertex outVertex, IVertex inVertex, string label)
        {
            GraphContract.ValidateAddEdge(id, outVertex, inVertex, label);

            var edge = new PartitionEdge(BaseGraph.AddEdge(id, 
                                                           ((PartitionVertex) outVertex).Vertex, 
                                                           ((PartitionVertex) inVertex).Vertex, 
                                                           label), 
                                         this);
            edge.SetPartition(_writePartition);
            return edge;
        }

        public IEdge GetEdge(object id)
        {
            var edge = BaseGraph.GetEdge(id);
            return null == edge ? null : new PartitionEdge(edge, this);
        }

        public IEnumerable<IEdge> GetEdges()
        {
            return new PartitionEdgeIterable(BaseGraph.GetEdges(), this);
        }

        public IEnumerable<IEdge> GetEdges(string key, object value)
        {
            return new PartitionEdgeIterable(BaseGraph.GetEdges(key, value), this);
        }

        public void RemoveEdge(IEdge edge)
        {
            BaseGraph.RemoveEdge(((PartitionEdge) edge).GetBaseEdge());
        }

        public void RemoveVertex(IVertex vertex)
        {
            GraphContract.ValidateRemoveVertex(vertex);
            BaseGraph.RemoveVertex(((PartitionVertex) vertex).Vertex);
        }

        public Features Features
        {
            get { return _features; }
        }

        public IQuery Query()
        {
            return new WrappedQuery(BaseGraph.Query(),
                                    t => new PartitionEdgeIterable(t.Edges(), this),
                                    t => new PartitionVertexIterable(t.Vertices(), this));
        }

        public void Shutdown()
        {
            BaseGraph.Shutdown();
        }

        public IGraph GetBaseGraph()
        {
            return BaseGraph;
        }

        public IEnumerable<string> GetReadPartitions()
        {
            return _readPartitions.ToArray();
        }

        public void RemoveReadPartition(string readPartition)
        {
            if (readPartition == null)
                throw new ArgumentNullException(nameof(readPartition));
            _readPartitions.Remove(readPartition);
        }

        public void AddReadPartition(string readPartition)
        {
            if (readPartition == null)
                throw new ArgumentNullException(nameof(readPartition));
            _readPartitions.Add(readPartition);
        }

        public bool IsInPartition(IElement element)
        {
            if (element == null)
                throw new ArgumentNullException(nameof(element));

            string writePartition;
            var partitionElement = element as PartitionElement;
            if (partitionElement != null)
                writePartition = partitionElement.GetPartition();
            else
                writePartition = (string) element.GetProperty(_partitionKey);
            return (null == writePartition || _readPartitions.Contains(writePartition));
        }

        public override string ToString()
        {
            return this.GraphString(BaseGraph.ToString());
        }
    }
}