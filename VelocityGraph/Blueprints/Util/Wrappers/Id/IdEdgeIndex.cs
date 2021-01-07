using System;
using System.Collections.Generic;
using Frontenac.Blueprints.Contracts;

namespace Frontenac.Blueprints.Util.Wrappers.Id
{
    public class IdEdgeIndex : IIndex
    {
        protected readonly IIndex BaseIndex;
        protected readonly IdGraph IdGraph;

        public IdEdgeIndex(IIndex baseIndex, IdGraph idGraph)
        {
            if (baseIndex == null)
                throw new ArgumentNullException(nameof(baseIndex));
            if (idGraph == null)
                throw new ArgumentNullException(nameof(idGraph));

            IdGraph = idGraph;
            BaseIndex = baseIndex;
        }

        public string Name
        {
            get
            {
                return BaseIndex.Name;
            }
        }

        public Type Type
        {
            get { return BaseIndex.Type; }
        }

        public void Put(string key, object value, IElement element)
        {
            IndexContract.ValidatePut(key, value, element);

            BaseIndex.Put(key, value, GetBaseElement(element));
        }

        public IEnumerable<IElement> Get(string key, object value)
        {
            IndexContract.ValidateGet(key, value);

            return new IdEdgeIterable(BaseIndex.Get(key, value), IdGraph);
        }

        public IEnumerable<IElement> Query(string key, object value)
        {
            return new IdEdgeIterable(BaseIndex.Query(key, value), IdGraph);
        }

        public long Count(string key, object value)
        {
            IndexContract.ValidateCount(key, value);

            return BaseIndex.Count(key, value);
        }

        public void Remove(string key, object value, IElement element)
        {
            IndexContract.ValidateRemove(key, value, element);

            BaseIndex.Remove(key, value, GetBaseElement(element));
        }

        public override string ToString()
        {
            return this.IndexString();
        }

        private static IEdge GetBaseElement(IElement e)
        {
            if(!(e is IdEdge))
                throw new ArgumentException("e must be of type IEdge");

            return ((IdEdge) e).GetBaseEdge();
        }
    }
}