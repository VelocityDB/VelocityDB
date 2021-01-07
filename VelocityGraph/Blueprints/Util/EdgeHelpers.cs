using System;
using System.Collections.Generic;

namespace Frontenac.Blueprints.Util
{
    public static class EdgeHelpers
    {
        /// <summary>
        ///     An edge is relabeled by creating a new edge with the same properties, but new label.
        ///     Note that an edge is deleted and an edge is added.
        /// </summary>
        /// <param name="graph">the graph to add the new edge to</param>
        /// <param name="oldEdge">the existing edge to "relabel"</param>
        /// <param name="newId">the id of the new edge</param>
        /// <param name="newLabel">the label of the new edge</param>
        /// <returns>the newly created edge</returns>
        public static IEdge RelabelEdge(this IEdge oldEdge, IGraph graph, object newId, string newLabel)
        {
            if (graph == null)
                throw new ArgumentNullException(nameof(graph));
            if (oldEdge == null)
                throw new ArgumentNullException(nameof(oldEdge));
            if (string.IsNullOrWhiteSpace(newLabel))
                throw new ArgumentNullException(nameof(newLabel));

            var outVertex = oldEdge.GetVertex(Direction.Out);
            var inVertex = oldEdge.GetVertex(Direction.In);
            var newEdge = graph.AddEdge(newId, outVertex, inVertex, newLabel);
            oldEdge.CopyProperties(newEdge);
            graph.RemoveEdge(oldEdge);
            return newEdge;
        }

        /// <summary>
        ///     Edges are relabeled by creating new edges with the same properties, but new label.
        ///     Note that for each edge is deleted and an edge is added.
        /// </summary>
        /// <param name="graph">the graph to add the new edge to</param>
        /// <param name="oldEdges">the existing edges to "relabel"</param>
        /// <param name="newLabel">the label of the new edge</param>
        public static void RelabelEdges(this IEnumerable<IEdge> oldEdges, IGraph graph, string newLabel)
        {
            if (graph == null)
                throw new ArgumentNullException(nameof(graph));
            if (oldEdges == null)
                throw new ArgumentNullException(nameof(oldEdges));
            if (string.IsNullOrWhiteSpace(newLabel))
                throw new ArgumentNullException(nameof(newLabel));

            foreach (var oldEdge in oldEdges)
            {
                var outVertex = oldEdge.GetVertex(Direction.Out);
                var inVertex = oldEdge.GetVertex(Direction.In);
                var newEdge = graph.AddEdge(null, outVertex, inVertex, newLabel);
                oldEdge.CopyProperties(newEdge);
                graph.RemoveEdge(oldEdge);
            }
        }
    }
}