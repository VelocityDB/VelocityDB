namespace Frontenac.Blueprints
{
    /// <summary>
    ///     A transactional graph supports the notion of transactions. A transaction scopes a logically coherent operation composed
    ///     of multiple read and write operations that either occurs at once or not at all. The exact notion of a transaction and its
    ///     isolational guarantees depend on the implementing graph database.
    ///     A transaction scopes a coherent and complete operations. Any element references created during a transaction should not
    ///     be accessed outside its scope (i.e. after the transaction is committed or rolled back). Accessing such references outside
    ///     the transactional context they were created in may lead to exceptions. If such access is necessary, the transactional
    ///     context should be extended.
    ///     By default, the first operation on a TransactionalGraph will start a transaction automatically.
    /// </summary>
    public interface ITransactionalGraph : IGraph
    {
        /// <summary>
        ///     Stop the current transaction and successfully apply mutations to the graph.
        /// </summary>
        void Commit();

        /// <summary>
        ///     Stop the current transaction and drop any mutations applied since the last transaction.
        /// </summary>
        void Rollback();
    }
}