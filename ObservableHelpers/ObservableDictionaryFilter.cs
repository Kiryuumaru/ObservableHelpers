using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;

namespace ObservableHelpers
{
    /// <summary>
    /// Provides a thread-safe observable dictionary query from <see cref="ObservableDictionary{TKey, TValue}"/> for use with data binding.
    /// </summary>
    /// <typeparam name="TKey">
    /// Specifies the type of the keys in this collection.
    /// </typeparam>
    /// <typeparam name="TValue">
    /// Specifies the type of the values in this collection.
    /// </typeparam>
    public class ObservableDictionaryFilter<TKey, TValue> :
        ObservableCollectionSyncContext,
        IReadOnlyDictionary<TKey, TValue>,
        INotifyCollectionChanged
    {
        #region Properties

        /// <inheritdoc/>
        public IEnumerable<TKey> Keys => CoreDictionary.Keys;

        /// <inheritdoc/>
        public IEnumerable<TValue> Values => CoreDictionary.Values;

        /// <inheritdoc/>
        public int Count => CoreDictionary.Count;

        /// <inheritdoc/>
        public TValue this[TKey key] => CoreDictionary[key];

        /// <summary>
        /// The core dictionary used.
        /// </summary>
        protected ConcurrentDictionary<TKey, TValue> CoreDictionary { get; private set; } = new ConcurrentDictionary<TKey, TValue>();

        private const string IndexerName = "Item[]";

        #endregion

        #region Initializers

        /// <summary>
        /// Creates new instance of the <see cref="ObservableDictionaryFilter{TKey, TValue}"/> class that is empty.
        /// </summary>
        public ObservableDictionaryFilter()
        {

        }

        /// <summary>
        /// Creates new instance of the <see cref="ObservableDictionaryFilter{TKey, TValue}"/> class that contains elements copied from the specified <paramref name="items"/>.
        /// </summary>
        public ObservableDictionaryFilter(IEnumerable<KeyValuePair<TKey, TValue>> items)
        {
            CoreDictionary = new ConcurrentDictionary<TKey, TValue>(items);
        }

        /// <summary>
        /// Creates new instance of the <see cref="ObservableDictionaryFilter{TKey, TValue}"/> class that is empty, and uses the specified <paramref name="comparer"/>.
        /// </summary>
        public ObservableDictionaryFilter(IEqualityComparer<TKey> comparer)
        {
            CoreDictionary = new ConcurrentDictionary<TKey, TValue>(comparer);
        }

        /// <summary>
        /// Creates new instance of the <see cref="ObservableDictionaryFilter{TKey, TValue}"/> class that contains elements copied from the specified <paramref name="items"/>, and uses the specified <paramref name="comparer"/>.
        /// </summary>
        public ObservableDictionaryFilter(IEnumerable<KeyValuePair<TKey, TValue>> items, IEqualityComparer<TKey> comparer)
        {
            CoreDictionary = new ConcurrentDictionary<TKey, TValue>(items, comparer);
        }

        /// <summary>
        /// Creates new instance of the <see cref="ObservableDictionaryFilter{TKey, TValue}"/> class that is empty, has the specified <paramref name="concurrencyLevel"/> and <paramref name="capacity"/>, and uses the default comparer for the key type.
        /// </summary>
        public ObservableDictionaryFilter(int concurrencyLevel, int capacity)
        {
            CoreDictionary = new ConcurrentDictionary<TKey, TValue>(concurrencyLevel, capacity);
        }

        /// <summary>
        /// Creates new instance of the <see cref="ObservableDictionaryFilter{TKey, TValue}"/> class that is empty, has the specified <paramref name="concurrencyLevel"/> and <paramref name="capacity"/>, and uses the specified <paramref name="comparer"/>.
        /// </summary>
        public ObservableDictionaryFilter(int concurrencyLevel, int capacity, IEqualityComparer<TKey> comparer)
        {
            CoreDictionary = new ConcurrentDictionary<TKey, TValue>(concurrencyLevel, capacity, comparer);
        }

        /// <summary>
        /// Creates new instance of the <see cref="ObservableDictionaryFilter{TKey, TValue}"/> class that contains elements copied from the specified <paramref name="items"/>, has the specified <paramref name="concurrencyLevel"/>, and uses the specified <paramref name="comparer"/>.
        /// </summary>
        public ObservableDictionaryFilter(int concurrencyLevel, IEnumerable<KeyValuePair<TKey, TValue>> items, IEqualityComparer<TKey> comparer)
        {
            CoreDictionary = new ConcurrentDictionary<TKey, TValue>(concurrencyLevel, items, comparer);
        }

        #endregion

        #region Methods

        /// <inheritdoc/>
        public override bool SetNull()
        {
            throw new NotSupportedException();
        }

        /// <inheritdoc/>
        public override bool IsNull()
        {
            if (IsDisposed)
            {
                return false;
            }

            return Count == 0;
        }

        /// <summary>
        /// Creates an observable filter that shadows the changes notifications from the parent observable.
        /// </summary>
        /// <param name="predicate">
        /// The predicate filter for child observable.
        /// </param>
        /// <returns>
        /// The created child filter observable.
        /// </returns>
        public ObservableDictionaryFilter<TKey, TValue> ObservableFilter(Predicate<KeyValuePair<TKey, TValue>> predicate)
        {
            if (predicate == null)
            {
                throw new ArgumentNullException(nameof(predicate));
            }

            ObservableDictionaryFilter<TKey, TValue> filter = new ObservableDictionaryFilter<TKey, TValue>(CoreDictionary.Where(i => predicate.Invoke(i)));
            void Filter_ImmediateCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
            {
                if (filter.IsDisposed)
                {
                    return;
                }

                if (e.NewItems != null)
                {
                    foreach (KeyValuePair<TKey, TValue> item in e.NewItems)
                    {
                        if (predicate.Invoke(item))
                        {
                            filter.CoreDictionary.AddOrUpdate(item.Key, item.Value, delegate { return item.Value; });
                        }
                    }
                }

                if (e.OldItems != null)
                {
                    foreach (KeyValuePair<TKey, TValue> item in e.OldItems)
                    {
                        if (predicate.Invoke(item))
                        {
                            filter.CoreDictionary.TryRemove(item.Key, out _);
                        }
                    }
                }

                if (e.NewItems == null && e.OldItems == null && filter.Count != 0)
                {
                    filter.CoreDictionary.Clear();
                }
            }
            ImmediateCollectionChanged += Filter_ImmediateCollectionChanged;
            filter.Disposing += (s, e) =>
            {
                ImmediateCollectionChanged -= Filter_ImmediateCollectionChanged;
            };
            return filter;
        }

        /// <inheritdoc/>
        public bool ContainsKey(TKey key)
        {
            return !IsDisposed && CoreDictionary.ContainsKey(key);
        }

        /// <inheritdoc/>
        public bool Contains(KeyValuePair<TKey, TValue> item)
        {
            return !IsDisposed && (CoreDictionary as ICollection<KeyValuePair<TKey, TValue>>).Contains(item);
        }

        /// <inheritdoc/>
        public bool TryGetValue(TKey key, out TValue value)
        {
            value = default;

            if (IsDisposed)
            {
                return false;
            }

            return CoreDictionary.TryGetValue(key, out value);
        }

        /// <inheritdoc/>
        public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
        {
            if (IsDisposed)
            {
                return;
            }

            (CoreDictionary as ICollection<KeyValuePair<TKey, TValue>>).CopyTo(array, arrayIndex);
        }

        /// <summary>
        /// Notifies observers of <see cref="ObservableSyncContext.ImmediatePropertyChanged"/> and <see cref="ObservableSyncContext.PropertyChanged"/> for an update to the dictionary.
        /// </summary>
        protected void OnPropertyChanged()
        {
            OnPropertyChanged(new PropertyChangedEventArgs(IndexerName));
            OnPropertyChanged(new PropertyChangedEventArgs(nameof(Count)));
            OnPropertyChanged(new PropertyChangedEventArgs(nameof(Keys)));
            OnPropertyChanged(new PropertyChangedEventArgs(nameof(Values)));
        }

        /// <summary>
        /// Helper to raise CollectionChanged event to any listeners
        /// </summary>
        protected void OnCollectionChanged(NotifyCollectionChangedAction action, object item, int index)
        {
            OnPropertyChanged();
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(action, item, index));
        }

        /// <summary>
        /// Helper to raise CollectionChanged event to any listeners
        /// </summary>
        protected void OnCollectionChanged(NotifyCollectionChangedAction action, object item, int index, int oldIndex)
        {
            OnPropertyChanged();
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(action, item, index, oldIndex));
        }

        /// <summary>
        /// Helper to raise CollectionChanged event to any listeners
        /// </summary>
        protected void OnCollectionChanged(NotifyCollectionChangedAction action, object oldItem, object newItem, int index)
        {
            OnPropertyChanged();
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(action, newItem, oldItem, index));
        }

        /// <summary>
        /// Helper to raise CollectionChanged event with action == Reset to any listeners
        /// </summary>
        protected void OnCollectionReset()
        {
            OnPropertyChanged();
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        }

        #endregion

        #region IEnumerable Members

        /// <inheritdoc/>
        IEnumerator<KeyValuePair<TKey, TValue>> IEnumerable<KeyValuePair<TKey, TValue>>.GetEnumerator()
        {
            return IsDisposed ? default : (CoreDictionary as ICollection<KeyValuePair<TKey, TValue>>).GetEnumerator();
        }

        /// <inheritdoc/>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return IsDisposed ? default : (CoreDictionary as ICollection<KeyValuePair<TKey, TValue>>).GetEnumerator();
        }

        #endregion
    }
}
