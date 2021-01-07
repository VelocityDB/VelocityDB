using System;
using System.Collections.Generic;
using Frontenac.Blueprints.Contracts;
using Frontenac.Blueprints.Util.Wrappers.Event.Listener;

namespace Frontenac.Blueprints.Util.Wrappers.Event
{
    /// <summary>
    ///     An element with a GraphChangedListener attached.  Those listeners are notified when changes occur to
    ///     the properties of the element.
    /// </summary>
    public abstract class EventElement : DictionaryElement
    {
        protected readonly IElement Element;
        protected readonly EventGraph EventInnerTinkerGrapĥ;

        protected EventElement(IElement element, EventGraph eventInnerTinkerGrapĥ):base(eventInnerTinkerGrapĥ)
        {
            if (element == null)
                throw new ArgumentNullException(nameof(element));

            if (eventInnerTinkerGrapĥ == null)
                throw new ArgumentNullException(nameof(eventInnerTinkerGrapĥ));

            Element = element;
            EventInnerTinkerGrapĥ = eventInnerTinkerGrapĥ;
        }

        public override IEnumerable<string> GetPropertyKeys()
        {
            return Element.GetPropertyKeys();
        }

        public override object Id
        {
            get { return Element.Id; }
        }

        /// <note>
        ///     Raises a vertexPropertyRemoved or edgePropertyRemoved event.
        /// </note>
        public override object RemoveProperty(string key)
        {
            ElementContract.ValidateRemoveProperty(key);
            var propertyRemoved = Element.RemoveProperty(key);

            var vertex = this as IVertex;
            if (vertex != null)
                OnVertexPropertyRemoved(vertex, key, propertyRemoved);
            else
            {
                var edge = this as IEdge;
                if (edge != null)
                    OnEdgePropertyRemoved(edge, key, propertyRemoved);
            }

            return propertyRemoved;
        }

        public override object GetProperty(string key)
        {
            return Element.GetProperty(key);
        }

        /// <note>
        ///     Raises a vertexPropertyRemoved or edgePropertyChanged event.
        /// </note>
        public override void SetProperty(string key, object value)
        {
            ElementContract.ValidateSetProperty(key, value);
            object oldValue = Element.GetProperty(key);
            Element.SetProperty(key, value);

            var vertex = this as IVertex;
            if (vertex != null)
                OnVertexPropertyChanged(vertex, key, oldValue, value);
            else
            {
                var edge = this as IEdge;
                if (edge != null)
                    OnEdgePropertyChanged(edge, key, oldValue, value);
            }
        }

        public override void Remove()
        {
            var vertex = this as IVertex;
            if (vertex != null)
                EventInnerTinkerGrapĥ.RemoveVertex(vertex);
            else
                EventInnerTinkerGrapĥ.RemoveEdge((IEdge) this);
        }

        protected void OnVertexPropertyChanged(IVertex vertex, string key, object oldValue, object newValue)
        {
            GraphChangedListenerContract.ValidateVertexPropertyChanged(vertex, key, oldValue, newValue);

            if (vertex == null)
                throw new ArgumentNullException(nameof(vertex));
            if (string.IsNullOrWhiteSpace(key))
                throw new ArgumentNullException(nameof(key));

            EventInnerTinkerGrapĥ.GetTrigger().AddEvent(new VertexPropertyChangedEvent(vertex, key, oldValue, newValue));
        }

        protected void OnEdgePropertyChanged(IEdge edge, string key, object oldValue, object newValue)
        {
            GraphChangedListenerContract.ValidateEdgePropertyChanged(edge, key, oldValue, newValue);

            if (edge == null)
                throw new ArgumentNullException(nameof(edge));
            if (string.IsNullOrWhiteSpace(key))
                throw new ArgumentNullException(nameof(key));

            EventInnerTinkerGrapĥ.GetTrigger().AddEvent(new EdgePropertyChangedEvent(edge, key, oldValue, newValue));
        }

        protected void OnVertexPropertyRemoved(IVertex vertex, string key, object removedValue)
        {
            GraphChangedListenerContract.ValidateVertexPropertyRemoved(vertex, key, removedValue);

            if (vertex == null)
                throw new ArgumentNullException(nameof(vertex));
            if (string.IsNullOrWhiteSpace(key))
                throw new ArgumentNullException(nameof(key));

            EventInnerTinkerGrapĥ.GetTrigger().AddEvent(new VertexPropertyRemovedEvent(vertex, key, removedValue));
        }

        protected void OnEdgePropertyRemoved(IEdge edge, string key, object removedValue)
        {
            GraphChangedListenerContract.ValidateEdgePropertyRemoved(edge, key, removedValue);

            if (edge == null)
                throw new ArgumentNullException(nameof(edge));
            if (string.IsNullOrWhiteSpace(key))
                throw new ArgumentNullException(nameof(key));

            EventInnerTinkerGrapĥ.GetTrigger().AddEvent(new EdgePropertyRemovedEvent(edge, key, removedValue));
        }

        public override string ToString()
        {
            return Element.ToString();
        }

        public override int GetHashCode()
        {
            return Element.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            return this.AreEqual(obj);
        }

        public IElement GetBaseElement()
        {
            return Element;
        }
    }
}