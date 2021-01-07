using System;
using System.Collections.Concurrent;
using System.Threading;
using Frontenac.Blueprints.Util.Wrappers.Event.Listener;

namespace Frontenac.Blueprints.Util.Wrappers.Event
{
    public class EventTrigger : IDisposable
    {
        /// <summary>
        ///     When set to true, events in the event queue will only be fired when a transaction
        ///     is committed.
        /// </summary>
        private readonly bool _enqueEvents;

        /// <summary>
        ///     A queue of events that are triggered by change to the graph.  The queue builds
        ///     up until the EventTrigger fires them in the order they were received.
        /// </summary>
        private readonly ThreadLocal<ConcurrentQueue<IEvent>> _eventQueue =
            new ThreadLocal<ConcurrentQueue<IEvent>>(() => new ConcurrentQueue<IEvent>());

        private readonly EventGraph _graph;

        public EventTrigger(EventGraph graph, bool enqueEvents)
        {
            if (graph == null)
                throw new ArgumentNullException(nameof(graph));

            _enqueEvents = enqueEvents;
            _graph = graph;
        }

        #region IDisposable

        private bool _disposed;

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        ~EventTrigger()
        {
            Dispose(false);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
                return;

            if (disposing)
            {
                _eventQueue.Dispose();
            }

            _disposed = true;
        }

        #endregion

        /// <summary>
        ///     Add an event to the event queue.
        ///     If the enqueEvents is false, then the queue fires and resets after each event
        /// </summary>
        /// <param name="evt">The event to add to the event queue</param>
        public void AddEvent(IEvent evt)
        {
            if (evt == null)
                throw new ArgumentNullException(nameof(evt));

            _eventQueue.Value.Enqueue(evt);

            if (!_enqueEvents)
            {
                FireEventQueue();
                ResetEventQueue();
            }
        }

        public void ResetEventQueue()
        {
            _eventQueue.Value = new ConcurrentQueue<IEvent>();
        }

        public void FireEventQueue()
        {
            var concurrentQueue = _eventQueue.Value;
            IEvent event_;
            while (concurrentQueue.TryDequeue(out event_))
            {
                event_.FireEvent(_graph.GetListenerIterator());
            }
        }
    }
}