using System;

namespace Frontenac.Blueprints.Util.Wrappers.Event.Listener
{
    /// <summary>
    ///     Event fired when an edge property is removed.
    /// </summary>
    public class EdgePropertyRemovedEvent : EdgePropertyEvent
    {
        public EdgePropertyRemovedEvent(IEdge vertex, string key, object oldValue)
            : base(vertex, key, oldValue, null)
        {
            if (vertex == null)
                throw new ArgumentNullException(nameof(vertex));
        }

        protected override void Fire(IGraphChangedListener listener, IEdge edge, string key, object oldValue,
                                     object newValue)
        {
            EdgePropertyEventContract.ValidateFire(listener, edge, key, oldValue, newValue);
            listener.EdgePropertyRemoved(edge, key, oldValue);
        }
    }
}