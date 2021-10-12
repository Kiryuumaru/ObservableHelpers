using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Threading;

namespace ObservableHelpers
{
    /// <summary>
    /// Provides a thread-safe observable collection used for data binding.
    /// </summary>
    /// <typeparam name="T">
    /// Specifies the type of the items in this collection.
    /// </typeparam>
    public class ObservableCollection1<T> :
        ObservableCollectionSyncContext,
        IReadOnlyList<T>,
        IList<T>,
        IList,
        INotifyCollectionChanged,
        INotifyPropertyChanged
    {
        #region Properties

        /// <summary>
        /// Gets or sets the element at the specified index.
        /// </summary>
        /// <param name="index">
        /// The zero-based index of the element to get or set.
        /// </param>
        /// <returns>
        /// The element at the specified index.
        /// </returns>
        /// <exception cref="ArgumentOutOfRangeException">
        /// <paramref name="index"/> is not a valid index in the <see cref="ObservableCollection1{T}"/>.
        /// </exception>
        /// <exception cref="NotSupportedException">
        /// The property is set and the <see cref="ObservableCollection1{T}"/> is read-only.
        /// </exception>
        public T this[int index]
        {
            get
            {
                try
                {
                    rwLock.EnterUpgradeableReadLock();
                    return Items[index];
                }
                catch
                {
                    throw;
                }
                finally
                {
                    rwLock.ExitUpgradeableReadLock();
                }
            }
            set
            {
                SetItem(index, value);
            }
        }

        /// <summary>
        /// Gets the number of elements contained in the <see cref="ObservableCollection1{T}"/> collection.
        /// </summary>
        public int Count
        {
            get
            {
                try
                {
                    rwLock.EnterUpgradeableReadLock();
                    return Items.Count;
                }
                catch
                {
                    throw;
                }
                finally
                {
                    rwLock.ExitUpgradeableReadLock();
                }
            }
        }

        /// <summary>
        /// Gets a <see cref="IList{T}"/> wrapper around the <see cref="ObservableCollection1{T}"/>.
        /// </summary>
        protected IList<T> Items { get; }

        // This must agree with Binding.IndexerName.  It is declared separately
        // here so as to avoid a dependency on PresentationFramework.dll.
        private const string IndexerName = "Item[]";

        private readonly ReaderWriterLockSlim rwLock = new ReaderWriterLockSlim();

        #endregion

        #region Initializers

        /// <summary>
        /// Initializes a new instance of the <see cref="ObservableCollection1{T}"/> class that contains empty elements and has sufficient capacity to accommodate the number of elements copied.
        /// </summary>
        public ObservableCollection1()
            : this(Array.Empty<T>())
        {

        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ObservableCollection1{T}"/> class that contains elements copied from the specified collection and has sufficient capacity to accommodate the number of elements copied.
        /// </summary>
        /// <param name="enumerable">
        /// The collection whose elements are copied to the new list.
        /// </param>
        /// <remarks>
        /// The elements are copied onto the <see cref="ObservableCollection1{T}"/> in the
        /// same order they are read by the enumerator of the collection.
        /// </remarks>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="enumerable"/> is a null reference.
        /// </exception>
        public ObservableCollection1(IEnumerable<T> enumerable)
        {
            if (enumerable == null)
            {
                throw new ArgumentNullException(nameof(enumerable));
            }

            Items = new List<T>(enumerable);
        }

        #endregion

        #region Members

        /// <summary>
        /// Adds an item to the <see cref="ObservableCollection1{T}"/>.
        /// </summary>
        /// <param name="item">
        /// The item to add to the <see cref="ObservableCollection1{T}"/>.</param>
        /// <exception cref="NotSupportedException">
        /// The <see cref="ObservableCollection1{T}"/> is read-only.
        /// </exception>
        public void Add(T item)
        {
            try
            {
                rwLock.EnterUpgradeableReadLock();
                int index = Items.Count;
                InsertItem(index, item);
            }
            catch
            {
                throw;
            }
            finally
            {
                rwLock.ExitUpgradeableReadLock();
            }
        }

        /// <summary>
        /// Removes all items from the <see cref="ObservableCollection1{T}"/>.
        /// </summary>
        /// <exception cref="NotSupportedException">
        /// The <see cref="ObservableCollection1{T}"/> is read-only.
        /// </exception>
        public void Clear()
        {
            ClearItems();
        }

        /// <summary>
        /// Determines whether the <see cref="ObservableCollection1{T}"/> contains a specific <paramref name="item"/>.
        /// </summary>
        /// <param name="item">
        /// The item to locate in the <see cref="ObservableCollection1{T}"/>.
        /// </param>
        /// <returns>
        /// <c>true</c> if item is found in the <see cref="ObservableCollection1{T}"/>; otherwise, <c>false</c>.
        /// </returns>
        public bool Contains(T item)
        {
            try
            {
                rwLock.EnterUpgradeableReadLock();
                return Items.Contains(item);
            }
            catch
            {
                throw;
            }
            finally
            {
                rwLock.ExitUpgradeableReadLock();
            }
        }

        /// <summary>
        /// Copies the elements of the <see cref="ObservableCollection1{T}"/> to an <see cref="Array"/>, starting at a particular <see cref="Array"/> index.
        /// </summary>
        /// <param name="array">
        /// The one-dimensional <see cref="Array"/> that is the destination of the elements copied from <see cref="ObservableCollection1{T}"/>. The <see cref="Array"/> must have zero-based indexing.
        /// </param>
        /// <param name="arrayIndex">
        /// The zero-based index in array at which copying begins.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="array"/> is a null reference.
        /// </exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// <paramref name="arrayIndex"/> is less than 0.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// The number of elements in the source <see cref="ObservableCollection1{T}"/> is greater than the available space from <paramref name="arrayIndex"/> to the end of the destination <paramref name="array"/>.
        /// </exception>
        public void CopyTo(T[] array, int arrayIndex)
        {
            try
            {
                rwLock.EnterUpgradeableReadLock();
                Items.CopyTo(array, arrayIndex);
            }
            catch
            {
                throw;
            }
            finally
            {
                rwLock.ExitUpgradeableReadLock();
            }
        }

        /// <summary>
        /// Returns an enumerator that iterates through the collection.
        /// </summary>
        /// <returns>
        /// An enumerator that can be used to iterate through the collection.
        /// </returns>
        public IEnumerator<T> GetEnumerator()
        {
            try
            {
                rwLock.EnterUpgradeableReadLock();
                return Items.GetEnumerator();
            }
            catch
            {
                throw;
            }
            finally
            {
                rwLock.ExitUpgradeableReadLock();
            }
        }

        /// <summary>
        /// Determines the index of a specific item in the <see cref="ObservableCollection1{T}"/>.
        /// </summary>
        /// <param name="value">
        /// The object to locate in the <see cref="ObservableCollection1{T}"/>.
        /// </param>
        /// <returns>
        /// The index of item if found in the list; otherwise, -1.
        /// </returns>
        public int IndexOf(T value)
        {
            try
            {
                rwLock.EnterUpgradeableReadLock();
                return Items.IndexOf(value);
            }
            catch
            {
                throw;
            }
            finally
            {
                rwLock.ExitUpgradeableReadLock();
            }
        }

        /// <summary>
        /// Inserts an item to the <see cref="ObservableCollection1{T}"/> at the specified index.
        /// </summary>
        /// <param name="index">
        /// The zero-based index at which item should be inserted.
        /// </param>
        /// <param name="item">
        /// The item to insert into the <see cref="ObservableCollection1{T}"/>.
        /// </param>
        /// <exception cref="ArgumentOutOfRangeException">
        /// <paramref name="index"/> is not a valid index in the <see cref="ObservableCollection1{T}"/>.
        /// </exception>
        /// <exception cref="NotSupportedException">
        /// The <see cref="ObservableCollection1{T}"/> is read-only.
        /// </exception>
        public void Insert(int index, T item)
        {
            InsertItem(index, item);
        }

        /// <summary>
        /// Move item at oldIndex to newIndex.
        /// </summary>
        public void Move(int oldIndex, int newIndex)
        {
            MoveItem(oldIndex, newIndex);
        }

        /// <summary>
        /// Removes the first occurrence of a specific object from the <see cref="ObservableCollection1{T}"/>.
        /// </summary>
        /// <param name="item">
        /// The object to remove from the <see cref="ObservableCollection1{T}"/>.
        /// </param>
        /// <returns>
        /// <c>true</c> if item was successfully removed from the <see cref="ObservableCollection1{T}"/>; otherwise, <c>false</c>. This method also returns false if item is not found in the <see cref="ObservableCollection1{T}"/>.
        /// </returns>
        /// <exception cref="NotSupportedException">
        /// The <see cref="ObservableCollection1{T}"/> is read-only.
        /// </exception>
        public bool Remove(T item)
        {
            try
            {
                rwLock.EnterUpgradeableReadLock();
                int index = Items.IndexOf(item);
                if (index < 0)
                {
                    return false;
                }
                RemoveItem(index);
            }
            catch
            {
                throw;
            }
            finally
            {
                rwLock.ExitUpgradeableReadLock();
            }
            return true;
        }

        /// <summary>
        /// Removes the <see cref="ObservableCollection1{T}"/> item at the specified index.
        /// </summary>
        /// <param name="index">
        /// The zero-based index of the item to remove.
        /// </param>
        /// <exception cref="ArgumentOutOfRangeException">
        /// <paramref name="index"/> is not a valid index in the <see cref="ObservableCollection1{T}"/>.
        /// </exception>
        /// <exception cref="NotSupportedException">
        /// The <see cref="ObservableCollection1{T}"/> is read-only.
        /// </exception>
        public void RemoveAt(int index)
        {
            RemoveItem(index);
        }

        /// <summary>
        /// Removes all elements from the <see cref="ObservableCollection1{T}"/>.
        /// </summary>
        protected virtual void ClearItems()
        {
            try
            {
                rwLock.EnterWriteLock();

                Items.Clear();

                OnPropertyChanged(nameof(Count));
                OnPropertyChanged(IndexerName);
                OnCollectionReset();
            }
            catch
            {
                throw;
            }
            finally
            {
                rwLock.ExitWriteLock();
            }

        }

        /// <summary>
        /// Inserts an element into the <see cref="ObservableCollection1{T}"/> at the specified <paramref name="index"/>.
        /// </summary>
        /// <param name="index">
        /// The zero-based index at which item should be inserted.
        /// </param>
        /// <param name="item">
        /// The object to insert. The value can be null for reference types.
        /// </param>
        /// <exception cref="ArgumentOutOfRangeException">
        /// <paramref name="index"/> is less than zero. -or- index is greater than <see cref="ObservableCollection1{T}.Count"/>.
        /// </exception>
        protected virtual void InsertItem(int index, T item)
        {
            try
            {
                rwLock.EnterWriteLock();

                if (index < 0 || index > Items.Count)
                {
                    throw new ArgumentOutOfRangeException(nameof(index));
                }

                Items.Insert(index, item);

                OnPropertyChanged(nameof(Count));
                OnPropertyChanged(IndexerName);
                OnCollectionAdd(item, index);
            }
            catch
            {
                throw;
            }
            finally
            {
                rwLock.ExitWriteLock();
            }
        }

        /// <summary>
        /// Called by base class ObservableCollection&lt;T&gt; when an item is to be moved within the list;
        /// raises a CollectionChanged event to any listeners.
        /// </summary>
        /// <param name="oldIndex">
        /// </param>
        /// <param name="newIndex">
        /// </param>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Either or both <paramref name="oldIndex"/> or <paramref name="newIndex"/> are less than zero. -or- is greater than <see cref="ObservableCollection1{T}.Count"/>.
        /// </exception>
        protected virtual void MoveItem(int oldIndex, int newIndex)
        {
            try
            {
                rwLock.EnterWriteLock();

                if (oldIndex < 0 || oldIndex >= Items.Count)
                {
                    throw new ArgumentOutOfRangeException(nameof(oldIndex));
                }

                if (newIndex < 0 || newIndex >= Items.Count)
                {
                    throw new ArgumentOutOfRangeException(nameof(newIndex));
                }

                T movedItem = this[oldIndex];

                Items.RemoveAt(oldIndex);
                Items.Insert(newIndex, movedItem);

                OnPropertyChanged(IndexerName);
                OnCollectionMove(movedItem, newIndex, oldIndex);
            }
            catch
            {
                throw;
            }
            finally
            {
                rwLock.ExitWriteLock();
            }
        }

        /// <summary>
        /// Removes the element at the specified <paramref name="index"/> of the <see cref="ObservableCollection1{T}"/>.
        /// </summary>
        /// <param name="index">
        /// The zero-based index of the element to remove.
        /// </param>
        /// <exception cref="ArgumentOutOfRangeException">
        /// <paramref name="index"/> is less than zero. -or- is greater than <see cref="ObservableCollection1{T}.Count"/>.
        /// </exception>
        protected virtual void RemoveItem(int index)
        {
            try
            {
                rwLock.EnterWriteLock();

                if (index < 0 || index >= Items.Count)
                {
                    throw new ArgumentOutOfRangeException(nameof(index));
                }

                T removedItem = this[index];

                Items.RemoveAt(index);

                OnPropertyChanged(nameof(Count));
                OnPropertyChanged(IndexerName);
                OnCollectionRemove(removedItem, index);
            }
            catch
            {
                throw;
            }
            finally
            {
                rwLock.ExitWriteLock();
            }
        }

        /// <summary>
        /// Replaces the element at the specified index.
        /// </summary>
        /// <param name="index">
        /// The zero-based index of the element to replace.
        /// </param>
        /// <param name="item">
        /// The new value for the element at the specified <paramref name="index"/>. The value can be <c>null</c> for reference types.
        /// </param>
        /// <exception cref="ArgumentOutOfRangeException">
        /// <paramref name="index"/> is not a valid index in the <see cref="ObservableCollection1{T}"/>.
        /// </exception>
        protected virtual void SetItem(int index, T item)
        {
            try
            {
                rwLock.EnterWriteLock();

                if (index < 0 || index >= Items.Count)
                {
                    throw new ArgumentOutOfRangeException(nameof(index));
                }

                T originalItem = this[index];

                Items[index] = item;

                OnPropertyChanged(IndexerName);
                OnCollectionReplace(originalItem, item, index);
            }
            catch
            {
                throw;
            }
            finally
            {
                rwLock.ExitWriteLock();
            }
        }

        private ArgumentException ThrowWrongValueType(string propertyName, Type providedType)
        {
            return new ArgumentException("Expected value type is " + typeof(T).FullName + " but collection was provided with " + providedType.FullName + ".", propertyName);
        }

        #endregion

        #region ObservableSyncContext Members

        /// <inheritdoc/>
        public override bool IsNull()
        {
            return Count == 0;
        }

        /// <inheritdoc/>
        public override bool SetNull()
        {
            bool isNull = Count == 0;
            ClearItems();
            return !isNull;
        }

        #endregion

        #region IReadOnlyList<T> Members

        T IReadOnlyList<T>.this[int index] => this[index];

        #endregion

        #region IReadOnlyCollection<T> Members

        int IReadOnlyCollection<T>.Count => Count;

        #endregion

        #region IList<T> Members

        T IList<T>.this[int index] { get => this[index]; set => this[index] = value; }

        int IList<T>.IndexOf(T item) => IndexOf(item);

        void IList<T>.Insert(int index, T item) => Insert(index, item);

        void IList<T>.RemoveAt(int index) => RemoveAt(index);

        #endregion

        #region IList Members

        object IList.this[int index]
        {
            get => this[index];
            set
            {
                if (value is null)
                {
                    if (default(T) == null)
                    {
                        this[index] = default;
                    }
                    else
                    {
                        throw ThrowWrongValueType(nameof(value), value?.GetType());
                    }
                }
                else if (value is T item)
                {
                    this[index] = item;
                }
                else
                {
                    throw ThrowWrongValueType(nameof(value), value?.GetType());
                }
            }
        }

        bool IList.IsReadOnly => false;

        bool IList.IsFixedSize => false;

        int IList.Add(object value)
        {
            int oldCount = Count;
            if (value is null)
            {
                if (default(T) == null)
                {
                    Add(default);
                }
                else
                {
                    throw ThrowWrongValueType(nameof(value), value?.GetType());
                }
            }
            else if (value is T item)
            {
                Add(item);
            }
            else
            {
                throw ThrowWrongValueType(nameof(value), value?.GetType());
            }
            int newCount = Count;
            return oldCount != newCount ? newCount : -1;
        }

        void IList.Clear() => Clear();

        bool IList.Contains(object value)
        {
            if (value is null)
            {
                if (default(T) == null)
                {
                    return Contains(default);
                }
                else
                {
                    throw ThrowWrongValueType(nameof(value), value?.GetType());
                }
            }
            else if (value is T item)
            {
                return Contains(item);
            }
            else
            {
                throw ThrowWrongValueType(nameof(value), value?.GetType());
            }
        }

        void ICollection.CopyTo(Array array, int index)
        {
            if (array is null)
            {
                CopyTo(default, index);
            }
            else if (array is T[] items)
            {
                CopyTo(items, index);
            }
            else
            {
                throw ThrowWrongValueType(nameof(array), array?.GetType());
            }
        }

        int IList.IndexOf(object value)
        {
            if (value is null)
            {
                if (default(T) == null)
                {
                    return IndexOf(default);
                }
                else
                {
                    throw ThrowWrongValueType(nameof(value), value?.GetType());
                }
            }
            else if (value is T item)
            {
                return IndexOf(item);
            }
            else
            {
                throw ThrowWrongValueType(nameof(value), value?.GetType());
            }
        }

        void IList.Insert(int index, object value)
        {
            if (value is null)
            {
                if (default(T) == null)
                {
                    Insert(index, default);
                }
                else
                {
                    throw ThrowWrongValueType(nameof(value), value?.GetType());
                }
            }
            else if (value is T item)
            {
                Insert(index, item);
            }
            else
            {
                throw ThrowWrongValueType(nameof(value), value?.GetType());
            }
        }

        void IList.Remove(object value)
        {
            if (value is null)
            {
                if (default(T) == null)
                {
                    Remove(default);
                }
                else
                {
                    throw ThrowWrongValueType(nameof(value), value?.GetType());
                }
            }
            else if (value is T item)
            {
                Remove(item);
            }
            else
            {
                throw ThrowWrongValueType(nameof(value), value?.GetType());
            }
        }

        void IList.RemoveAt(int index) => RemoveAt(index);

        #endregion

        #region ICollection<T> Members

        int ICollection<T>.Count => Count;

        bool ICollection<T>.IsReadOnly => false;

        void ICollection<T>.Add(T item) => Add(item);

        void ICollection<T>.Clear() => Clear();

        bool ICollection<T>.Contains(T item) => Contains(item);

        void ICollection<T>.CopyTo(T[] array, int arrayIndex) => CopyTo(array, arrayIndex);

        bool ICollection<T>.Remove(T item) => Remove(item);

        #endregion

        #region ICollection Members

        private object syncRoot;

        int ICollection.Count => Count;

        bool ICollection.IsSynchronized => true;

        object ICollection.SyncRoot
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

        #endregion

        #region IEnumerable<T> Members

        IEnumerator<T> IEnumerable<T>.GetEnumerator() => GetEnumerator();

        #endregion

        #region IEnumerable Members

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        #endregion
    }
}
