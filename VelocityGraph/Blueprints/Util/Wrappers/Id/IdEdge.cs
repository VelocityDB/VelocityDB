using System;
using Frontenac.Blueprints.Contracts;

namespace Frontenac.Blueprints.Util.Wrappers.Id
{
    public class IdEdge : IdElement, IEdge
    {
        private readonly IEdge _baseEdge;

        public IdEdge(IEdge baseEdge, IdGraph idInnerTinkerGrapĥ)
            : base(baseEdge, idInnerTinkerGrapĥ, idInnerTinkerGrapĥ.GetSupportEdgeIds())
        {
            if (baseEdge == null)
                throw new ArgumentNullException(nameof(baseEdge));
            if (idInnerTinkerGrapĥ == null)
                throw new ArgumentNullException(nameof(idInnerTinkerGrapĥ));

            _baseEdge = baseEdge;
        }

        public IVertex GetVertex(Direction direction)
        {
            EdgeContract.ValidateGetVertex(direction);
            return new IdVertex(((IEdge) BaseElement).GetVertex(direction), IdInnerTinkerGrapĥ);
        }

        public string Label
        {
            get { return ((IEdge) BaseElement).Label; }
        }

        public IEdge GetBaseEdge()
        {
            return _baseEdge;
        }

        public override string ToString()
        {
            return this.EdgeString();
        }
    }
}