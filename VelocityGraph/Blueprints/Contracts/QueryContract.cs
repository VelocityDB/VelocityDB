using System;
namespace Frontenac.Blueprints.Contracts
{
    public static class QueryContract
    {
        public static void ValidateHas(string key, object value)
        {
            if (string.IsNullOrWhiteSpace(key))
                throw new ArgumentNullException(nameof(key));
        }

        public static void ValidateHas<T>(string key, Compare compare, T value)
        {
            if (string.IsNullOrWhiteSpace(key))
                throw new ArgumentNullException(nameof(key));
        }

        public static void ValidateInterval<T>(string key, T startValue, T endValue)
        {
            if (string.IsNullOrWhiteSpace(key))
                throw new ArgumentNullException(nameof(key));
        }

        public static void ValidateLimit(long max)
        {
            if (max <= 0)
                throw new ArgumentException("max must be greater than zero");
        }
    }
}