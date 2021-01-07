using System;
using System.Collections.Generic;

namespace Frontenac.Blueprints.Util.Wrappers.Event.Listener
{
    /// <summary>
    ///     Base class for property changed events.
    /// </summary>
    public abstract class VertexPropertyEvent : IEvent
    {
        private readonly string _key;
        private readonly object _newValue;
        private readonly object _oldValue;
        private readonly IVertex _vertex;

        protected VertexPropertyEvent(IVertex vertex, string key, object oldValue, object newValue)
        {
            if (vertex == null)
                throw new ArgumentNullException(nameof(vertex));
            if (string.IsNullOrWhiteSpace(key))
                throw new ArgumentNullException(nameof(key));

            _vertex = vertex;
            _key = key;
            _oldValue = oldValue;
            _newValue = newValue;
        }

        public void FireEvent(IEnumerator<IGraphChangedListener> eventListeners)
        {
            EventContract.ValidateFireEvent(eventListeners);

            while (eventListeners.MoveNext())
            {
                Fire(eventListeners.Current, _vertex, _key, _oldValue, _newValue);
            }
        }

        protected abstract void Fire(IGraphChangedListener listener, IVertex vertex, string key, object oldValue,
                                     object newValue);
    }
}