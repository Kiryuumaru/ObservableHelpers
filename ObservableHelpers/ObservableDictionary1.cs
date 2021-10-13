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

        public TValue this[TKey key]
        {
            get => throw new NotImplementedException();
            set => throw new NotImplementedException();
        }

        public int Count => throw new NotImplementedException();

        public DictionaryKeys Keys { get; }

        public DictionaryValues<TKey, TValue> Values { get; }

        /// <summary>
        /// Gets a value indicating whether the <see cref="ObservableDictionary1{TKey, TValue}"/> is read-only.
        /// </summary>
        public bool IsReadOnly { get; private set; }

        public object SyncRoot
        {
            get
            {
                if (syncRoot == null)
                {
                    Interlocked.CompareExchange(ref syncRoot, new object(), null);
                }
                return syncRoot;
            }
        }

        /// <summary>
        /// Gets a <see cref="IDictionary{TKey, TValue}"/> wrapper around the <see cref="ObservableDictionary1{TKey, TValue}"/>.
        /// </summary>
        protected IDictionary<TKey, TValue> Dictionary { get; private set; }

        // This must agree with Binding.IndexerName. It is declared separately
        // here so as to avoid a dependency on PresentationFramework.dll.
        private const string IndexerName = "Item[]";

        private readonly ReaderWriterLockSlim rwLock = new ReaderWriterLockSlim();

        private object syncRoot;

        #endregion

        #region Initializers

        public ObservableDictionary1()
        {
            Dictionary = new Dictionary<TKey, TValue>();
        }

        public ObservableDictionary1(IEnumerable<KeyValuePair<TKey, TValue>> items)
        {
            Dictionary = new Dictionary<TKey, TValue>();
            Populate(items);
        }

        public ObservableDictionary1(IEqualityComparer<TKey> comparer)
        {
            Dictionary = new Dictionary<TKey, TValue>(comparer);
        }

        public ObservableDictionary1(IEnumerable<KeyValuePair<TKey, TValue>> items, IEqualityComparer<TKey> comparer)
        {
            Dictionary = new Dictionary<TKey, TValue>(comparer);
            Populate(items);
        }

        private void Populate(IEnumerable<KeyValuePair<TKey, TValue>> items)
        {
            foreach (KeyValuePair<TKey, TValue> pair in items)
            {
                Dictionary.Add(pair);
            }
        }

        #endregion

        #region Methods

        public void Add(TKey key, TValue value)
        {
            throw new NotImplementedException();
        }

        public bool ContainsKey(TKey key)
        {
            throw new NotImplementedException();
        }

        public DictionaryEnumerator<TKey, TValue> GetEnumerator()
        {
            throw new NotImplementedException();
        }

        public bool Remove(TKey key)
        {
            throw new NotImplementedException();
        }

        public bool TryGetValue(TKey key, out TValue value)
        {
            throw new NotImplementedException();
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

        /// <inheritdoc/>
        public override void Add(KeyValuePair<TKey, TValue> item)
        {
            Add(item.Key, item.Value);
        }

        /// <inheritdoc/>
        public override bool Remove(KeyValuePair<TKey, TValue> item)
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
        /// Contains the keys of the <see cref="ObservableDictionary1{TKey, TValue}"/>.
        /// </summary>
        public class DictionaryKeys : ObservableCollectionBase<TKey, IList<TKey>>
        {
            #region Properties

            private readonly ObservableDictionary1<TKey, TValue> dictionary;

            #endregion

            #region Initializers

            internal DictionaryKeys(ObservableDictionary1<TKey, TValue> dictionary)
                : base(() => new List<TKey>(dictionary.Items.Select(i => i.Key)))
            {
                this.dictionary = dictionary;
            }

            #endregion

            #region ObservableCollectionBase<TKey, TCollectionWrapper> Members

            /// <inheritdoc/>
            public override void Add(TKey item) => throw ReadOnlyException(nameof(Add));

            /// <inheritdoc/>
            public override bool Remove(TKey item) => throw ReadOnlyException(nameof(Remove));

            #endregion
        }

        public class DictionaryValues : ObservableCollectionBase<TValue, IList<TValue>>
        {
            #region Properties

            private readonly ObservableDictionary1<TKey, TValue> dictionary;

            #endregion

            #region Initializers

            internal DictionaryKeys(ObservableDictionary1<TKey, TValue> dictionary)
                : base(() => new List<TValue>(dictionary.Items.Select(i => i.Value)))
            {
                this.dictionary = dictionary;
            }

            #endregion

            #region ObservableCollectionBase<TKey, TCollectionWrapper> Members

            /// <inheritdoc/>
            public override void Add(TValue item) => throw ReadOnlyException(nameof(Add));

            /// <inheritdoc/>
            public override bool Remove(TValue item) => throw ReadOnlyException(nameof(Remove));

            #endregion
        }

        public class DictionaryEnumerator<TKey, TValue> : IEnumerator<KeyValuePair<TKey, TValue>>, IDictionaryEnumerator
        {
            #region Properties

            private readonly ObservableDictionary1<TKey, TValue> dictionary;

            #endregion

            #region Initializers

            internal DictionaryEnumerator(ObservableDictionary1<TKey, TValue> dictionary)
            {
                this.dictionary = dictionary;
            }

            #endregion
        }

        #endregion
    }
}
