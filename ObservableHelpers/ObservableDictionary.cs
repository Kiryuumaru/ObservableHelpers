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

        public override bool SetNull()
        {
            VerifyNotDisposed();

            var hasChanges = Count != 0;

            Clear();

            return hasChanges;
        }

        public override bool IsNull()
        {
            VerifyNotDisposed();

            return Count == 0;
        }

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

            return ContainsKeyCore(key);
        }

        public virtual bool Contains(KeyValuePair<TKey, TValue> item)
        {
            VerifyNotDisposed();

            return ContainsCore(item);
        }

        public virtual bool TryGetValue(TKey key, out TValue value)
        {
            VerifyNotDisposed();

            return TryGetValueCore(key, out value);
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

            ClearCore();
            NotifyObserversOfChange();
        }

        public virtual void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
        {
            VerifyNotDisposed();

            CopyToCore(array, arrayIndex);
        }

        protected virtual void NotifyObserversOfChange()
        {
            VerifyNotDisposed();

            var args = new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset);

            ContextPost(delegate
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

            bool result = TryAddCore(key, MakeValue(key, value).value);
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

            UpdateCore(key, MakeValue(key, value).value);
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

            return TryRemoveCore(key, out value);
        }

        protected (TKey key, TValue value) MakeValue(TKey key, TValue value)
        {
            VerifyNotDisposed();

            var pair = ValueFactory(key, value);

            if (value is ISyncObject sync)
            {
                sync.SyncOperation.SetContext(this);
            }

            return pair;
        }

        protected bool TryGetValueCore(TKey key, out TValue value)
        {
            return dictionary.TryGetValue(key, out value);
        }

        protected bool ContainsKeyCore(TKey key)
        {
            return dictionary.ContainsKey(key);
        }

        protected bool ContainsCore(KeyValuePair<TKey, TValue> item)
        {
            return ((ICollection<KeyValuePair<TKey, TValue>>)dictionary).Contains(item);
        }

        protected bool TryAddCore(TKey key, TValue value)
        {
            return dictionary.TryAdd(key, value);
        }

        protected void UpdateCore(TKey key, TValue value)
        {
            dictionary[key] = value;
        }

        protected bool TryRemoveCore(TKey key, out TValue value)
        {
            return dictionary.TryRemove(key, out value);
        }

        protected void ClearCore()
        {
            dictionary.Clear();
        }

        protected void CopyToCore(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
        {
            ((ICollection<KeyValuePair<TKey, TValue>>)dictionary).CopyTo(array, arrayIndex);
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
