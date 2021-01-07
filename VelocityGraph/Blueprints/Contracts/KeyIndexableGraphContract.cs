using System;

namespace Frontenac.Blueprints.Contracts
{
    public static class KeyIndexableGraphContract
    {
        public static void ValidateDropKeyIndex(string key, Type elementClass)
        {
            if (string.IsNullOrWhiteSpace(key))
                throw new ArgumentNullException(nameof(key));

            if (elementClass == null)
                throw new ArgumentNullException(nameof(elementClass));
            if (!(elementClass.IsAssignableFrom(typeof(IVertex)) ||
                  elementClass.IsAssignableFrom(typeof(IEdge))))
                throw new ArgumentException("elementClass must be assignable from IVertex of IEdge");
        }

        public static void ValidateCreateKeyIndex(string key, Type elementClass, params Parameter[] indexParameters)
        {
            if (string.IsNullOrWhiteSpace(key))
                throw new ArgumentNullException(nameof(key));

            if (elementClass == null)
                throw new ArgumentNullException(nameof(elementClass));
            if(!(elementClass.IsAssignableFrom(typeof (IVertex)) ||
                 elementClass.IsAssignableFrom(typeof (IEdge))))
                throw new ArgumentException("elementClass must be assignable from IVertex of IEdge");
        }

        public static void ValidateGetIndexedKeys(Type elementClass)
        {
            if (elementClass == null)
                throw new ArgumentNullException(nameof(elementClass));
            if (!(elementClass.IsAssignableFrom(typeof(IVertex)) ||
                  elementClass.IsAssignableFrom(typeof(IEdge))))
                throw new ArgumentException("elementClass must be assignable from IVertex of IEdge");
        }
    }
}