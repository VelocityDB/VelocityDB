using System;
using System.Collections.Generic;
using Frontenac.Blueprints.Contracts;

namespace Frontenac.Blueprints.Util.Wrappers.Id
{
    public class IdVertexIndex : IIndex
    {
        private readonly IIndex _baseIndex;
        private readonly IdGraph _idGraph;

        public IdVertexIndex(IIndex baseIndex, IdGraph idGraph)
        {
            if (baseIndex == null)
                throw new ArgumentNullException(nameof(baseIndex));
            if (idGraph == null)
                throw new ArgumentNullException(nameof(idGraph));

            _idGraph = idGraph;
            _baseIndex = baseIndex;
        }

        public string Name
        {
            get { return _baseIndex.Name; }
        }

        public Type Type
        {
            get { return _baseIndex.Type; }
        }

        public void Put(string key, object value, IElement element)
        {
            IndexContract.ValidatePut(key, value, element);

            _baseIndex.Put(key, value, GetBaseElement(element));
        }

        public IEnumerable<IElement> Get(string key, object value)
        {
            IndexContract.ValidateGet(key, value);

            return new IdVertexIterable(_baseIndex.Get(key, value), _idGraph);
        }

        public IEnumerable<IElement> Query(string key, object value)
        {
            return new IdVertexIterable(_baseIndex.Query(key, value), _idGraph);
        }

        public long Count(string key, object value)
        {
            IndexContract.ValidateCount(key, value);

            return _baseIndex.Count(key, value);
        }

        public void Remove(string key, object value, IElement element)
        {
            IndexContract.ValidateRemove(key, value, element);

            _baseIndex.Remove(key, value, GetBaseElement(element));
        }

        private IVertex GetBaseElement(IElement e)
        {
            if(!(e is IdVertex))
                throw new ArgumentException("e must be of type IdVertex");

            return ((IdVertex) e).GetBaseVertex();
        }

        public override string ToString()
        {
            return this.IndexString();
        }
    }
}