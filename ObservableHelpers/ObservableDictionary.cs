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
    /// <summary>
    /// Provides a thread-safe observable dictionary for use with data binding.
    /// </summary>
    /// <typeparam name="TKey">
    /// Specifies the type of the keys in this collection.
    /// </typeparam>
    /// <typeparam name="TValue">
    /// Specifies the type of the values in this collection.
    /// </typeparam>
    public class ObservableDictionary<TKey, TValue> : Observable,
        ICollection<KeyValuePair<TKey, TValue>>, IDictionary<TKey, TValue>,
        INotifyCollectionChanged
    {
        #region Properties

        private readonly ConcurrentDictionary<TKey, TValue> dictionary = new ConcurrentDictionary<TKey, TValue>();

        /// <summary>
        /// Event raised on the current context when the collection changes.
        /// </summary>
        public event NotifyCollectionChangedEventHandler CollectionChanged;

        /// <inheritdoc/>
        public ICollection<TKey> Keys
        {
            get => dictionary.Keys;
        }

        /// <inheritdoc/>
        public ICollection<TValue> Values
        {
            get => dictionary.Values;
        }

        /// <inheritdoc/>
        public int Count
        {
            get => ((ICollection<KeyValuePair<TKey, TValue>>)dictionary).Count;
        }

        /// <inheritdoc/>
        public bool IsReadOnly
        {
            get => ((ICollection<KeyValuePair<TKey, TValue>>)dictionary).IsReadOnly;
        }

        /// <inheritdoc/>
        public TValue this[TKey key]
        {
            get => dictionary[key];
            set => UpdateWithNotification(key, value);
        }

        #endregion

        #region Initializers

        /// <summary>
        /// Creates new instance of the <see cref="ObservableDictionary{TKey, TValue}"/> class.
        /// </summary>
        public ObservableDictionary()
        {

        }

        #endregion

        #region Methods

        /// <inheritdoc/>
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

        /// <inheritdoc/>
        public override bool IsNull()
        {
            if (IsDisposed)
            {
                return true;
            }

            return Count == 0;
        }

        /// <inheritdoc/>
        public void Add(TKey key, TValue value)
        {
            if (IsDisposed)
            {
                return;
            }

            TryAddWithNotification(key, value);
        }

        /// <inheritdoc/>
        public void Add(KeyValuePair<TKey, TValue> item)
        {
            if (IsDisposed)
            {
                return;
            }

            TryAddWithNotification(item);
        }

        /// <inheritdoc/>
        public bool ContainsKey(TKey key)
        {
            if (IsDisposed)
            {
                return false;
            }

            return ContainsKeyCore(key);
        }

        /// <inheritdoc/>
        public bool Contains(KeyValuePair<TKey, TValue> item)
        {
            if (IsDisposed)
            {
                return false;
            }

            return ContainsCore(item);
        }

        /// <inheritdoc/>
        public bool TryGetValue(TKey key, out TValue value)
        {
            if (IsDisposed)
            {
                value = default;
                return false;
            }

            return TryGetValueCore(key, out value);
        }

        /// <inheritdoc/>
        public bool Remove(TKey key)
        {
            if (IsDisposed)
            {
                return false;
            }

            return TryRemoveWithNotification(key, out _);
        }

        /// <inheritdoc/>
        public bool Remove(KeyValuePair<TKey, TValue> item)
        {
            if (IsDisposed)
            {
                return false;
            }

            return TryRemoveWithNotification(item.Key, out _);
        }

        /// <inheritdoc/>
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

        /// <inheritdoc/>
        public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
        {
            if (IsDisposed)
            {
                return;
            }

            CopyToCore(array, arrayIndex);
        }

        /// <summary>
        /// Attempts to add an item to the dictionary, notifying observers of any changes.
        /// </summary>
        /// <param name="item">
        /// The item to be added.
        /// </param>
        /// <returns>
        /// <c>true</c> whether the item was added; otherwise, <c>false</c>.
        /// </returns>
        protected bool TryAddWithNotification(KeyValuePair<TKey, TValue> item)
        {
            if (IsDisposed)
            {
                return false;
            }

            return TryAddWithNotification(item.Key, item.Value);
        }

        /// <summary>
        /// Attempts to add an item to the dictionary, notifying observers of any changes.
        /// </summary>
        /// <param name="key">
        /// The key of the item to be added.
        /// </param>
        /// <param name="value">
        /// The value of the item to be added.
        /// </param>
        /// <returns>
        /// <c>true</c> whether the item was added; otherwise, <c>false</c>.
        /// </returns>
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

        /// <summary>
        /// Attempts to remove an item from the dictionary, notifying observers of any changes.
        /// </summary>
        /// <param name="key">
        /// The key of the item to be removed.
        /// </param>
        /// <param name="value">
        /// The value of the item removed.
        /// </param>
        /// <returns>
        /// <c>true</c> whether the item was removed; otherwise, <c>false</c>.
        /// </returns>
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

        /// <summary>
        /// Attempts to add or update an item in the dictionary, notifying observers of any changes.
        /// </summary>
        /// <param name="key">
        /// The key of the item to be updated.
        /// </param>
        /// <param name="value">
        /// The new value to set for the item.
        /// </param>
        /// <returns>
        /// <c>true</c> whether the item was updated; otherwise, <c>false</c>.
        /// </returns>
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

        /// <summary>
        /// <para>The core implementation for <see cref="TryGetValue"/>.</para>
        /// <para>Attempts to get the value associated with the specified key from the <see cref="ObservableDictionary{TKey, TValue}"/>.</para>
        /// </summary>
        /// <param name="key">
        /// The key of the item to get.
        /// </param>
        /// <param name="value">
        /// When this method returns, contains the object from the <see cref="ObservableDictionary{TKey, TValue}"/> that has the specified key, or the default value of the type if the operation failed.
        /// </param>
        /// <returns>
        /// <c>true</c> if the key was found in the <see cref="ObservableDictionary{TKey, TValue}"/>; otherwise, <c>false</c>.
        /// </returns>
        protected bool TryGetValueCore(TKey key, out TValue value)
        {
            if (IsDisposed)
            {
                value = default;
                return false;
            }

            return dictionary.TryGetValue(key, out value);
        }

        /// <summary>
        /// <para>The core implementation for <see cref="ContainsKey"/>.</para>
        /// <para>Determines whether the <see cref="ObservableDictionary{TKey, TValue}"/> contains the specified key.</para>
        /// </summary>
        /// <param name="key">
        /// The key to locate in the <see cref="ObservableDictionary{TKey, TValue}"/>.
        /// </param>
        /// <returns>
        /// </returns>
        protected bool ContainsKeyCore(TKey key)
        {
            if (IsDisposed)
            {
                return false;
            }

            return dictionary.ContainsKey(key);
        }

        /// <summary>
        /// <para>The core implementation for <see cref="Contains"/></para>
        /// <para>Determines whether the <see cref="ObservableDictionary{TKey, TValue}"/> contains a specific value.</para>
        /// </summary>
        /// <param name="item">
        /// The object to locate in the <see cref="ObservableDictionary{TKey, TValue}"/>.
        /// </param>
        /// <returns>
        /// <c>true</c> if the item is found in the <see cref="ObservableDictionary{TKey, TValue}"/>; otherwise, <c>false</c>.
        /// </returns>
        protected bool ContainsCore(KeyValuePair<TKey, TValue> item)
        {
            if (IsDisposed)
            {
                return false;
            }

            return ((ICollection<KeyValuePair<TKey, TValue>>)dictionary).Contains(item);
        }

        /// <summary>
        /// <para>The core implementation for <see cref="TryAddWithNotification(TKey, TValue)"/></para>
        /// <para>Attempts to add the specified key and value to the <see cref="ObservableDictionary{TKey, TValue}"/>.</para>
        /// </summary>
        /// <param name="key">
        /// The key of the element to add.
        /// </param>
        /// <param name="value">
        /// The value of the element to add. The value can be null for reference types.
        /// </param>
        /// <returns>
        /// </returns>
        protected bool TryAddCore(TKey key, TValue value)
        {
            if (IsDisposed)
            {
                return false;
            }

            return dictionary.TryAdd(key, value);
        }

        /// <summary>
        /// <para>The core implementation for <see cref="UpdateWithNotification"/></para>
        /// <para>Updates the specified key and value to the <see cref="ObservableDictionary{TKey, TValue}"/>.</para>
        /// </summary>
        /// <param name="key">
        /// </param>
        /// <param name="value">
        /// </param>
        protected void UpdateCore(TKey key, TValue value)
        {
            if (IsDisposed)
            {
                return;
            }

            dictionary[key] = value;
        }

        /// <summary>
        /// <para>The core implementation for <see cref="TryRemoveWithNotification"/></para>
        /// <para>Attempts to remove and return the value that has the specified key from the <see cref="ObservableDictionary{TKey, TValue}"/>.</para>
        /// </summary>
        /// <param name="key">
        /// The key of the element to remove and return.
        /// </param>
        /// <param name="value">
        /// When this method returns, contains the object removed from the <see cref="ObservableDictionary{TKey, TValue}"/>, or the default value of the TValue type if key does not exist.
        /// </param>
        /// <returns>
        /// <c>true</c> if the object was removed; otherwise, <c>false</c>.
        /// </returns>
        protected bool TryRemoveCore(TKey key, out TValue value)
        {
            if (IsDisposed)
            {
                value = default;
                return false;
            }

            return dictionary.TryRemove(key, out value);
        }

        /// <summary>
        /// <para>The core implementation for <see cref="Clear"/></para>
        /// <para>Removes all keys and values from the <see cref="ObservableDictionary{TKey, TValue}"/>.</para>
        /// </summary>
        protected void ClearCore()
        {
            if (IsDisposed)
            {
                return;
            }

            dictionary.Clear();
        }

        /// <summary>
        /// <para>The core implementation for <see cref="CopyTo"/></para>
        /// <para>Copies the elements of the <see cref="ObservableDictionary{TKey, TValue}"/> to an <see cref="Array"/>, starting at a particular <see cref="Array"/> index.</para>
        /// </summary>
        /// <param name="array">
        /// The one-dimensional <see cref="Array"/> that is the destination of the elements copied from <see cref="ObservableDictionary{TKey, TValue}"/>. The <see cref="Array"/> must have zero-based indexing.
        /// </param>
        /// <param name="arrayIndex">
        /// The zero-based index in array at which copying begins.
        /// </param>
        protected void CopyToCore(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
        {
            if (IsDisposed)
            {
                return;
            }

            ((ICollection<KeyValuePair<TKey, TValue>>)dictionary).CopyTo(array, arrayIndex);
        }

        /// <summary>
        /// Validates <paramref name="value"/> to be add or update before adding or updating.
        /// </summary>
        /// <param name="key">
        /// The key of the value to be validated.
        /// </param>
        /// <param name="value">
        /// The value to be validated.
        /// </param>
        /// <returns>
        /// <c>true</c> if value is valid to add or update; otherwise, <c>false</c>.
        /// </returns>
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

        /// <summary>
        /// Validates <paramref name="key"/> to be remove before removing.
        /// </summary>
        /// <param name="key">
        /// The key of the value to be validated.
        /// </param>
        /// <returns>
        /// <c>true</c> if value is valid to remove; otherwise, <c>false</c>.
        /// </returns>
        protected virtual bool ValidateRemoveItem(TKey key)
        {
            if (IsDisposed)
            {
                return false;
            }

            return ContainsKeyCore(key);
        }

        /// <summary>
        /// Validates clear before clearing.
        /// </summary>
        /// <returns>
        /// <c>true</c> if value is valid to clear; otherwise, <c>false</c>.
        /// </returns>
        protected virtual bool ValidateClear()
        {
            if (IsDisposed)
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Notifies observers of <see cref="CollectionChanged"/> or <see cref="INotifyCollectionChanged.CollectionChanged"/> of an update to the dictionary.
        /// </summary>
        protected virtual void NotifyObserversOfChange()
        {
            if (IsDisposed)
            {
                return;
            }

            var args = new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset);

            OnPropertyChanged(new PropertyChangedEventArgs(nameof(Keys)));
            OnPropertyChanged(new PropertyChangedEventArgs(nameof(Values)));
            OnPropertyChanged(new PropertyChangedEventArgs(nameof(Count)));

            ContextPost(delegate
            {
                CollectionChanged?.Invoke(this, args);
            });
        }

        /// <inheritdoc/>
        IEnumerator<KeyValuePair<TKey, TValue>> IEnumerable<KeyValuePair<TKey, TValue>>.GetEnumerator()
        {
            if (IsDisposed)
            {
                return default;
            }

            return ((ICollection<KeyValuePair<TKey, TValue>>)dictionary).GetEnumerator();
        }

        /// <inheritdoc/>
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
