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
    public class ObservableDictionary2<TKey, TValue> :
        ObservableCollection<KeyValuePair<TKey, TValue>>,
        IDictionary<TKey, TValue>
    {
        public TValue this[TKey key]
        {
            get => this[key];
            set => this[key] = value;
        }

        public ICollection<TKey> Keys => AsCollection().Select(i => i.Key).ToList();

        public ICollection<TValue> Values => AsCollection().Select(i => i.Value).ToList();

        public void Add(TKey key, TValue value)
        {

        }

        public bool ContainsKey(TKey key)
        {
            throw new Exception();
        }

        public bool Remove(TKey key)
        {
            throw new Exception();
        }

        public bool TryGetValue(TKey key, out TValue value)
        {
            throw new Exception();
        }

        private ObservableCollection<KeyValuePair<TKey, TValue>> AsCollection()
        {
            return this;
        }
    }
}
