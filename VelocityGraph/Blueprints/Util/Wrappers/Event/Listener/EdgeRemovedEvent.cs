using System;
using System.Collections.Generic;

namespace Frontenac.Blueprints.Util.Wrappers.Event.Listener
{
    /// <summary>
    ///     Event fired when an edge is removed.
    /// </summary>
    public class EdgeRemovedEvent : IEvent
    {
        private readonly IEdge _edge;
        private readonly IDictionary<string, object> _props;

        public EdgeRemovedEvent(IEdge edge, IDictionary<string, object> props)
        {
            if (edge == null)
                throw new ArgumentNullException(nameof(edge));
            if (props == null)
                throw new ArgumentNullException(nameof(props));

            _edge = edge;
            _props = props;
        }

        public void FireEvent(IEnumerator<IGraphChangedListener> eventListeners)
        {
            EventContract.ValidateFireEvent(eventListeners);

            while (eventListeners.MoveNext())
            {
                eventListeners.Current.EdgeRemoved(_edge, _props);
            }
        }
    }
}