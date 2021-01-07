using System.Collections.Generic;

namespace Frontenac.Blueprints.Util.Wrappers.Event.Listener
{
    /// <summary>
    ///     Interface for a listener to eventInnerTinkerGrapĥ change events.
    ///     Implementations of this interface should be added to the list of listeners on the addListener method on
    ///     the eventInnerTinkerGrapĥ.
    /// </summary>
    public interface IGraphChangedListener
    {
        /// <summary>
        ///     Raised when a new Vertex is added.
        /// </summary>
        /// <param name="vertex">the vertex that was added</param>
        void VertexAdded(IVertex vertex);

        /// <summary>
        ///     Raised after the property of a vertex changed.
        /// </summary>
        /// <param name="vertex">the vertex that changed</param>
        /// <param name="key">the key of the property that changed</param>
        /// <param name="oldValue">the old value of the property</param>
        /// <param name="setValue">the new value of the property</param>
        void VertexPropertyChanged(IVertex vertex, string key, object oldValue, object setValue);

        /// <summary>
        ///     Raised after a vertex property was removed.
        /// </summary>
        /// <param name="vertex">the vertex that changed</param>
        /// <param name="key">the key that was removed</param>
        /// <param name="removedValue">the value of the property that was removed</param>
        void VertexPropertyRemoved(IVertex vertex, string key, object removedValue);

        /// <summary>
        ///     Raised after a vertex was removed from the graph.
        /// </summary>
        /// <param name="vertex">the vertex that was removed</param>
        /// <param name="props" />
        void VertexRemoved(IVertex vertex, IDictionary<string, object> props);

        /// <summary>
        ///     Raised after a new edge is added.
        /// </summary>
        void EdgeAdded(IEdge edge);

        /// <summary>
        ///     Raised after the property of a edge changed.
        /// </summary>
        /// <param name="edge">the edge that changed</param>
        /// <param name="key">the key of the property that changed</param>
        /// <param name="oldValue">the old value of the property</param>
        /// <param name="setValue">the new value of the property</param>
        void EdgePropertyChanged(IEdge edge, string key, object oldValue, object setValue);

        /// <summary>
        ///     Raised after an edge property was removed.
        /// </summary>
        /// <param name="edge">the edge that changed</param>
        /// <param name="key">the key that was removed</param>
        /// <param name="removedValue">the value of the property that was removed</param>
        void EdgePropertyRemoved(IEdge edge, string key, object removedValue);

        /// <summary>
        ///     Raised after an edge was removed from the graph.
        /// </summary>
        /// <param name="edge">the edge that was removed</param>
        /// <param name="props"></param>
        void EdgeRemoved(IEdge edge, IDictionary<string, object> props);
    }
}