using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;

namespace ObservableHelpers
{
    /// <summary>
    /// Provides a thread-safe observable collection for use with data binding.
    /// </summary>
    /// <typeparam name="T">
    /// Specifies the type of the items in this collection.
    /// </typeparam>
    public class ObservableCollection<T> :
        ObservableCollectionFilter<T>,
        IList<T>,
        IList
    {
        #region Properties

        /// <inheritdoc/>
        public new T this[int index]
        {
            get => InternalObservableCollection[index];
            set => InternalObservableCollection[index] = value;
        }

        #endregion

        #region Initializers

        /// <inheritdoc/>
        public ObservableCollection()
            : base()
        {

        }

        /// <inheritdoc/>
        public ObservableCollection(IEnumerable<T> collection)
            : base(collection)
        {
        
        }

        #endregion

        #region Methods

        /// <inheritdoc/>
        public void Add(T item) => InternalObservableCollection.Add(item);

        /// <inheritdoc/>
        public void Clear() => InternalObservableCollection.Clear();

        /// <inheritdoc/>
        public void Insert(int index, T item) => InternalObservableCollection.Insert(index, item);

        /// <inheritdoc/>
        public bool Remove(T item) => InternalObservableCollection.Remove(item);

        /// <inheritdoc/>
        public void RemoveAt(int index) => InternalObservableCollection.RemoveAt(index);

        /// <inheritdoc/>
        public override bool SetNull()
        {
            bool isNull = IsNull();
            Clear();
            return isNull;
        }

        #endregion

        #region IList Members

        /// <inheritdoc/>
        object IList.this[int index]
        {
            get => (InternalObservableCollection as IList)[index];
            set => (InternalObservableCollection as IList)[index] = value;
        }

        /// <inheritdoc/>
        bool IList.IsReadOnly => (InternalObservableCollection as IList).IsReadOnly;

        /// <inheritdoc/>
        bool IList.IsFixedSize => (InternalObservableCollection as IList).IsFixedSize;

        /// <inheritdoc/>
        int IList.Add(object value) => (InternalObservableCollection as IList).Add(value);

        /// <inheritdoc/>
        bool IList.Contains(object value) => (InternalObservableCollection as IList).Contains(value);

        /// <inheritdoc/>
        int IList.IndexOf(object value) => (InternalObservableCollection as IList).IndexOf(value);

        /// <inheritdoc/>
        void IList.Insert(int index, object value) => (InternalObservableCollection as IList).Insert(index, value);

        /// <inheritdoc/>
        void IList.Remove(object value) => (InternalObservableCollection as IList).Remove(value);

        #endregion

        #region IEnumerable Members

        /// <inheritdoc/>
        IEnumerator IEnumerable.GetEnumerator() => (InternalObservableCollection as IEnumerable).GetEnumerator();

        #endregion

        #region ICollection Members

        /// <inheritdoc/>
        bool ICollection<T>.IsReadOnly => (InternalObservableCollection as ICollection<T>).IsReadOnly;

        /// <inheritdoc/>
        bool ICollection.IsSynchronized => (InternalObservableCollection as ICollection).IsSynchronized;

        /// <inheritdoc/>
        object ICollection.SyncRoot => (InternalObservableCollection as ICollection).SyncRoot;

        /// <inheritdoc/>
        void ICollection.CopyTo(Array array, int index) => (InternalObservableCollection as IList).CopyTo(array, index);

        #endregion
    }
}
