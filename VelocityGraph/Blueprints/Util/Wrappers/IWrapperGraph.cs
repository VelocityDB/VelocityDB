namespace Frontenac.Blueprints.Util.Wrappers
{
    /// <summary>
    ///     A WrapperGraph has an underlying graph object to which it delegates its operations.
    /// </summary>
    public interface IWrapperGraph
    {
        /// <summary>
        ///     Get the graph this wrapper delegates to.
        /// </summary>
        /// <returns>the underlying graph that this WrapperGraph delegates its operations to.</returns>
        IGraph GetBaseGraph();
    }
}