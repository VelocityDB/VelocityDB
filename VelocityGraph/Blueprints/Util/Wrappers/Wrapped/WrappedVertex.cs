using System;
using System.Collections.Generic;

namespace Frontenac.Blueprints.Util.Wrappers.Wrapped
{
    public class WrappedVertex : WrappedElement, IVertex
    {
        public WrappedVertex(IVertex vertex)
            : base(vertex)
        {
            if (vertex == null)
                throw new ArgumentNullException(nameof(vertex));

            Vertex = vertex;
        }

        public IEnumerable<IEdge> GetEdges(Direction direction, params string[] labels)
        {
            return Vertex.GetEdges(direction, labels);
        }

        public IEnumerable<IVertex> GetVertices(Direction direction, params string[] labels)
        {
            return Vertex.GetVertices(direction, labels);
        }

        public IVertexQuery Query()
        {
            return new WrapperVertexQuery(Vertex.Query(), t => t.Edges(), t => t.Vertices());
        }

        public IEdge AddEdge(object id, string label, IVertex vertex)
        {
            if (vertex is WrappedVertex)
                return new WrappedEdge(Vertex.AddEdge(id, label, (vertex as WrappedVertex).Vertex));
            
            return new WrappedEdge(Vertex.AddEdge(id, label, vertex));
        }

        public IVertex Vertex { get; protected set; }
    }
}