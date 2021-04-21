using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using VelocityGraph.Frontenac.Blueprints.Util;

namespace VelocityGraph.Frontenac.Blueprints.Impls.TG
{
    [Serializable]
    internal class TinkerIndex : IIndex
    {
        protected readonly Type IndexClass;
        protected readonly string IndexName;

        internal ConcurrentDictionary<string, ConcurrentDictionary<object, ConcurrentDictionary<string, IElement>>> Index =
            new ConcurrentDictionary<string, ConcurrentDictionary<object, ConcurrentDictionary<string, IElement>>>();

        public TinkerIndex(string indexName, Type indexClass)
        {
            Contract.Requires(indexClass != null);
            Contract.Requires(typeof (IVertex).IsAssignableFrom(indexClass) ||
                              typeof (IEdge).IsAssignableFrom(indexClass));

            IndexName = indexName;
            IndexClass = indexClass;
        }

        public string Name
        {
            get { return IndexName; }
        }

        public Type Type
        {
            get { return IndexClass; }
        }

        public void Put(string key, object value, IElement element)
        {
            var keyMap = Index.Get(key);
            if (keyMap == null)
            {
                keyMap = new ConcurrentDictionary<object, ConcurrentDictionary<string, IElement>>();
                Index.Put(key, keyMap);
            }
            var objects = keyMap.Get(value);
            if (null == objects)
            {
                objects = new ConcurrentDictionary<string, IElement>();
                keyMap.Put(value, objects);
            }
            objects.TryAdd(element.Id.ToString(), element);
        }

        public IEnumerable<IElement> Get(string key, object value)
        {
            var keyMap = Index.Get(key);
            if (null == keyMap)
                return new WrappingCloseableIterable<IElement>(Enumerable.Empty<IElement>());

            var set = keyMap.Get(value);
            return null == set
                       ? new WrappingCloseableIterable<IElement>(Enumerable.Empty<IElement>())
                       : new WrappingCloseableIterable<IElement>(new List<IElement>(set.Values));
        }

        public IEnumerable<IElement> Query(string key, object query)
        {
            throw new NotImplementedException();
        }

        public long Count(string key, object value)
        {
            var keyMap = Index.Get(key);
            if (null == keyMap)
                return 0;
            var set = keyMap.Get(value);
            return null == set ? 0 : set.LongCount();
        }

        public void Remove(string key, object value, IElement element)
        {
            var keyMap = Index.Get(key);
            if (null != keyMap)
            {
                var objects = keyMap.Get(value);
                if (null != objects)
                {
                    IElement removedElement;
                    objects.TryRemove(element.Id.ToString(), out removedElement);
                    if (objects.Count == 0)
                    {
                        ConcurrentDictionary<string, IElement> removedElements;
                        keyMap.TryRemove(value, out removedElements);
                    }
                }
            }
        }

        public void RemoveElement(IElement element)
        {
            Contract.Requires(element != null);

            if (!IndexClass.IsInstanceOfType(element)) return;

            foreach (var map in Index.Values)
            {
                foreach (var set in map.Values)
                {
                    IElement removedElement;
                    set.TryRemove(element.Id.ToString(), out removedElement);
                }
            }
        }

        public override string ToString()
        {
            return this.IndexString();
        }
    }
}