using System;
using Frontenac.Blueprints.Contracts;

namespace Frontenac.Blueprints.Util.Wrappers.Wrapped
{
    public class WrappedEdge : WrappedElement, IEdge
    {
        private readonly IEdge _edge;

        public WrappedEdge(IEdge edge)
            : base(edge)
        {
            if (edge == null)
                throw new ArgumentNullException(nameof(edge));

            _edge = edge;
        }

        public IVertex GetVertex(Direction direction)
        {
            EdgeContract.ValidateGetVertex(direction);
            return new WrappedVertex(_edge.GetVertex(direction));
        }

        public string Label
        {
            get { return _edge.Label; }
        }

        public IEdge Edge
        {
            get
            {
                return _edge;
            }
        }
    }
}