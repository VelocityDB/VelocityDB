using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace Frontenac.Blueprints.Util.Wrappers.Event.Listener
{
    /// <summary>
    ///     An event listener that acts as a counter for changes to the graph.
    /// </summary>
    public class StubGraphChangedListener : IGraphChangedListener
    {
        private long _addEdgeEvent;
        private long _addVertexEvent;
        private long _edgePropertyChangedEvent;
        private long _edgePropertyRemovedEvent;
        private long _edgeRemovedEvent;

        private ConcurrentQueue<string> _order = new ConcurrentQueue<string>();
        private long _vertexPropertyChangedEvent;
        private long _vertexPropertyRemovedEvent;
        private long _vertexRemovedEvent;

        public void VertexAdded(IVertex vertex)
        {
            GraphChangedListenerContract.ValidateVertexAdded(vertex);

            Interlocked.Increment(ref _addVertexEvent);
            _order.Enqueue(string.Concat("v-added-", vertex.Id));
        }

        public void VertexPropertyChanged(IVertex vertex, string s, object o, object n)
        {
            GraphChangedListenerContract.ValidateVertexPropertyChanged(vertex, s, o, n);

            Interlocked.Increment(ref _vertexPropertyChangedEvent);
            _order.Enqueue(string.Concat("v-property-changed-", vertex.Id, "-", s, ":", o, "->", n));
        }

        public void VertexPropertyRemoved(IVertex vertex, string s, object o)
        {
            GraphChangedListenerContract.ValidateVertexPropertyRemoved(vertex, s, o);

            Interlocked.Increment(ref _vertexPropertyRemovedEvent);
            _order.Enqueue(string.Concat("v-property-removed-", vertex.Id, "-", s, ":", o));
        }

        public void VertexRemoved(IVertex vertex, IDictionary<string, object> props)
        {
            GraphChangedListenerContract.ValidateVertexRemoved(vertex, props);

            Interlocked.Increment(ref _vertexRemovedEvent);
            _order.Enqueue(string.Concat("v-removed-", vertex.Id));
        }

        public void EdgeAdded(IEdge edge)
        {
            GraphChangedListenerContract.ValidateEdgeAdded(edge);

            Interlocked.Increment(ref _addEdgeEvent);
            _order.Enqueue(string.Concat("e-added-", edge.Id));
        }

        public void EdgePropertyChanged(IEdge edge, string s, object o, object n)
        {
            GraphChangedListenerContract.ValidateEdgePropertyChanged(edge, s, o, n);

            Interlocked.Increment(ref _edgePropertyChangedEvent);
            _order.Enqueue(string.Concat("e-property-changed-", edge.Id, "-", s, ":", o, "->", n));
        }

        public void EdgePropertyRemoved(IEdge edge, string s, object o)
        {
            GraphChangedListenerContract.ValidateEdgePropertyRemoved(edge, s, o);

            Interlocked.Increment(ref _edgePropertyRemovedEvent);
            _order.Enqueue(string.Concat("e-property-removed-", edge.Id, "-", s, ":", o));
        }

        public void EdgeRemoved(IEdge edge, IDictionary<string, object> props)
        {
            GraphChangedListenerContract.ValidateEdgeRemoved(edge, props);

            Interlocked.Increment(ref _edgeRemovedEvent);
            _order.Enqueue(string.Concat("e-removed-", edge.Id));
        }

        public void Reset()
        {
            Interlocked.Exchange(ref _addEdgeEvent, 0);
            Interlocked.Exchange(ref _addVertexEvent, 0);
            Interlocked.Exchange(ref _vertexPropertyChangedEvent, 0);
            Interlocked.Exchange(ref _vertexPropertyRemovedEvent, 0);
            Interlocked.Exchange(ref _vertexRemovedEvent, 0);
            Interlocked.Exchange(ref _edgePropertyChangedEvent, 0);
            Interlocked.Exchange(ref _edgePropertyRemovedEvent, 0);
            Interlocked.Exchange(ref _edgeRemovedEvent, 0);

            _order = new ConcurrentQueue<string>();
        }

        public List<string> GetOrder()
        {
            return _order.ToList();
        }

        public long AddEdgeEventRecorded()
        {
            return Interlocked.Read(ref _addEdgeEvent);
        }

        public long AddVertexEventRecorded()
        {
            return Interlocked.Read(ref _addVertexEvent);
        }

        public long VertexPropertyChangedEventRecorded()
        {
            return Interlocked.Read(ref _vertexPropertyChangedEvent);
        }

        public long VertexPropertyRemovedEventRecorded()
        {
            return Interlocked.Read(ref _vertexPropertyRemovedEvent);
        }

        public long VertexRemovedEventRecorded()
        {
            return Interlocked.Read(ref _vertexRemovedEvent);
        }

        public long EdgePropertyChangedEventRecorded()
        {
            return Interlocked.Read(ref _edgePropertyChangedEvent);
        }

        public long EdgePropertyRemovedEventRecorded()
        {
            return Interlocked.Read(ref _edgePropertyRemovedEvent);
        }

        public long EdgeRemovedEventRecorded()
        {
            return Interlocked.Read(ref _edgeRemovedEvent);
        }
    }
}