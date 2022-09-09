using ObservableHelpers.Utilities;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace ObservableHelpers.Collections;

/// <summary>
/// Provides a thread-safe observable dictionary for use with data binding.
/// </summary>
/// <typeparam name="TKey">
/// Specifies the type of the keys in this collection.
/// </typeparam>
/// <typeparam name="TValue">
/// Specifies the type of the values in this collection.
/// </typeparam>
public class ObservableConcurrentDictionary<TKey, TValue> :
    ObservableConcurrentCollectionBase<KeyValuePair<TKey, TValue>>,
    IReadOnlyDictionary<TKey, TValue>,
    IDictionary<TKey, TValue>,
    IDictionary
    where TKey : notnull
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
    /// The property is set and the <see cref="ObservableConcurrentDictionary{TKey, TValue}"/> is read-only.
    /// </exception>
    public TValue this[TKey key]
    {
        get
        {
            ArgumentNullException.ThrowIfNull(key);

            return RWLock.LockRead(() => dictionary[key]);
        }
        set
        {
            ArgumentNullException.ThrowIfNull(key);
            ThrowIfReadOnly(nameof(IndexerName));

            AddOrUpdate(key, value);
        }
    }

    /// <summary>
    /// Gets an <see cref="DictionaryKeys"/> containing the keys of the <see cref="ObservableConcurrentDictionary{TKey, TValue}"/>.
    /// </summary>
    public DictionaryKeys Keys { get; }

    /// <summary>
    /// Gets an <see cref="DictionaryValues"/> containing the values of the <see cref="ObservableConcurrentDictionary{TKey, TValue}"/>.
    /// </summary>
    public DictionaryValues Values { get; }

    private readonly Dictionary<TKey, TValue> dictionary;

    #endregion

    #region Initializers

    /// <summary>
    /// Creates new instance of the <see cref="ObservableConcurrentDictionary{TKey, TValue}"/> class that is empty.
    /// </summary>
    public ObservableConcurrentDictionary()
        : base(_ => new List<KeyValuePair<TKey, TValue>>())
    {
        dictionary = new Dictionary<TKey, TValue>();
        Keys = new DictionaryKeys();
        Values = new DictionaryValues();
    }

    /// <summary>
    /// Creates new instance of the <see cref="ObservableConcurrentDictionary{TKey, TValue}"/> class that contains elements copied from the specified <paramref name="items"/>.
    /// </summary>
    /// <param name="items">
    /// The initial items of the dictionary.
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="items"/> is a null reference.
    /// </exception>
    public ObservableConcurrentDictionary(IEnumerable<KeyValuePair<TKey, TValue>> items)
        : base(_ => new List<KeyValuePair<TKey, TValue>>())
    {
        ArgumentNullException.ThrowIfNull(items);

        dictionary = new Dictionary<TKey, TValue>();
        Keys = new DictionaryKeys();
        Values = new DictionaryValues();
        Populate(items);
    }

    /// <summary>
    /// Creates new instance of the <see cref="ObservableConcurrentDictionary{TKey, TValue}"/> class that is empty, and uses the specified <paramref name="comparer"/>.
    /// </summary>
    /// <param name="comparer">
    /// The default item comparer of the dictionary.
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="comparer"/> is a null reference.
    /// </exception>
    public ObservableConcurrentDictionary(IEqualityComparer<TKey> comparer)
        : base(_ => new List<KeyValuePair<TKey, TValue>>())
    {
        ArgumentNullException.ThrowIfNull(comparer);

        dictionary = new Dictionary<TKey, TValue>(comparer);
        Keys = new DictionaryKeys();
        Values = new DictionaryValues();
    }

    /// <summary>
    /// Creates new instance of the <see cref="ObservableConcurrentDictionary{TKey, TValue}"/> class that contains elements copied from the specified <paramref name="items"/>, and uses the specified <paramref name="comparer"/>.
    /// </summary>
    /// <param name="items">
    /// The initial items of the dictionary.
    /// </param>
    /// <param name="comparer">
    /// The default item comparer of the dictionary.
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// Either <paramref name="items"/> or <paramref name="comparer"/> is a null reference.
    /// </exception>
    public ObservableConcurrentDictionary(IEnumerable<KeyValuePair<TKey, TValue>> items, IEqualityComparer<TKey> comparer)
        : base(_ => new List<KeyValuePair<TKey, TValue>>())
    {
        ArgumentNullException.ThrowIfNull(items);
        ArgumentNullException.ThrowIfNull(comparer);

        dictionary = new Dictionary<TKey, TValue>(comparer);
        Keys = new DictionaryKeys();
        Values = new DictionaryValues();
        Populate(items);
    }

    private void Populate(IEnumerable<KeyValuePair<TKey, TValue>> items)
    {
        int index = 0;
        InsertItems(index++, items, out _);
    }

    #endregion

    #region Methods

    /// <summary>
    /// Adds the specified <paramref name="key"/> and <paramref name="value"/> to the <see cref="ObservableConcurrentDictionary{TKey, TValue}"/> and notify observers if the specified <paramref name="key"/> does not already exists.
    /// </summary>
    /// <param name="key">
    /// The key of the element to add.
    /// </param>
    /// <param name="value">
    /// The value of the element to add.
    /// </param>
    /// <exception cref="ArgumentException">
    /// An element with the same key already exists in the <see cref="ObservableConcurrentDictionary{TKey, TValue}"/>.
    /// </exception>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="key"/> is a null reference.
    /// </exception>
    /// <exception cref="NotSupportedException">
    /// The <see cref="ObservableConcurrentDictionary{TKey, TValue}"/> is read-only.
    /// </exception>
    public void Add(TKey key, TValue value)
    {
        Add(new KeyValuePair<TKey, TValue>(key, value));
    }

    /// <summary>
    /// Adds the specified <paramref name="item"/> to the <see cref="ObservableConcurrentDictionary{TKey, TValue}"/> and notify observers if the specified <paramref name="item"/> does not already exists.
    /// </summary>
    /// <param name="item">
    /// The element to add.
    /// </param>
    /// <exception cref="ArgumentException">
    /// An element with the same key already exists in the <see cref="ObservableConcurrentDictionary{TKey, TValue}"/>.
    /// </exception>
    /// <exception cref="ArgumentNullException">
    /// <see cref="KeyValuePair{TKey, TValue}.Key"/> of the <paramref name="item"/> is a null reference.
    /// </exception>
    /// <exception cref="NotSupportedException">
    /// The <see cref="ObservableConcurrentDictionary{TKey, TValue}"/> is read-only.
    /// </exception>
    public void Add(KeyValuePair<TKey, TValue> item)
    {
        ArgumentNullException.ThrowIfNull(item);
        ThrowIfReadOnly(nameof(Add));

        RWLock.LockUpgradeableRead(() =>
        {
            int index = Items.Count;
            InsertItem(index, item, out _);
        });
    }

    /// <summary>
    /// Adds or updates the value of the specified <paramref name="key"/> to the <see cref="ObservableConcurrentDictionary{TKey, TValue}"/> and notify observers.
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
    /// <exception cref="NotSupportedException">
    /// The <see cref="ObservableConcurrentDictionary{TKey, TValue}"/> is read-only.
    /// </exception>
    public TValue AddOrUpdate(TKey key, TValue value)
    {
        return AddOrUpdate(key, _ => value, _ => value);
    }

    /// <summary>
    /// Adds or updates the value of the specified <paramref name="key"/> to the <see cref="ObservableConcurrentDictionary{TKey, TValue}"/> and notify observers.
    /// </summary>
    /// <param name="key">
    /// The key to be added or whose value should be updated.
    /// </param>
    /// <param name="valueFactory">
    /// The function used to create or the add value.
    /// </param>
    /// <returns>
    /// The new value for the key.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// Either <paramref name="key"/> or <paramref name="valueFactory"/> is a null reference.
    /// </exception>
    /// <exception cref="NotSupportedException">
    /// The <see cref="ObservableConcurrentDictionary{TKey, TValue}"/> is read-only.
    /// </exception>
    public TValue AddOrUpdate(TKey key, Func<TKey, TValue> valueFactory)
    {
        return AddOrUpdate(key, valueFactory, _ => valueFactory.Invoke(key));
    }

    /// <summary>
    /// Adds or updates the value of the specified <paramref name="key"/> to the <see cref="ObservableConcurrentDictionary{TKey, TValue}"/> and notify observers.
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
    /// <exception cref="NotSupportedException">
    /// The <see cref="ObservableConcurrentDictionary{TKey, TValue}"/> is read-only.
    /// </exception>
    public TValue AddOrUpdate(TKey key, TValue addValue, TValue updateValue)
    {
        return AddOrUpdate(key, _ => addValue, _ => updateValue);
    }

    /// <summary>
    /// Adds or updates the value of the specified <paramref name="key"/> to the <see cref="ObservableConcurrentDictionary{TKey, TValue}"/> and notify observers.
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
    /// <exception cref="NotSupportedException">
    /// The <see cref="ObservableConcurrentDictionary{TKey, TValue}"/> is read-only.
    /// </exception>
    public TValue AddOrUpdate(TKey key, TValue addValue, Func<(TKey key, TValue oldValue), TValue> updateValueFactory)
    {
        return AddOrUpdate(key, _ => addValue, updateValueFactory);
    }

    /// <summary>
    /// Adds or updates the value of the specified <paramref name="key"/> to the <see cref="ObservableConcurrentDictionary{TKey, TValue}"/> and notify observers.
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
    /// <exception cref="NotSupportedException">
    /// The <see cref="ObservableConcurrentDictionary{TKey, TValue}"/> is read-only.
    /// </exception>
    public TValue AddOrUpdate(TKey key, Func<TKey, TValue> addValueFactory, TValue updateValue)
    {
        return AddOrUpdate(key, addValueFactory, _ => updateValue);
    }

    /// <summary>
    /// Adds or updates the value of the specified <paramref name="key"/> to the <see cref="ObservableConcurrentDictionary{TKey, TValue}"/> and notify observers.
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
    /// <exception cref="NotSupportedException">
    /// The <see cref="ObservableConcurrentDictionary{TKey, TValue}"/> is read-only.
    /// </exception>
    public TValue AddOrUpdate(TKey key, Func<TKey, TValue> addValueFactory, Func<(TKey key, TValue oldValue), TValue> updateValueFactory)
    {
        ArgumentNullException.ThrowIfNull(key);
        ArgumentNullException.ThrowIfNull(addValueFactory);
        ArgumentNullException.ThrowIfNull(updateValueFactory);
        ThrowIfReadOnly(nameof(AddOrUpdate));

        return RWLock.LockUpgradeableRead(() =>
        {
            TValue value;
            KeyValuePair<TKey, TValue> item;
            if (dictionary.TryGetValue(key, out TValue? oldValue))
            {
                value = updateValueFactory.Invoke((key, oldValue));
                item = new KeyValuePair<TKey, TValue>(key, value);
                int index = Items.FindIndex(i => EqualityComparer<TKey>.Default.Equals(i.Key, key));
                SetItem(index, item, out _);
            }
            else
            {
                value = addValueFactory.Invoke(key);
                item = new KeyValuePair<TKey, TValue>(key, value);
                int index = Items.Count;
                InsertItem(index, item, out _);
            }
            return value;
        });
    }

    /// <summary>
    /// Adds an item range to the <see cref="ObservableConcurrentDictionary{TKey, TValue}"/> and notify the observers for changes.
    /// </summary>
    /// <param name="items">
    /// The items to add to the <see cref="ObservableConcurrentDictionary{TKey, TValue}"/>.
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="items"/> is a null reference.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// An element with the same key already exists in the <see cref="ObservableConcurrentDictionary{TKey, TValue}"/>.
    /// </exception>
    /// <exception cref="NotSupportedException">
    /// The <see cref="ObservableConcurrentDictionary{TKey, TValue}"/> is read-only.
    /// </exception>
    public void AddRange(params KeyValuePair<TKey, TValue>[] items)
    {
        AddRange(items as IEnumerable<KeyValuePair<TKey, TValue>>);
    }

    /// <summary>
    /// Adds an item range to the <see cref="ObservableConcurrentDictionary{TKey, TValue}"/> and notify the observers for changes.
    /// </summary>
    /// <param name="items">
    /// The items to add to the <see cref="ObservableConcurrentDictionary{TKey, TValue}"/>.
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="items"/> is a null reference.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// An element with the same key already exists in the <see cref="ObservableConcurrentDictionary{TKey, TValue}"/>.
    /// </exception>
    /// <exception cref="NotSupportedException">
    /// The <see cref="ObservableConcurrentDictionary{TKey, TValue}"/> is read-only.
    /// </exception>
    public void AddRange(IEnumerable<KeyValuePair<TKey, TValue>> items)
    {
        ArgumentNullException.ThrowIfNull(items);
        ThrowIfReadOnly(nameof(AddRange));

        RWLock.LockUpgradeableRead(() =>
        {
            int index = Items.Count;
            InsertItems(index, items, out _);
        });
    }

    /// <summary>
    /// Removes all elements from the <see cref="ObservableConcurrentDictionary{TKey, TValue}"/> and notify the observers for changes.
    /// </summary>
    /// <exception cref="NotSupportedException">
    /// The <see cref="ObservableConcurrentDictionary{TKey, TValue}"/> is read-only.
    /// </exception>
    public void Clear()
    {
        ThrowIfReadOnly(nameof(Clear));

        ClearItems(out _);
    }

    /// <summary>
    /// Determines whether the <see cref="ObservableConcurrentDictionary{TKey, TValue}"/> contains an element with the specified <paramref name="key"/>.
    /// </summary>
    /// <param name="key">
    /// The key to locate in the <see cref="ObservableConcurrentDictionary{TKey, TValue}"/>.
    /// </param>
    /// <returns>
    /// <c>true</c> if the <see cref="ObservableConcurrentDictionary{TKey, TValue}"/> contains an element with the specified <paramref name="key"/>; otherwise, <c>false</c>.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="key"/> is a null reference.
    /// </exception>
    public bool ContainsKey(TKey key)
    {
        ArgumentNullException.ThrowIfNull(key);

        return RWLock.LockRead(() => dictionary.ContainsKey(key));
    }

    /// <summary>
    /// Adds or gets a key/value pair to the <see cref="ObservableConcurrentDictionary{TKey, TValue}"/> and notify observers if the specified <paramref name="key"/> does not already exist.
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
    /// <exception cref="NotSupportedException">
    /// The <see cref="ObservableConcurrentDictionary{TKey, TValue}"/> is read-only.
    /// </exception>
    public TValue GetOrAdd(TKey key, TValue value)
    {
        return GetOrAdd(key, _ => value);
    }

    /// <summary>
    /// Adds or gets a key/value pair to the <see cref="ObservableConcurrentDictionary{TKey, TValue}"/> and notify observers if the specified <paramref name="key"/> does not already exist.
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
    /// <exception cref="NotSupportedException">
    /// The <see cref="ObservableConcurrentDictionary{TKey, TValue}"/> is read-only.
    /// </exception>
    public TValue GetOrAdd(TKey key, Func<TKey, TValue> valueFactory)
    {
        ArgumentNullException.ThrowIfNull(key);
        ArgumentNullException.ThrowIfNull(valueFactory);
        ThrowIfReadOnly(nameof(GetOrAdd));

        return RWLock.LockUpgradeableRead(() =>
        {
            if (!dictionary.TryGetValue(key, out TValue? value))
            {
                value = valueFactory.Invoke(key);
                int index = Items.Count;
                InsertItem(index, new KeyValuePair<TKey, TValue>(key, value), out _);
            }
            return value;
        });
    }

    /// <summary>
    /// Inserts an item to the <see cref="ObservableConcurrentDictionary{TKey, TValue}"/> at the specified index and notify the observers for changes.
    /// </summary>
    /// <param name="index">
    /// The zero-based index at which item should be inserted.
    /// </param>
    /// <param name="item">
    /// The item to insert into the <see cref="ObservableConcurrentDictionary{TKey, TValue}"/>.
    /// </param>
    /// <exception cref="ArgumentException">
    /// An element with the same key already exists in the <see cref="ObservableConcurrentDictionary{TKey, TValue}"/>.
    /// </exception>
    /// <exception cref="ArgumentOutOfRangeException">
    /// <paramref name="index"/> is not a valid index in the <see cref="ObservableConcurrentDictionary{TKey, TValue}"/>.
    /// </exception>
    /// <exception cref="NotSupportedException">
    /// The <see cref="ObservableConcurrentDictionary{TKey, TValue}"/> is read-only.
    /// </exception>
    public void Insert(int index, KeyValuePair<TKey, TValue> item)
    {
        ThrowIfReadOnly(nameof(Insert));

        InsertItem(index, item, out _);
    }

    /// <summary>
    /// Inserts an item range to the <see cref="ObservableConcurrentDictionary{TKey, TValue}"/> at the specified index and notify the observers for changes.
    /// </summary>
    /// <param name="index">
    /// The zero-based index at which item should be inserted.
    /// </param>
    /// <param name="items">
    /// The item to insert into the <see cref="ObservableConcurrentDictionary{TKey, TValue}"/>.
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="items"/> is a null reference.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// An element with the same key already exists in the <see cref="ObservableConcurrentDictionary{TKey, TValue}"/>.
    /// </exception>
    /// <exception cref="ArgumentOutOfRangeException">
    /// <paramref name="index"/> is not a valid index in the <see cref="ObservableConcurrentDictionary{TKey, TValue}"/>.
    /// </exception>
    /// <exception cref="NotSupportedException">
    /// The <see cref="ObservableConcurrentDictionary{TKey, TValue}"/> is read-only.
    /// </exception>
    public void InsertRange(int index, params KeyValuePair<TKey, TValue>[] items)
    {
        InsertRange(index, items as IEnumerable<KeyValuePair<TKey, TValue>>);
    }

    /// <summary>
    /// Inserts an item range to the <see cref="ObservableConcurrentDictionary{TKey, TValue}"/> at the specified index and notify the observers for changes.
    /// </summary>
    /// <param name="index">
    /// The zero-based index at which item should be inserted.
    /// </param>
    /// <param name="items">
    /// The item to insert into the <see cref="ObservableConcurrentDictionary{TKey, TValue}"/>.
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="items"/> is a null reference.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// An element with the same key already exists in the <see cref="ObservableConcurrentDictionary{TKey, TValue}"/>.
    /// </exception>
    /// <exception cref="ArgumentOutOfRangeException">
    /// <paramref name="index"/> is not a valid index in the <see cref="ObservableConcurrentDictionary{TKey, TValue}"/>.
    /// </exception>
    /// <exception cref="NotSupportedException">
    /// The <see cref="ObservableConcurrentDictionary{TKey, TValue}"/> is read-only.
    /// </exception>
    public void InsertRange(int index, IEnumerable<KeyValuePair<TKey, TValue>> items)
    {
        ArgumentNullException.ThrowIfNull(items);
        ThrowIfReadOnly(nameof(InsertRange));

        InsertItems(index, items, out _);
    }

    /// <summary>
    /// Moves an element at the specified <paramref name="oldIndex"/> to the specified <paramref name="newIndex"/> of the <see cref="ObservableConcurrentDictionary{TKey, TValue}"/> and notify the observers.
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Either or both <paramref name="oldIndex"/> or <paramref name="newIndex"/> are less than zero. -or- is greater than <see cref="ObservableConcurrentCollectionBase{T}.Count"/>.
    /// </exception>
    /// <exception cref="NotSupportedException">
    /// The <see cref="ObservableConcurrentDictionary{TKey, TValue}"/> is read-only.
    /// </exception>
    public void Move(int oldIndex, int newIndex)
    {
        ThrowIfReadOnly(nameof(Move));

        MoveItem(oldIndex, newIndex, out _);
    }

    /// <summary>
    /// Removes the element of the specified <paramref name="key"/> from the <see cref="ObservableConcurrentDictionary{TKey, TValue}"/> and notify observers.
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
    /// <exception cref="NotSupportedException">
    /// The <see cref="ObservableConcurrentDictionary{TKey, TValue}"/> is read-only.
    /// </exception>
    public bool Remove(TKey key)
    {
        return TryRemove(key);
    }

    /// <summary>
    /// Removes the first occurrence of a specific object from the <see cref="ObservableConcurrentDictionary{TKey, TValue}"/>.
    /// </summary>
    /// <param name="item">
    /// The object to remove from the <see cref="ObservableConcurrentDictionary{TKey, TValue}"/>.
    /// </param>
    /// <returns>
    /// <c>true</c> if item was successfully removed from the <see cref="ObservableConcurrentDictionary{TKey, TValue}"/>; otherwise, <c>false</c>. This method also returns false if item is not found in the <see cref="ObservableConcurrentDictionary{TKey, TValue}"/>.
    /// </returns>
    /// <exception cref="NotSupportedException">
    /// The <see cref="ObservableConcurrentDictionary{TKey, TValue}"/> is read-only.
    /// </exception>
    /// <exception cref="ArgumentNullException">
    /// <see cref="KeyValuePair{TKey, TValue}.Key"/> of the <paramref name="item"/> is a null reference.
    /// </exception>
    public bool Remove(KeyValuePair<TKey, TValue> item)
    {
        return TryRemove(item.Key);
    }

    /// <summary>
    /// Removes the <see cref="ObservableConcurrentDictionary{TKey, TValue}"/> item at the specified index.
    /// </summary>
    /// <param name="index">
    /// The zero-based index of the item to remove.
    /// </param>
    /// <exception cref="ArgumentOutOfRangeException">
    /// <paramref name="index"/> is not a valid index in the <see cref="ObservableConcurrentDictionary{TKey, TValue}"/>.
    /// </exception>
    /// <exception cref="NotSupportedException">
    /// The <see cref="ObservableConcurrentDictionary{TKey, TValue}"/> is read-only.
    /// </exception>
    public void RemoveAt(int index)
    {
        ThrowIfReadOnly(nameof(RemoveAt));

        RemoveItem(index, out _);
    }

    /// <summary>
    /// Removes a specific object range from the <see cref="ObservableConcurrentDictionary{TKey, TValue}"/>.
    /// </summary>
    /// <param name="index">
    /// The zero-based starting index of the elements to remove.
    /// </param>
    /// <param name="count">
    /// The count of elements to remove.
    /// </param>
    /// <returns>
    /// <c>true</c> if items was successfully removed from the <see cref="ObservableConcurrentDictionary{TKey, TValue}"/>; otherwise, <c>false</c>.
    /// </returns>
    /// <exception cref="ArgumentException">
    /// <paramref name="index"/> and <paramref name="count"/> do not denote a valid range of elements in the <see cref="ObservableConcurrentDictionary{TKey, TValue}"/>.
    /// </exception>
    /// <exception cref="ArgumentOutOfRangeException">
    /// <paramref name="index"/> is less than zero. -or- is greater than <see cref="ObservableConcurrentCollectionBase{T}.Count"/>.
    /// </exception>
    /// <exception cref="NotSupportedException">
    /// The <see cref="ObservableConcurrentDictionary{TKey, TValue}"/> is read-only.
    /// </exception>
    public bool RemoveRange(int index, int count)
    {
        ThrowIfReadOnly(nameof(RemoveRange));

        return RemoveItems(index, count, out _);
    }

    /// <summary>
    /// Attempts to add the specified <paramref name="item"/> to the <see cref="ObservableConcurrentDictionary{TKey, TValue}"/> and notify observers.
    /// </summary>
    /// <param name="item">
    /// The element to add.
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// <see cref="KeyValuePair{TKey, TValue}.Key"/> of the <paramref name="item"/> is a null reference.
    /// </exception>
    /// <exception cref="NotSupportedException">
    /// The <see cref="ObservableConcurrentDictionary{TKey, TValue}"/> is read-only.
    /// </exception>
    public bool TryAdd(KeyValuePair<TKey, TValue> item)
    {
        return TryAdd(item.Key, _ => item.Value);
    }

    /// <summary>
    /// Attempts to add the specified <paramref name="key"/> and value to the <see cref="ObservableConcurrentDictionary{TKey, TValue}"/> and notify observers.
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
    /// <exception cref="NotSupportedException">
    /// The <see cref="ObservableConcurrentDictionary{TKey, TValue}"/> is read-only.
    /// </exception>
    public bool TryAdd(TKey key, TValue value)
    {
        return TryAdd(key, _ => value);
    }

    /// <summary>
    /// Attempts to add the specified <paramref name="key"/> and value to the <see cref="ObservableConcurrentDictionary{TKey, TValue}"/> and notify observers.
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
    /// <exception cref="NotSupportedException">
    /// The <see cref="ObservableConcurrentDictionary{TKey, TValue}"/> is read-only.
    /// </exception>
    public bool TryAdd(TKey key, Func<TKey, TValue> valueFactory)
    {
        ArgumentNullException.ThrowIfNull(key);
        ArgumentNullException.ThrowIfNull(valueFactory);
        ThrowIfReadOnly(nameof(TryAdd));

        return RWLock.LockUpgradeableRead(() =>
        {
            if (!dictionary.ContainsKey(key))
            {
                TValue value = valueFactory.Invoke(key);
                int index = Items.Count;
                InsertItem(index, new KeyValuePair<TKey, TValue>(key, value), out _);
                return true;
            }
            return false;
        });
    }

    /// <summary>
    /// Gets the value that is associated with the specified <paramref name="key"/>.
    /// </summary>
    /// <param name="key">
    /// The key to locate.
    /// </param>
    /// <returns>
    /// <c>true</c> if the <see cref="ObservableConcurrentDictionary{TKey, TValue}"/> contains an element that has the specified <paramref name="key"/>; otherwise, <c>false</c>.
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
    /// <c>true</c> if the <see cref="ObservableConcurrentDictionary{TKey, TValue}"/> contains an element that has the specified <paramref name="key"/>; otherwise, <c>false</c>.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="key"/> is a null reference.
    /// </exception>
    public bool TryGetValue(TKey key, [MaybeNullWhen(false)] out TValue value)
    {
        value = default;

        ArgumentNullException.ThrowIfNull(key);

        TValue? proxy = default;
        bool exists = RWLock.LockRead(() => dictionary.TryGetValue(key, out proxy));
        value = proxy;
        return exists;
    }

    /// <summary>
    /// Attempts to remove and return the value that has the specified <see cref="KeyValuePair{TKey, TValue}.Key"/> of the <paramref name="item"/> from the <see cref="ObservableConcurrentDictionary{TKey, TValue}"/> and notify observers.
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
    /// <exception cref="NotSupportedException">
    /// The <see cref="ObservableConcurrentDictionary{TKey, TValue}"/> is read-only.
    /// </exception>
    public bool TryRemove(KeyValuePair<TKey, TValue> item)
    {
        return TryRemove(item.Key, out _);
    }

    /// <summary>
    /// Attempts to remove and return the value that has the specified <see cref="KeyValuePair{TKey, TValue}.Key"/> of the <paramref name="item"/> from the <see cref="ObservableConcurrentDictionary{TKey, TValue}"/> and notify observers.
    /// </summary>
    /// <param name="item">
    /// The element to remove.
    /// </param>
    /// <param name="removed">
    /// When this method returns, contains the object removed from the <see cref="ObservableConcurrentDictionary{TKey, TValue}"/> or the default value of the object if specified <paramref name="item"/> does not exist.
    /// </param>
    /// <returns>
    /// <c>rtrue</c> if the object was removed successfully; otherwise, <c>false</c>.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="item"/> or <see cref="KeyValuePair{TKey, TValue}.Key"/> of the <paramref name="item"/> is a null reference.
    /// </exception>
    /// <exception cref="NotSupportedException">
    /// The <see cref="ObservableConcurrentDictionary{TKey, TValue}"/> is read-only.
    /// </exception>
    public bool TryRemove(KeyValuePair<TKey, TValue> item, [MaybeNullWhen(false)] out KeyValuePair<TKey, TValue?> removed)
    {
        bool isRemoved = TryRemove(item.Key, out TValue? value);
        removed = new KeyValuePair<TKey, TValue?>(item.Key, value);
        return isRemoved;
    }

    /// <summary>
    /// Attempts to remove and return the value that has the specified <paramref name="key"/> from the <see cref="ObservableConcurrentDictionary{TKey, TValue}"/> and notify observers.
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
    /// <exception cref="NotSupportedException">
    /// The <see cref="ObservableConcurrentDictionary{TKey, TValue}"/> is read-only.
    /// </exception>
    public bool TryRemove(TKey key)
    {
        return TryRemove(key, out _);
    }

    /// <summary>
    /// Attempts to remove and return the value that has the specified <paramref name="key"/> from the <see cref="ObservableConcurrentDictionary{TKey, TValue}"/> and notify observers.
    /// </summary>
    /// <param name="key">
    /// The key of the element to remove.
    /// </param>
    /// <param name="value">
    /// When this method returns, contains the object removed from the <see cref="ObservableConcurrentDictionary{TKey, TValue}"/> or the default value of the object if specified <paramref name="key"/> does not exist.
    /// </param>
    /// <returns>
    /// <c>true</c> if the object was removed successfully; otherwise, <c>false</c>.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="key"/> is a null reference.
    /// </exception>
    /// <exception cref="NotSupportedException">
    /// The <see cref="ObservableConcurrentDictionary{TKey, TValue}"/> is read-only.
    /// </exception>
    public bool TryRemove(TKey key, [MaybeNullWhen(false)] out TValue value)
    {
        value = default;

        ArgumentNullException.ThrowIfNull(key);
        ThrowIfReadOnly(nameof(TryRemove));

        TValue? proxy = default;
        bool exists = RWLock.LockUpgradeableRead(() =>
        {
            if (dictionary.TryGetValue(key, out proxy))
            {
                int index = Items.FindIndex(i => EqualityComparer<TKey>.Default.Equals(i.Key, key));
                return RemoveItem(index, out _);
            }
            return false;
        });
        value = proxy;
        return exists;
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
    /// <exception cref="NotSupportedException">
    /// The <see cref="ObservableConcurrentDictionary{TKey, TValue}"/> is read-only.
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
    /// <exception cref="NotSupportedException">
    /// The <see cref="ObservableConcurrentDictionary{TKey, TValue}"/> is read-only.
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
    /// <exception cref="NotSupportedException">
    /// The <see cref="ObservableConcurrentDictionary{TKey, TValue}"/> is read-only.
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
    /// <exception cref="NotSupportedException">
    /// The <see cref="ObservableConcurrentDictionary{TKey, TValue}"/> is read-only.
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
    /// <exception cref="NotSupportedException">
    /// The <see cref="ObservableConcurrentDictionary{TKey, TValue}"/> is read-only.
    /// </exception>
    public bool TryUpdate(TKey key, Func<TKey, TValue> newValueFactory, TValue comparisonValue)
    {
        return TryUpdate(key, newValueFactory, a => !EqualityComparer<TValue>.Default.Equals(a.oldValue, comparisonValue));
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
    /// <exception cref="NotSupportedException">
    /// The <see cref="ObservableConcurrentDictionary{TKey, TValue}"/> is read-only.
    /// </exception>
    public bool TryUpdate(TKey key, Func<TKey, TValue> newValueFactory, Func<(TKey key, TValue newValue, TValue oldValue), bool> validation)
    {
        ArgumentNullException.ThrowIfNull(key);
        ArgumentNullException.ThrowIfNull(newValueFactory);
        ArgumentNullException.ThrowIfNull(validation);
        ThrowIfReadOnly(nameof(TryUpdate));

        return RWLock.LockUpgradeableRead(() =>
        {
            if (dictionary.TryGetValue(key, out TValue? oldValue))
            {
                TValue newValue = newValueFactory.Invoke(key);
                if (validation.Invoke((key, newValue, oldValue)))
                {
                    int index = Items.FindIndex(i => EqualityComparer<TKey>.Default.Equals(i.Key, key));
                    SetItem(index, new KeyValuePair<TKey, TValue>(key, newValue), out _);
                    return true;
                }
            }
            return false;
        });
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
    /// <exception cref="NotSupportedException">
    /// The <see cref="ObservableConcurrentDictionary{TKey, TValue}"/> is read-only.
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
    /// <exception cref="NotSupportedException">
    /// The <see cref="ObservableConcurrentDictionary{TKey, TValue}"/> is read-only.
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
    /// <exception cref="NotSupportedException">
    /// The <see cref="ObservableConcurrentDictionary{TKey, TValue}"/> is read-only.
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
    /// <exception cref="NotSupportedException">
    /// The <see cref="ObservableConcurrentDictionary{TKey, TValue}"/> is read-only.
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
    /// <exception cref="NotSupportedException">
    /// The <see cref="ObservableConcurrentDictionary{TKey, TValue}"/> is read-only.
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
    /// <exception cref="NotSupportedException">
    /// The <see cref="ObservableConcurrentDictionary{TKey, TValue}"/> is read-only.
    /// </exception>
    public void Update(TKey key, Func<TKey, TValue> newValueFactory, Func<(TKey key, TValue newValue, TValue oldValue), bool> validation)
    {
        TryUpdate(key, newValueFactory, validation);
    }

    private static ArgumentException WrongKeyTypeException(string propertyName, Type? providedType)
    {
        return new ArgumentException("Expected key type is \"" + typeof(TKey).FullName + "\" but dictionary was provided with \"" + (providedType?.FullName ?? "unknown") + "\" key type.", propertyName);
    }

    private static ArgumentException WrongValueTypeException(string propertyName, Type? providedType)
    {
        return new ArgumentException("Expected value type is \"" + typeof(TValue).FullName + "\" but dictionary was provided with \"" + (providedType?.FullName ?? "unknown") + "\" value type.", propertyName);
    }

    #endregion

    #region ObservableConcurrentCollectionsBase<T> Members

    /// <inheritdoc/>
    public override IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
    {
        return new DictionaryEnumerator(this);
    }

    /// <inheritdoc/>
    protected override bool InternalClearItems([MaybeNullWhen(false)] out IEnumerable<KeyValuePair<TKey, TValue>> oldItems)
    {
        if (base.InternalClearItems(out oldItems))
        {
            dictionary.Clear();
            Keys.ExposedClearItemsOperationInvoke(out _);
            Values.ExposedClearItemsOperationInvoke(out _);
            RWLock.InvokeOnLockExit(() =>
            {
                Keys.ExposedClearItemsObservableInvoke();
                Values.ExposedClearItemsObservableInvoke();
            });
            return true;
        }
        return false;
    }

    /// <inheritdoc/>
    protected override bool InternalInsertItems(int index, IEnumerable<KeyValuePair<TKey, TValue>> items, out int lastCount)
    {
        lastCount = Items.Count;
        bool isAllNew = true;
        foreach (KeyValuePair<TKey, TValue> item in items)
        {
            if (dictionary.ContainsKey(item.Key))
            {
                isAllNew = false;
                break;
            }
        }
        if (isAllNew)
        {
            if (base.InternalInsertItems(index, items, out lastCount))
            {
                foreach (KeyValuePair<TKey, TValue> item in items)
                {
                    dictionary.Add(item.Key, item.Value);
                }
                IEnumerable<TKey> insertedKeys = items.Select(i => i.Key);
                IEnumerable<TValue> insertedValues = items.Select(i => i.Value);
                Keys.ExposedInsertItemsOperationInvoke(index, insertedKeys, out _);
                Values.ExposedInsertItemsOperationInvoke(index, insertedValues, out _);
                RWLock.InvokeOnLockExit(() =>
                {
                    Keys.ExposedInsertItemsObservableInvoke(index, insertedKeys);
                    Values.ExposedInsertItemsObservableInvoke(index, insertedValues);
                });
                return true;
            }
        }
        else
        {
            throw new ArgumentException("An element with the same key already exists in the " + GetType().FullName);
        }
        return false;
    }

    /// <inheritdoc/>
    protected override bool InternalMoveItem(int oldIndex, int newIndex, [MaybeNullWhen(false)] out KeyValuePair<TKey, TValue> movedItem)
    {
        if (base.InternalMoveItem(oldIndex, newIndex, out movedItem))
        {
            Keys.ExposedMoveItemOperationInvoke(oldIndex, newIndex, out TKey? movedKey);
            Values.ExposedMoveItemOperationInvoke(oldIndex, newIndex, out TValue? movedValue);
            RWLock.InvokeOnLockExit(() =>
            {
                Keys.ExposedMoveItemObservableInvoke(oldIndex, newIndex, movedKey);
                Values.ExposedMoveItemObservableInvoke(oldIndex, newIndex, movedValue);
            });
            return true;
        }
        return false;
    }

    /// <inheritdoc/>
    protected override bool InternalRemoveItems(int index, int count, [MaybeNullWhen(false)] out IEnumerable<KeyValuePair<TKey, TValue>> oldItems)
    {
        IEnumerable<KeyValuePair<TKey, TValue>>? proxy = default;
        bool isAllExists = true;
        for (int i = 0; i < count; i++)
        {
            var item = Items[index + i];
            if (!dictionary.ContainsKey(item.Key))
            {
                isAllExists = false;
                break;
            }
        }
        if (isAllExists)
        {
            if (base.InternalRemoveItems(index, count, out proxy) && proxy != null)
            {
                foreach (KeyValuePair<TKey, TValue> item in proxy)
                {
                    dictionary.Remove(item.Key);
                }
                Keys.ExposedRemoveItemsOperationInvoke(index, count, out IEnumerable<TKey>? removedKeys);
                Values.ExposedRemoveItemsOperationInvoke(index, count, out IEnumerable<TValue>? removedValues);
                RWLock.InvokeOnLockExit(() =>
                {
                    Keys.ExposedRemoveItemsObservableInvoke(index, removedKeys);
                    Values.ExposedRemoveItemsObservableInvoke(index, removedValues);
                });
                oldItems = proxy;
                return true;
            }
        }
        oldItems = proxy ?? Array.Empty<KeyValuePair<TKey, TValue>>();
        return false;
    }

    /// <inheritdoc/>
    protected override bool InternalSetItem(int index, KeyValuePair<TKey, TValue> item, out KeyValuePair<TKey, TValue> originalItem)
    {
        originalItem = default;
        if (dictionary.ContainsKey(item.Key))
        {
            if (base.InternalSetItem(index, item, out originalItem))
            {
                dictionary[item.Key] = item.Value;
                Keys.ExposedSetItemOperationInvoke(index, item.Key, out TKey? originalKey);
                Values.ExposedSetItemOperationInvoke(index, item.Value, out TValue? originaValue);
                RWLock.InvokeOnLockExit(() =>
                {
                    Keys.ExposedSetItemObservableInvoke(index, item.Key, originalKey);
                    Values.ExposedSetItemObservableInvoke(index, item.Value, originaValue);
                });
                return true;
            }
        }
        return false;
    }

    #endregion

    #region IReadOnlyDictionary<TKey, TValue> Members

    TValue IReadOnlyDictionary<TKey, TValue>.this[TKey key] => this[key];

    IEnumerable<TKey> IReadOnlyDictionary<TKey, TValue>.Keys => Keys;

    IEnumerable<TValue> IReadOnlyDictionary<TKey, TValue>.Values => Values;

    bool IReadOnlyDictionary<TKey, TValue>.ContainsKey(TKey key) => ContainsKey(key);

#pragma warning disable CS8769 // Nullability of reference types in type of parameter doesn't match implemented member (possibly because of nullability attributes).
    bool IReadOnlyDictionary<TKey, TValue>.TryGetValue(TKey key, [MaybeNullWhen(false)] out TValue value) => TryGetValue(key, out value);
#pragma warning restore CS8769 // Nullability of reference types in type of parameter doesn't match implemented member (possibly because of nullability attributes).

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

#pragma warning disable CS8769 // Nullability of reference types in type of parameter doesn't match implemented member (possibly because of nullability attributes).
    bool IDictionary<TKey, TValue>.TryGetValue(TKey key, [MaybeNullWhen(false)] out TValue value) => TryGetValue(key, out value);
#pragma warning restore CS8769 // Nullability of reference types in type of parameter doesn't match implemented member (possibly because of nullability attributes).

    #endregion

    #region IDictionary Members

#pragma warning disable CS8601 // Possible null reference assignment.
#pragma warning disable CS8604 // Possible null reference argument.
    object? IDictionary.this[object? key]
    {
        get
        {
            TKey? tKey;
            if (key is null)
            {
                if (default(TKey) == null)
                {
                    tKey = default;
                }
                else
                {
                    throw ObservableConcurrentDictionary<TKey, TValue>.WrongKeyTypeException(nameof(key), key?.GetType());
                }
            }
            else if (key is TKey k)
            {
                tKey = k;
            }
            else
            {
                throw ObservableConcurrentDictionary<TKey, TValue>.WrongKeyTypeException(nameof(key), key?.GetType());
            }

            return this[tKey];
        }
        set
        {
            TKey? tKey;
            TValue? tValue;
            if (key is null)
            {
                if (default(TKey) == null)
                {
                    tKey = default;
                }
                else
                {
                    throw ObservableConcurrentDictionary<TKey, TValue>.WrongKeyTypeException(nameof(key), key?.GetType());
                }
            }
            else if (key is TKey k)
            {
                tKey = k;
            }
            else
            {
                throw ObservableConcurrentDictionary<TKey, TValue>.WrongKeyTypeException(nameof(key), key?.GetType());
            }

            if (value is null)
            {
                if (default(TValue) == null)
                {
                    tValue = default;
                }
                else
                {
                    throw ObservableConcurrentDictionary<TKey, TValue>.WrongValueTypeException(nameof(value), value?.GetType());
                }
            }
            else if (value is TValue v)
            {
                tValue = v;
            }
            else
            {
                throw ObservableConcurrentDictionary<TKey, TValue>.WrongValueTypeException(nameof(value), value?.GetType());
            }

            this[tKey] = tValue;
        }
    }

    ICollection IDictionary.Keys => Keys;

    ICollection IDictionary.Values => Values;

    bool IDictionary.IsReadOnly => IsReadOnly;

    bool IDictionary.IsFixedSize => false;

    void IDictionary.Add(object? key, object? value)
    {
        TKey? tKey;
        TValue? tValue;
        if (key is null)
        {
            if (default(TKey) == null)
            {
                tKey = default;
            }
            else
            {
                throw ObservableConcurrentDictionary<TKey, TValue>.WrongKeyTypeException(nameof(key), key?.GetType());
            }
        }
        else if (key is TKey k)
        {
            tKey = k;
        }
        else
        {
            throw ObservableConcurrentDictionary<TKey, TValue>.WrongKeyTypeException(nameof(key), key?.GetType());
        }

        if (value is null)
        {
            if (default(TValue) == null)
            {
                tValue = default;
            }
            else
            {
                throw ObservableConcurrentDictionary<TKey, TValue>.WrongValueTypeException(nameof(value), value?.GetType());
            }
        }
        else if (value is TValue v)
        {
            tValue = v;
        }
        else
        {
            throw ObservableConcurrentDictionary<TKey, TValue>.WrongValueTypeException(nameof(value), value?.GetType());
        }

        Add(tKey, tValue);
    }

    void IDictionary.Clear() => Clear();

    bool IDictionary.Contains(object? key)
    {
        if (key is null)
        {
            if (default(TKey) == null)
            {
                return ContainsKey(default);
            }
            else
            {
                throw ObservableConcurrentDictionary<TKey, TValue>.WrongKeyTypeException(nameof(key), key?.GetType());
            }
        }
        else if (key is TKey tKey)
        {
            return ContainsKey(tKey);
        }
        else
        {
            throw ObservableConcurrentDictionary<TKey, TValue>.WrongKeyTypeException(nameof(key), key?.GetType());
        }
    }

    IDictionaryEnumerator IDictionary.GetEnumerator() => (GetEnumerator() as IDictionaryEnumerator) ?? new DictionaryEnumerator(this);

    void IDictionary.Remove(object? key)
    {
        if (key is null)
        {
            if (default(TKey) == null)
            {
                Remove(default(TKey));
            }
            else
            {
                throw ObservableConcurrentDictionary<TKey, TValue>.WrongKeyTypeException(nameof(key), key?.GetType());
            }
        }
        else if (key is TKey tKey)
        {
            Remove(tKey);
        }
        else
        {
            throw ObservableConcurrentDictionary<TKey, TValue>.WrongKeyTypeException(nameof(key), key?.GetType());
        }
    }
#pragma warning restore CS8604 // Possible null reference argument.
#pragma warning restore CS8601 // Possible null reference assignment.

#endregion

    #region Helper Classes

    /// <summary>
    /// Provides the implementations for dictionary observables.
    /// </summary>
    public class DictionaryObservables<T> : ObservableConcurrentCollection<T>
    {
        #region Initializers

        internal DictionaryObservables()
        {
            IsReadOnly = true;
        }

        #endregion

        #region Methods

        internal bool ExposedClearItemsOperationInvoke([MaybeNullWhen(false)] out IEnumerable<T> oldItems)
        {
            return ClearItemsOperationInvoke(out oldItems);
        }

        internal void ExposedClearItemsObservableInvoke()
        {
            ClearItemsObservableInvoke();
        }

        internal bool ExposedInsertItemsOperationInvoke(int index, IEnumerable<T> items, out int lastCount)
        {
            return InsertItemsOperationInvoke(index, items, out lastCount);
        }

        internal void ExposedInsertItemsObservableInvoke(int index, IEnumerable<T>? items)
        {
            InsertItemsObservableInvoke(index, items);
        }

        internal bool ExposedMoveItemOperationInvoke(int oldIndex, int newIndex, [MaybeNullWhen(false)] out T movedItem)
        {
            return MoveItemOperationInvoke(oldIndex, newIndex, out movedItem);
        }

        internal void ExposedMoveItemObservableInvoke(int oldIndex, int newIndex, T? movedItem)
        {
            MoveItemObservableInvoke(oldIndex, newIndex, movedItem);
        }

        internal bool ExposedRemoveItemsOperationInvoke(int index, int count, [MaybeNullWhen(false)] out IEnumerable<T> removedItems)
        {
            return RemoveItemsOperationInvoke(index, count, out removedItems);
        }

        internal void ExposedRemoveItemsObservableInvoke(int index, IEnumerable<T>? removedItems)
        {
            RemoveItemsObservableInvoke(index, removedItems);
        }

        internal bool ExposedSetItemOperationInvoke(int index, T item, out T? originalItem)
        {
            return SetItemOperationInvoke(index, item, out originalItem);
        }

        internal void ExposedSetItemObservableInvoke(int index, T item, T? originalItem)
        {
            SetItemObservableInvoke(index, item, originalItem);
        }

        #endregion
    }

    /// <summary>
    /// Provides the keys of the <see cref="ObservableConcurrentDictionary{TKey, TValue}"/>.
    /// </summary>
    public class DictionaryKeys : DictionaryObservables<TKey>
    {
        #region Initializers

        internal DictionaryKeys()
        {
            IsReadOnly = true;
        }

        #endregion
    }

    /// <summary>
    /// Provides the values of the <see cref="ObservableConcurrentDictionary{TKey, TValue}"/>.
    /// </summary>
    public class DictionaryValues : DictionaryObservables<TValue>
    {
        #region Initializers

        internal DictionaryValues()
        {
            IsReadOnly = true;
        }

        #endregion
    }

    /// <summary>
    /// Provides the default enumerator of the <see cref="ObservableConcurrentDictionary{TKey, TValue}"/>.
    /// </summary>
    public struct DictionaryEnumerator : IEnumerator<KeyValuePair<TKey, TValue>>, IDictionaryEnumerator
    {
        #region Properties

        /// <inheritdoc/>
        public KeyValuePair<TKey, TValue> Current => enumerator.Current;

        /// <inheritdoc/>
        public object Key => enumerator.Current.Key;

        /// <inheritdoc/>
        public object? Value => enumerator.Current.Value;

        /// <inheritdoc/>
        public DictionaryEntry Entry => new(Key, Value);

        private readonly IEnumerator<KeyValuePair<TKey, TValue>> enumerator;

        #endregion

        #region Initializers

        internal DictionaryEnumerator(ObservableConcurrentDictionary<TKey, TValue> dictionary)
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

        #region IEnumerator Members

        object IEnumerator.Current => enumerator.Current;

        #endregion
    }

    #endregion
}
