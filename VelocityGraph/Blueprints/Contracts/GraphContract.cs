using System;

namespace Frontenac.Blueprints.Contracts
{
    public static class GraphContract
    {
        public static void ValidateGetVertex(object id)
        {
            if (id == null)
                throw new ArgumentNullException(nameof(id));
        }

        public static void ValidateRemoveVertex(IVertex vertex)
        {
            if (vertex == null)
                throw new ArgumentNullException(nameof(vertex));
        }
        

        public static void ValidateGetVertices(string key, object value)
        {
            if (string.IsNullOrWhiteSpace(key))
                throw new ArgumentNullException(nameof(key));
        }

        public static void ValidateAddEdge(object id, IVertex outVertex, IVertex inVertex, string label)
        {
            if (outVertex == null)
                throw new ArgumentNullException(nameof(outVertex));
            if (inVertex == null)
                throw new ArgumentNullException(nameof(inVertex));
            if (string.IsNullOrWhiteSpace(label))
                throw new ArgumentNullException(nameof(label));
        }

        public static void ValidateGetEdge(object id)
        {
            if (id == null)
                throw new ArgumentNullException(nameof(id));
        }

        public static void ValidateRemoveEdge(IEdge edge)
        {
            if (edge == null)
                throw new ArgumentNullException(nameof(edge));
        }
        
        public static void ValidateGetEdges(string key, object value)
        {
            if (string.IsNullOrWhiteSpace(key))
                throw new ArgumentNullException(nameof(key));
        }
    }
}