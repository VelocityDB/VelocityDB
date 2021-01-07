using System;
using System.Collections.Generic;

namespace Frontenac.Blueprints.Util.Wrappers.Event.Listener
{
    public class EdgeAddedEvent : IEvent
    {
        private readonly IEdge _edge;

        public EdgeAddedEvent(IEdge edge)
        {
            if (edge == null)
                throw new ArgumentNullException(nameof(edge));

            _edge = edge;
        }

        public void FireEvent(IEnumerator<IGraphChangedListener> eventListeners)
        {
            EventContract.ValidateFireEvent(eventListeners);

            while (eventListeners.MoveNext())
            {
                eventListeners.Current.EdgeAdded(_edge);
            }
        }
    }
}