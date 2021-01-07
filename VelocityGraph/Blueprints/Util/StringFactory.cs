using System;

namespace Frontenac.Blueprints.Util
{
    /// <summary>
    ///     A collection of helpful methods for creating standard toString() representations of graph-related objects.
    /// </summary>
    public static class StringFactory
    {
        public const string V = "v";
        public const string E = "e";
        public const string LBracket = "[";
        public const string RBracket = "]";
        public const string Dash = "-";
        public const string Arrow = "->";
        public const string Colon = ":";

        public const string Id = "id";
        public const string Label = "label";
        public const string EmptyString = "";

        public static string VertexString(this IVertex vertex)
        {
            if (vertex == null)
                throw new ArgumentNullException(nameof(vertex));

            return string.Concat(V, LBracket, vertex.Id, RBracket);
        }

        public static string EdgeString(this IEdge edge)
        {
            if (edge == null)
                throw new ArgumentNullException(nameof(edge));

            return string.Concat(E, LBracket, edge.Id, RBracket, LBracket, edge.GetVertex(Direction.Out).Id,
                                 Dash, edge.Label, Arrow, edge.GetVertex(Direction.In).Id, RBracket);
        }

        public static string GraphString(this IGraph graph, string internalString)
        {
            if (graph == null)
                throw new ArgumentNullException(nameof(graph));
            if (string.IsNullOrWhiteSpace(internalString))
                throw new ArgumentNullException(nameof(internalString));

            return string.Concat(graph.GetType().Name.ToLower(), LBracket, internalString, RBracket);
        }

        public static string IndexString(this IIndex index)
        {
            if (index == null)
                throw new ArgumentNullException(nameof(index));

            return string.Concat("index", LBracket, index.Name, Colon, index.Type.Name, RBracket);
        }
    }
}