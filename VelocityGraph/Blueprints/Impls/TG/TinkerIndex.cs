using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Frontenac.Blueprints.Contracts;
using Frontenac.Blueprints.Util;

namespace Frontenac.Blueprints.Impls.TG
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
            if (indexClass == null)
                throw new ArgumentNullException(nameof(indexClass));

            if (!(typeof(IVertex).IsAssignableFrom(indexClass) ||
                  typeof(IEdge).IsAssignableFrom(indexClass)))
                throw new ArgumentException("indexClass must be assignable from IVertex of IEdge");

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
            IndexContract.ValidatePut(key, value, element);

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
            IndexContract.ValidateGet(key, value);

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
            IndexContract.ValidateCount(key, value);

            var keyMap = Index.Get(key);
            if (null == keyMap)
                return 0;
            var set = keyMap.Get(value);
            return null == set ? 0 : set.LongCount();
        }

        public void Remove(string key, object value, IElement element)
        {
            IndexContract.ValidateRemove(key, value, element);

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
            if (element == null)
                throw new ArgumentNullException(nameof(element));

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