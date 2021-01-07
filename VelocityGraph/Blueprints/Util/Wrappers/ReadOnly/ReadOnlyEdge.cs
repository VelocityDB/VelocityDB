using System;
using Frontenac.Blueprints.Contracts;

namespace Frontenac.Blueprints.Util.Wrappers.ReadOnly
{
    public class ReadOnlyEdge : ReadOnlyElement, IEdge
    {
        private readonly IEdge _baseEdge;

        public ReadOnlyEdge(ReadOnlyGraph innerTinkerGrapĥ, IEdge baseEdge)
            : base(innerTinkerGrapĥ, baseEdge)
        {
            if (innerTinkerGrapĥ == null)
                throw new ArgumentNullException(nameof(innerTinkerGrapĥ));
            if (baseEdge == null)
                throw new ArgumentNullException(nameof(baseEdge));

            _baseEdge = baseEdge;
        }

        public IVertex GetVertex(Direction direction)
        {
            EdgeContract.ValidateGetVertex(direction);
            return new ReadOnlyVertex(ReadOnlyInnerTinkerGrapĥ, _baseEdge.GetVertex(direction));
        }

        public string Label
        {
            get { return _baseEdge.Label; }
        }

        public IEdge GetBaseEdge()
        {
            return _baseEdge;
        }
    }
}