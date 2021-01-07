using System;
using System.Collections.Generic;

namespace Frontenac.Blueprints.Util.Wrappers.Event.Listener
{
    /// <summary>
    ///     An example listener that writes a message to the console for each event that fires from the graph.
    /// </summary>
    public class ConsoleGraphChangedListener : IGraphChangedListener
    {
        private readonly IGraph _graph;

        public ConsoleGraphChangedListener(IGraph graph)
        {
            if (graph == null)
                throw new ArgumentNullException(nameof(graph));

            _graph = graph;
        }

        public void VertexAdded(IVertex vertex)
        {
            GraphChangedListenerContract.ValidateVertexAdded(vertex);

            Console.WriteLine(string.Concat("Vertex [", vertex, "] added to graph [", _graph, "]"));
        }

        public void VertexPropertyChanged(IVertex vertex, string key, object oldValue, object newValue)
        {
            GraphChangedListenerContract.ValidateVertexPropertyChanged(vertex, key, oldValue, newValue);

            Console.WriteLine(string.Concat("Vertex [", vertex, "] property [", key, "] change value from [", oldValue,
                                            "] to [", newValue, "] in graph [", _graph, "]"));
        }

        public void VertexPropertyRemoved(IVertex vertex, string key, object removedValue)
        {
            GraphChangedListenerContract.ValidateVertexPropertyRemoved(vertex, key, removedValue);

            Console.WriteLine(string.Concat("Vertex [", vertex, "] property [", key, "] with value of [", removedValue,
                                            "] removed in graph [", _graph, "]"));
        }

        public void VertexRemoved(IVertex vertex, IDictionary<string, object> props)
        {
            GraphChangedListenerContract.ValidateVertexRemoved(vertex, props);

            Console.WriteLine(string.Concat("Vertex [", vertex, "] removed from graph [", _graph, "]"));
        }

        public void EdgeAdded(IEdge edge)
        {
            GraphChangedListenerContract.ValidateEdgeAdded(edge);

            Console.WriteLine(string.Concat("Edge [", edge, "] added to graph [", _graph, "]"));
        }

        public void EdgePropertyChanged(IEdge edge, string key, object oldValue, object newValue)
        {
            GraphChangedListenerContract.ValidateEdgePropertyChanged(edge, key, oldValue, newValue);

            Console.WriteLine(string.Concat("Edge [", edge, "] property [", key, "] change value from [", oldValue,
                                            "] to [", newValue, "] in graph [", _graph, "]"));
        }

        public void EdgePropertyRemoved(IEdge edge, string key, object removedValue)
        {
            GraphChangedListenerContract.ValidateEdgePropertyRemoved(edge, key, removedValue);

            Console.WriteLine(string.Concat("Edge [", edge, "] property [", key, "] with value of [", removedValue,
                                            "] removed in graph [", _graph, "]"));
        }

        public void EdgeRemoved(IEdge edge, IDictionary<string, object> props)
        {
            GraphChangedListenerContract.ValidateEdgeRemoved(edge, props);

            Console.WriteLine(string.Concat("Edge [", edge, "] removed from graph [", _graph, "]"));
        }
    }
}