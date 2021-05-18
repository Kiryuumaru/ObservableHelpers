using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ObservableHelpers
{
    public class ObservableDictionary<TKey, TValue> :
        ICollection<KeyValuePair<TKey, TValue>>, IDictionary<TKey, TValue>,
        INotifyCollectionChanged, IObservable
    {
        #region Properties

        private readonly SynchronizationContext context = AsyncOperationManager.SynchronizationContext;
        private readonly ConcurrentDictionary<TKey, TValue> dictionary = new ConcurrentDictionary<TKey, TValue>();

        public event PropertyChangedEventHandler PropertyChanged;
        public event NotifyCollectionChangedEventHandler CollectionChanged;
        public event EventHandler<ContinueExceptionEventArgs> PropertyError;

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

        private void NotifyObserversOfChange()
        {
            context.Post(s =>
            {
                lock (this)
                {
                    CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Count)));
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Keys)));
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Values)));
                }
            }, null);
        }

        private bool TryAddWithNotification(KeyValuePair<TKey, TValue> item)
        {
            return TryAddWithNotification(item.Key, item.Value);
        }

        private bool TryAddWithNotification(TKey key, TValue value)
        {
            bool result = dictionary.TryAdd(key, ValueFactory(key, value).value);
            if (result) NotifyObserversOfChange();
            return result;
        }

        private bool TryRemoveWithNotification(TKey key, out TValue value)
        {
            bool result = dictionary.TryRemove(key, out value);
            if (result) NotifyObserversOfChange();
            return result;
        }

        private void UpdateWithNotification(TKey key, TValue value)
        {
            dictionary[key] = ValueFactory(key, value).value;
            NotifyObserversOfChange();
        }

        protected virtual (TKey key, TValue value) ValueFactory(TKey key, TValue value)
        {
            return (key, value);
        }

        public virtual void OnError(Exception exception, bool defaultIgnoreAndContinue = true)
        {
            var args = new ContinueExceptionEventArgs(exception, defaultIgnoreAndContinue);
            PropertyError?.Invoke(this, args);
            if (!args.IgnoreAndContinue)
            {
                throw args.Exception;
            }
        }

        public virtual void OnError(ContinueExceptionEventArgs args)
        {
            PropertyError?.Invoke(this, args);
            if (!args.IgnoreAndContinue)
            {
                throw args.Exception;
            }
        }

        public void Add(TKey key, TValue value)
        {
            TryAddWithNotification(key, value);
        }

        public void Add(KeyValuePair<TKey, TValue> item)
        {
            TryAddWithNotification(item);
        }

        public bool ContainsKey(TKey key)
        {
            return dictionary.ContainsKey(key);
        }

        public bool Contains(KeyValuePair<TKey, TValue> item)
        {
            return ((ICollection<KeyValuePair<TKey, TValue>>)dictionary).Contains(item);
        }

        public bool TryGetValue(TKey key, out TValue value)
        {
            return dictionary.TryGetValue(key, out value);
        }

        public bool Remove(TKey key)
        {
            return TryRemoveWithNotification(key, out _);
        }

        public bool Remove(KeyValuePair<TKey, TValue> item)
        {
            return TryRemoveWithNotification(item.Key, out _);
        }

        public void Clear()
        {
            ((ICollection<KeyValuePair<TKey, TValue>>)dictionary).Clear();
            NotifyObserversOfChange();
        }

        public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
        {
            ((ICollection<KeyValuePair<TKey, TValue>>)dictionary).CopyTo(array, arrayIndex);
        }

        IEnumerator<KeyValuePair<TKey, TValue>> IEnumerable<KeyValuePair<TKey, TValue>>.GetEnumerator()
        {
            return ((ICollection<KeyValuePair<TKey, TValue>>)dictionary).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((ICollection<KeyValuePair<TKey, TValue>>)dictionary).GetEnumerator();
        }

        #endregion
    }
}
