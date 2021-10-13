﻿using ObservableHelpers.Abstraction;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Threading;

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
    public class ObservableDictionary1<TKey, TValue> :
        ObservableCollectionBase<KeyValuePair<TKey, TValue>, IDictionary<TKey, TValue>>,
        IReadOnlyDictionary<TKey, TValue>,
        IDictionary<TKey, TValue>,
        IDictionary
    {
        #region Properties

        /// <summary>
        /// Gets or sets the element with the specified <paramref name="key"/>.
        /// </summary>
        /// <param name="key">
        /// The key of the element to get or set.
        /// </param>
        /// <returns>
        /// The element with the specified <paramref name="key"/>.</returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="key"/> is a null reference.
        /// </exception>
        /// <exception cref="KeyNotFoundException">
        /// The property is retrieved and key is not found.
        /// </exception>
        /// <exception cref="NotSupportedException">
        /// The property is set and the <see cref="ObservableDictionary1{TKey, TValue}"/> is read-only.
        /// </exception>
        public TValue this[TKey key]
        {
            get
            {
                if (IsDisposed)
                {
                    return default;
                }
                if (key == null)
                {
                    throw new ArgumentNullException(nameof(key));
                }

                return LockRead(() => Items[key]);
            }
            set
            {
                if (IsDisposed)
                {
                    return;
                }
                if (key == null)
                {
                    throw new ArgumentNullException(nameof(key));
                }
                if (IsReadOnly)
                {
                    throw ReadOnlyException(nameof(IndexerName));
                }

                AddOrUpdate(key, value);
            }
        }

        /// <summary>
        /// Gets an <see cref="DictionaryKeys"/> containing the keys of the <see cref="ObservableDictionary1{TKey, TValue}"/>.
        /// </summary>
        public DictionaryKeys Keys { get; }

        /// <summary>
        /// Gets an <see cref="DictionaryValues"/> containing the values of the <see cref="ObservableDictionary1{TKey, TValue}"/>.
        /// </summary>
        public DictionaryValues Values { get; }

        #endregion

        #region Initializers

        /// <summary>
        /// Creates new instance of the <see cref="ObservableDictionary1{TKey, TValue}"/> class that is empty.
        /// </summary>
        public ObservableDictionary1()
            : base(() => new Dictionary<TKey, TValue>())
        {

        }

        /// <summary>
        /// Creates new instance of the <see cref="ObservableDictionary1{TKey, TValue}"/> class that contains elements copied from the specified <paramref name="items"/>.
        /// </summary>
        /// <param name="items">
        /// The initial items of the dictionary.
        /// </param>
        public ObservableDictionary1(IEnumerable<KeyValuePair<TKey, TValue>> items)
            : base(() => new Dictionary<TKey, TValue>())
        {
            Populate(items);
            Keys = new DictionaryKeys(this);
            Values = new DictionaryValues(this);
        }

        /// <summary>
        /// Creates new instance of the <see cref="ObservableDictionary1{TKey, TValue}"/> class that is empty, and uses the specified <paramref name="comparer"/>.
        /// </summary>
        /// <param name="comparer">
        /// The default item comparer of the dictionary.
        /// </param>
        public ObservableDictionary1(IEqualityComparer<TKey> comparer)
            : base(() => new Dictionary<TKey, TValue>(comparer))
        {
            Keys = new DictionaryKeys(this);
            Values = new DictionaryValues(this);
        }

        /// <summary>
        /// Creates new instance of the <see cref="ObservableDictionary1{TKey, TValue}"/> class that contains elements copied from the specified <paramref name="items"/>, and uses the specified <paramref name="comparer"/>.
        /// </summary>
        /// <param name="items">
        /// The initial items of the dictionary.
        /// </param>
        /// <param name="comparer">
        /// The default item comparer of the dictionary.
        /// </param>
        public ObservableDictionary1(IEnumerable<KeyValuePair<TKey, TValue>> items, IEqualityComparer<TKey> comparer)
            : base(() => new Dictionary<TKey, TValue>(comparer))
        {
            Populate(items);
            Keys = new DictionaryKeys(this);
            Values = new DictionaryValues(this);
        }

        private void Populate(IEnumerable<KeyValuePair<TKey, TValue>> items)
        {
            foreach (KeyValuePair<TKey, TValue> pair in items)
            {
                Items.Add(pair);
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Adds the specified <paramref name="key"/> and <paramref name="value"/> to the <see cref="ObservableDictionary1{TKey, TValue}"/> and notify observers if the specified <paramref name="key"/> does not already exists.
        /// </summary>
        /// <param name="key">
        /// The key of the element to add.
        /// </param>
        /// <param name="value">
        /// The value of the element to add.
        /// </param>
        /// <returns>
        /// <c>true</c> if the key/value pair was added to the <see cref="ObservableDictionary1{TKey, TValue}"/> successfully; otherwise <c>false</c> if the specified <paramref name="key"/> already exists.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="key"/> is a null reference.
        /// </exception>
        public void Add(TKey key, TValue value)
        {
            Add(key, _ => value);
        }

        /// <summary>
        /// Adds the specified <paramref name="key"/> and the result of the <paramref name="valueFactory"/> to the <see cref="ObservableDictionary1{TKey, TValue}"/> and notify observers if the specified <paramref name="key"/> does not already exists.
        /// </summary>
        /// <param name="key">
        /// The key of the element to add.
        /// </param>
        /// <param name="valueFactory">
        /// The function used to create the value of the element.
        /// </param>
        /// <returns>
        /// <c>true</c> if the key/value pair was added to the <see cref="ObservableDictionary1{TKey, TValue}"/> successfully; otherwise <c>false</c> if the specified <paramref name="key"/> already exists.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="key"/> is a null reference.
        /// </exception>
        public void Add(TKey key, Func<TKey, TValue> valueFactory)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Adds or updates the value of the specified <paramref name="key"/> to the <see cref="ObservableDictionary1{TKey, TValue}"/> and notify observers.
        /// </summary>
        /// <param name="key">
        /// The key to be added or whose value should be updated.
        /// </param>
        /// <param name="value">
        /// The value to be added or updated.
        /// </param>
        /// <returns>
        /// The new value for the key.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="key"/> is a null reference.
        /// </exception>
        public TValue AddOrUpdate(TKey key, TValue value)
        {
            return AddOrUpdate(key, _ => value, _ => value);
        }

        /// <summary>
        /// Adds or updates the value of the specified <paramref name="key"/> to the <see cref="ObservableDictionary1{TKey, TValue}"/> and notify observers.
        /// </summary>
        /// <param name="key">
        /// The key to be added or whose value should be updated.
        /// </param>
        /// <param name="addValue">
        /// The value to be added.
        /// </param>
        /// <param name="updateValue">
        /// The value to be updated.
        /// </param>
        /// <returns>
        /// The new value for the specified <paramref name="key"/>. This will be either the <paramref name="addValue"/> (if the specified <paramref name="key"/> was absent) or the <paramref name="updateValue"/> (if the specified <paramref name="key"/> was present).
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="key"/> is a null reference.
        /// </exception>
        public TValue AddOrUpdate(TKey key, TValue addValue, TValue updateValue)
        {
            return AddOrUpdate(key, _ => addValue, _ => updateValue);
        }

        /// <summary>
        /// Adds or updates the value of the specified <paramref name="key"/> to the <see cref="ObservableDictionary1{TKey, TValue}"/> and notify observers.
        /// </summary>
        /// <param name="key">
        /// The key to be added or whose value should be updated.
        /// </param>
        /// <param name="addValue">
        /// The value to be added.
        /// </param>
        /// <param name="updateValueFactory">
        /// The function used to create the update value.
        /// </param>
        /// <returns>
        /// The new value for the specified <paramref name="key"/>. This will be either the <paramref name="addValue"/> (if the specified <paramref name="key"/> was absent) or the result of <paramref name="updateValueFactory"/> (if the specified <paramref name="key"/> was present).
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// Either <paramref name="key"/> or <paramref name="updateValueFactory"/> is a null reference.
        /// </exception>
        public TValue AddOrUpdate(TKey key, TValue addValue, Func<(TKey key, TValue oldValue), TValue> updateValueFactory)
        {
            return AddOrUpdate(key, _ => addValue, updateValueFactory);
        }

        /// <summary>
        /// Adds or updates the value of the specified <paramref name="key"/> to the <see cref="ObservableDictionary1{TKey, TValue}"/> and notify observers.
        /// </summary>
        /// <param name="key">
        /// The key to be added or whose value should be updated.
        /// </param>
        /// <param name="addValueFactory">
        /// The function used to create the add value.
        /// </param>
        /// <param name="updateValue">
        /// The value to be updated.
        /// </param>
        /// <returns>
        /// The new value for the specified <paramref name="key"/>. This will be either the result of <paramref name="addValueFactory"/> (if the specified <paramref name="key"/> was absent) or the <paramref name="updateValue"/> (if the specified <paramref name="key"/> was present).
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// Either <paramref name="key"/> or <paramref name="addValueFactory"/> is a null reference.
        /// </exception>
        public TValue AddOrUpdate(TKey key, Func<TKey, TValue> addValueFactory, TValue updateValue)
        {
            return AddOrUpdate(key, addValueFactory, _ => updateValue);
        }

        /// <summary>
        /// Adds or updates the value of the specified <paramref name="key"/> to the <see cref="ObservableDictionary1{TKey, TValue}"/> and notify observers.
        /// </summary>
        /// <param name="key">
        /// The key to be added or whose value should be updated.
        /// </param>
        /// <param name="addValueFactory">
        /// The function used to create the add value.
        /// </param>
        /// <param name="updateValueFactory">
        /// The function used to create the update value.
        /// </param>
        /// <returns>
        /// The new value for the specified <paramref name="key"/>. This will be either the result of <paramref name="addValueFactory"/> (if the specified <paramref name="key"/> was absent) or the result of <paramref name="updateValueFactory"/> (if the specified <paramref name="key"/> was present).
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// Either <paramref name="key"/>, <paramref name="addValueFactory"/> or <paramref name="updateValueFactory"/> is a null reference.
        /// </exception>
        public TValue AddOrUpdate(TKey key, Func<TKey, TValue> addValueFactory, Func<(TKey key, TValue oldValue), TValue> updateValueFactory)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Determines whether the <see cref="ObservableDictionary1{TKey, TValue}"/> contains an element with the specified <paramref name="key"/>.
        /// </summary>
        /// <param name="key">
        /// The key to locate in the <see cref="ObservableDictionary1{TKey, TValue}"/>.
        /// </param>
        /// <returns>
        /// <c>true</c> if the <see cref="ObservableDictionary1{TKey, TValue}"/> contains an element with the specified <paramref name="key"/>; otherwise, <c>false</c>.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="key"/> is a null reference.
        /// </exception>
        public bool ContainsKey(TKey key)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Adds or gets a key/value pair to the <see cref="ObservableDictionary1{TKey, TValue}"/> and notify observers if the specified <paramref name="key"/> does not already exist.
        /// </summary>
        /// <param name="key">
        /// The key of the element to add.
        /// </param>
        /// <param name="value">
        /// The value to be added, if the specified <paramref name="key"/> does not already exist.
        /// </param>
        /// <returns>
        /// The value for the specified <paramref name="key"/>. This will be either the existing value for the specified <paramref name="key"/> if the <paramref name="key"/> is already in the dictionary, or the new value if the specified <paramref name="key"/> was not in the dictionary.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="key"/> is a null reference.
        /// </exception>
        public TValue GetOrAdd(TKey key, TValue value)
        {
            return GetOrAdd(key, _ => value);
        }

        /// <summary>
        /// Adds or gets a key/value pair to the <see cref="ObservableDictionary1{TKey, TValue}"/> and notify observers if the specified <paramref name="key"/> does not already exist.
        /// </summary>
        /// <param name="key">
        /// The key of the element to add.
        /// </param>
        /// <param name="valueFactory">
        /// The function used to create the value to be added, if the specified <paramref name="key"/> does not already exist.
        /// </param>
        /// <returns>
        /// The value for the specified <paramref name="key"/>. This will be either the existing value for the specified <paramref name="key"/> if the <paramref name="key"/> is already in the dictionary, or the new value if the specified <paramref name="key"/> was not in the dictionary.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="key"/> is a null reference.
        /// </exception>
        public TValue GetOrAdd(TKey key, Func<TKey, TValue> valueFactory)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Removes the element of the specified <paramref name="key"/> from the <see cref="ObservableDictionary1{TKey, TValue}"/> and notify observers.
        /// </summary>
        /// <param name="key">
        /// The key of the element to remove.
        /// </param>
        /// <returns>
        /// <c>true</c> if the object was removed successfully; otherwise, <c>false</c>.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="key"/> is a null reference.
        /// </exception>
        public bool Remove(TKey key)
        {
            return TryRemove(key);
        }

        /// <summary>
        /// Attempts to add the specified <paramref name="item"/> to the <see cref="ObservableDictionary1{TKey, TValue}"/> and notify observers.
        /// </summary>
        /// <param name="item">
        /// The element to add.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// <see cref="KeyValuePair{TKey, TValue}.Key"/> of the <paramref name="item"/> is a null reference.
        /// </exception>
        public bool TryAdd(KeyValuePair<TKey, TValue> item)
        {
            return TryAdd(item.Key, _ => item.Value);
        }

        /// <summary>
        /// Attempts to add the specified <paramref name="key"/> and value to the <see cref="ObservableDictionary1{TKey, TValue}"/> and notify observers.
        /// </summary>
        /// <param name="key">
        /// The key of the element to add.
        /// </param>
        /// <param name="value">
        /// The value of the element to add. The value can be null for reference types.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="key"/> is a null reference.
        /// </exception>
        public bool TryAdd(TKey key, TValue value)
        {
            return TryAdd(key, _ => value);
        }

        /// <summary>
        /// Attempts to add the specified <paramref name="key"/> and value to the <see cref="ObservableDictionary1{TKey, TValue}"/> and notify observers.
        /// </summary>
        /// <param name="key">
        /// The key of the element to add.
        /// </param>
        /// <param name="valueFactory">
        /// The function used to create the value of the element to add. The value can be null for reference types.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// Either <paramref name="key"/> or <paramref name="valueFactory"/> is a null reference.
        /// </exception>
        public bool TryAdd(TKey key, Func<TKey, TValue> valueFactory)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Gets the value that is associated with the specified <paramref name="key"/>.
        /// </summary>
        /// <param name="key">
        /// The key to locate.
        /// </param>
        /// <returns>
        /// <c>true</c> if the <see cref="ObservableDictionary1{TKey, TValue}"/> contains an element that has the specified <paramref name="key"/>; otherwise, <c>false</c>.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="key"/> is a null reference.
        /// </exception>
        public bool TryGetValue(TKey key)
        {
            return TryGetValue(key, out _);
        }

        /// <summary>
        /// Gets the value that is associated with the specified <paramref name="key"/>.
        /// </summary>
        /// <param name="key">
        /// The key to locate.
        /// </param>
        /// <param name="value">
        /// When this method returns, the value associated with the specified <paramref name="key"/>, if the specified <paramref name="key"/> is found; otherwise, the default value for the type of the value parameter. This parameter is passed uninitialized.
        /// </param>
        /// <returns>
        /// <c>true</c> if the <see cref="ObservableDictionary1{TKey, TValue}"/> contains an element that has the specified <paramref name="key"/>; otherwise, <c>false</c>.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="key"/> is a null reference.
        /// </exception>
        public bool TryGetValue(TKey key, out TValue value)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Attempts to remove and return the value that has the specified <see cref="KeyValuePair{TKey, TValue}.Key"/> of the <paramref name="item"/> from the <see cref="ObservableDictionary1{TKey, TValue}"/> and notify observers.
        /// </summary>
        /// <param name="item">
        /// The element to remove.
        /// </param>
        /// <returns>
        /// <c>rtrue</c> if the object was removed successfully; otherwise, <c>false</c>.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="item"/> or <see cref="KeyValuePair{TKey, TValue}.Key"/> of the <paramref name="item"/> is a null reference.
        /// </exception>
        public bool TryRemove(KeyValuePair<TKey, TValue> item)
        {
            return TryRemove(item.Key, out _);
        }

        /// <summary>
        /// Attempts to remove and return the value that has the specified <see cref="KeyValuePair{TKey, TValue}.Key"/> of the <paramref name="item"/> from the <see cref="ObservableDictionary1{TKey, TValue}"/> and notify observers.
        /// </summary>
        /// <param name="item">
        /// The element to remove.
        /// </param>
        /// <param name="removed">
        /// When this method returns, contains the object removed from the <see cref="ObservableDictionary1{TKey, TValue}"/> or the default value of the object if specified <paramref name="item"/> does not exist.
        /// </param>
        /// <returns>
        /// <c>rtrue</c> if the object was removed successfully; otherwise, <c>false</c>.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="item"/> or <see cref="KeyValuePair{TKey, TValue}.Key"/> of the <paramref name="item"/> is a null reference.
        /// </exception>
        public bool TryRemove(KeyValuePair<TKey, TValue> item, out KeyValuePair<TKey, TValue> removed)
        {
            bool isRemoved = TryRemove(item.Key, out TValue value);
            removed = new KeyValuePair<TKey, TValue>(item.Key, value);
            return isRemoved;
        }

        /// <summary>
        /// Attempts to remove and return the value that has the specified <paramref name="key"/> from the <see cref="ObservableDictionary1{TKey, TValue}"/> and notify observers.
        /// </summary>
        /// <param name="key">
        /// The key of the element to remove.
        /// </param>
        /// <returns>
        /// <c>true</c> if the object was removed successfully; otherwise, <c>false</c>.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="key"/> is a null reference.
        /// </exception>
        public bool TryRemove(TKey key)
        {
            return TryRemove(key, out _);
        }

        /// <summary>
        /// Attempts to remove and return the value that has the specified <paramref name="key"/> from the <see cref="ObservableDictionary1{TKey, TValue}"/> and notify observers.
        /// </summary>
        /// <param name="key">
        /// The key of the element to remove.
        /// </param>
        /// <param name="value">
        /// When this method returns, contains the object removed from the <see cref="ObservableDictionary1{TKey, TValue}"/> or the default value of the object if specified <paramref name="key"/> does not exist.
        /// </param>
        /// <returns>
        /// <c>true</c> if the object was removed successfully; otherwise, <c>false</c>.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="key"/> is a null reference.
        /// </exception>
        public bool TryRemove(TKey key, out TValue value)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Attempts to update the value of the specified <paramref name="key"/> with <paramref name="newValue"/> and notify observers if the element for the specified <paramref name="key"/> exists.
        /// </summary>
        /// <param name="key">
        /// The key whose value is possibly replaced with <paramref name="newValue"/> if found.
        /// </param>
        /// <param name="newValue">
        /// The value that replaces the value of the element that has the specified <paramref name="key"/> if found.
        /// </param>
        /// <returns>
        /// <c>true</c> if the value with specified <paramref name="key"/> was found and was replaced with <paramref name="newValue"/>; otherwise, false.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="key"/> is a null reference.
        /// </exception>
        public bool TryUpdate(TKey key, TValue newValue)
        {
            return TryUpdate(key, _ => newValue, _ => true);
        }

        /// <summary>
        /// Attempts to update the value of the specified <paramref name="key"/> with <paramref name="newValue"/> and notify observers if the existing value for the specified <paramref name="key"/> is not equal to the specified <paramref name="comparisonValue"/>.
        /// </summary>
        /// <param name="key">
        /// The key whose value is compared with <paramref name="comparisonValue"/> and possibly replaced with <paramref name="newValue"/> if the comparison results in equality.
        /// </param>
        /// <param name="newValue">
        /// The value that replaces the value of the element that has the specified <paramref name="key"/> if the comparison results in equality.
        /// </param>
        /// <param name="comparisonValue">
        /// The value that is compared to the value of the element that has the specified <paramref name="key"/>.
        /// </param>
        /// <returns>
        /// <c>true</c> if the value with specified <paramref name="key"/> was equal to <paramref name="comparisonValue"/> and was replaced with <paramref name="newValue"/>; otherwise, <c>false</c>.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="key"/> is a null reference.
        /// </exception>
        public bool TryUpdate(TKey key, TValue newValue, TValue comparisonValue)
        {
            return TryUpdate(key, _ => newValue, a => !EqualityComparer<TValue>.Default.Equals(a.oldValue, comparisonValue));
        }

        /// <summary>
        /// Attempts to update the value of the specified <paramref name="key"/> with <paramref name="newValue"/> and notify observers if the element was validated <c>true</c> with the specified <paramref name="validation"/>.
        /// </summary>
        /// <param name="key">
        /// The key whose value is validated and possibly replaced with <paramref name="newValue"/> if the validation results in <c>true</c>.
        /// </param>
        /// <param name="newValue">
        /// The value that replaces the value of the element that has the specified <paramref name="key"/> if the validation results in <c>true</c>.
        /// </param>
        /// <param name="validation">
        /// The function used to validate the found element.
        /// </param>
        /// <returns>
        /// <c>true</c> if the value with specified <paramref name="key"/> was validated <c>true</c> and was replaced with <paramref name="newValue"/>; otherwise, <c>false</c>.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="key"/> is a null reference.
        /// </exception>
        public bool TryUpdate(TKey key, TValue newValue, Func<(TKey key, TValue newValue, TValue oldValue), bool> validation)
        {
            return TryUpdate(key, _ => newValue, validation);
        }

        /// <summary>
        /// Attempts to update the value of the specified <paramref name="key"/> with the result of <paramref name="newValueFactory"/> and notify observers if the element for the specified <paramref name="key"/> exists.
        /// </summary>
        /// <param name="key">
        /// The key whose value is possibly replaced with the result of <paramref name="newValueFactory"/> if found.
        /// </param>
        /// <param name="newValueFactory">
        /// The function used to create the value that replaces the value of the element that has the specified <paramref name="key"/> if found.
        /// </param>
        /// <returns>
        /// <c>true</c> if the value with specified <paramref name="key"/> was found and was replaced with the result of <paramref name="newValueFactory"/>; otherwise, false.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="key"/> is a null reference.
        /// </exception>
        public bool TryUpdate(TKey key, Func<TKey, TValue> newValueFactory)
        {
            return TryUpdate(key, newValueFactory, _ => true);
        }

        /// <summary>
        /// Attempts to update the value of the specified <paramref name="key"/> with the result of <paramref name="newValueFactory"/> and notify observers if the existing value for the specified <paramref name="key"/> is not equal to the specified <paramref name="comparisonValue"/>.
        /// </summary>
        /// <param name="key">
        /// The key whose value is compared with <paramref name="comparisonValue"/> and possibly replaced with the result of <paramref name="newValueFactory"/> if the comparison results in equality.
        /// </param>
        /// <param name="newValueFactory">
        /// The function that is used to create the value that replaces the value of the element that has the specified <paramref name="key"/> if the comparison results in equality.
        /// </param>
        /// <param name="comparisonValue">
        /// The value that is compared to the value of the element that has the specified <paramref name="key"/>.
        /// </param>
        /// <returns>
        /// <c>true</c> if the value with specified <paramref name="key"/> was equal to <paramref name="comparisonValue"/> and was replaced with the result of <paramref name="newValueFactory"/>; otherwise, <c>false</c>.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="key"/> is a null reference.
        /// </exception>
        public bool TryUpdate(TKey key, Func<TKey, TValue> newValueFactory, TValue comparisonValue)
        {
            return TryUpdate(key, newValueFactory, a => EqualityComparer<TValue>.Default.Equals(a.oldValue, comparisonValue));
        }

        /// <summary>
        /// Attempts to update the value of the specified <paramref name="key"/> with the result of <paramref name="newValueFactory"/> and notify observers if the element was validated <c>true</c> with the specified <paramref name="validation"/>.
        /// </summary>
        /// <param name="key">
        /// The key whose value is validated and possibly replaced with the result of <paramref name="newValueFactory"/> if the validation results in <c>true</c>..
        /// </param>
        /// <param name="newValueFactory">
        /// The value that replaces the value of the element that has the specified <paramref name="key"/> if the validation results in <c>true</c>.
        /// </param>
        /// <param name="validation">
        /// The function used to validate the found element.
        /// </param>
        /// <returns>
        /// <c>true</c> if the value with specified <paramref name="key"/> was validated <c>true</c> and was replaced with the result of <paramref name="newValueFactory"/>; otherwise, <c>false</c>.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="key"/> is a null reference.
        /// </exception>
        public bool TryUpdate(TKey key, Func<TKey, TValue> newValueFactory, Func<(TKey key, TValue newValue, TValue oldValue), bool> validation)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Attempts to update the value of the specified <paramref name="key"/> with <paramref name="newValue"/> and notify observers if the element for the specified <paramref name="key"/> exists.
        /// </summary>
        /// <param name="key">
        /// The key whose value is possibly replaced with <paramref name="newValue"/> if found.
        /// </param>
        /// <param name="newValue">
        /// The value that replaces the value of the element that has the specified <paramref name="key"/> if found.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="key"/> is a null reference.
        /// </exception>
        public void Update(TKey key, TValue newValue)
        {
            TryUpdate(key, newValue);
        }

        /// <summary>
        /// Attempts to update the value of the specified <paramref name="key"/> with <paramref name="newValue"/> and notify observers if the existing value for the specified <paramref name="key"/> is not equal to the specified <paramref name="comparisonValue"/>.
        /// </summary>
        /// <param name="key">
        /// The key whose value is compared with <paramref name="comparisonValue"/> and possibly replaced with <paramref name="newValue"/> if the comparison results in equality.
        /// </param>
        /// <param name="newValue">
        /// The value that replaces the value of the element that has the specified <paramref name="key"/> if the comparison results in equality.
        /// </param>
        /// <param name="comparisonValue">
        /// The value that is compared to the value of the element that has the specified <paramref name="key"/>.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="key"/> is a null reference.
        /// </exception>
        public void Update(TKey key, TValue newValue, TValue comparisonValue)
        {
            TryUpdate(key, newValue, comparisonValue);
        }

        /// <summary>
        /// Attempts to update the value of the specified <paramref name="key"/> with <paramref name="newValue"/> and notify observers if the element was validated <c>true</c> with the specified <paramref name="validation"/>.
        /// </summary>
        /// <param name="key">
        /// The key whose value is validated and possibly replaced with <paramref name="newValue"/> if the validation results in <c>true</c>.
        /// </param>
        /// <param name="newValue">
        /// The value that replaces the value of the element that has the specified <paramref name="key"/> if the validation results in <c>true</c>.
        /// </param>
        /// <param name="validation">
        /// The function used to validate the found element.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="key"/> is a null reference.
        /// </exception>
        public void Update(TKey key, TValue newValue, Func<(TKey key, TValue newValue, TValue oldValue), bool> validation)
        {
            TryUpdate(key, newValue, validation);
        }

        /// <summary>
        /// Attempts to update the value of the specified <paramref name="key"/> with the result of <paramref name="newValueFactory"/> and notify observers if the element for the specified <paramref name="key"/> exists.
        /// </summary>
        /// <param name="key">
        /// The key whose value is possibly replaced with the result of <paramref name="newValueFactory"/> if found.
        /// </param>
        /// <param name="newValueFactory">
        /// The function used to create the value that replaces the value of the element that has the specified <paramref name="key"/> if found.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="key"/> is a null reference.
        /// </exception>
        public void Update(TKey key, Func<TKey, TValue> newValueFactory)
        {
            TryUpdate(key, newValueFactory);
        }

        /// <summary>
        /// Attempts to update the value of the specified <paramref name="key"/> with the result of <paramref name="newValueFactory"/> and notify observers if the existing value for the specified <paramref name="key"/> is not equal to the specified <paramref name="comparisonValue"/>.
        /// </summary>
        /// <param name="key">
        /// The key whose value is compared with <paramref name="comparisonValue"/> and possibly replaced with the result of <paramref name="newValueFactory"/> if the comparison results in equality.
        /// </param>
        /// <param name="newValueFactory">
        /// The function that is used to create the value that replaces the value of the element that has the specified <paramref name="key"/> if the comparison results in equality.
        /// </param>
        /// <param name="comparisonValue">
        /// The value that is compared to the value of the element that has the specified <paramref name="key"/>.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="key"/> is a null reference.
        /// </exception>
        public void Update(TKey key, Func<TKey, TValue> newValueFactory, TValue comparisonValue)
        {
            TryUpdate(key, newValueFactory, comparisonValue);
        }

        /// <summary>
        /// Attempts to update the value of the specified <paramref name="key"/> with the result of <paramref name="newValueFactory"/> and notify observers if the element was validated <c>true</c> with the specified <paramref name="validation"/>.
        /// </summary>
        /// <param name="key">
        /// The key whose value is validated and possibly replaced with the result of <paramref name="newValueFactory"/> if the validation results in <c>true</c>..
        /// </param>
        /// <param name="newValueFactory">
        /// The value that replaces the value of the element that has the specified <paramref name="key"/> if the validation results in <c>true</c>.
        /// </param>
        /// <param name="validation">
        /// The function used to validate the found element.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="key"/> is a null reference.
        /// </exception>
        public void Update(TKey key, Func<TKey, TValue> newValueFactory, Func<(TKey key, TValue newValue, TValue oldValue), bool> validation)
        {
            TryUpdate(key, newValueFactory, validation);
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

        private ArgumentException WrongKeyTypeException(string propertyName, Type providedType)
        {
            return new ArgumentException("Expected key type is \"" + typeof(TKey).FullName + "\" but dictionary was provided with \"" + providedType.FullName + "\" key type.", propertyName);
        }

        private ArgumentException WrongValueTypeException(string propertyName, Type providedType)
        {
            return new ArgumentException("Expected value type is \"" + typeof(TValue).FullName + "\" but dictionary was provided with \"" + providedType.FullName + "\" value type.", propertyName);
        }

        #endregion

        #region ObservableCollectionBase<T, TCollectionWrapper> Members

        /// <summary>
        /// Returns a dictionary enumerator that iterates through the dictionary collection.
        /// </summary>
        /// <returns>
        /// An enumerator that can be used to iterate through the dictionary collection.
        /// </returns>
        public new DictionaryEnumerator GetEnumerator()
        {
            if (IsDisposed)
            {
                return default;
            }

            return new DictionaryEnumerator(this);
        }

        /// <inheritdoc/>
        protected override void AddItem(KeyValuePair<TKey, TValue> item)
        {
            Add(item.Key, item.Value);
        }

        /// <inheritdoc/>
        protected override bool RemoveItem(KeyValuePair<TKey, TValue> item)
        {
            return Remove(item.Key);
        }

        #endregion

        #region IReadOnlyDictionary<TKey, TValue> Members

        TValue IReadOnlyDictionary<TKey, TValue>.this[TKey key] => this[key];

        IEnumerable<TKey> IReadOnlyDictionary<TKey, TValue>.Keys => Keys;

        IEnumerable<TValue> IReadOnlyDictionary<TKey, TValue>.Values => Values;

        bool IReadOnlyDictionary<TKey, TValue>.ContainsKey(TKey key) => ContainsKey(key);

        bool IReadOnlyDictionary<TKey, TValue>.TryGetValue(TKey key, out TValue value) => TryGetValue(key, out value);

        #endregion

        #region IReadOnlyCollection<T> Members

        int IReadOnlyCollection<KeyValuePair<TKey, TValue>>.Count => Count;

        #endregion

        #region IDictionary<TKey, TValue> Members

        TValue IDictionary<TKey, TValue>.this[TKey key]
        {
            get => this[key];
            set => this[key] = value;
        }

        ICollection<TKey> IDictionary<TKey, TValue>.Keys => Keys;

        ICollection<TValue> IDictionary<TKey, TValue>.Values => Values;

        void IDictionary<TKey, TValue>.Add(TKey key, TValue value) => Add(key, value);

        bool IDictionary<TKey, TValue>.ContainsKey(TKey key) => ContainsKey(key);

        bool IDictionary<TKey, TValue>.Remove(TKey key) => Remove(key);

        bool IDictionary<TKey, TValue>.TryGetValue(TKey key, out TValue value) => TryGetValue(key, out value);

        #endregion

        #region IDictionary Members

        object IDictionary.this[object key]
        {
            get
            {
                TKey tKey;
                if (key is null)
                {
                    if (default(TKey) == null)
                    {
                        tKey = default;
                    }
                    else
                    {
                        throw WrongKeyTypeException(nameof(key), key?.GetType());
                    }
                }
                else if (key is TKey k)
                {
                    tKey = k;
                }
                else
                {
                    throw WrongKeyTypeException(nameof(key), key?.GetType());
                }

                return this[tKey];
            }
            set
            {
                TKey tKey;
                TValue tValue;
                if (key is null)
                {
                    if (default(TKey) == null)
                    {
                        tKey = default;
                    }
                    else
                    {
                        throw WrongKeyTypeException(nameof(key), key?.GetType());
                    }
                }
                else if (key is TKey k)
                {
                    tKey = k;
                }
                else
                {
                    throw WrongKeyTypeException(nameof(key), key?.GetType());
                }

                if (value is null)
                {
                    if (default(TValue) == null)
                    {
                        tValue = default;
                    }
                    else
                    {
                        throw WrongValueTypeException(nameof(value), value?.GetType());
                    }
                }
                else if (value is TValue v)
                {
                    tValue = v;
                }
                else
                {
                    throw WrongValueTypeException(nameof(value), value?.GetType());
                }

                this[tKey] = tValue;
            }
        }

        ICollection IDictionary.Keys => Keys;

        ICollection IDictionary.Values => Values;

        bool IDictionary.IsReadOnly => IsReadOnly;

        bool IDictionary.IsFixedSize => false;

        void IDictionary.Add(object key, object value)
        {
            TKey tKey;
            TValue tValue;
            if (key is null)
            {
                if (default(TKey) == null)
                {
                    tKey = default;
                }
                else
                {
                    throw WrongKeyTypeException(nameof(key), key?.GetType());
                }
            }
            else if (key is TKey k)
            {
                tKey = k;
            }
            else
            {
                throw WrongKeyTypeException(nameof(key), key?.GetType());
            }

            if (value is null)
            {
                if (default(TValue) == null)
                {
                    tValue = default;
                }
                else
                {
                    throw WrongValueTypeException(nameof(value), value?.GetType());
                }
            }
            else if (value is TValue v)
            {
                tValue = v;
            }
            else
            {
                throw WrongValueTypeException(nameof(value), value?.GetType());
            }

            Add(tKey, tValue);
        }

        void IDictionary.Clear() => Clear();

        bool IDictionary.Contains(object key)
        {
            if (key is null)
            {
                if (default(TKey) == null)
                {
                    return ContainsKey(default);
                }
                else
                {
                    throw WrongKeyTypeException(nameof(key), key?.GetType());
                }
            }
            else if (key is TKey tKey)
            {
                return ContainsKey(tKey);
            }
            else
            {
                throw WrongKeyTypeException(nameof(key), key?.GetType());
            }
        }

        IDictionaryEnumerator IDictionary.GetEnumerator() => GetEnumerator();

        void IDictionary.Remove(object key)
        {
            if (key is null)
            {
                if (default(TKey) == null)
                {
                    Remove(default);
                }
                else
                {
                    throw WrongKeyTypeException(nameof(key), key?.GetType());
                }
            }
            else if (key is TKey tKey)
            {
                Remove(tKey);
            }
            else
            {
                throw WrongKeyTypeException(nameof(key), key?.GetType());
            }
        }

        #endregion

        #region Helper Classes

        /// <summary>
        /// Provides the keys of the <see cref="ObservableDictionary1{TKey, TValue}"/>.
        /// </summary>
        public class DictionaryKeys : ObservableCollectionBase<TKey, IList<TKey>>
        {
            #region Properties

            /// <inheritdoc/>
            protected override IList<TKey> Items
            {
                get
                {
                    if (isItemsOutdated)
                    {
                        items = dictionary.Items.Keys.ToList();
                        isItemsOutdated = false;
                    }
                    return items;
                }
                set => throw ReadOnlyException(nameof(Items));
            }

            private readonly ObservableDictionary1<TKey, TValue> dictionary;
            private List<TKey> items = new List<TKey>();
            private bool isItemsOutdated = true;

            #endregion

            #region Initializers

            internal DictionaryKeys(ObservableDictionary1<TKey, TValue> dictionary)
                : base(() => new List<TKey>())
            {
                this.dictionary = dictionary;
                IsReadOnly = true;
                items = dictionary.Items.Keys.ToList();
                dictionary.ImmediatePropertyChanged += (s, e) =>
                {
                    if (e.PropertyName == nameof(IndexerName))
                    {
                        isItemsOutdated = true;
                    }
                    if (e.PropertyName == nameof(Count) ||
                        e.PropertyName == nameof(IndexerName))
                    {
                        OnPropertyChanged(e.PropertyName);
                    }
                };
                dictionary.ImmediateCollectionChanged += (s, e) =>
                {
                    switch (e.Action)
                    {
                        case NotifyCollectionChangedAction.Add:
                            OnCollectionAdd(
                                e.NewItems?.Cast<KeyValuePair<TKey, TValue>>().Select(i => i.Key),
                                e.NewStartingIndex);
                            break;
                        case NotifyCollectionChangedAction.Remove:
                            OnCollectionRemove(
                                e.OldItems?.Cast<KeyValuePair<TKey, TValue>>().Select(i => i.Key),
                                e.OldStartingIndex);
                            break;
                        case NotifyCollectionChangedAction.Replace:
                            OnCollectionReplace(
                                e.OldItems?.Cast<KeyValuePair<TKey, TValue>>().Select(i => i.Key),
                                e.NewItems?.Cast<KeyValuePair<TKey, TValue>>().Select(i => i.Key),
                                e.NewStartingIndex);
                            break;
                        case NotifyCollectionChangedAction.Move:
                            OnCollectionMove(
                                e.NewItems?.Cast<KeyValuePair<TKey, TValue>>().Select(i => i.Key),
                                e.NewStartingIndex,
                                e.OldStartingIndex);
                            break;
                        case NotifyCollectionChangedAction.Reset:
                            OnCollectionReset();
                            break;
                    }
                };
            }

            #endregion
        }

        /// <summary>
        /// Provides the values of the <see cref="ObservableDictionary1{TKey, TValue}"/>.
        /// </summary>
        public class DictionaryValues : ObservableCollectionBase<TValue, IList<TValue>>
        {
            #region Properties

            /// <inheritdoc/>
            protected override IList<TValue> Items
            {
                get
                {
                    if (isItemsOutdated)
                    {
                        items = dictionary.Items.Values.ToList();
                        isItemsOutdated = false;
                    }
                    return items;
                }
                set => throw ReadOnlyException(nameof(Items));
            }

            private readonly ObservableDictionary1<TKey, TValue> dictionary;
            private List<TValue> items = new List<TValue>();
            private bool isItemsOutdated = true;

            #endregion

            #region Initializers

            internal DictionaryValues(ObservableDictionary1<TKey, TValue> dictionary)
                : base(() => new List<TValue>())
            {
                this.dictionary = dictionary;
                IsReadOnly = true;
                dictionary.ImmediatePropertyChanged += (s, e) =>
                {
                    if (e.PropertyName == nameof(IndexerName))
                    {
                        isItemsOutdated = true;
                    }
                    if (e.PropertyName == nameof(Count) ||
                        e.PropertyName == nameof(IndexerName))
                    {
                        OnPropertyChanged(e.PropertyName);
                    }
                };
                dictionary.ImmediateCollectionChanged += (s, e) =>
                {
                    switch (e.Action)
                    {
                        case NotifyCollectionChangedAction.Add:
                            OnCollectionAdd(
                                e.NewItems?.Cast<KeyValuePair<TKey, TValue>>().Select(i => i.Value),
                                e.NewStartingIndex);
                            break;
                        case NotifyCollectionChangedAction.Remove:
                            OnCollectionRemove(
                                e.OldItems?.Cast<KeyValuePair<TKey, TValue>>().Select(i => i.Value),
                                e.OldStartingIndex);
                            break;
                        case NotifyCollectionChangedAction.Replace:
                            OnCollectionReplace(
                                e.OldItems?.Cast<KeyValuePair<TKey, TValue>>().Select(i => i.Value),
                                e.NewItems?.Cast<KeyValuePair<TKey, TValue>>().Select(i => i.Value),
                                e.NewStartingIndex);
                            break;
                        case NotifyCollectionChangedAction.Move:
                            OnCollectionMove(
                                e.NewItems?.Cast<KeyValuePair<TKey, TValue>>().Select(i => i.Value),
                                e.NewStartingIndex,
                                e.OldStartingIndex);
                            break;
                        case NotifyCollectionChangedAction.Reset:
                            OnCollectionReset();
                            break;
                    }
                };
            }

            #endregion
        }

        /// <summary>
        /// Provides the default enumerator of the <see cref="ObservableDictionary1{TKey, TValue}"/>.
        /// </summary>
        public class DictionaryEnumerator : IEnumerator<KeyValuePair<TKey, TValue>>, IDictionaryEnumerator
        {
            #region Properties

            /// <inheritdoc/>
            public KeyValuePair<TKey, TValue> Current => enumerator.Current;

            /// <inheritdoc/>
            public DictionaryEntry Entry => new DictionaryEntry(Current.Key, Current.Value);

            /// <inheritdoc/>
            public object Key => Current.Key;

            /// <inheritdoc/>
            public object Value => Current.Value;

            object IEnumerator.Current => Current;

            private readonly IEnumerator<KeyValuePair<TKey, TValue>> enumerator;

            #endregion

            #region Initializers

            internal DictionaryEnumerator(ObservableDictionary1<TKey, TValue> dictionary)
            {
                enumerator = dictionary.Items.GetEnumerator();
            }

            #endregion

            #region Methods

            /// <inheritdoc/>
            public void Dispose() => enumerator.Dispose();

            /// <inheritdoc/>
            public bool MoveNext() => enumerator.MoveNext();

            /// <inheritdoc/>
            public void Reset() => enumerator.Reset();

            #endregion
        }

        #endregion
    }
}