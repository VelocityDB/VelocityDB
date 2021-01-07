using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Frontenac.Blueprints.Contracts;
using Frontenac.Blueprints.Util;

namespace Frontenac.Blueprints.Impls.TG
{
    [Serializable]
    internal class TinkerVertex : TinkerElement, IVertex
    {
        public ConcurrentDictionary<string, ConcurrentDictionary<string, IEdge>> InEdges = new ConcurrentDictionary<string, ConcurrentDictionary<string, IEdge>>();
        public ConcurrentDictionary<string, ConcurrentDictionary<string, IEdge>> OutEdges = new ConcurrentDictionary<string, ConcurrentDictionary<string, IEdge>>();

        public TinkerVertex(string id, TinkerGrapĥ tinkerGrapĥ)
            : base(id, tinkerGrapĥ)
        {
            if (id == null)
                throw new ArgumentNullException(nameof(id));
            if (tinkerGrapĥ == null)
                throw new ArgumentNullException(nameof(tinkerGrapĥ));
        }

        public IEnumerable<IEdge> GetEdges(Direction direction, params string[] labels)
        {
            if (direction == Direction.Out)
                return FilterEdgesByLabel(OutEdges, labels);
            if (direction == Direction.In)
                return FilterEdgesByLabel(InEdges, labels);
            return
                new MultiIterable<IEdge>(new List<IEnumerable<IEdge>>
                    {
                        FilterEdgesByLabel(InEdges, labels),
                        FilterEdgesByLabel(OutEdges, labels)
                    });
        }

        public IEnumerable<IVertex> GetVertices(Direction direction, params string[] labels)
        {
            return new VerticesFromEdgesIterable(this, direction, labels);
        }

        public IVertexQuery Query()
        {
            return new DefaultVertexQuery(this);
        }

        public IEdge AddEdge(object id, string label, IVertex inVertex)
        {
            VertexContract.ValidateAddEdge(id, label, inVertex);

            return Graph.AddEdge(id, this, inVertex, label);
        }

        private static IEnumerable<IEdge> FilterEdgesByLabel(IDictionary<string, ConcurrentDictionary<string, IEdge>> edgesToGet,
                                                             params string[] labels)
        {
            if (edgesToGet == null)
                throw new ArgumentNullException(nameof(edgesToGet));
            if (labels == null)
                throw new ArgumentNullException(nameof(labels));

            if (labels.Length == 0)
            {
                var totalEdgesList = new List<IEdge>();
                foreach (var edges in edgesToGet.Values)
                    totalEdgesList.AddRange(edges.Values);

                return totalEdgesList;
            }
            if (labels.Length == 1)
            {
                var edges = edgesToGet.Get(labels[0]);
                return null == edges ? Enumerable.Empty<IEdge>() : new List<IEdge>(edges.Values);
            }
            var totalEdges = new List<IEdge>();
            foreach (var edges in labels.Select(edgesToGet.Get).Where(edges => null != edges))
            {
                totalEdges.AddRange(edges.Values);
            }
            return totalEdges;
        }

        public override string ToString()
        {
            return this.VertexString();
        }

        public void AddOutEdge(string label, IEdge edge)
        {
            if (string.IsNullOrWhiteSpace(label))
                throw new ArgumentNullException(nameof(label));
            if (edge == null)
                throw new ArgumentNullException(nameof(edge));

            var edges = OutEdges.Get(label);
            if (null == edges)
            {
                edges = new ConcurrentDictionary<string, IEdge>();
                OutEdges.Put(label, edges);
            }
            edges.TryAdd(edge.Id.ToString(), edge);
        }

        public void AddInEdge(string label, IEdge edge)
        {
            if (string.IsNullOrWhiteSpace(label))
                throw new ArgumentNullException(nameof(label));
            if (edge == null)
                throw new ArgumentNullException(nameof(edge));

            var edges = InEdges.Get(label);
            if (null == edges)
            {
                edges = new ConcurrentDictionary<string, IEdge>();
                InEdges.Put(label, edges);
            }
            edges.TryAdd(edge.Id.ToString(), edge);
        }
    }
}