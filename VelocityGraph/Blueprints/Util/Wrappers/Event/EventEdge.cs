using System;
using Frontenac.Blueprints.Contracts;

namespace Frontenac.Blueprints.Util.Wrappers.Event
{
    /// <summary>
    ///     An edge with a GraphChangedListener attached.  Those listeners are notified when changes occur to
    ///     the properties of the edge.
    /// </summary>
    public class EventEdge : EventElement, IEdge
    {
        private readonly IEdge _edge;

        public EventEdge(IEdge edge, EventGraph eventInnerTinkerGrapĥ)
            : base(edge, eventInnerTinkerGrapĥ)
        {
            if (edge == null)
                throw new ArgumentNullException(nameof(edge));
            if (eventInnerTinkerGrapĥ == null)
                throw new ArgumentNullException(nameof(eventInnerTinkerGrapĥ));

            _edge = edge;
        }

        public IVertex GetVertex(Direction direction)
        {
            EdgeContract.ValidateGetVertex(direction);
            return new EventVertex(GetBaseEdge().GetVertex(direction), EventInnerTinkerGrapĥ);
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