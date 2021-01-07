using System;
using System.Collections;
using System.Collections.Generic;

namespace Frontenac.Blueprints.Util
{
    public class WrappingCloseableIterable<T> : ICloseableIterable<T>
    {
        private readonly IEnumerable<T> _iterable;
        private bool _disposed;

        public WrappingCloseableIterable(IEnumerable<T> iterable)
        {
            if (iterable == null)
                throw new ArgumentNullException(nameof(iterable));

            _iterable = iterable;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public IEnumerator<T> GetEnumerator()
        {
            return _iterable.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return (this as IEnumerable<T>).GetEnumerator();
        }

        ~WrappingCloseableIterable()
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

        public override string ToString()
        {
            return _iterable.ToString();
        }
    }
}