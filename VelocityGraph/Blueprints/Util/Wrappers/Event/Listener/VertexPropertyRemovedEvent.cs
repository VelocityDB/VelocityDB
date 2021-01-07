using System;

namespace Frontenac.Blueprints.Util.Wrappers.Event.Listener
{
    /// <summary>
    ///     Event fired when a vertex property is removed.
    /// </summary>
    public class VertexPropertyRemovedEvent : VertexPropertyEvent
    {
        public VertexPropertyRemovedEvent(IVertex vertex, string key, object removedValue)
            : base(vertex, key, removedValue, null)
        {
            if (vertex == null)
                throw new ArgumentNullException(nameof(vertex));
        }

        protected override void Fire(IGraphChangedListener listener, IVertex vertex, string key, object oldValue,
                                     object newValue)
        {
            VertexPropertyEventContract.ValidateFire(listener, vertex, key, oldValue, newValue);

            listener.VertexPropertyRemoved(vertex, key, oldValue);
        }
    }
}