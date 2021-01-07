using System;

namespace Frontenac.Blueprints.Util.Wrappers.Event.Listener
{
    public static class EdgePropertyEventContract
    {
        public static void ValidateFire(IGraphChangedListener listener, IEdge edge, string key, object oldValue,
                                     object newValue)
        {
            if (listener == null)
                throw new ArgumentNullException(nameof(listener));
            if (edge == null)
                throw new ArgumentNullException(nameof(edge));
            if (string.IsNullOrWhiteSpace(key))
                throw new ArgumentNullException(nameof(key));
        }
    }
}