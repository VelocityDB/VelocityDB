using System;
using System.Collections.Generic;

namespace Frontenac.Blueprints.Util.Wrappers.Event.Listener
{
    public static class GraphChangedListenerContract
    {
        public static void ValidateVertexAdded(IVertex vertex)
        {
            if (vertex == null)
                throw new ArgumentNullException(nameof(vertex));
        }

        public static void ValidateVertexPropertyChanged(IVertex vertex, string key, object oldValue, object setValue)
        {
            if (vertex == null)
                throw new ArgumentNullException(nameof(vertex));
            if (string.IsNullOrWhiteSpace(key))
                throw new ArgumentNullException(nameof(key));
        }

        public static void ValidateVertexPropertyRemoved(IVertex vertex, string key, object removedValue)
        {
            if (vertex == null)
                throw new ArgumentNullException(nameof(vertex));
            if (string.IsNullOrWhiteSpace(key))
                throw new ArgumentNullException(nameof(key));
        }

        public static void ValidateVertexRemoved(IVertex vertex, IDictionary<string, object> props)
        {
            if (vertex == null)
                throw new ArgumentNullException(nameof(vertex));
            if (props == null)
                throw new ArgumentNullException(nameof(props));
        }

        public static void ValidateEdgeAdded(IEdge edge)
        {
            if (edge == null)
                throw new ArgumentNullException(nameof(edge));
        }

        public static void ValidateEdgePropertyChanged(IEdge edge, string key, object oldValue, object setValue)
        {
            if (edge == null)
                throw new ArgumentNullException(nameof(edge));
            if (string.IsNullOrWhiteSpace(key))
                throw new ArgumentNullException(nameof(key));
        }

        public static void ValidateEdgePropertyRemoved(IEdge edge, string key, object removedValue)
        {
            if (edge == null)
                throw new ArgumentNullException(nameof(edge));
            if (string.IsNullOrWhiteSpace(key))
                throw new ArgumentNullException(nameof(key));
        }

        public static void ValidateEdgeRemoved(IEdge edge, IDictionary<string, object> props)
        {
            if (edge == null)
                throw new ArgumentNullException(nameof(edge));
            if (props == null)
                throw new ArgumentNullException(nameof(props));
        }
    }
}