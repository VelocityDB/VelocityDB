using System;
using System.Collections.Generic;
using Frontenac.Blueprints.Contracts;
using Frontenac.Blueprints.Util.Wrappers.Event.Listener;

namespace Frontenac.Blueprints.Util.Wrappers.Event
{
    /// <summary>
    ///     An eventInnerTinkerGrapĥ is a wrapper to existing Graph implementations and provides for graph events to be raised
    ///     to one or more listeners on changes to the Graph. Notifications to the listeners occur for the
    ///     following events: new vertex/edge, vertex/edge property changed, vertex/edge property removed,
    ///     vertex/edge removed.
    ///     The limiting factor to events being raised is related to out-of-process functions changing graph elements.
    ///     To gather events from eventInnerTinkerGrapĥ, simply provide an implementation of the {@link GraphChangedListener} to
    ///     the eventInnerTinkerGrapĥ by utilizing the addListener method. eventInnerTinkerGrapĥ allows the addition of multiple GraphChangedListener
    ///     implementations. Each listener will be notified in the order that it was added.
    /// </summary>
    public class EventGraph : IGraph, IWrapperGraph, IDisposable
    {
        protected readonly List<IGraphChangedListener> GraphChangedListeners = new List<IGraphChangedListener>();
        private readonly Features _features;
        protected IGraph BaseGraph;
        protected EventTrigger Trigger;

        public EventGraph(IGraph baseGraph)
        {
            if (baseGraph == null)
                throw new ArgumentNullException(nameof(baseGraph));

            BaseGraph = baseGraph;
            _features = BaseGraph.Features.CopyFeatures();
            _features.IsWrapper = true;

            Trigger = new EventTrigger(this, false);
        }

        #region IDisposable

        private bool _disposed;

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        ~EventGraph()
        {
            Dispose(false);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
                return;

            if (disposing)
            {
                Trigger.Dispose();
            }

            _disposed = true;
        }

        #endregion

        /// <note>
        ///     Raises a vertexAdded event.
        /// </note>
        public IVertex AddVertex(object id)
        {
            var vertex = BaseGraph.AddVertex(id);
            if (vertex == null)
                return null;
            OnVertexAdded(vertex);
            return new EventVertex(vertex, this);
        }

        public IVertex GetVertex(object id)
        {
            GraphContract.ValidateGetVertex(id);
            var vertex = BaseGraph.GetVertex(id);
            return null == vertex ? null : new EventVertex(vertex, this);
        }

        /// <note>
        ///     Raises a vertexRemoved event.
        /// </note>
        public void RemoveVertex(IVertex vertex)
        {
            GraphContract.ValidateRemoveVertex(vertex);
            var vertexToRemove = vertex;
            if (vertex is EventVertex)
                vertexToRemove = (vertex as EventVertex).Vertex;

            var props = vertex.GetProperties();
            if (vertexToRemove != null)
            {
                BaseGraph.RemoveVertex(vertexToRemove);
                OnVertexRemoved(vertex, props);
            }
        }

        public IEnumerable<IVertex> GetVertices()
        {
            return new EventVertexIterable(BaseGraph.GetVertices(), this);
        }

        public IEnumerable<IVertex> GetVertices(string key, object value)
        {
            GraphContract.ValidateGetVertices(key, value);
            return new EventVertexIterable(BaseGraph.GetVertices(key, value), this);
        }

        /// <note>
        ///     Raises an edgeAdded event.
        /// </note>
        public IEdge AddEdge(object id, IVertex outVertex, IVertex inVertex, string label)
        {
            GraphContract.ValidateAddEdge(id, outVertex, inVertex, label);
            var outVertexToSet = outVertex;
            if (outVertex is EventVertex)
                outVertexToSet = (outVertex as EventVertex).Vertex;

            var inVertexToSet = inVertex;
            if (inVertex is EventVertex)
                inVertexToSet = (inVertex as EventVertex).Vertex;

            if(inVertexToSet == null || outVertexToSet == null)
                throw new InvalidOperationException();
            var edge = BaseGraph.AddEdge(id, outVertexToSet, inVertexToSet, label);
            OnEdgeAdded(edge);
            return new EventEdge(edge, this);
        }

        public IEdge GetEdge(object id)
        {
            var edge = BaseGraph.GetEdge(id);
            return null == edge ? null : new EventEdge(edge, this);
        }

        /// <note>
        ///     Raises an edgeRemoved event.
        /// </note>
        public void RemoveEdge(IEdge edge)
        {
            var edgeToRemove = edge;
            if (edge is EventEdge)
                edgeToRemove = (edge as EventEdge).GetBaseEdge();

            var props = edge.GetProperties();
            BaseGraph.RemoveEdge(edgeToRemove);
            OnEdgeRemoved(edge, props);
        }

        public IEnumerable<IEdge> GetEdges()
        {
            return new EventEdgeIterable(BaseGraph.GetEdges(), this);
        }

        public IEnumerable<IEdge> GetEdges(string key, object value)
        {
            return new EventEdgeIterable(BaseGraph.GetEdges(key, value), this);
        }

        public IQuery Query()
        {
            return new WrappedQuery(BaseGraph.Query(),
                                    t => new EventEdgeIterable(t.Edges(), this),
                                    t => new EventVertexIterable(t.Vertices(), this));
        }

        public void Shutdown()
        {
            BaseGraph.Shutdown();

            // TODO: hmmmmmm??
            Trigger.FireEventQueue();
            Trigger.ResetEventQueue();
        }

        public Features Features
        {
            get { return _features; }
        }

        public IGraph GetBaseGraph()
        {
            return BaseGraph;
        }

        public void RemoveAllListeners()
        {
            GraphChangedListeners.Clear();
        }

        public void AddListener(IGraphChangedListener listener)
        {
            if (listener == null)
                throw new ArgumentNullException(nameof(listener));

            GraphChangedListeners.Add(listener);
        }

        public IEnumerator<IGraphChangedListener> GetListenerIterator()
        {
            return GraphChangedListeners.GetEnumerator();
        }

        public EventTrigger GetTrigger()
        {
            return Trigger;
        }

        public void RemoveListener(IGraphChangedListener listener)
        {
            if (listener == null)
                throw new ArgumentNullException(nameof(listener));

            GraphChangedListeners.Remove(listener);
        }

        protected void OnVertexAdded(IVertex vertex)
        {
            GraphChangedListenerContract.ValidateVertexAdded(vertex);

            if (vertex == null)
                throw new ArgumentNullException(nameof(vertex));

            Trigger.AddEvent(new VertexAddedEvent(vertex));
        }

        protected void OnVertexRemoved(IVertex vertex, IDictionary<string, object> props)
        {
            GraphChangedListenerContract.ValidateVertexRemoved(vertex, props);

            if (vertex == null)
                throw new ArgumentNullException(nameof(vertex));
            if (props == null)
                throw new ArgumentNullException(nameof(props));

            Trigger.AddEvent(new VertexRemovedEvent(vertex, props));
        }

        protected void OnEdgeAdded(IEdge edge)
        {
            GraphChangedListenerContract.ValidateEdgeAdded(edge);

            if (edge == null)
                throw new ArgumentNullException(nameof(edge));

            Trigger.AddEvent(new EdgeAddedEvent(edge));
        }

        protected void OnEdgeRemoved(IEdge edge, IDictionary<string, object> props)
        {
            GraphChangedListenerContract.ValidateEdgeRemoved(edge, props);

            if (edge == null)
                throw new ArgumentNullException(nameof(edge));
            if (props == null)
                throw new ArgumentNullException(nameof(props));

            Trigger.AddEvent(new EdgeRemovedEvent(edge, props));
        }

        public override string ToString()
        {
            return this.GraphString(BaseGraph.ToString());
        }
    }
}