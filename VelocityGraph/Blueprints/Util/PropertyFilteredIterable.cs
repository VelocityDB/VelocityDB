using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Frontenac.Blueprints.Util
{
    /// <summary>
    ///     This is a helper class for filtering an IEnumerable of elements by their key/value.
    ///     Useful for Graph implementations that do no support automatic key indices and need to filter on Graph.getVertices/Edges(key,value).
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class PropertyFilteredIterable<T> : ICloseableIterable<T> where T : class, IElement
    {
        private readonly IEnumerable<T> _iterable;
        private readonly string _key;
        private readonly object _value;
        private bool _disposed;

        public PropertyFilteredIterable(string key, object value, IEnumerable<T> iterable)
        {
            if (string.IsNullOrWhiteSpace(key))
                throw new ArgumentNullException(nameof(key));
            if (iterable == null)
                throw new ArgumentNullException(nameof(iterable));

            _key = key;
            _value = value;
            _iterable = iterable;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public IEnumerator<T> GetEnumerator()
        {
            return new PropertyFilteredIterableIterable<T>(this).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return (this as IEnumerable<T>).GetEnumerator();
        }

        ~PropertyFilteredIterable()
        {
            Dispose(false);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
                return;

            if (disposing)
            {
                if (_iterable is IDisposable)
                    (_iterable as IDisposable).Dispose();
            }

            _disposed = true;
        }

        private class PropertyFilteredIterableIterable<TU> : IEnumerable<TU> where TU : class, IElement
        {
            private readonly IEnumerator<TU> _itty;
            private readonly PropertyFilteredIterable<TU> _propertyFilteredIterable;
            private TU _nextElement;

            public PropertyFilteredIterableIterable(PropertyFilteredIterable<TU> propertyFilteredIterable)
            {
                if (propertyFilteredIterable == null)
                    throw new ArgumentNullException(nameof(propertyFilteredIterable));

                _propertyFilteredIterable = propertyFilteredIterable;
                _itty = _propertyFilteredIterable._iterable.GetEnumerator();
            }

            public IEnumerator<TU> GetEnumerator()
            {
                while (HasNext())
                {
                    if (null != _nextElement)
                    {
                        var temp = _nextElement;
                        _nextElement = default(TU);
                        yield return temp;
                    }
                    else
                    {
                        while (_itty.MoveNext())
                        {
                            var element = _itty.Current;
                            if (element.GetPropertyKeys().Contains(_propertyFilteredIterable._key) &&
                                AreEqual(element.GetProperty(_propertyFilteredIterable._key),
                                         _propertyFilteredIterable._value))
                                yield return element;
                        }
                    }
                }
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }

            private bool HasNext()
            {
                if (_nextElement != null)
                    return true;
                while (_itty.MoveNext())
                {
                    var element = _itty.Current;
                    var temp = element.GetProperty(_propertyFilteredIterable._key);
                    if (null != temp)
                    {
                        if (AreEqual(temp, _propertyFilteredIterable._value))
                        {
                            _nextElement = element;
                            return true;
                        }
                    }
                    else
                    {
                        if (_propertyFilteredIterable._value == null)
                        {
                            _nextElement = element;
                            return true;
                        }
                    }
                }

                _nextElement = default(TU);
                return false;
            }

            private static bool AreEqual(object aVal, object bVal)
            {
                if (aVal == null && bVal == null)
                    return true;
                if ((aVal == null) || (bVal == null))
                    return false;
                if (Blueprints.GraphHelpers.IsNumber(aVal) && Blueprints.GraphHelpers.IsNumber(bVal))
                    return Convert.ToDouble(aVal).CompareTo(Convert.ToDouble(bVal)) == 0;
                return aVal.Equals(bVal);
            }
        }
    }
}