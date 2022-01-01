using ObservableHelpers.Utilities;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace ObservableHelpers
{
    /// <summary>
    /// Provides a thread-safe observable collection used for data binding.
    /// </summary>
    /// <typeparam name="T">
    /// Specifies the type of the items in this collection.
    /// </typeparam>
    public class ObservableCollection<T> :
        ObservableCollectionBase<T>,
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
        public new T this[int index]
        {
            get => base[index];
            set
            {
                if (IsDisposed)
                {
                    return;
                }
                if (IsReadOnly)
                {
                    throw ReadOnlyException(nameof(IndexerName));
                }

                SetItem(index, value, out _);
            }
        }

        #endregion

        #region Initializers

        /// <summary>
        /// Initializes a new instance of the <see cref="ObservableCollection{T}"/> class that contains empty elements and has sufficient capacity to accommodate the number of elements copied.
        /// </summary>
        public ObservableCollection()
            : base(instance => new List<T>())
        {

        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ObservableCollection{T}"/> class that contains elements copied from the specified collection and has sufficient capacity to accommodate the number of elements copied.
        /// </summary>
        /// <param name="enumerable">
        /// The collection whose elements are copied to the new list.
        /// </param>
        /// <remarks>
        /// The elements are copied onto the <see cref="ObservableCollection{T}"/> in the same order they are read by the enumerator of the collection.
        /// </remarks>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="enumerable"/> is a null reference.
        /// </exception>
        public ObservableCollection(IEnumerable<T> enumerable)
            : base(_ => new List<T>(enumerable))
        {

        }

        #endregion

        #region Members

        /// <summary>
        /// Adds an item to the <see cref="ObservableCollection{T}"/> and notify the observers for changes.
        /// </summary>
        /// <param name="item">
        /// The item to add to the <see cref="ObservableCollection{T}"/>.
        /// </param>
        /// <exception cref="NotSupportedException">
        /// The <see cref="ObservableCollection{T}"/> is read-only.
        /// </exception>
        public void Add(T item)
        {
            if (IsDisposed)
            {
                return;
            }
            if (IsReadOnly)
            {
                throw ReadOnlyException(nameof(Add));
            }

            RWLock.LockReadUpgradable(() =>
            {
                int index = Items.Count;
                InsertItem(index, item, out _);
            });
        }

        /// <summary>
        /// Adds an item range to the <see cref="ObservableCollection{T}"/> and notify the observers for changes.
        /// </summary>
        /// <param name="items">
        /// The items to add to the <see cref="ObservableCollection{T}"/>.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="items"/> is a null reference.
        /// </exception>
        /// <exception cref="NotSupportedException">
        /// The <see cref="ObservableCollection{T}"/> is read-only.
        /// </exception>
        public void AddRange(params T[] items)
        {
            AddRange(items as IEnumerable<T>);
        }

        /// <summary>
        /// Adds an item range to the <see cref="ObservableCollection{T}"/> and notify the observers for changes.
        /// </summary>
        /// <param name="items">
        /// The items to add to the <see cref="ObservableCollection{T}"/>.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="items"/> is a null reference.
        /// </exception>
        /// <exception cref="NotSupportedException">
        /// The <see cref="ObservableCollection{T}"/> is read-only.
        /// </exception>
        public void AddRange(IEnumerable<T> items)
        {
            if (IsDisposed)
            {
                return;
            }
            if (items == null)
            {
                throw new ArgumentNullException(nameof(items));
            }
            if (IsReadOnly)
            {
                throw ReadOnlyException(nameof(AddRange));
            }

            RWLock.LockReadUpgradable(() =>
            {
                int index = Items.Count;
                InsertItems(index, items, out _);
            });
        }

        /// <summary>
        /// Removes all elements from the <see cref="ObservableCollection{T}"/> and notify the observers for changes.
        /// </summary>
        /// <exception cref="NotSupportedException">
        /// The <see cref="ObservableCollection{T}"/> is read-only.
        /// </exception>
        public void Clear()
        {
            if (IsDisposed)
            {
                return;
            }
            if (IsReadOnly)
            {
                throw ReadOnlyException(nameof(Clear));
            }

            ClearItems(out _);
        }

        /// <summary>
        /// Inserts an item to the <see cref="ObservableCollection{T}"/> at the specified index and notify the observers for changes.
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
            if (IsDisposed)
            {
                return;
            }
            if (IsReadOnly)
            {
                throw ReadOnlyException(nameof(Insert));
            }

            InsertItem(index, item, out _);
        }

        /// <summary>
        /// Inserts an item range to the <see cref="ObservableCollection{T}"/> at the specified index and notify the observers for changes.
        /// </summary>
        /// <param name="index">
        /// The zero-based index at which item should be inserted.
        /// </param>
        /// <param name="items">
        /// The item to insert into the <see cref="ObservableCollection{T}"/>.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="items"/> is a null reference.
        /// </exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// <paramref name="index"/> is not a valid index in the <see cref="ObservableCollection{T}"/>.
        /// </exception>
        /// <exception cref="NotSupportedException">
        /// The <see cref="ObservableCollection{T}"/> is read-only.
        /// </exception>
        public void InsertRange(int index, params T[] items)
        {
            InsertRange(index, items as IEnumerable<T>);
        }

        /// <summary>
        /// Inserts an item range to the <see cref="ObservableCollection{T}"/> at the specified index and notify the observers for changes.
        /// </summary>
        /// <param name="index">
        /// The zero-based index at which item should be inserted.
        /// </param>
        /// <param name="items">
        /// The item to insert into the <see cref="ObservableCollection{T}"/>.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="items"/> is a null reference.
        /// </exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// <paramref name="index"/> is not a valid index in the <see cref="ObservableCollection{T}"/>.
        /// </exception>
        /// <exception cref="NotSupportedException">
        /// The <see cref="ObservableCollection{T}"/> is read-only.
        /// </exception>
        public void InsertRange(int index, IEnumerable<T> items)
        {
            if (IsDisposed)
            {
                return;
            }
            if (items == null)
            {
                throw new ArgumentNullException(nameof(items));
            }
            if (IsReadOnly)
            {
                throw ReadOnlyException(nameof(InsertRange));
            }

            InsertItems(index, items, out _);
        }

        /// <summary>
        /// Moves an element at the specified <paramref name="oldIndex"/> to the specified <paramref name="newIndex"/> of the <see cref="ObservableCollection{T}"/> and notify the observers.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Either or both <paramref name="oldIndex"/> or <paramref name="newIndex"/> are less than zero. -or- is greater than <see cref="ObservableCollectionBase{T}.Count"/>.
        /// </exception>
        /// <exception cref="NotSupportedException">
        /// The <see cref="ObservableCollection{T}"/> is read-only.
        /// </exception>
        public void Move(int oldIndex, int newIndex)
        {
            if (IsDisposed)
            {
                return;
            }
            if (IsReadOnly)
            {
                throw ReadOnlyException(nameof(Move));
            }

            MoveItem(oldIndex, newIndex, out _);
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
            if (IsDisposed)
            {
                return default;
            }
            if (IsReadOnly)
            {
                throw ReadOnlyException(nameof(Remove));
            }

            return RWLock.LockReadUpgradable(() =>
            {
                int index = Items.IndexOf(item);
                if (index == -1)
                {
                    return false;
                }
                return RemoveItem(index, out _);
            });
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
            if (IsDisposed)
            {
                return;
            }
            if (IsReadOnly)
            {
                throw ReadOnlyException(nameof(RemoveAt));
            }

            RemoveItem(index, out _);
        }

        /// <summary>
        /// Removes a specific object range from the <see cref="ObservableCollection{T}"/>.
        /// </summary>
        /// <param name="index">
        /// The zero-based starting index of the elements to remove.
        /// </param>
        /// <param name="count">
        /// The count of elements to remove.
        /// </param>
        /// <returns>
        /// <c>true</c> if items was successfully removed from the <see cref="ObservableCollection{T}"/>; otherwise, <c>false</c>.
        /// </returns>
        /// <exception cref="ArgumentException">
        /// <paramref name="index"/> and <paramref name="count"/> do not denote a valid range of elements in the <see cref="ObservableCollection{T}"/>.
        /// </exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// <paramref name="index"/> is less than zero. -or- is greater than <see cref="ObservableCollectionBase{T}.Count"/>.
        /// </exception>
        /// <exception cref="NotSupportedException">
        /// The <see cref="ObservableCollection{T}"/> is read-only.
        /// </exception>
        public bool RemoveRange(int index, int count)
        {
            if (IsDisposed)
            {
                return default;
            }
            if (IsReadOnly)
            {
                throw ReadOnlyException(nameof(RemoveRange));
            }

            return RemoveItems(index, count, out _);
        }

        #endregion

        #region ObservableSyncContext Members

        /// <inheritdoc/>
        public override bool SetNull()
        {
            if (IsDisposed)
            {
                return default;
            }

            return RWLock.LockReadUpgradable(() =>
            {
                ClearItems(out IEnumerable<T> oldItems);
                return oldItems.Count() != 0;
            });
        }

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
                if (IsDisposed)
                {
                    return;
                }

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
            if (IsDisposed)
            {
                return default;
            }

            return RWLock.LockReadUpgradable(() =>
            {
                int oldCount = Items.Count;
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
                int newCount = Items.Count;
                return oldCount != newCount ? newCount : -1;
            });
        }

        void IList.Clear() => Clear();

        bool IList.Contains(object value)
        {
            if (IsDisposed)
            {
                return default;
            }

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
            if (IsDisposed)
            {
                return default;
            }

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
            if (IsDisposed)
            {
                return;
            }

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
            if (IsDisposed)
            {
                return;
            }

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

        #region ICollection<T> Members

        int ICollection<T>.Count => Count;

        bool ICollection<T>.IsReadOnly => IsReadOnly;

        void ICollection<T>.Add(T item) => Add(item);

        void ICollection<T>.Clear() => Clear();

        bool ICollection<T>.Contains(T item) => Contains(item);

        void ICollection<T>.CopyTo(T[] array, int arrayIndex) => CopyTo(array, arrayIndex);

        bool ICollection<T>.Remove(T item) => Remove(item);

        #endregion

        #region IEnumerable<T> Members

        IEnumerator<T> IEnumerable<T>.GetEnumerator() => GetEnumerator();

        #endregion

        #region IEnumerable Members

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        #endregion
    }
}
