using System;
using System.Collections.Generic;

namespace Frontenac.Blueprints.Util.Wrappers.Event.Listener
{
    /// <summary>
    ///     Event fired when a vertex is removed.
    /// </summary>
    public class VertexRemovedEvent : IEvent
    {
        private readonly IDictionary<string, object> _props;
        private readonly IVertex _vertex;

        public VertexRemovedEvent(IVertex vertex, IDictionary<string, object> props)
        {
            if (vertex == null)
                throw new ArgumentNullException(nameof(vertex));
            if (props == null)
                throw new ArgumentNullException(nameof(props));

            _vertex = vertex;
            _props = props;
        }

        public void FireEvent(IEnumerator<IGraphChangedListener> eventListeners)
        {
            EventContract.ValidateFireEvent(eventListeners);

            while (eventListeners.MoveNext())
            {
                eventListeners.Current.VertexRemoved(_vertex, _props);
            }
        }
    }
}