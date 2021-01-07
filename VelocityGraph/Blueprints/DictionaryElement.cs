using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Frontenac.Blueprints
{
    [Serializable]
    public abstract class DictionaryElement : IElement
    {
        protected DictionaryElement(IGraph tinkerGrapĥ)
        {
            Graph = tinkerGrapĥ;
        }
        
        public virtual IEnumerator<KeyValuePair<string, object>> GetEnumerator()
        {
            return GetPropertyKeys()
                .Select(property => new KeyValuePair<string, object>(property, GetProperty(property)))
                .GetEnumerator();
        }

        public void Remove(object key)
        {
            RemoveProperty(key.ToString());
        }

        object IDictionary.this[object key]
        {
            get { return this[key.ToString()]; }
            set { this[key.ToString()] = value; }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public virtual void Add(KeyValuePair<string, object> item)
        {
            SetProperty(item.Key, item.Value);
        }

        public bool Contains(object key)
        {
            return ContainsKey(key.ToString());
        }

        public virtual void Add(object key, object value)
        {
            Add(new KeyValuePair<string, object>(key.ToString(), value));
        }

        public virtual void Clear()
        {
            foreach (var property in GetPropertyKeys())
                RemoveProperty(property);
        }

        struct DictionaryEnumerator : IDictionaryEnumerator
        {
            readonly IEnumerator<KeyValuePair<string, object>> _en;

            public DictionaryEnumerator(IEnumerator<KeyValuePair<string, object>> en)
            {
                _en = en;
            }

            public object Current
            {
                get
                {
                    return Entry;
                }
            }

            public DictionaryEntry Entry
            {
                get
                {
                    var kvp = _en.Current;
                    return new DictionaryEntry(kvp.Key, kvp.Value);
                }
            }

            public bool MoveNext()
            {
                var result = _en.MoveNext();
                return result;
            }

            public void Reset()
            {
                throw new NotSupportedException();
            }

            public object Key
            {
                get
                {
                    var kvp = _en.Current;
                    return kvp.Key;
                }
            }

            public object Value
            {
                get
                {
                    var kvp = _en.Current;
                    return kvp.Value;
                }
            }
        }

        IDictionaryEnumerator IDictionary.GetEnumerator()
        {
            return new DictionaryEnumerator(GetEnumerator());
        }

        public bool Contains(KeyValuePair<string, object> item)
        {
            return ContainsKey(item.Key) && GetProperty(item.Key) == item.Value;
        }

        public virtual void CopyTo(KeyValuePair<string, object>[] array, int arrayIndex)
        {
            foreach (var key in GetPropertyKeys())
            {
                if (arrayIndex >= array.Length) break;
                array.SetValue(new KeyValuePair<string, object>(key, GetProperty(key)), arrayIndex);
                arrayIndex++;
            }
        }

        public virtual bool Remove(KeyValuePair<string, object> item)
        {
            var result = (Contains(item));
            if (result)
                RemoveProperty(item.Key);
            return result;
        }

        public void CopyTo(Array array, int index)
        {
            foreach (var key in GetPropertyKeys())
            {
                if (index >= array.Length) break;
                array.SetValue(new DictionaryEntry(key, GetProperty(key)), index);
                index++;
            }
        }

        public virtual int Count
        {
            get { return GetPropertyKeys().Count(); }
        }

        public virtual object SyncRoot 
        {
            get { return this; }
        }

        public virtual bool IsSynchronized { get; protected set; }

        ICollection IDictionary.Values
        {
            get { return GetPropertyKeys().Select(GetProperty).ToList(); }
        }

        public bool IsReadOnly { get; protected set; }
        public bool IsFixedSize { get; protected set; }

        public virtual bool ContainsKey(string key)
        {
            return GetPropertyKeys().Contains(key);
        }

        public virtual void Add(string key, object value)
        {
            SetProperty(key, value);
        }

        public virtual bool Remove(string key)
        {
            var result = ContainsKey(key);
            if (result)
                RemoveProperty(key);
            return result;
        }

        public virtual bool TryGetValue(string key, out object value)
        {
            var result = ContainsKey(key);
            value = result ? GetProperty(key) : null;
            return result;
        }

        public virtual object this[string key]
        {
            get { return GetProperty(key); }
            set { SetProperty(key, value); }
        }

        public virtual ICollection<string> Keys
        {
            get { return GetPropertyKeys().ToList(); }
        }

        ICollection IDictionary.Keys
        {
            get { return GetPropertyKeys().ToList(); }
        }

        public virtual ICollection<object> Values
        {
            get { return GetPropertyKeys().Select(GetProperty).ToList(); }
        }

        public abstract object Id { get; }
        public IGraph Graph { get; private set; }
        public abstract object GetProperty(string key);
        public abstract IEnumerable<string> GetPropertyKeys();
        public abstract void SetProperty(string key, object value);
        public abstract object RemoveProperty(string key);
        public abstract void Remove();
    }
}