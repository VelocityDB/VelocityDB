using System;

namespace Frontenac.Blueprints.Util.Wrappers.Event
{
    public class EventTransactionalGraph : EventGraph, ITransactionalGraph
    {
        protected readonly ITransactionalGraph TransactionalGraph;

        public EventTransactionalGraph(ITransactionalGraph transactionalGraph)
            : base(transactionalGraph)
        {
            if (transactionalGraph == null)
                throw new ArgumentNullException(nameof(transactionalGraph));

            TransactionalGraph = transactionalGraph;
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