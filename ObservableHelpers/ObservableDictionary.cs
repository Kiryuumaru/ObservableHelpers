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
            if (IsDisposed)
            {
                return false;
            }

            var hasChanges = Count != 0;

            Clear();

            return hasChanges;
        }

        public override bool IsNull()
        {
            if (IsDisposed)
            {
                return true;
            }

            return Count == 0;
        }

        public void Add(TKey key, TValue value)
        {
            if (IsDisposed)
            {
                return;
            }

            TryAddWithNotification(key, value);
        }

        public void Add(KeyValuePair<TKey, TValue> item)
        {
            if (IsDisposed)
            {
                return;
            }

            TryAddWithNotification(item);
        }

        public bool ContainsKey(TKey key)
        {
            if (IsDisposed)
            {
                return false;
            }

            return ContainsKeyCore(key);
        }

        public bool Contains(KeyValuePair<TKey, TValue> item)
        {
            if (IsDisposed)
            {
                return false;
            }

            return ContainsCore(item);
        }

        public bool TryGetValue(TKey key, out TValue value)
        {
            if (IsDisposed)
            {
                value = default;
                return false;
            }

            return TryGetValueCore(key, out value);
        }

        public bool Remove(TKey key)
        {
            if (IsDisposed)
            {
                return false;
            }

            return TryRemoveWithNotification(key, out _);
        }

        public bool Remove(KeyValuePair<TKey, TValue> item)
        {
            if (IsDisposed)
            {
                return false;
            }

            return TryRemoveWithNotification(item.Key, out _);
        }

        public void Clear()
        {
            if (IsDisposed)
            {
                return;
            }

            if (ValidateClear())
            {
                ClearCore();
                NotifyObserversOfChange();
            }
        }

        public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
        {
            if (IsDisposed)
            {
                return;
            }

            CopyToCore(array, arrayIndex);
        }

        protected bool TryAddWithNotification(KeyValuePair<TKey, TValue> item)
        {
            if (IsDisposed)
            {
                return false;
            }

            return TryAddWithNotification(item.Key, item.Value);
        }

        protected bool TryAddWithNotification(TKey key, TValue value)
        {
            if (IsDisposed)
            {
                return false;
            }

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
            if (IsDisposed)
            {
                value = default;
                return false;
            }

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
            if (IsDisposed)
            {
                return;
            }

            if (ValidateSetItem(key, value))
            {
                UpdateCore(key, value);
                NotifyObserversOfChange();
            }
        }

        protected bool TryGetValueCore(TKey key, out TValue value)
        {
            if (IsDisposed)
            {
                value = default;
                return false;
            }

            return dictionary.TryGetValue(key, out value);
        }

        protected bool ContainsKeyCore(TKey key)
        {
            if (IsDisposed)
            {
                return false;
            }

            return dictionary.ContainsKey(key);
        }

        protected bool ContainsCore(KeyValuePair<TKey, TValue> item)
        {
            if (IsDisposed)
            {
                return false;
            }

            return ((ICollection<KeyValuePair<TKey, TValue>>)dictionary).Contains(item);
        }

        protected bool TryAddCore(TKey key, TValue value)
        {
            if (IsDisposed)
            {
                return false;
            }

            return dictionary.TryAdd(key, value);
        }

        protected void UpdateCore(TKey key, TValue value)
        {
            if (IsDisposed)
            {
                return;
            }

            dictionary[key] = value;
        }

        protected bool TryRemoveCore(TKey key, out TValue value)
        {
            if (IsDisposed)
            {
                value = default;
                return false;
            }

            return dictionary.TryRemove(key, out value);
        }

        protected void ClearCore()
        {
            if (IsDisposed)
            {
                return;
            }

            dictionary.Clear();
        }

        protected void CopyToCore(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
        {
            if (IsDisposed)
            {
                return;
            }

            ((ICollection<KeyValuePair<TKey, TValue>>)dictionary).CopyTo(array, arrayIndex);
        }

        protected virtual bool ValidateSetItem(TKey key, TValue value)
        {
            if (IsDisposed)
            {
                return false;
            }

            if (value is ISyncObject sync)
            {
                sync.SyncOperation.SetContext(this);
            }

            return true;
        }

        protected virtual bool ValidateRemoveItem(TKey key)
        {
            if (IsDisposed)
            {
                return false;
            }

            return ContainsKeyCore(key);
        }

        protected virtual bool ValidateClear()
        {
            if (IsDisposed)
            {
                return false;
            }

            return true;
        }

        protected virtual void NotifyObserversOfChange()
        {
            if (IsDisposed)
            {
                return;
            }

            var args = new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset);

            ContextPost(delegate
            {
                CollectionChanged?.Invoke(this, args);
            });
        }

        IEnumerator<KeyValuePair<TKey, TValue>> IEnumerable<KeyValuePair<TKey, TValue>>.GetEnumerator()
        {
            if (IsDisposed)
            {
                return default;
            }

            return ((ICollection<KeyValuePair<TKey, TValue>>)dictionary).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            if (IsDisposed)
            {
                return default;
            }

            return ((ICollection<KeyValuePair<TKey, TValue>>)dictionary).GetEnumerator();
        }

        #endregion
    }
}
