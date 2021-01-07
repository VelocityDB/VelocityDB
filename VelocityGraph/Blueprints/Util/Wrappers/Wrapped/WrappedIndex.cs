using System;
using System.Collections.Generic;
using Frontenac.Blueprints.Contracts;

namespace Frontenac.Blueprints.Util.Wrappers.Wrapped
{
    public class WrappedIndex : IIndex
    {
        protected IIndex RawIndex;

        public WrappedIndex(IIndex rawIndex)
        {
            if (rawIndex == null)
                throw new ArgumentNullException(nameof(rawIndex));

            RawIndex = rawIndex;
        }

        public string Name
        {
            get { return RawIndex.Name; }
        }

        public Type Type
        {
            get { return RawIndex.Type; }
        }

        public long Count(string key, object value)
        {
            IndexContract.ValidateCount(key, value);

            return RawIndex.Count(key, value);
        }

        public void Remove(string key, object value, IElement element)
        {
            IndexContract.ValidateRemove(key, value, element);

            var wrappedElement = element as WrappedElement;
            if (wrappedElement != null)
                RawIndex.Remove(key, value, wrappedElement.Element);
        }

        public void Put(string key, object value, IElement element)
        {
            IndexContract.ValidatePut(key, value, element);

            var wrappedElement = element as WrappedElement;
            if (wrappedElement != null)
                RawIndex.Put(key, value, wrappedElement.Element);
        }

        public IEnumerable<IElement> Get(string key, object value)
        {
            IndexContract.ValidateGet(key, value);

            return RawIndex.Get(key, value);
        }

        public IEnumerable<IElement> Query(string key, object value)
        {
            return RawIndex.Query(key, value);
        }

        public override string ToString()
        {
            return this.IndexString();
        }
    }
}