using ObservableHelpers.Abstraction;
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
    /// Provides a thread-safe observable dictionary for use with data binding.
    /// </summary>
    /// <typeparam name="TKey">
    /// Specifies the type of the keys in this collection.
    /// </typeparam>
    /// <typeparam name="TValue">
    /// Specifies the type of the values in this collection.
    /// </typeparam>
    public class ObservableDictionary<TKey, TValue> :
        ObservableDictionaryFilter<TKey, TValue>,
        IDictionary<TKey, TValue>
    {
        #region Properties

        /// <inheritdoc/>
        ICollection<TKey> IDictionary<TKey, TValue>.Keys => CoreDictionary.Keys;

        /// <inheritdoc/>
        ICollection<TValue> IDictionary<TKey, TValue>.Values => CoreDictionary.Values;

        /// <inheritdoc/>
        public bool IsReadOnly => ((ICollection<KeyValuePair<TKey, TValue>>)CoreDictionary).IsReadOnly;

        /// <inheritdoc/>
        public new TValue this[TKey key]
        {
            get => CoreDictionary[key];
            set => AddOrUpdate(key, value);
        }

        #endregion

        #region Initializers

        /// <summary>
        /// Creates new instance of the <see cref="ObservableDictionary{TKey, TValue}"/> class that is empty.
        /// </summary>
        public ObservableDictionary()
            : base()
        {

        }

        /// <summary>
        /// Creates new instance of the <see cref="ObservableDictionary{TKey, TValue}"/> class that contains elements copied from the specified <paramref name="items"/>.
        /// </summary>
        public ObservableDictionary(IEnumerable<KeyValuePair<TKey, TValue>> items)
            : base(items)
        {

        }

        /// <summary>
        /// Creates new instance of the <see cref="ObservableDictionary{TKey, TValue}"/> class that is empty, and uses the specified <paramref name="comparer"/>.
        /// </summary>
        public ObservableDictionary(IEqualityComparer<TKey> comparer)
            : base(comparer)
        {

        }

        /// <summary>
        /// Creates new instance of the <see cref="ObservableDictionary{TKey, TValue}"/> class that contains elements copied from the specified <paramref name="items"/>, and uses the specified <paramref name="comparer"/>.
        /// </summary>
        public ObservableDictionary(IEnumerable<KeyValuePair<TKey, TValue>> items, IEqualityComparer<TKey> comparer)
            : base(items, comparer)
        {

        }

        /// <summary>
        /// Creates new instance of the <see cref="ObservableDictionary{TKey, TValue}"/> class that is empty, has the specified <paramref name="concurrencyLevel"/> and <paramref name="capacity"/>, and uses the default comparer for the key type.
        /// </summary>
        public ObservableDictionary(int concurrencyLevel, int capacity)
            : base(concurrencyLevel, capacity)
        {
        
        }

        /// <summary>
        /// Creates new instance of the <see cref="ObservableDictionary{TKey, TValue}"/> class that is empty, has the specified <paramref name="concurrencyLevel"/> and <paramref name="capacity"/>, and uses the specified <paramref name="comparer"/>.
        /// </summary>
        public ObservableDictionary(int concurrencyLevel, int capacity, IEqualityComparer<TKey> comparer)
            : base(concurrencyLevel, capacity, comparer)
        {
        
        }

        /// <summary>
        /// Creates new instance of the <see cref="ObservableDictionary{TKey, TValue}"/> class that contains elements copied from the specified <paramref name="items"/>, has the specified <paramref name="concurrencyLevel"/>, and uses the specified <paramref name="comparer"/>.
        /// </summary>
        public ObservableDictionary(int concurrencyLevel, IEnumerable<KeyValuePair<TKey, TValue>> items, IEqualityComparer<TKey> comparer)
            : base(concurrencyLevel, items, comparer)
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

            bool hasChanges = Count != 0;

            Clear();

            return hasChanges;
        }

        /// <summary>
        /// Add the specified key and value to the <see cref="ObservableDictionary{TKey, TValue}"/>.
        /// </summary>
        /// <param name="item">
        /// The element to add.
        /// </param>
        /// <returns>
        /// true if the key/value pair was added to the <see cref="ObservableDictionary{TKey, TValue}"/> successfully; otherwise false if the key already exists.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// key is null.
        /// </exception>
        /// <exception cref="OverflowException">
        /// The dictionary already contains the maximum number of elements (System.Int32.MaxValue).
        /// </exception>
        public void Add(KeyValuePair<TKey, TValue> item)
        {
            _ = TryAdd(item);
        }

        /// <summary>
        /// Attempts to add the specified key and value to the <see cref="ObservableDictionary{TKey, TValue}"/>.
        /// </summary>
        /// <param name="item">
        /// The element to add.
        /// </param>
        /// <returns>
        /// true if the key/value pair was added to the <see cref="ObservableDictionary{TKey, TValue}"/> successfully; otherwise false if the key already exists.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// key is null.
        /// </exception>
        /// <exception cref="OverflowException">
        /// The dictionary already contains the maximum number of elements (System.Int32.MaxValue).
        /// </exception>
        public bool TryAdd(KeyValuePair<TKey, TValue> item)
        {
            return TryAdd(item.Key, item.Value);
        }

        /// <summary>
        /// Add the specified key and value to the <see cref="ObservableDictionary{TKey, TValue}"/>.
        /// </summary>
        /// <param name="key">
        /// The key of the element to add.
        /// </param>
        /// <param name="value">
        /// The value of the element to add. The value can be null for reference types.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// key is null.
        /// </exception>
        /// <exception cref="OverflowException">
        /// The dictionary already contains the maximum number of elements (System.Int32.MaxValue).
        /// </exception>
        public void Add(TKey key, TValue value)
        {
            _ = TryAdd(key, value);
        }

        /// <summary>
        /// Attempts to add the specified key and value to the <see cref="ObservableDictionary{TKey, TValue}"/>.
        /// </summary>
        /// <param name="key">
        /// The key of the element to add.
        /// </param>
        /// <param name="value">
        /// The value of the element to add. The value can be null for reference types.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// key is null.
        /// </exception>
        /// <exception cref="OverflowException">
        /// The dictionary already contains the maximum number of elements (System.Int32.MaxValue).
        /// </exception>
        public bool TryAdd(TKey key, TValue value)
        {
            if (IsDisposed)
            {
                return false;
            }

            bool result = false;
            if (!CoreDictionary.ContainsKey(key))
            {
                PreAddItem(key, value);
                result = CoreDictionary.TryAdd(key, value);
                if (result)
                {
                    int index = Array.IndexOf(CoreDictionary.Keys.ToArray(), key);
                    OnCollectionChanged(NotifyCollectionChangedAction.Add, value, index);
                }
            }
            return result;
        }

        /// <summary>
        /// Remove the value that has the specified key from the <see cref="ObservableDictionary{TKey, TValue}"/>.
        /// </summary>
        /// <param name="item">
        /// The element to remove.
        /// </param>
        /// <returns>
        /// true if the object was removed successfully; otherwise, false.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// key is null.
        /// </exception>
        public bool Remove(KeyValuePair<TKey, TValue> item)
        {
            return TryRemove(item);
        }

        /// <summary>
        /// Attempts to remove and return the value that has the specified key from the <see cref="ObservableDictionary{TKey, TValue}"/>.
        /// </summary>
        /// <param name="item">
        /// The element to remove.
        /// </param>
        /// <returns>
        /// true if the object was removed successfully; otherwise, false.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// key is null.
        /// </exception>
        public bool TryRemove(KeyValuePair<TKey, TValue> item)
        {
            return TryRemove(item, out _);
        }

        /// <summary>
        /// Attempts to remove and return the value that has the specified key from the <see cref="ObservableDictionary{TKey, TValue}"/>.
        /// </summary>
        /// <param name="item">
        /// The element to remove.
        /// </param>
        /// <param name="value">
        /// When this method returns, contains the object removed from the <see cref="ObservableDictionary{TKey, TValue}"/> or the default value of the TValue type if key does not exist.
        /// </param>
        /// <returns>
        /// true if the object was removed successfully; otherwise, false.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// key is null.
        /// </exception>
        public bool TryRemove(KeyValuePair<TKey, TValue> item, out TValue value)
        {
            return TryRemove(item.Key, out value);
        }

        /// <summary>
        /// Attempts to remove and return the value that has the specified key from the <see cref="ObservableDictionary{TKey, TValue}"/>.
        /// </summary>
        /// <param name="key">
        /// The key of the element to remove.
        /// </param>
        /// <returns>
        /// true if the object was removed successfully; otherwise, false.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// key is null.
        /// </exception>
        public bool Remove(TKey key)
        {
            return TryRemove(key);
        }

        /// <summary>
        /// Attempts to remove and return the value that has the specified key from the <see cref="ObservableDictionary{TKey, TValue}"/>.
        /// </summary>
        /// <param name="key">
        /// The key of the element to remove.
        /// </param>
        /// <returns>
        /// true if the object was removed successfully; otherwise, false.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// key is null.
        /// </exception>
        public bool TryRemove(TKey key)
        {
            return TryRemove(key, out _);
        }

        /// <summary>
        /// Attempts to remove and return the value that has the specified key from the <see cref="ObservableDictionary{TKey, TValue}"/>.
        /// </summary>
        /// <param name="key">
        /// The key of the element to remove.
        /// </param>
        /// <param name="value">
        /// When this method returns, contains the object removed from the <see cref="ObservableDictionary{TKey, TValue}"/> or the default value of the TValue type if key does not exist.
        /// </param>
        /// <returns>
        /// true if the object was removed successfully; otherwise, false.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// key is null.
        /// </exception>
        public bool TryRemove(TKey key, out TValue value)
        {
            if (IsDisposed)
            {
                value = default;
                return false;
            }

            bool result = false;
            if (CoreDictionary.ContainsKey(key))
            {
                PreRemoveItem(key);
                int index = Array.IndexOf(CoreDictionary.Keys.ToArray(), key);
                result = CoreDictionary.TryRemove(key, out value);
                if (result)
                {
                    OnCollectionChanged(NotifyCollectionChangedAction.Remove, value, index);
                }
            }
            else
            {
                value = default;
            }
            return result;
        }

        /// <summary>
        /// Compares the existing value for the specified key with a specified value, and if they are equal, updates the key with a third value and notify observers.
        /// </summary>
        /// <param name="key">
        /// The key whose value is compared with comparisonValue and possibly replaced.
        /// </param>
        /// <param name="newValue">
        /// The value that replaces the value of the element that has the specified key if the comparison results in equality.
        /// </param>
        /// <returns>
        /// true if the value with key was equal to comparisonValue and was replaced with newValue; otherwise, false.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// key is null.
        /// </exception>
        /// <exception cref="KeyNotFoundException">
        /// The property is retrieved and key does not exist in the collection.
        /// </exception>
        public void Update(TKey key, TValue newValue)
        {
            if (IsDisposed)
            {
                return;
            }

            if (CoreDictionary.TryGetValue(key, out TValue oldValue))
            {
                PreUpdateItem(key, oldValue, newValue);
                CoreDictionary[key] = newValue;
                int index = Array.IndexOf(CoreDictionary.Keys.ToArray(), key);
                OnCollectionChanged(NotifyCollectionChangedAction.Replace, oldValue, newValue, index);
            }
            else
            {
                throw new KeyNotFoundException();
            }
        }

        /// <summary>
        /// Compares the existing value for the specified key with a specified value, and if they are equal, updates the key with a third value and notify observers.
        /// </summary>
        /// <param name="key">
        /// The key whose value is compared with comparisonValue and possibly replaced.
        /// </param>
        /// <param name="newValue">
        /// The value that replaces the value of the element that has the specified key if the comparison results in equality.
        /// </param>
        /// <param name="comparisonValue">
        /// The value that is compared to the value of the element that has the specified key.
        /// </param>
        /// <returns>
        /// true if the value with key was equal to comparisonValue and was replaced with newValue; otherwise, false.
        /// </returns>
        public void Update(TKey key, TValue newValue, TValue comparisonValue)
        {
            _ = TryUpdate(key, newValue, comparisonValue);
        }

        /// <summary>
        /// Compares the existing value for the specified key with a specified value, and if they are equal, updates the key with a third value and notify observers.
        /// </summary>
        /// <param name="key">
        /// The key whose value is compared with comparisonValue and possibly replaced.
        /// </param>
        /// <param name="newValue">
        /// The value that replaces the value of the element that has the specified key if the comparison results in equality.
        /// </param>
        /// <param name="comparisonValue">
        /// The value that is compared to the value of the element that has the specified key.
        /// </param>
        /// <returns>
        /// true if the value with key was equal to comparisonValue and was replaced with newValue; otherwise, false.
        /// </returns>
        public bool TryUpdate(TKey key, TValue newValue, TValue comparisonValue)
        {
            if (IsDisposed)
            {
                return false;
            }

            if (CoreDictionary.TryGetValue(key, out TValue oldValue))
            {
                PreUpdateItem(key, oldValue, newValue);
                if (CoreDictionary.TryUpdate(key, newValue, comparisonValue))
                {
                    int index = Array.IndexOf(CoreDictionary.Keys.ToArray(), key);
                    OnCollectionChanged(NotifyCollectionChangedAction.Replace, oldValue, newValue, index);
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Adds a key/value pair to the <see cref="ObservableDictionary{TKey, TValue}"/> if the key does not already exist.
        /// </summary>
        /// <param name="key">
        /// The key of the element to add.
        /// </param>
        /// <param name="value">
        /// The value to be added, if the key does not already exist.
        /// </param>
        /// <returns>
        /// The value for the key. This will be either the existing value for the key if the key is already in the dictionary, or the new value if the key was not in the dictionary.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// key is null.
        /// </exception>
        /// <exception cref="OverflowException">
        /// The dictionary already contains the maximum number of elements (System.Int32.MaxValue).
        /// </exception>
        public TValue GetOrAdd(TKey key, TValue value)
        {
            return GetOrAdd(key, _ => value);
        }

        /// <summary>
        /// Adds a key/value pair to the <see cref="ObservableDictionary{TKey, TValue}"/> by using the specified function if the key does not already exist, or returns the existing value if the key exists.
        /// </summary>
        /// <param name="key">
        /// The key of the element to add.
        /// </param>
        /// <param name="valueFactory">
        /// The function used to generate a value for the key.
        /// </param>
        /// <returns>
        /// The value for the key. This will be either the existing value for the key if the key is already in the dictionary, or the new value if the key was not in the dictionary.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// key or valueFactory is null.
        /// </exception>
        /// <exception cref="OverflowException">
        /// The dictionary already contains the maximum number of elements (System.Int32.MaxValue).
        /// </exception>
        public TValue GetOrAdd(TKey key, Func<TKey, TValue> valueFactory)
        {
            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }
            if (valueFactory == null)
            {
                throw new ArgumentNullException(nameof(valueFactory));
            }
            if (IsDisposed)
            {
                return default;
            }

            TValue ret = CoreDictionary.GetOrAdd(key, delegate
            {
                TValue value = valueFactory.Invoke(key);
                PreAddItem(key, value);
                int index = Array.IndexOf(CoreDictionary.Keys.ToArray(), key);
                OnCollectionChanged(NotifyCollectionChangedAction.Add, value, index);
                return value;
            });
            return ret;
        }

        /// <summary>
        /// Adds or updates a key/value pair to the <see cref="ObservableDictionary{TKey, TValue}"/> if the key does not already exist, or updates a key/value pair in the <see cref="ObservableDictionary{TKey, TValue}"/> by using the specified function if the key already exists.
        /// </summary>
        /// <param name="key">
        /// The key to be added or whose value should be updated.
        /// </param>
        /// <param name="value">
        /// The value to be added or updated.
        /// </param>
        /// <returns>
        /// The new value for the key. This will be either be addValue (if the key was absent) or the result of updateValueFactory (if the key was present).
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// key or updateValueFactory is null.
        /// </exception>
        /// <exception cref="OverflowException">
        /// The dictionary already contains the maximum number of elements <see cref="int"/>.
        /// </exception>
        public TValue AddOrUpdate(TKey key, TValue value)
        {
            return AddOrUpdate(key, _ => value, delegate { return value; });
        }

        /// <summary>
        /// Adds a key/value pair to the <see cref="ObservableDictionary{TKey, TValue}"/> if the key does not already exist, or updates a key/value pair in the <see cref="ObservableDictionary{TKey, TValue}"/> by using the specified function if the key already exists.
        /// </summary>
        /// <param name="key">
        /// The key to be added or whose value should be updated.
        /// </param>
        /// <param name="addValue">
        /// The value to be added for an absent key.
        /// </param>
        /// <param name="updateValue">
        /// A new value for an existing key based on the key's existing value.
        /// </param>
        /// <returns>
        /// The new value for the key. This will be either be addValue (if the key was absent) or the result of updateValueFactory (if the key was present).
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// key or updateValueFactory is null.
        /// </exception>
        /// <exception cref="OverflowException">
        /// The dictionary already contains the maximum number of elements <see cref="int"/>.
        /// </exception>
        public TValue AddOrUpdate(TKey key, TValue addValue, TValue updateValue)
        {
            return AddOrUpdate(key, _ => addValue, delegate { return updateValue; });
        }

        /// <summary>
        /// Adds a key/value pair to the <see cref="ObservableDictionary{TKey, TValue}"/> if the key does not already exist, or updates a key/value pair in the <see cref="ObservableDictionary{TKey, TValue}"/> by using the specified function if the key already exists.
        /// </summary>
        /// <param name="key">
        /// The key to be added or whose value should be updated.
        /// </param>
        /// <param name="addValue">
        /// The value to be added for an absent key.
        /// </param>
        /// <param name="updateValueFactory">
        /// The function used to generate a new value for an existing key based on the key's existing value.
        /// </param>
        /// <returns>
        /// The new value for the key. This will be either be addValue (if the key was absent) or the result of updateValueFactory (if the key was present).
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// key or updateValueFactory is null.
        /// </exception>
        /// <exception cref="OverflowException">
        /// The dictionary already contains the maximum number of elements <see cref="int"/>.
        /// </exception>
        public TValue AddOrUpdate(TKey key, TValue addValue, Func<TKey, TValue, TValue> updateValueFactory)
        {
            return AddOrUpdate(key, _ => addValue, updateValueFactory);
        }

        /// <summary>
        /// Uses the specified functions to add a key/value pair to the <see cref="ObservableDictionary{TKey, TValue}"/> if the key does not already exist, or to update a key/value pair in the <see cref="ObservableDictionary{TKey, TValue}"/> if the key already exists.
        /// </summary>
        /// <param name="key">
        /// The key to be added or whose value should be updated.
        /// </param>
        /// <param name="addValueFactory">
        /// The function used to generate a value for an absent key.
        /// </param>
        /// <param name="updateValueFactory">
        /// The function used to generate a new value for an existing key based on the key's existing value
        /// </param>
        /// <returns>
        /// The new value for the key. This will be either be the result of addValueFactory (if the key was absent) or the result of updateValueFactory (if the key was present).
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// key, addValueFactory, or updateValueFactory is null.
        /// </exception>
        /// <exception cref="OverflowException">
        /// The dictionary already contains the maximum number of elements <see cref="int"/>.
        /// </exception>
        public TValue AddOrUpdate(TKey key, Func<TKey, TValue> addValueFactory, Func<TKey, TValue, TValue> updateValueFactory)
        {
            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }
            if (addValueFactory == null)
            {
                throw new ArgumentNullException(nameof(addValueFactory));
            }
            if (updateValueFactory == null)
            {
                throw new ArgumentNullException(nameof(updateValueFactory));
            }
            if (IsDisposed)
            {
                return default;
            }

            TValue ret = CoreDictionary.AddOrUpdate(key, delegate
            {
                TValue value = addValueFactory.Invoke(key);
                PreAddItem(key, value);
                int index = Array.IndexOf(CoreDictionary.Keys.ToArray(), key);
                OnCollectionChanged(NotifyCollectionChangedAction.Add, value, index);
                return value;
            }, (_, oldValue) =>
            {
                TValue value = updateValueFactory.Invoke(key, oldValue);
                PreUpdateItem(key, oldValue, value);
                int index = Array.IndexOf(CoreDictionary.Keys.ToArray(), key);
                OnCollectionChanged(NotifyCollectionChangedAction.Replace, oldValue, value, index);
                return value;
            });
            return ret;
        }

        /// <inheritdoc/>
        public void Clear()
        {
            if (IsDisposed)
            {
                return;
            }

            PreClear();
            CoreDictionary.Clear();
            OnCollectionReset();
        }

        /// <summary>
        /// Executed before adding or updating.
        /// </summary>
        /// <param name="key">
        /// The key of the value to be added.
        /// </param>
        /// <param name="value">
        /// The value to be added.
        /// </param>
        protected virtual void PreAddItem(TKey key, TValue value)
        {
            if (IsDisposed)
            {
                return;
            }

            if (value is ISyncObject sync)
            {
                sync.SyncOperation.SetContext(this);
            }
        }

        /// <summary>
        /// Executed before adding or updating.
        /// </summary>
        /// <param name="key">
        /// The key of the value to be added.
        /// </param>
        /// <param name="oldValue">
        /// The old value to be updated.
        /// </param>
        /// <param name="newValue">
        /// The new value to be updated.
        /// </param>
        protected virtual void PreUpdateItem(TKey key, TValue oldValue, TValue newValue)
        {
            if (IsDisposed)
            {
                return;
            }

            if (newValue is ISyncObject sync)
            {
                sync.SyncOperation.SetContext(this);
            }
        }

        /// <summary>
        /// Executed before removing.
        /// </summary>
        /// <param name="key">
        /// The key of the value to be removed.
        /// </param>
        protected virtual void PreRemoveItem(TKey key)
        {

        }

        /// <summary>
        /// Executed before clearing.
        /// </summary>
        protected virtual void PreClear()
        {

        }

        #endregion
    }
}
