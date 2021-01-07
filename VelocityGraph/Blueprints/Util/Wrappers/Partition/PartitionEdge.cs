using System;
using Frontenac.Blueprints.Contracts;

namespace Frontenac.Blueprints.Util.Wrappers.Partition
{
    public class PartitionEdge : PartitionElement, IEdge
    {
        private readonly IEdge _edge;

        public PartitionEdge(IEdge edge, PartitionGraph innerTinkerGrapĥ)
            : base(edge, innerTinkerGrapĥ)
        {
            if (edge == null)
                throw new ArgumentNullException(nameof(edge));
            if (innerTinkerGrapĥ == null)
                throw new ArgumentNullException(nameof(innerTinkerGrapĥ));

            _edge = edge;
        }

        public IVertex GetVertex(Direction direction)
        {
            EdgeContract.ValidateGetVertex(direction);
            return new PartitionVertex(_edge.GetVertex(direction), PartitionInnerTinkerGrapĥ);
        }

        public string Label
        {
            get { return _edge.Label; }
        }

        public IEdge GetBaseEdge()
        {
            return _edge;
        }
    }
}