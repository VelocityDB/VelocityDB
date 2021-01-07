using System;
using System.Linq;

namespace Frontenac.Blueprints.Util
{
    public static class IndexableGraphHelpers
    {
        /// <summary>
        ///     Add a vertex to a graph only if no other vertex in the provided Index is indexed by the property key/value pair.
        ///     If a vertex already exists with that key/value pair, return the pre-existing vertex.
        /// </summary>
        /// <param name="graph">the graph to add the vertex to</param>
        /// <param name="id">the id of the vertex to create (can be null)</param>
        /// <param name="index">the index to determine if another vertex with the same key/value exists</param>
        /// <param name="uniqueKey">the key to check on for uniqueness of the vertex</param>
        /// <param name="uniqueValue">the value to check on for uniqueness of the vertex</param>
        /// <returns>the newly created vertex or the vertex that satisfies the uniqueness criteria</returns>
        public static IVertex AddUniqueVertex(this IIndexableGraph graph, object id, IIndex index, string uniqueKey,
                                              object uniqueValue)
        {
            if (graph == null)
                throw new ArgumentNullException(nameof(graph));
            if (index == null)
                throw new ArgumentNullException(nameof(index));
            if (string.IsNullOrWhiteSpace(uniqueKey))
                throw new ArgumentNullException(nameof(uniqueKey));

            var result = (IVertex) index.Get(uniqueKey, uniqueValue).FirstOrDefault();
            if (result == null)
            {
                result = graph.AddVertex(id);
                result.SetProperty(uniqueKey, uniqueValue);
            }
            return result;
        }
    }
}