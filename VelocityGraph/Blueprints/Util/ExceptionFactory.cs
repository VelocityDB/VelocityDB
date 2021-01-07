using System;

namespace Frontenac.Blueprints.Util
{
    /// <summary>
    ///     The ExceptionFactory provides standard exceptions that should be used by all Blueprints implementations.
    ///     This ensures that the look-and-feel of all implementations are the same in terms of terminology and punctuation.
    /// </summary>
    public static class ExceptionFactory
    {
        // Graph related exceptions

        public static ArgumentException VertexIdCanNotBeNull()
        {
            return new ArgumentException("Vertex id can not be null");
        }

        public static ArgumentException EdgeIdCanNotBeNull()
        {
            return new ArgumentException("Edge id can not be null");
        }

        public static ArgumentException VertexWithIdAlreadyExists(object id)
        {
            return new ArgumentException(string.Format("Vertex with id already exists: {0}", id));
        }

        public static ArgumentException EdgeWithIdAlreadyExist(object id)
        {
            return new ArgumentException(string.Format("Edge with id already exists: {0}", id));
        }

        public static ArgumentException BothIsNotSupported()
        {
            return new ArgumentException("A direction of BOTH is not supported");
        }

        // Element related exceptions

        public static ArgumentException PropertyKeyIsReserved(string key)
        {
            return new ArgumentException(string.Format("Property key is reserved for all elements: {0}", key));
        }

        public static ArgumentException PropertyKeyIdIsReserved()
        {
            return new ArgumentException("Property key is reserved for all elements: id");
        }

        public static ArgumentException PropertyKeyLabelIsReservedForEdges()
        {
            return new ArgumentException("Property key is reserved for all edges: label");
        }

        public static ArgumentException PropertyKeyCanNotBeEmpty()
        {
            return new ArgumentException("Property key can not be the empty string");
        }

        public static ArgumentException PropertyKeyCanNotBeNull()
        {
            return new ArgumentException("Property key can not be null");
        }

        public static ArgumentException PropertyValueCanNotBeNull()
        {
            return new ArgumentException("Property value can not be null");
        }

        // IIndexableGraph related exceptions

        public static ArgumentException IndexAlreadyExists(string indexName)
        {
            return new ArgumentException(string.Format("Index already exists: {0}", indexName));
        }

        public static InvalidOperationException IndexDoesNotSupportClass(string indexName, Type clazz)
        {
            return new InvalidOperationException(string.Format("{0} does not support class: {1}", indexName, clazz));
        }

        // KeyIndexableGraph related exceptions

        public static ArgumentException ClassIsNotIndexable(Type clazz)
        {
            return new ArgumentException(string.Format("Class is not indexable: {0}", clazz));
        }

        // TransactionalGraph related exceptions

        public static InvalidOperationException TransactionAlreadyStarted()
        {
            return new InvalidOperationException("Stop the current transaction before starting another");
        }
    }
}