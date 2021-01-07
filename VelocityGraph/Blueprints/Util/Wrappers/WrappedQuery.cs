using System;
using System.Collections.Generic;
using Frontenac.Blueprints.Contracts;

namespace Frontenac.Blueprints.Util.Wrappers
{
    public class WrappedQuery : IQuery
    {
        protected Func<IQuery, IEnumerable<IEdge>> EdgesSelector;
        protected IQuery Query;
        protected Func<IQuery, IEnumerable<IVertex>> VerticesSelector;

        public WrappedQuery(IQuery query, Func<IQuery, IEnumerable<IEdge>> edgesSelector,
                            Func<IQuery, IEnumerable<IVertex>> verticesSelector)
        {
            if (query == null)
                throw new ArgumentNullException(nameof(query));
            if (edgesSelector == null)
                throw new ArgumentNullException(nameof(edgesSelector));
            if (verticesSelector == null)
                throw new ArgumentNullException(nameof(verticesSelector));

            Query = query;
            EdgesSelector = edgesSelector;
            VerticesSelector = verticesSelector;
        }

        public IQuery Has(string key, object value)
        {
            QueryContract.ValidateHas(key, value);

            Query = Query.Has(key, value);
            return this;
        }

        public IQuery Has<T>(string key, Compare compare, T value)
        {
            QueryContract.ValidateHas(key, compare, value);

            Query = Query.Has(key, compare, value);
            return this;
        }

        public IQuery Interval<T>(string key, T startValue, T endValue)
        {
            QueryContract.ValidateInterval(key, startValue, endValue);

            Query = Query.Interval(key, startValue, endValue);
            return this;
        }

        public IQuery Limit(long max)
        {
            QueryContract.ValidateLimit(max);

            Query = Query.Limit(max);
            return this;
        }

        public IEnumerable<IEdge> Edges()
        {
            return EdgesSelector(Query);
        }

        public IEnumerable<IVertex> Vertices()
        {
            return VerticesSelector(Query);
        }
    }
}