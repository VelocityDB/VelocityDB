using System;

namespace Frontenac.Blueprints.Contracts
{
    public static class IndexableGraphContract
    {
        public static void ValidateCreateIndex(string indexName, Type indexClass, params Parameter[] indexParameters)
        {
            if (string.IsNullOrWhiteSpace(indexName))
                throw new ArgumentNullException(nameof(indexName));

            if (indexClass == null)
                throw new ArgumentNullException(nameof(indexClass));

            if (!(indexClass.IsAssignableFrom(typeof(IVertex)) ||
                  indexClass.IsAssignableFrom(typeof(IEdge))))
                throw new ArgumentException("indexClass must be assignable from IVertex of IEdge");
        }

        public static void ValidateGetIndex(string indexName, Type indexClass)
        {
            if (string.IsNullOrWhiteSpace(indexName))
                throw new ArgumentNullException(nameof(indexName));
            if (indexClass == null)
                throw new ArgumentNullException(nameof(indexClass));
            if (!(indexClass.IsAssignableFrom(typeof(IVertex)) ||
                  indexClass.IsAssignableFrom(typeof(IEdge))))
                throw new ArgumentException("indexClass must be assignable from IVertex of IEdge");
        }

        public static void ValidateDropIndex(string indexName)
        {
            if (string.IsNullOrWhiteSpace(indexName))
                throw new ArgumentNullException(nameof(indexName));
        }
    }
}