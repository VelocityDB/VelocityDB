using System;
using Frontenac.Blueprints.Util.Wrappers.Batch.Cache;

namespace Frontenac.Blueprints.Util.Wrappers.Batch
{
    /// <summary>
    ///     Type of vertex ids expected by BatchGraph. The default is IdType.OBJECT.
    ///     Use the IdType that best matches the used vertex id types in order to save memory.
    /// </summary>
    public enum VertexIdType
    {
        Object,
        Number,
        String,
        Url
    }

    public static class VertexIdTypes
    {
        public static IVertexCache GetVertexCache(this VertexIdType vertexIdType)
        {
            switch (vertexIdType)
            {
                case VertexIdType.Object:
                    return new ObjectIdVertexCache();
                case VertexIdType.Number:
                    return new LongIdVertexCache();
                case VertexIdType.String:
                    return new StringIdVertexCache();
                case VertexIdType.Url:
                    return new StringIdVertexCache(new UrlCompression());
                default:
                    throw new ArgumentException(string.Concat("Unrecognized ID type: ", vertexIdType));
            }
        }
    }
}