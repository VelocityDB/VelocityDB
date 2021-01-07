using System;

namespace Frontenac.Blueprints.Contracts
{
    public static class IndexContract
    {
        
        public static void ValidatePut(string key, object value, IElement element)
        {
            if (string.IsNullOrWhiteSpace(key))
                throw new ArgumentNullException(nameof(key));

            if (element == null)
                throw new ArgumentNullException(nameof(element));
        }

        public static void ValidateGet(string key, object value)
        {
            if (string.IsNullOrWhiteSpace(key))
                throw new ArgumentNullException(nameof(key));
        }

        public static void ValidateQuery(string key, object query)
        {
            if (string.IsNullOrWhiteSpace(key))
                throw new ArgumentNullException(nameof(key));
        }

        public static void ValidateCount(string key, object value)
        {
            if (string.IsNullOrWhiteSpace(key))
                throw new ArgumentNullException(nameof(key));
        }

        public static void ValidateRemove(string key, object value, IElement element)
        {
            if (string.IsNullOrWhiteSpace(key))
                throw new ArgumentNullException(nameof(key));

            if (element == null)
                throw new ArgumentNullException(nameof(element));
        }
    }
}