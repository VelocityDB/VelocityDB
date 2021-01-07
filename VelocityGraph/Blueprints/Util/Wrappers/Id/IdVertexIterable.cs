using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Frontenac.Blueprints.Util.Wrappers.Id
{
    internal class IdVertexIterable : ICloseableIterable<IVertex>
    {
        private readonly IdGraph _idGraph;
        private readonly IEnumerable<IElement> _iterable;
        private bool _disposed;

        public IdVertexIterable(IEnumerable<IElement> iterable, IdGraph idGraph)
        {
            if (iterable == null)
                throw new ArgumentNullException(nameof(iterable));
            if (idGraph == null)
                throw new ArgumentNullException(nameof(idGraph));

            _iterable = iterable;
            _idGraph = idGraph;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public IEnumerator<IVertex> GetEnumerator()
        {
            return (_iterable.OfType<IVertex>().Select(v => new IdVertex(v, _idGraph))).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return (this as IEnumerable<IVertex>).GetEnumerator();
        }

        ~IdVertexIterable()
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
    }
}