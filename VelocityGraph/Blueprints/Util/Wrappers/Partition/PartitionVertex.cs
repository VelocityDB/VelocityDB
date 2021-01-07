using System;
using System.Collections.Generic;

namespace Frontenac.Blueprints.Util.Wrappers.Partition
{
    public class PartitionVertex : PartitionElement, IVertex
    {
        public PartitionVertex(IVertex vertex, PartitionGraph innerTinkerGrapĥ)
            : base(vertex, innerTinkerGrapĥ)
        {
            if (vertex == null)
                throw new ArgumentNullException(nameof(vertex));
            if (innerTinkerGrapĥ == null)
                throw new ArgumentNullException(nameof(innerTinkerGrapĥ));

            Vertex = vertex;
        }

        public IEnumerable<IEdge> GetEdges(Direction direction, params string[] labels)
        {
            return new PartitionEdgeIterable(Vertex.GetEdges(direction, labels), PartitionInnerTinkerGrapĥ);
        }

        public IEnumerable<IVertex> GetVertices(Direction direction, params string[] labels)
        {
            return new PartitionVertexIterable(Vertex.GetVertices(direction, labels), PartitionInnerTinkerGrapĥ);
        }

        public IVertexQuery Query()
        {
            return new WrapperVertexQuery(Vertex.Query(),
                                          t => new PartitionEdgeIterable(t.Edges(), PartitionInnerTinkerGrapĥ),
                                          t => new PartitionVertexIterable(t.Vertices(), PartitionInnerTinkerGrapĥ));
        }

        public IEdge AddEdge(object id, string label, IVertex vertex)
        {
            return Graph.AddEdge(id, this, vertex, label);
        }

        public IVertex Vertex { get; protected set; }
    }
}