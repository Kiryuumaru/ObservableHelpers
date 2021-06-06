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
            VerifyNotDisposed();

            TryAddWithNotification(key, value);
        }

        public virtual void Add(KeyValuePair<TKey, TValue> item)
        {
            VerifyNotDisposed();

            TryAddWithNotification(item);
        }

        public virtual bool ContainsKey(TKey key)
        {
            VerifyNotDisposed();

            return dictionary.ContainsKey(key);
        }

        public virtual bool Contains(KeyValuePair<TKey, TValue> item)
        {
            VerifyNotDisposed();

            return ((ICollection<KeyValuePair<TKey, TValue>>)dictionary).Contains(item);
        }

        public virtual bool TryGetValue(TKey key, out TValue value)
        {
            VerifyNotDisposed();

            return dictionary.TryGetValue(key, out value);
        }

        public virtual bool Remove(TKey key)
        {
            VerifyNotDisposed();

            return TryRemoveWithNotification(key, out _);
        }

        public virtual bool Remove(KeyValuePair<TKey, TValue> item)
        {
            VerifyNotDisposed();

            return TryRemoveWithNotification(item.Key, out _);
        }

        public virtual void Clear()
        {
            VerifyNotDisposed();

            ((ICollection<KeyValuePair<TKey, TValue>>)dictionary).Clear();
            NotifyObserversOfChange();
        }

        public virtual void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
        {
            VerifyNotDisposed();

            ((ICollection<KeyValuePair<TKey, TValue>>)dictionary).CopyTo(array, arrayIndex);
        }

        protected virtual void NotifyObserversOfChange()
        {
            VerifyNotDisposed();

            var args = new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset);

            SynchronizationContextPost(delegate
            {
                CollectionChanged?.Invoke(this, args);
            });
        }

        protected virtual bool TryAddWithNotification(KeyValuePair<TKey, TValue> item)
        {
            VerifyNotDisposed();

            return TryAddWithNotification(item.Key, item.Value);
        }

        protected virtual bool TryAddWithNotification(TKey key, TValue value)
        {
            VerifyNotDisposed();

            bool result = dictionary.TryAdd(key, ValueFactory(key, value).value);
            if (result) NotifyObserversOfChange();
            return result;
        }

        protected virtual bool TryRemoveWithNotification(TKey key, out TValue value)
        {
            VerifyNotDisposed();

            bool result = ValueRemove(key, out value);
            if (result) NotifyObserversOfChange();
            return result;
        }

        protected virtual void UpdateWithNotification(TKey key, TValue value)
        {
            VerifyNotDisposed();

            dictionary[key] = ValueFactory(key, value).value;
            NotifyObserversOfChange();
        }

        protected virtual (TKey key, TValue value) ValueFactory(TKey key, TValue value)
        {
            VerifyNotDisposed();

            return (key, value);
        }

        protected virtual bool ValueRemove(TKey key, out TValue value)
        {
            VerifyNotDisposed();

            bool result = dictionary.TryRemove(key, out value);
            if (result) NotifyObserversOfChange();
            return result;
        }

        IEnumerator<KeyValuePair<TKey, TValue>> IEnumerable<KeyValuePair<TKey, TValue>>.GetEnumerator()
        {
            VerifyNotDisposed();

            return ((ICollection<KeyValuePair<TKey, TValue>>)dictionary).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            VerifyNotDisposed();

            return ((ICollection<KeyValuePair<TKey, TValue>>)dictionary).GetEnumerator();
        }

        #endregion
    }
}
