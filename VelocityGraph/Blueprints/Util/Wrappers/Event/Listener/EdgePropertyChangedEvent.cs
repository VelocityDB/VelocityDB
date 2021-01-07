using System;

namespace Frontenac.Blueprints.Util.Wrappers.Event.Listener
{
    public class EdgePropertyChangedEvent : EdgePropertyEvent
    {
        public EdgePropertyChangedEvent(IEdge edge, string key, object oldValue, object newValue) :
            base(edge, key, oldValue, newValue)
        {
            if (edge == null)
                throw new ArgumentNullException(nameof(edge));
        }

        protected override void Fire(IGraphChangedListener listener, IEdge edge, string key, object oldValue,
                                     object newValue)
        {
            EdgePropertyEventContract.ValidateFire(listener, edge, key, oldValue, newValue);
            listener.EdgePropertyChanged(edge, key, oldValue, newValue);
        }
    }
}