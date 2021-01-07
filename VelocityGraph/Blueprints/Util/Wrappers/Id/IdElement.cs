using System;
using System.Collections.Generic;
using Frontenac.Blueprints.Contracts;

namespace Frontenac.Blueprints.Util.Wrappers.Id
{
    public abstract class IdElement : DictionaryElement
    {
        protected readonly IElement BaseElement;
        protected readonly IdGraph IdInnerTinkerGrapĥ;
        protected readonly bool PropertyBased;

        protected IdElement(IElement baseElement, IdGraph idInnerTinkerGrapĥ, bool propertyBased):base(idInnerTinkerGrapĥ)
        {
            if (baseElement == null)
                throw new ArgumentNullException(nameof(baseElement));
            if (idInnerTinkerGrapĥ == null)
                throw new ArgumentNullException(nameof(idInnerTinkerGrapĥ));

            BaseElement = baseElement;
            IdInnerTinkerGrapĥ = idInnerTinkerGrapĥ;
            PropertyBased = propertyBased;
        }

        public override object GetProperty(string key)
        {
            if (PropertyBased && key == IdGraph.Id)
                return null;

            return BaseElement.GetProperty(key);
        }

        public override IEnumerable<string> GetPropertyKeys()
        {
            if (PropertyBased)
            {
                var keys = BaseElement.GetPropertyKeys();
                var s = new HashSet<string>(keys);
                s.Remove(IdGraph.Id);
                return s;
            }

            return BaseElement.GetPropertyKeys();
        }

        public override void SetProperty(string key, object value)
        {
            ElementContract.ValidateSetProperty(key, value);

            if (PropertyBased && key == IdGraph.Id)
                throw new ArgumentException(string.Concat("Unable to set value for reserved property ", IdGraph.Id));

            BaseElement.SetProperty(key, value);
        }

        public override object RemoveProperty(string key)
        {
            ElementContract.ValidateRemoveProperty(key);
            if (PropertyBased && key == IdGraph.Id)
                throw new ArgumentException(string.Concat("Unable to remove value for reserved property ", IdGraph.Id));

            return BaseElement.RemoveProperty(key);
        }

        public override object Id
        {
            get
            {
                return PropertyBased
                           ? BaseElement.GetProperty(IdGraph.Id)
                           : BaseElement.Id;
            }
        }

        public override void Remove()
        {
            var vertex = this as IVertex;
            if (vertex != null)
                IdInnerTinkerGrapĥ.RemoveVertex(vertex);
            else
                IdInnerTinkerGrapĥ.RemoveEdge((IEdge) this);
        }

        public override int GetHashCode()
        {
            return BaseElement.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            return this.AreEqual(obj);
        }

        public override string ToString()
        {
            return BaseElement.ToString();
        }
    }
}