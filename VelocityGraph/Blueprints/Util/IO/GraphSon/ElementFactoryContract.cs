using System;

namespace Frontenac.Blueprints.Util.IO.GraphSON
{
    public static class ElementFactoryContract
    {
        public static void ValidateCreateEdge(object id, IVertex out_, IVertex in_, string label)
        {
            if (out_ == null)
                throw new ArgumentNullException(nameof(out_));
            if (in_ == null)
                throw new ArgumentNullException(nameof(in_));
            if (string.IsNullOrWhiteSpace(label))
                throw new ArgumentNullException(nameof(label));
        }
    }
}