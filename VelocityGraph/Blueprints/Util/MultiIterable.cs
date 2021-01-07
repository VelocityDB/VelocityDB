using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Frontenac.Blueprints.Util
{
    /// <summary>
    ///     A helper class that is used to combine multiple iterables into a single closeable IEnumerable.
    /// </summary>
    /// <typeparam name="TS"></typeparam>
    public class MultiIterable<TS> : ICloseableIterable<TS>
    {
        private readonly IEnumerable<IEnumerable<TS>> _iterables;
        private bool _disposed;

        public MultiIterable(IEnumerable<IEnumerable<TS>> iterables)
        {
            if (iterables == null)
                throw new ArgumentNullException(nameof(iterables));

            _iterables = iterables;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public IEnumerator<TS> GetEnumerator()
        {
            return _iterables.SelectMany(current => current).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return (this as IEnumerable<TS>).GetEnumerator();
        }

        ~MultiIterable()
        {
            Dispose(false);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
                return;

            if (disposing)
            {
                foreach (var itty in _iterables.OfType<IDisposable>())
                {
                    itty.Dispose();
                }
            }

            _disposed = true;
        }
    }
}