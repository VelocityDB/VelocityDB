using System;
using System.Collections.Generic;

namespace Frontenac.Blueprints.Util.Wrappers.Event.Listener
{
    /// <summary>
    ///     Event that fires when a vertex is added to a graph.
    /// </summary>
    public class VertexAddedEvent : IEvent
    {
        private readonly IVertex _vertex;

        public VertexAddedEvent(IVertex vertex)
        {
            if (vertex == null)
                throw new ArgumentNullException(nameof(vertex));

            _vertex = vertex;
        }

        public void FireEvent(IEnumerator<IGraphChangedListener> eventListeners)
        {
            EventContract.ValidateFireEvent(eventListeners);

            while (eventListeners.MoveNext())
            {
                eventListeners.Current.VertexAdded(_vertex);
            }
        }
    }
}