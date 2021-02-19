using System;
using System.Collections.Generic;

namespace RelSandbox
{
    public static class DictionaryExtensions
    {
        public static TValue GetOrAdd<TKey, TValue>(this IDictionary<TKey, TValue> obj, TKey key, Func<TKey, TValue> valueFactory)
        {
            TValue v;

            if (obj.TryGetValue(key, out v) == false)
            {
                v = valueFactory(key);
                obj.Add(key, v);
            }

            return v;
        }
    }
}
