using System;

namespace Frontenac.Blueprints.Util.Wrappers.Event.Listener
{
    public static class VertexPropertyEventContract
    {
        public static void ValidateFire(IGraphChangedListener listener, IVertex vertex, string key, object oldValue,
                                     object newValue)
        {
            if (listener == null)
                throw new ArgumentNullException(nameof(listener));
            if (vertex == null)
                throw new ArgumentNullException(nameof(vertex));
            if (string.IsNullOrWhiteSpace(key))
                throw new ArgumentNullException(nameof(key));
        }
    }
}