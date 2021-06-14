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

        public void Add(TKey key, TValue value)
        {
            VerifyNotDisposed();

            TryAddWithNotification(key, value);
        }

        public void Add(KeyValuePair<TKey, TValue> item)
        {
            VerifyNotDisposed();

            TryAddWithNotification(item);
        }

        public bool ContainsKey(TKey key)
        {
            VerifyNotDisposed();

            return ContainsKeyCore(key);
        }

        public bool Contains(KeyValuePair<TKey, TValue> item)
        {
            VerifyNotDisposed();

            return ContainsCore(item);
        }

        public bool TryGetValue(TKey key, out TValue value)
        {
            VerifyNotDisposed();

            return TryGetValueCore(key, out value);
        }

        public bool Remove(TKey key)
        {
            VerifyNotDisposed();

            return TryRemoveWithNotification(key, out _);
        }

        public bool Remove(KeyValuePair<TKey, TValue> item)
        {
            VerifyNotDisposed();

            return TryRemoveWithNotification(item.Key, out _);
        }

        public void Clear()
        {
            VerifyNotDisposed();

            if (ValidateClear())
            {
                ClearCore();
                NotifyObserversOfChange();
            }
        }

        public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
        {
            VerifyNotDisposed();

            CopyToCore(array, arrayIndex);
        }

        protected bool TryAddWithNotification(KeyValuePair<TKey, TValue> item)
        {
            VerifyNotDisposed();

            return TryAddWithNotification(item.Key, item.Value);
        }

        protected bool TryAddWithNotification(TKey key, TValue value)
        {
            VerifyNotDisposed();

            if (ValidateSetItem(key, value))
            {
                bool result = TryAddCore(key, value);
                if (result) NotifyObserversOfChange();
                return result;
            }
            else
            {
                return false;
            }
        }

        protected bool TryRemoveWithNotification(TKey key, out TValue value)
        {
            VerifyNotDisposed();

            if (ValidateRemoveItem(key))
            {
                bool result = TryRemoveCore(key, out value);
                if (result) NotifyObserversOfChange();
                return result;
            }
            else
            {
                value = default;
                return false;
            }
        }

        protected void UpdateWithNotification(TKey key, TValue value)
        {
            VerifyNotDisposed();

            if (ValidateSetItem(key, value))
            {
                UpdateCore(key, value);
                NotifyObserversOfChange();
            }
        }

        protected bool TryGetValueCore(TKey key, out TValue value)
        {
            VerifyNotDisposed();

            return dictionary.TryGetValue(key, out value);
        }

        protected bool ContainsKeyCore(TKey key)
        {
            VerifyNotDisposed();

            return dictionary.ContainsKey(key);
        }

        protected bool ContainsCore(KeyValuePair<TKey, TValue> item)
        {
            VerifyNotDisposed();

            return ((ICollection<KeyValuePair<TKey, TValue>>)dictionary).Contains(item);
        }

        protected bool TryAddCore(TKey key, TValue value)
        {
            VerifyNotDisposed();

            return dictionary.TryAdd(key, value);
        }

        protected void UpdateCore(TKey key, TValue value)
        {
            VerifyNotDisposed();

            dictionary[key] = value;
        }

        protected bool TryRemoveCore(TKey key, out TValue value)
        {
            VerifyNotDisposed();

            return dictionary.TryRemove(key, out value);
        }

        protected void ClearCore()
        {
            VerifyNotDisposed();

            dictionary.Clear();
        }

        protected void CopyToCore(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
        {
            VerifyNotDisposed();

            ((ICollection<KeyValuePair<TKey, TValue>>)dictionary).CopyTo(array, arrayIndex);
        }

        protected virtual bool ValidateSetItem(TKey key, TValue value)
        {
            VerifyNotDisposed();

            if (value is ISyncObject sync)
            {
                sync.SyncOperation.SetContext(this);
            }

            return true;
        }

        protected virtual bool ValidateRemoveItem(TKey key)
        {
            VerifyNotDisposed();

            return ContainsKeyCore(key);
        }

        protected virtual bool ValidateClear()
        {
            VerifyNotDisposed();

            return true;
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
