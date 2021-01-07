using System;

namespace Frontenac.Blueprints.Util.Wrappers.Event
{
    /// <summary>
    ///     The transactional and indexable implementation of eventInnerTinkerGrapĥ where events are raised in batch in the order they
    ///     changes occured to the graph, but only after a successful commit to the underlying graph.
    /// </summary>
    public class EventTransactionalIndexableGraph : EventIndexableGraph, ITransactionalGraph
    {
        protected readonly ITransactionalGraph TransactionalGraph;

        public EventTransactionalIndexableGraph(IIndexableGraph baseIndexableGraph)
            : base(baseIndexableGraph)
        {
            if(!(baseIndexableGraph is ITransactionalGraph))
                throw new ArgumentException("baseIndexableGraph must be of type ITransactionalGraph");

            TransactionalGraph = baseIndexableGraph as ITransactionalGraph;
            Trigger = new EventTrigger(this, true);
        }

        /// <summary>
        ///     A commit only fires the event queue on successful operation.  If the commit operation to the underlying
        ///     graph fails, the event queue will not fire and the queue will not be reset.
        /// </summary>
        public void Commit()
        {
            var transactionFailure = false;
            try
            {
                TransactionalGraph.Commit();
            }
            catch (Exception)
            {
                transactionFailure = true;
                throw;
            }
            finally
            {
                if (!transactionFailure)
                {
                    Trigger.FireEventQueue();
                    Trigger.ResetEventQueue();
                }
            }
        }

        /// <summary>
        ///     A rollback only resets the event queue on successful operation.  If the rollback operation to the underlying
        ///     graph fails, the event queue will not be reset.
        /// </summary>
        public void Rollback()
        {
            var transactionFailure = false;
            try
            {
                TransactionalGraph.Rollback();
            }
            catch (Exception)
            {
                transactionFailure = true;
                throw;
            }
            finally
            {
                if (!transactionFailure)
                {
                    Trigger.ResetEventQueue();
                }
            }
        }
    }
}