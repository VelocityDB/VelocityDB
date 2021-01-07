using System;
using System.Collections.Generic;

namespace Frontenac.Blueprints.Util.Wrappers.ReadOnly
{
    public class ReadOnlyVertex : ReadOnlyElement, IVertex
    {
        private readonly IVertex _baseVertex;

        public ReadOnlyVertex(ReadOnlyGraph innerTinkerGrapĥ, IVertex baseVertex)
            : base(innerTinkerGrapĥ, baseVertex)
        {
            if (innerTinkerGrapĥ == null)
                throw new ArgumentNullException(nameof(innerTinkerGrapĥ));
            if (baseVertex == null)
                throw new ArgumentNullException(nameof(baseVertex));

            _baseVertex = baseVertex;
        }

        public IEnumerable<IEdge> GetEdges(Direction direction, params string[] labels)
        {
            return new ReadOnlyEdgeIterable(ReadOnlyInnerTinkerGrapĥ, ((IVertex) BaseElement).GetEdges(direction, labels));
        }

        public IEnumerable<IVertex> GetVertices(Direction direction, params string[] labels)
        {
            return new ReadOnlyVertexIterable(ReadOnlyInnerTinkerGrapĥ, ((IVertex) BaseElement).GetVertices(direction, labels));
        }

        public IEdge AddEdge(object id, string label, IVertex vertex)
        {
            throw new InvalidOperationException(ReadOnlyTokens.MutateErrorMessage);
        }

        public IVertexQuery Query()
        {
            return new WrapperVertexQuery(_baseVertex.Query(),
                                          t => new ReadOnlyEdgeIterable(ReadOnlyInnerTinkerGrapĥ, t.Edges()),
                                          t => new ReadOnlyVertexIterable(ReadOnlyInnerTinkerGrapĥ, t.Vertices()));
        }
    }
}