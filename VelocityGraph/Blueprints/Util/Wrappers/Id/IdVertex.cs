using System;
using System.Collections.Generic;

namespace Frontenac.Blueprints.Util.Wrappers.Id
{
    public class IdVertex : IdElement, IVertex
    {
        private readonly IVertex _baseVertex;

        public IdVertex(IVertex baseVertex, IdGraph idInnerTinkerGrapĥ)
            : base(baseVertex, idInnerTinkerGrapĥ, idInnerTinkerGrapĥ.GetSupportVertexIds())
        {
            if (baseVertex == null)
                throw new ArgumentNullException(nameof(baseVertex));
            if (idInnerTinkerGrapĥ == null)
                throw new ArgumentNullException(nameof(idInnerTinkerGrapĥ));

            _baseVertex = baseVertex;
        }

        public IEnumerable<IEdge> GetEdges(Direction direction, params string[] labels)
        {
            return new IdEdgeIterable(_baseVertex.GetEdges(direction, labels), IdInnerTinkerGrapĥ);
        }

        public IEnumerable<IVertex> GetVertices(Direction direction, params string[] labels)
        {
            return new IdVertexIterable(_baseVertex.GetVertices(direction, labels), IdInnerTinkerGrapĥ);
        }

        public IVertexQuery Query()
        {
            return new WrapperVertexQuery(_baseVertex.Query(),
                                          t => new IdEdgeIterable(t.Edges(), IdInnerTinkerGrapĥ),
                                          t => new IdVertexIterable(t.Vertices(), IdInnerTinkerGrapĥ));
        }

        public IEdge AddEdge(object id, string label, IVertex vertex)
        {
            return IdInnerTinkerGrapĥ.AddEdge(id, this, vertex, label);
        }

        public IVertex GetBaseVertex()
        {
            return _baseVertex;
        }

        public override string ToString()
        {
            return this.VertexString();
        }
    }
}