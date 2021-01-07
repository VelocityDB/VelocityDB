using System;
using System.Collections.Generic;
using Frontenac.Blueprints.Contracts;

namespace Frontenac.Blueprints.Util.Wrappers.ReadOnly
{
    public abstract class ReadOnlyElement : DictionaryElement
    {
        protected readonly IElement BaseElement;
        protected readonly ReadOnlyGraph ReadOnlyInnerTinkerGrapĥ;

        protected ReadOnlyElement(ReadOnlyGraph innerTinkerGrapĥ, IElement baseElement):base(innerTinkerGrapĥ)
        {
            if (innerTinkerGrapĥ == null)
                throw new ArgumentNullException(nameof(innerTinkerGrapĥ));
            if (baseElement == null)
                throw new ArgumentNullException(nameof(baseElement));

            IsReadOnly = true;
            BaseElement = baseElement;
            ReadOnlyInnerTinkerGrapĥ = innerTinkerGrapĥ;
        }

        public override IEnumerable<string> GetPropertyKeys()
        {
            return BaseElement.GetPropertyKeys();
        }

        public override object Id
        {
            get { return BaseElement.Id; }
        }

        public override object RemoveProperty(string key)
        {
            ElementContract.ValidateRemoveProperty(key);
            throw new InvalidOperationException(ReadOnlyTokens.MutateErrorMessage);
        }

        public override object GetProperty(string key)
        {
            ElementContract.ValidateGetProperty(key);
            return BaseElement.GetProperty(key);
        }

        public override void SetProperty(string key, object value)
        {
            throw new InvalidOperationException(ReadOnlyTokens.MutateErrorMessage);
        }

        public override void Remove()
        {
            throw new InvalidOperationException(ReadOnlyTokens.MutateErrorMessage);
        }

        public override string ToString()
        {
            return BaseElement.ToString();
        }

        public override int GetHashCode()
        {
            return BaseElement.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            return this.AreEqual(obj);
        }
    }
}