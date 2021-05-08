using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ObservableHelpers.Observables
{
    public class ObservableDictionary<TKey, TValue> :
        ICollection<KeyValuePair<TKey, TValue>>, IDictionary<TKey, TValue>,
        INotifyCollectionChanged, IObservable
    {
        #region Properties

        public AttributeHolder Holder { get; } = new AttributeHolder();

        private SynchronizationContext Context
        {
            get => Holder.GetAttribute<SynchronizationContext>(AsyncOperationManager.SynchronizationContext);
            set => Holder.SetAttribute(value);
        }

        private ConcurrentDictionary<TKey, TValue> Dictionary
        {
            get => Holder.GetAttribute<ConcurrentDictionary<TKey, TValue>>(new ConcurrentDictionary<TKey, TValue>());
            set => Holder.SetAttribute(value);
        }

        private PropertyChangedEventHandler PropertyChangedHandler
        {
            get => Holder.GetAttribute<PropertyChangedEventHandler>(delegate { });
            set => Holder.SetAttribute(value);
        }

        private NotifyCollectionChangedEventHandler CollectionChangedHandler
        {
            get => Holder.GetAttribute<NotifyCollectionChangedEventHandler>(delegate { });
            set => Holder.SetAttribute(value);
        }

        private EventHandler<ContinueExceptionEventArgs> PropertyErrorHandler
        {
            get => Holder.GetAttribute<EventHandler<ContinueExceptionEventArgs>>(delegate { });
            set => Holder.SetAttribute(value);
        }

        public event PropertyChangedEventHandler PropertyChanged
        {
            add
            {
                lock (this)
                {
                    PropertyChangedHandler += value;
                }
            }
            remove
            {
                lock (this)
                {
                    PropertyChangedHandler -= value;
                }
            }
        }

        public event NotifyCollectionChangedEventHandler CollectionChanged
        {
            add
            {
                lock (this)
                {
                    CollectionChangedHandler += value;
                }
            }
            remove
            {
                lock (this)
                {
                    CollectionChangedHandler -= value;
                }
            }
        }

        public event EventHandler<ContinueExceptionEventArgs> PropertyError
        {
            add
            {
                lock (this)
                {
                    PropertyErrorHandler += value;
                }
            }
            remove
            {
                lock (this)
                {
                    PropertyErrorHandler -= value;
                }
            }
        }

        public ICollection<TKey> Keys
        {
            get => Dictionary.Keys;
        }

        public ICollection<TValue> Values
        {
            get => Dictionary.Values;
        }

        public int Count
        {
            get => ((ICollection<KeyValuePair<TKey, TValue>>)Dictionary).Count;
        }

        public bool IsReadOnly
        {
            get => ((ICollection<KeyValuePair<TKey, TValue>>)Dictionary).IsReadOnly;
        }

        public TValue this[TKey key]
        {
            get => Dictionary[key];
            set => UpdateWithNotification(key, value);
        }

        #endregion

        #region Initializers

        public ObservableDictionary(IAttributed attributed)
        {
            Holder.Inherit(attributed);
        }

        public ObservableDictionary()
        {
            Holder.Inherit(null);
        }

        #endregion

        #region Methods

        private void NotifyObserversOfChange()
        {
            var collectionHandler = CollectionChangedHandler;
            var propertyHandler = PropertyChangedHandler;
            if (collectionHandler != null || propertyHandler != null)
            {
                Context.Post(s =>
                {
                    lock (this)
                    {
                        if (collectionHandler != null)
                        {
                            collectionHandler(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
                        }
                        if (propertyHandler != null)
                        {
                            propertyHandler(this, new PropertyChangedEventArgs("Count"));
                            propertyHandler(this, new PropertyChangedEventArgs("Keys"));
                            propertyHandler(this, new PropertyChangedEventArgs("Values"));
                        }
                    }
                }, null);
            }
        }

        private bool TryAddWithNotification(KeyValuePair<TKey, TValue> item)
        {
            return TryAddWithNotification(item.Key, item.Value);
        }

        private bool TryAddWithNotification(TKey key, TValue value)
        {
            bool result = Dictionary.TryAdd(key, ValueFactory(key, value).value);
            if (result) NotifyObserversOfChange();
            return result;
        }

        private bool TryRemoveWithNotification(TKey key, out TValue value)
        {
            bool result = Dictionary.TryRemove(key, out value);
            if (result) NotifyObserversOfChange();
            return result;
        }

        private void UpdateWithNotification(TKey key, TValue value)
        {
            Dictionary[key] = ValueFactory(key, value).value;
            NotifyObserversOfChange();
        }

        protected virtual (TKey key, TValue value) ValueFactory(TKey key, TValue value)
        {
            return (key, value);
        }

        public virtual void OnError(Exception exception, bool defaultIgnoreAndContinue = true)
        {
            var args = new ContinueExceptionEventArgs(exception, defaultIgnoreAndContinue);
            PropertyErrorHandler?.Invoke(this, args);
            if (!args.IgnoreAndContinue)
            {
                throw args.Exception;
            }
        }

        public virtual void OnError(ContinueExceptionEventArgs args)
        {
            PropertyErrorHandler?.Invoke(this, args);
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
            return Dictionary.ContainsKey(key);
        }

        public bool Contains(KeyValuePair<TKey, TValue> item)
        {
            return ((ICollection<KeyValuePair<TKey, TValue>>)Dictionary).Contains(item);
        }

        public bool TryGetValue(TKey key, out TValue value)
        {
            return Dictionary.TryGetValue(key, out value);
        }

        public bool Remove(TKey key)
        {
            TValue temp;
            return TryRemoveWithNotification(key, out temp);
        }

        public bool Remove(KeyValuePair<TKey, TValue> item)
        {
            TValue temp;
            return TryRemoveWithNotification(item.Key, out temp);
        }

        public void Clear()
        {
            ((ICollection<KeyValuePair<TKey, TValue>>)Dictionary).Clear();
            NotifyObserversOfChange();
        }

        public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
        {
            ((ICollection<KeyValuePair<TKey, TValue>>)Dictionary).CopyTo(array, arrayIndex);
        }

        IEnumerator<KeyValuePair<TKey, TValue>> IEnumerable<KeyValuePair<TKey, TValue>>.GetEnumerator()
        {
            return ((ICollection<KeyValuePair<TKey, TValue>>)Dictionary).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((ICollection<KeyValuePair<TKey, TValue>>)Dictionary).GetEnumerator();
        }

        #endregion
    }
}
