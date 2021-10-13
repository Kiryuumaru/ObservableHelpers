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
    public class ObservableCollection<T> :
        ObservableCollectionBase<T, IList<T>>,
        IReadOnlyList<T>,
        IList<T>,
        IList
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
        /// <paramref name="index"/> is not a valid index in the <see cref="ObservableCollection{T}"/>.
        /// </exception>
        /// <exception cref="NotSupportedException">
        /// The property is set and the <see cref="ObservableCollection{T}"/> is read-only.
        /// </exception>
        public T this[int index]
        {
            get
            {
                try
                {
                    RWLock.EnterUpgradeableReadLock();
                    return Items[index];
                }
                catch
                {
                    throw;
                }
                finally
                {
                    RWLock.ExitUpgradeableReadLock();
                }
            }
            set
            {
                if (IsReadOnly)
                {
                    throw ReadOnlyException(nameof(IndexerName));
                }

                SetItem(index, value);
            }
        }

        #endregion

        #region Initializers

        /// <summary>
        /// Initializes a new instance of the <see cref="ObservableCollection{T}"/> class that contains empty elements and has sufficient capacity to accommodate the number of elements copied.
        /// </summary>
        public ObservableCollection()
            : base(() => new List<T>())
        {

        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ObservableCollection{T}"/> class that contains elements copied from the specified collection and has sufficient capacity to accommodate the number of elements copied.
        /// </summary>
        /// <param name="enumerable">
        /// The collection whose elements are copied to the new list.
        /// </param>
        /// <remarks>
        /// The elements are copied onto the <see cref="ObservableCollection{T}"/> in the
        /// same order they are read by the enumerator of the collection.
        /// </remarks>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="enumerable"/> is a null reference.
        /// </exception>
        public ObservableCollection(IEnumerable<T> enumerable)
            : base(() =>
            {
                if (enumerable == null)
                {
                    throw new ArgumentNullException(nameof(enumerable));
                }
                return new List<T>(enumerable);
            })
        {

        }

        #endregion

        #region Members

        /// <summary>
        /// Adds an item range to the <see cref="ObservableCollection{T}"/>.
        /// </summary>
        /// <param name="items">
        /// The items to add to the <see cref="ObservableCollection{T}"/>.
        /// </param>
        /// <exception cref="NotSupportedException">
        /// The <see cref="ObservableCollection{T}"/> is read-only.
        /// </exception>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="items"/> is a null reference.
        /// </exception>
        public void AddRange(IEnumerable<T> items)
        {
            if (IsReadOnly)
            {
                throw ReadOnlyException(nameof(AddRange));
            }

            if (items == null)
            {
                throw new ArgumentNullException(nameof(items));
            }

            AddRange(items.ToArray());
        }

        /// <summary>
        /// Adds an item range to the <see cref="ObservableCollection{T}"/>.
        /// </summary>
        /// <param name="items">
        /// The items to add to the <see cref="ObservableCollection{T}"/>.
        /// </param>
        /// <exception cref="NotSupportedException">
        /// The <see cref="ObservableCollection{T}"/> is read-only.
        /// </exception>
        public void AddRange(params T[] items)
        {
            if (IsReadOnly)
            {
                throw ReadOnlyException(nameof(AddRange));
            }

            if (items?.Length == 0)
            {
                return;
            }

            try
            {
                RWLock.EnterUpgradeableReadLock();
                int index = Items.Count;
                InsertItem(index, items);
            }
            catch
            {
                throw;
            }
            finally
            {
                RWLock.ExitUpgradeableReadLock();
            }
        }

        /// <summary>
        /// Determines the index of a specific item in the <see cref="ObservableCollection{T}"/>.
        /// </summary>
        /// <param name="value">
        /// The object to locate in the <see cref="ObservableCollection{T}"/>.
        /// </param>
        /// <returns>
        /// The index of item if found in the list; otherwise, -1.
        /// </returns>
        public int IndexOf(T value)
        {
            try
            {
                RWLock.EnterUpgradeableReadLock();
                return Items.IndexOf(value);
            }
            catch
            {
                throw;
            }
            finally
            {
                RWLock.ExitUpgradeableReadLock();
            }
        }

        /// <summary>
        /// Inserts an item to the <see cref="ObservableCollection{T}"/> at the specified index.
        /// </summary>
        /// <param name="index">
        /// The zero-based index at which item should be inserted.
        /// </param>
        /// <param name="item">
        /// The item to insert into the <see cref="ObservableCollection{T}"/>.
        /// </param>
        /// <exception cref="ArgumentOutOfRangeException">
        /// <paramref name="index"/> is not a valid index in the <see cref="ObservableCollection{T}"/>.
        /// </exception>
        /// <exception cref="NotSupportedException">
        /// The <see cref="ObservableCollection{T}"/> is read-only.
        /// </exception>
        public void Insert(int index, T item)
        {
            if (IsReadOnly)
            {
                throw ReadOnlyException(nameof(Insert));
            }

            InsertItem(index, item);
        }

        /// <summary>
        /// Inserts an item range to the <see cref="ObservableCollection{T}"/> at the specified index.
        /// </summary>
        /// <param name="index">
        /// The zero-based index at which item should be inserted.
        /// </param>
        /// <param name="items">
        /// The item to insert into the <see cref="ObservableCollection{T}"/>.
        /// </param>
        /// <exception cref="ArgumentOutOfRangeException">
        /// <paramref name="index"/> is not a valid index in the <see cref="ObservableCollection{T}"/>.
        /// </exception>
        /// <exception cref="NotSupportedException">
        /// The <see cref="ObservableCollection{T}"/> is read-only.
        /// </exception>
        public void InsertRange(int index, IEnumerable<T> items)
        {
            if (items == null)
            {
                throw new ArgumentNullException(nameof(items));
            }

            InsertRange(index, items.ToArray());
        }

        /// <summary>
        /// Inserts an item range to the <see cref="ObservableCollection{T}"/> at the specified index.
        /// </summary>
        /// <param name="index">
        /// The zero-based index at which item should be inserted.
        /// </param>
        /// <param name="items">
        /// The item to insert into the <see cref="ObservableCollection{T}"/>.
        /// </param>
        /// <exception cref="ArgumentOutOfRangeException">
        /// <paramref name="index"/> is not a valid index in the <see cref="ObservableCollection{T}"/>.
        /// </exception>
        /// <exception cref="NotSupportedException">
        /// The <see cref="ObservableCollection{T}"/> is read-only.
        /// </exception>
        public void InsertRange(int index, params T[] items)
        {
            if (IsReadOnly)
            {
                throw ReadOnlyException(nameof(InsertRange));
            }

            if (items?.Length == 0)
            {
                return;
            }

            InsertItem(index, items);
        }

        /// <summary>
        /// Move item at oldIndex to newIndex.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Either or both <paramref name="oldIndex"/> or <paramref name="newIndex"/> are less than zero. -or- is greater than <see cref="ObservableCollectionBase{T, TCollectionWrapper}.Count"/>.
        /// </exception>
        /// <exception cref="NotSupportedException">
        /// The <see cref="ObservableCollection{T}"/> is read-only.
        /// </exception>
        public void Move(int oldIndex, int newIndex)
        {
            if (IsReadOnly)
            {
                throw ReadOnlyException(nameof(Move));
            }

            MoveItem(oldIndex, newIndex);
        }

        /// <summary>
        /// Removes the <see cref="ObservableCollection{T}"/> item at the specified index.
        /// </summary>
        /// <param name="index">
        /// The zero-based index of the item to remove.
        /// </param>
        /// <exception cref="ArgumentOutOfRangeException">
        /// <paramref name="index"/> is not a valid index in the <see cref="ObservableCollection{T}"/>.
        /// </exception>
        /// <exception cref="NotSupportedException">
        /// The <see cref="ObservableCollection{T}"/> is read-only.
        /// </exception>
        public void RemoveAt(int index)
        {
            if (IsReadOnly)
            {
                throw ReadOnlyException(nameof(RemoveAt));
            }

            RemoveItem(index);
        }

        /// <summary>
        /// Inserts an element into the <see cref="ObservableCollection{T}"/> at the specified <paramref name="index"/>.
        /// </summary>
        /// <param name="index">
        /// The zero-based index at which item should be inserted.
        /// </param>
        /// <param name="items">
        /// The objects to insert. The value can be null for reference types.
        /// </param>
        /// <exception cref="ArgumentOutOfRangeException">
        /// <paramref name="index"/> is less than zero. -or- index is greater than <see cref="ObservableCollectionBase{T, TCollectionWrapper}.Count"/>.
        /// </exception>
        protected virtual void InsertItem(int index, params T[] items)
        {
            try
            {
                RWLock.EnterWriteLock();

                if (index < 0 || index > Items.Count)
                {
                    throw new ArgumentOutOfRangeException(nameof(index));
                }

                for (int i = 0; i < items.Length; i++)
                {
                    Items.Insert(index + i, items[i]);
                }

                OnPropertyChanged(nameof(Count));
                OnPropertyChanged(IndexerName);
                OnCollectionAdd(items, index);
            }
            catch
            {
                throw;
            }
            finally
            {
                RWLock.ExitWriteLock();
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
        /// Either or both <paramref name="oldIndex"/> or <paramref name="newIndex"/> are less than zero. -or- is greater than <see cref="ObservableCollectionBase{T, TCollectionWrapper}.Count"/>.
        /// </exception>
        protected virtual void MoveItem(int oldIndex, int newIndex)
        {
            try
            {
                RWLock.EnterWriteLock();

                if (oldIndex < 0 || oldIndex >= Items.Count)
                {
                    throw new ArgumentOutOfRangeException(nameof(oldIndex));
                }

                if (newIndex < 0 || newIndex >= Items.Count)
                {
                    throw new ArgumentOutOfRangeException(nameof(newIndex));
                }

                T movedItem = Items[oldIndex];

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
                RWLock.ExitWriteLock();
            }
        }

        /// <summary>
        /// Removes the element at the specified <paramref name="index"/> of the <see cref="ObservableCollection{T}"/>.
        /// </summary>
        /// <param name="index">
        /// The zero-based index of the element to remove.
        /// </param>
        /// <exception cref="ArgumentOutOfRangeException">
        /// <paramref name="index"/> is less than zero. -or- is greater than <see cref="ObservableCollectionBase{T, TCollectionWrapper}.Count"/>.
        /// </exception>
        protected virtual void RemoveItem(int index)
        {
            try
            {
                RWLock.EnterWriteLock();

                if (index < 0 || index >= Items.Count)
                {
                    throw new ArgumentOutOfRangeException(nameof(index));
                }

                T removedItem = Items[index];

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
                RWLock.ExitWriteLock();
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
        /// <paramref name="index"/> is not a valid index in the <see cref="ObservableCollection{T}"/>.
        /// </exception>
        protected virtual void SetItem(int index, T item)
        {
            try
            {
                RWLock.EnterWriteLock();

                if (index < 0 || index >= Items.Count)
                {
                    throw new ArgumentOutOfRangeException(nameof(index));
                }

                T originalItem = Items[index];

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
                RWLock.ExitWriteLock();
            }
        }

        #endregion

        #region ObservableCollectionBase<T, TCollectionWrapper> Members

        /// <summary>
        /// Adds an item to the <see cref="ObservableCollection{T}"/>.
        /// </summary>
        /// <param name="item">
        /// The item to add to the <see cref="ObservableCollection{T}"/>.
        /// </param>
        /// <exception cref="NotSupportedException">
        /// The <see cref="ObservableCollection{T}"/> is read-only.
        /// </exception>
        public void Add(T item)
        {
            if (IsReadOnly)
            {
                throw ReadOnlyException(nameof(Add));
            }

            try
            {
                RWLock.EnterUpgradeableReadLock();
                int index = Items.Count;
                InsertItem(index, item);
            }
            catch
            {
                throw;
            }
            finally
            {
                RWLock.ExitUpgradeableReadLock();
            }
        }

        /// <summary>
        /// Removes the first occurrence of a specific object from the <see cref="ObservableCollection{T}"/>.
        /// </summary>
        /// <param name="item">
        /// The object to remove from the <see cref="ObservableCollection{T}"/>.
        /// </param>
        /// <returns>
        /// <c>true</c> if item was successfully removed from the <see cref="ObservableCollection{T}"/>; otherwise, <c>false</c>. This method also returns false if item is not found in the <see cref="ObservableCollection{T}"/>.
        /// </returns>
        /// <exception cref="NotSupportedException">
        /// The <see cref="ObservableCollection{T}"/> is read-only.
        /// </exception>
        public bool Remove(T item)
        {
            if (IsReadOnly)
            {
                throw ReadOnlyException(nameof(Remove));
            }

            try
            {
                RWLock.EnterUpgradeableReadLock();
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
                RWLock.ExitUpgradeableReadLock();
            }
            return true;
        }

        #endregion

        #region IReadOnlyList<T> Members

        T IReadOnlyList<T>.this[int index] => this[index];

        #endregion

        #region IList<T> Members

        T IList<T>.this[int index]
        {
            get => this[index];
            set => this[index] = value;
        }

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
                        throw WrongTypeException(nameof(value), value?.GetType());
                    }
                }
                else if (value is T item)
                {
                    this[index] = item;
                }
                else
                {
                    throw WrongTypeException(nameof(value), value?.GetType());
                }
            }
        }

        bool IList.IsReadOnly => IsReadOnly;

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
                    throw WrongTypeException(nameof(value), value?.GetType());
                }
            }
            else if (value is T item)
            {
                Add(item);
            }
            else
            {
                throw WrongTypeException(nameof(value), value?.GetType());
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
                    throw WrongTypeException(nameof(value), value?.GetType());
                }
            }
            else if (value is T item)
            {
                return Contains(item);
            }
            else
            {
                throw WrongTypeException(nameof(value), value?.GetType());
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
                    throw WrongTypeException(nameof(value), value?.GetType());
                }
            }
            else if (value is T item)
            {
                return IndexOf(item);
            }
            else
            {
                throw WrongTypeException(nameof(value), value?.GetType());
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
                    throw WrongTypeException(nameof(value), value?.GetType());
                }
            }
            else if (value is T item)
            {
                Insert(index, item);
            }
            else
            {
                throw WrongTypeException(nameof(value), value?.GetType());
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
                    throw WrongTypeException(nameof(value), value?.GetType());
                }
            }
            else if (value is T item)
            {
                Remove(item);
            }
            else
            {
                throw WrongTypeException(nameof(value), value?.GetType());
            }
        }

        void IList.RemoveAt(int index) => RemoveAt(index);

        #endregion
    }
}
