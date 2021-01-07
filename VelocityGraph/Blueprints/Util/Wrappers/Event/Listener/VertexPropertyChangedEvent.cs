using System;

namespace Frontenac.Blueprints.Util.Wrappers.Event.Listener
{
    /// <summary>
    ///     Event that fires when a property changes on a vertex.
    /// </summary>
    public class VertexPropertyChangedEvent : VertexPropertyEvent
    {
        public VertexPropertyChangedEvent(IVertex vertex, string key, object oldValue, object newValue)
            : base(vertex, key, oldValue, newValue)
        {
            if (vertex == null)
                throw new ArgumentNullException(nameof(vertex));
        }

        protected override void Fire(IGraphChangedListener listener, IVertex vertex, string key, object oldValue,
                                     object newValue)
        {
            VertexPropertyEventContract.ValidateFire(listener, vertex, key, oldValue, newValue);

            listener.VertexPropertyChanged(vertex, key, oldValue, newValue);
        }
    }
}