using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ObservableHelpers
{
    public class ObservableDictionary<TKey, TValue> : Observable,
        ICollection<KeyValuePair<TKey, TValue>>, IDictionary<TKey, TValue>,
        INotifyCollectionChanged
    {
        #region Properties

        private readonly ConcurrentDictionary<TKey, TValue> dictionary = new ConcurrentDictionary<TKey, TValue>();

        public event NotifyCollectionChangedEventHandler CollectionChanged;
        public event NotifyCollectionChangedEventHandler CollectionChangedInternal;

        public ICollection<TKey> Keys
        {
            get => dictionary.Keys;
        }

        public ICollection<TValue> Values
        {
            get => dictionary.Values;
        }

        public int Count
        {
            get => ((ICollection<KeyValuePair<TKey, TValue>>)dictionary).Count;
        }

        public bool IsReadOnly
        {
            get => ((ICollection<KeyValuePair<TKey, TValue>>)dictionary).IsReadOnly;
        }

        public TValue this[TKey key]
        {
            get => dictionary[key];
            set => UpdateWithNotification(key, value);
        }

        #endregion

        #region Methods

        public virtual void Add(TKey key, TValue value)
        {
            VerifyNotDisposedOrDisposing();

            TryAddWithNotification(key, value);
        }

        public virtual void Add(KeyValuePair<TKey, TValue> item)
        {
            VerifyNotDisposedOrDisposing();

            TryAddWithNotification(item);
        }

        public virtual bool ContainsKey(TKey key)
        {
            VerifyNotDisposedOrDisposing();

            return dictionary.ContainsKey(key);
        }

        public virtual bool Contains(KeyValuePair<TKey, TValue> item)
        {
            VerifyNotDisposedOrDisposing();

            return ((ICollection<KeyValuePair<TKey, TValue>>)dictionary).Contains(item);
        }

        public virtual bool TryGetValue(TKey key, out TValue value)
        {
            VerifyNotDisposedOrDisposing();

            return dictionary.TryGetValue(key, out value);
        }

        public virtual bool Remove(TKey key)
        {
            VerifyNotDisposedOrDisposing();

            return TryRemoveWithNotification(key, out _);
        }

        public virtual bool Remove(KeyValuePair<TKey, TValue> item)
        {
            VerifyNotDisposedOrDisposing();

            return TryRemoveWithNotification(item.Key, out _);
        }

        public virtual void Clear()
        {
            VerifyNotDisposedOrDisposing();

            ((ICollection<KeyValuePair<TKey, TValue>>)dictionary).Clear();
            NotifyObserversOfChange();
        }

        public virtual void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
        {
            VerifyNotDisposedOrDisposing();

            ((ICollection<KeyValuePair<TKey, TValue>>)dictionary).CopyTo(array, arrayIndex);
        }

        protected virtual void NotifyObserversOfChange()
        {
            VerifyNotDisposedOrDisposing();

            CollectionChangedInternal?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
            SynchronizationContextPost(delegate { CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset)); });
        }

        protected virtual bool TryAddWithNotification(KeyValuePair<TKey, TValue> item)
        {
            VerifyNotDisposedOrDisposing();

            return TryAddWithNotification(item.Key, item.Value);
        }

        protected virtual bool TryAddWithNotification(TKey key, TValue value)
        {
            VerifyNotDisposedOrDisposing();

            bool result = dictionary.TryAdd(key, ValueFactory(key, value).value);
            if (result) NotifyObserversOfChange();
            return result;
        }

        protected virtual bool TryRemoveWithNotification(TKey key, out TValue value)
        {
            VerifyNotDisposedOrDisposing();

            bool result = ValueRemove(key, out value);
            if (result) NotifyObserversOfChange();
            return result;
        }

        protected virtual void UpdateWithNotification(TKey key, TValue value)
        {
            VerifyNotDisposedOrDisposing();

            dictionary[key] = ValueFactory(key, value).value;
            NotifyObserversOfChange();
        }

        protected virtual (TKey key, TValue value) ValueFactory(TKey key, TValue value)
        {
            VerifyNotDisposedOrDisposing();

            return (key, value);
        }

        protected virtual bool ValueRemove(TKey key, out TValue value)
        {
            VerifyNotDisposedOrDisposing();

            bool result = dictionary.TryRemove(key, out value);
            if (result) NotifyObserversOfChange();
            return result;
        }

        IEnumerator<KeyValuePair<TKey, TValue>> IEnumerable<KeyValuePair<TKey, TValue>>.GetEnumerator()
        {
            VerifyNotDisposedOrDisposing();

            return ((ICollection<KeyValuePair<TKey, TValue>>)dictionary).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            VerifyNotDisposedOrDisposing();

            return ((ICollection<KeyValuePair<TKey, TValue>>)dictionary).GetEnumerator();
        }

        #endregion
    }
}
