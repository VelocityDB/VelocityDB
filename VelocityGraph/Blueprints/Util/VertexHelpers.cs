using System;
using System.Collections.Generic;
using System.Linq;

namespace Frontenac.Blueprints.Util
{
    public static class VertexHelpers
    {
        /// <summary>
        ///     Test whether the two vertices have equal properties and edge sets.
        /// </summary>
        /// <param name="a">the first vertex </param>
        /// <param name="b">the second vertex </param>
        /// <param name="checkIdEquality">whether to check on vertex and edge ids </param>
        /// <returns>whether the two vertices are semantically the same </returns>
        public static bool HaveEqualNeighborhood(this IVertex a, IVertex b, bool checkIdEquality)
        {
            if (a == null)
                throw new ArgumentNullException(nameof(a));
            if (b == null)
                throw new ArgumentNullException(nameof(b));

            if (checkIdEquality && !a.HaveEqualIds(b))
                return false;

            return a.HaveEqualProperties(b) && HaveEqualEdges(a, b, checkIdEquality);
        }

        /// <summary>
        ///     Test whether the two vertices have equal edge sets
        /// </summary>
        /// <param name="a">the first vertex</param>
        /// <param name="b">the second vertex</param>
        /// <param name="checkIdEquality">whether to check on vertex and edge ids</param>
        /// <returns>whether the two vertices have the same edge sets</returns>
        public static bool HaveEqualEdges(this IVertex a, IVertex b, bool checkIdEquality)
        {
            if (a == null)
                throw new ArgumentNullException(nameof(a));
            if (b == null)
                throw new ArgumentNullException(nameof(b));

            var aEdgeSet = new HashSet<IEdge>(a.GetEdges(Direction.Out));
            var bEdgeSet = new HashSet<IEdge>(b.GetEdges(Direction.Out));

            if (!HasEqualEdgeSets(aEdgeSet, bEdgeSet, checkIdEquality))
                return false;

            aEdgeSet.Clear();
            bEdgeSet.Clear();

            foreach (var edge in a.GetEdges(Direction.In))
                aEdgeSet.Add(edge);

            foreach (var edge in b.GetEdges(Direction.In))
                bEdgeSet.Add(edge);

            return HasEqualEdgeSets(aEdgeSet, bEdgeSet, checkIdEquality);
        }

        private static bool HasEqualEdgeSets(this ICollection<IEdge> aEdgeSet, ICollection<IEdge> bEdgeSet,
                                             bool checkIdEquality)
        {
            if (aEdgeSet == null)
                throw new ArgumentNullException(nameof(aEdgeSet));
            if (bEdgeSet == null)
                throw new ArgumentNullException(nameof(bEdgeSet));

            if (aEdgeSet.Count != bEdgeSet.Count)
                return false;

            foreach (var aEdge in aEdgeSet)
            {
                IEdge tempEdge = null;
                var edge = aEdge;
                foreach (var bEdge in bEdgeSet.Where(bEdge => bEdge.Label == edge.Label))
                {
                    if (checkIdEquality)
                    {
                        if (aEdge.HaveEqualIds(bEdge) &&
                            aEdge.GetVertex(Direction.In).HaveEqualIds(bEdge.GetVertex(Direction.In)) &&
                            aEdge.GetVertex(Direction.Out).HaveEqualIds(bEdge.GetVertex(Direction.Out)) &&
                            aEdge.HaveEqualProperties(bEdge))
                        {
                            tempEdge = bEdge;
                            break;
                        }
                    }
                    else if (aEdge.HaveEqualProperties(bEdge))
                    {
                        tempEdge = bEdge;
                        break;
                    }
                }
                if (tempEdge == null)
                    return false;
                bEdgeSet.Remove(tempEdge);
            }
            return bEdgeSet.Count == 0;
        }
    }
}