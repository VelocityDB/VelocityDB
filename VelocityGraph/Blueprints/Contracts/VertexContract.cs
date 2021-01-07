using System;

namespace Frontenac.Blueprints.Contracts
{
    public static class VertexContract
    {
        public static void ValidateAddEdge(object id, string label, IVertex inVertex)
        {
            if (string.IsNullOrWhiteSpace(label))
                throw new ArgumentNullException(nameof(label));
            if (inVertex == null)
                throw new ArgumentNullException(nameof(inVertex));
        }
    }
}