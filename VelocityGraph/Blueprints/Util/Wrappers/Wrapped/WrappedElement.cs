using System;
using System.Collections.Generic;
using Frontenac.Blueprints.Contracts;

namespace Frontenac.Blueprints.Util.Wrappers.Wrapped
{
    public abstract class WrappedElement : DictionaryElement
    {
        protected WrappedElement(IElement element):base(element.Graph)
        {
            if (element == null)
                throw new ArgumentNullException(nameof(element));

            Element = element;
        }

        public override void SetProperty(string key, object value)
        {
            Element.SetProperty(key, value);
        }

        public override object GetProperty(string key)
        {
            ElementContract.ValidateGetProperty(key);
            return Element.GetProperty(key);
        }

        public override object RemoveProperty(string key)
        {
            ElementContract.ValidateRemoveProperty(key);
            return Element.RemoveProperty(key);
        }

        public override IEnumerable<string> GetPropertyKeys()
        {
            return Element.GetPropertyKeys();
        }

        public override object Id
        {
            get { return Element.Id; }
        }

        public override void Remove()
        {
            Element.Remove();
        }

        public override bool Equals(object obj)
        {
            return this.AreEqual(obj);
        }

        public override int GetHashCode()
        {
            return Element.GetHashCode();
        }

        public IElement Element { get; protected set; }

        public override string ToString()
        {
            return Element.ToString();
        }
    }
}