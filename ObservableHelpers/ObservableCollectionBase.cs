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
    public abstract class ObservableCollectionBase<T> :
        ObservableCollectionSyncContext,
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
        /// <paramref name="index"/> is not a valid index in the <see cref="ObservableCollectionBase{T}"/>.
        /// </exception>
        /// <exception cref="NotSupportedException">
        /// The property is set and the <see cref="ObservableCollectionBase{T}"/> is read-only.
        /// </exception>
        public T this[int index]
        {
            get
            {
                if (IsDisposed)
                {
                    return default;
                }

                return LockRead(() =>
                {
                    if (index < 0 || index >= Items.Count)
                    {
                        throw new ArgumentOutOfRangeException(nameof(index));
                    }
                    return Items[index];
                });
            }
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

                SetItem(index, value);
            }
        }

        /// <summary>
        /// Gets the number of elements contained in the <see cref="ObservableCollectionBase{T}"/> collection.
        /// </summary>
        public int Count
        {
            get
            {
                if (IsDisposed)
                {
                    return default;
                }

                //return LockRead(() => Items.Count);
                return Items.Count;
            }
        }

        /// <summary>
        /// Gets a value indicating whether the <see cref="ObservableCollectionBase{T}"/> is read-only.
        /// </summary>
        public bool IsReadOnly { get; protected set; }

        /// <summary>
        /// Gets an object that can be used to synchronize access to the <see cref="ObservableCollectionBase{T}"/>.
        /// </summary>
        public object SyncRoot
        {
            get
            {
                if (IsDisposed)
                {
                    return default;
                }

                if (syncRoot == null)
                {
                    Interlocked.CompareExchange(ref syncRoot, new object(), null);
                }
                return syncRoot;
            }
        }

        /// <summary>
        /// Gets a <see cref="List{T}"/> wrapper around the <see cref="ObservableCollectionBase{T}"/>.
        /// </summary>
        protected virtual List<T> Items { get; set; }

        // This must agree with Binding.IndexerName. It is declared separately
        // here so as to avoid a dependency on PresentationFramework.dll.
        private protected const string IndexerName = "Item[]";

        private readonly ReaderWriterLockSlim rwLock = new ReaderWriterLockSlim();

        private object syncRoot;

        #endregion

        #region Initializers

        /// <summary>
        /// Initializes a new instance of the <see cref="ObservableCollectionBase{T}"/> class.
        /// </summary>
        /// <param name="collectionWrapperFactory">
        /// The function used to create the <see cref="ObservableCollectionBase{T}.Items"/>.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="collectionWrapperFactory"/> is a null reference.
        /// </exception>
        public ObservableCollectionBase(Func<List<T>> collectionWrapperFactory)
        {
            Items = collectionWrapperFactory.Invoke();
        }

        #endregion

        #region Members

        /// <summary>
        /// Adds an item to the <see cref="ObservableCollectionBase{T}"/> and notify the observers for changes.
        /// </summary>
        /// <param name="item">
        /// The item to add to the <see cref="ObservableCollectionBase{T}"/>.
        /// </param>
        /// <exception cref="NotSupportedException">
        /// The <see cref="ObservableCollectionBase{T}"/> is read-only.
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

            LockRead(() =>
            {
                int index = Items.Count;
                InsertItem(index, item);
            });
        }

        /// <summary>
        /// Adds an item range to the <see cref="ObservableCollectionBase{T}"/> and notify the observers for changes.
        /// </summary>
        /// <param name="items">
        /// The items to add to the <see cref="ObservableCollectionBase{T}"/>.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="items"/> is a null reference.
        /// </exception>
        /// <exception cref="NotSupportedException">
        /// The <see cref="ObservableCollectionBase{T}"/> is read-only.
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

            AddRange(items.ToArray());
        }

        /// <summary>
        /// Adds an item range to the <see cref="ObservableCollectionBase{T}"/> and notify the observers for changes.
        /// </summary>
        /// <param name="items">
        /// The items to add to the <see cref="ObservableCollectionBase{T}"/>.
        /// </param>
        /// <exception cref="NotSupportedException">
        /// The <see cref="ObservableCollectionBase{T}"/> is read-only.
        /// </exception>
        public void AddRange(params T[] items)
        {
            if (IsDisposed)
            {
                return;
            }
            if (IsReadOnly)
            {
                throw ReadOnlyException(nameof(AddRange));
            }

            if (items?.Length == 0)
            {
                return;
            }

            LockRead(() =>
            {
                foreach (T item in items)
                {
                    int index = Items.Count;
                    InsertItem(index, item);
                }
            });
        }

        /// <summary>
        /// Removes all elements from the <see cref="ObservableCollectionBase{T}"/> and notify the observers for changes.
        /// </summary>
        /// <exception cref="NotSupportedException">
        /// The <see cref="ObservableCollectionBase{T}"/> is read-only.
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

            ClearItems();
        }

        /// <summary>
        /// Determines whether the <see cref="ObservableCollectionBase{T}"/> contains a specific <paramref name="item"/>.
        /// </summary>
        /// <param name="item">
        /// The item to locate in the <see cref="ObservableCollectionBase{T}"/>.
        /// </param>
        /// <returns>
        /// <c>true</c> if item is found in the <see cref="ObservableCollectionBase{T}"/>; otherwise, <c>false</c>.
        /// </returns>
        public bool Contains(T item)
        {
            if (IsDisposed)
            {
                return default;
            }

            return LockRead(() => Items.Contains(item));
        }

        /// <summary>
        /// Copies the elements of the <see cref="ObservableCollectionBase{T}"/> to an <see cref="Array"/>, starting at a particular <see cref="Array"/> index.
        /// </summary>
        /// <param name="array">
        /// The one-dimensional <see cref="Array"/> that is the destination of the elements copied from <see cref="ObservableCollectionBase{T}"/>. The <see cref="Array"/> must have zero-based indexing.
        /// </param>
        /// <param name="arrayIndex">
        /// The zero-based index in array at which copying begins.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="array"/> is a null reference.
        /// </exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// <paramref name="arrayIndex"/> is less than 0 or the number of elements in the source <see cref="ObservableCollectionBase{T}"/> is greater than the available space from <paramref name="arrayIndex"/> to the end of the destination <paramref name="array"/>.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// The number of elements in the source <see cref="ObservableCollectionBase{T}"/> is greater than the available space from <paramref name="arrayIndex"/> to the end of the destination <paramref name="array"/>.
        /// </exception>
        public void CopyTo(T[] array, int arrayIndex)
        {
            if (IsDisposed)
            {
                return;
            }
            if (array == null)
            {
                throw new ArgumentNullException(nameof(array));
            }

            LockRead(() =>
            {
                if (arrayIndex < 0 || Items.Count < arrayIndex + array.Length)
                {
                    throw new ArgumentOutOfRangeException(nameof(array));
                }

                int i = 0;
                foreach (T item in Items)
                {
                    array.SetValue(item, arrayIndex + i++);
                }
            });
        }

        /// <summary>
        /// Copies the elements of the <see cref="ObservableCollectionBase{T}"/> to an <see cref="Array"/>, starting at a particular <see cref="Array"/> index.
        /// </summary>
        /// <param name="array">
        /// The one-dimensional <see cref="Array"/> that is the destination of the elements copied from <see cref="ObservableCollectionBase{T}"/>. The <see cref="Array"/> must have zero-based indexing.
        /// </param>
        /// <param name="arrayIndex">
        /// The zero-based index in array at which copying begins.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="array"/> is a null reference.
        /// </exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// <paramref name="arrayIndex"/> is less than 0 or the number of elements in the source <see cref="ObservableCollectionBase{T}"/> is greater than the available space from <paramref name="arrayIndex"/> to the end of the destination <paramref name="array"/>.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// <paramref name="array"/> is multi dimension.
        /// </exception>
        public void CopyTo(Array array, int arrayIndex)
        {
            if (IsDisposed)
            {
                return;
            }
            if (array == null)
            {
                throw new ArgumentNullException(nameof(array));
            }
            if (array.Rank != 1)
            {
                throw new ArgumentException("Array is multi dimension.", nameof(array));
            }

            LockRead(() =>
            {
                if (arrayIndex < 0 || Items.Count < arrayIndex + array.Length)
                {
                    throw new ArgumentOutOfRangeException(nameof(array));
                }

                int i = 0;
                foreach (T item in Items)
                {
                    array.SetValue(item, arrayIndex + i++);
                }
            });
        }

        /// <summary>
        /// Returns an enumerator that iterates through the collection.
        /// </summary>
        /// <returns>
        /// An enumerator that can be used to iterate through the collection.
        /// </returns>
        public IEnumerator<T> GetEnumerator()
        {
            if (IsDisposed)
            {
                return default;
            }

            return LockRead(() => Items.GetEnumerator());
        }

        /// <summary>
        /// Determines the index of a specific item in the <see cref="ObservableCollectionBase{T}"/>.
        /// </summary>
        /// <param name="value">
        /// The object to locate in the <see cref="ObservableCollectionBase{T}"/>.
        /// </param>
        /// <returns>
        /// The index of item if found in the list; otherwise, -1.
        /// </returns>
        public int IndexOf(T value)
        {
            if (IsDisposed)
            {
                return default;
            }

            return LockRead(() => Items.IndexOf(value));
        }

        /// <summary>
        /// Inserts an item to the <see cref="ObservableCollectionBase{T}"/> at the specified index and notify the observers for changes.
        /// </summary>
        /// <param name="index">
        /// The zero-based index at which item should be inserted.
        /// </param>
        /// <param name="item">
        /// The item to insert into the <see cref="ObservableCollectionBase{T}"/>.
        /// </param>
        /// <exception cref="ArgumentOutOfRangeException">
        /// <paramref name="index"/> is not a valid index in the <see cref="ObservableCollectionBase{T}"/>.
        /// </exception>
        /// <exception cref="NotSupportedException">
        /// The <see cref="ObservableCollectionBase{T}"/> is read-only.
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

            InsertItem(index, item);
        }

        /// <summary>
        /// Inserts an item range to the <see cref="ObservableCollectionBase{T}"/> at the specified index and notify the observers for changes.
        /// </summary>
        /// <param name="index">
        /// The zero-based index at which item should be inserted.
        /// </param>
        /// <param name="items">
        /// The item to insert into the <see cref="ObservableCollectionBase{T}"/>.
        /// </param>
        /// <exception cref="ArgumentOutOfRangeException">
        /// <paramref name="index"/> is not a valid index in the <see cref="ObservableCollectionBase{T}"/>.
        /// </exception>
        /// <exception cref="NotSupportedException">
        /// The <see cref="ObservableCollectionBase{T}"/> is read-only.
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

            InsertRange(index, items.ToArray());
        }

        /// <summary>
        /// Inserts an item range to the <see cref="ObservableCollectionBase{T}"/> at the specified index and notify the observers for changes.
        /// </summary>
        /// <param name="index">
        /// The zero-based index at which item should be inserted.
        /// </param>
        /// <param name="items">
        /// The item to insert into the <see cref="ObservableCollectionBase{T}"/>.
        /// </param>
        /// <exception cref="ArgumentOutOfRangeException">
        /// <paramref name="index"/> is not a valid index in the <see cref="ObservableCollectionBase{T}"/>.
        /// </exception>
        /// <exception cref="NotSupportedException">
        /// The <see cref="ObservableCollectionBase{T}"/> is read-only.
        /// </exception>
        public void InsertRange(int index, params T[] items)
        {
            if (IsDisposed)
            {
                return;
            }
            if (IsReadOnly)
            {
                throw ReadOnlyException(nameof(InsertRange));
            }

            if (items?.Length == 0)
            {
                return;
            }

            LockRead(() =>
            {
                for (int i = 0; i < items.Length; i++)
                {
                    InsertItem(index + i, items[i]);
                }
            });
        }

        /// <summary>
        /// Moves an element at the specified <paramref name="oldIndex"/> to the specified <paramref name="newIndex"/> of the <see cref="ObservableCollectionBase{T}"/> and notify the observers.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Either or both <paramref name="oldIndex"/> or <paramref name="newIndex"/> are less than zero. -or- is greater than <see cref="ObservableCollectionBase{T}.Count"/>.
        /// </exception>
        /// <exception cref="NotSupportedException">
        /// The <see cref="ObservableCollectionBase{T}"/> is read-only.
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

            MoveItem(oldIndex, newIndex);
        }

        /// <summary>
        /// Creates an observable filter that shadows the changes notifications from the parent observable.
        /// </summary>
        /// <param name="predicate">
        /// The predicate filter for child observable.
        /// </param>
        /// <returns>
        /// The created child filter read-only observable.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="predicate"/> is a null reference.
        /// </exception>
        public ObservableCollectionFilter ObservableFilter(Predicate<T> predicate)
        {
            if (IsDisposed)
            {
                return default;
            }
            if (predicate == null)
            {
                throw new ArgumentNullException(nameof(predicate));
            }

            ObservableCollectionFilter filter = LockRead(() =>
            {
                filter = new ObservableCollectionFilter(Items.Where(i => predicate.Invoke(i)));
                filter.SyncOperation.SetContext(this);
                return filter;
            });

            void Filter_ImmediateCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
            {
                filter.LockWrite(() =>
                {
                    filter.Items = Items.Where(i => predicate.Invoke(i)).ToList();
                    filter.OnCollectionReset();
                });
            }

            ImmediateCollectionChanged += Filter_ImmediateCollectionChanged;
            filter.Disposing += (s, e) =>
            {
                ImmediateCollectionChanged -= Filter_ImmediateCollectionChanged;
            };
            return filter;
        }

        /// <summary>
        /// Removes the first occurrence of a specific object from the <see cref="ObservableCollectionBase{T}"/>.
        /// </summary>
        /// <param name="item">
        /// The object to remove from the <see cref="ObservableCollectionBase{T}"/>.
        /// </param>
        /// <returns>
        /// <c>true</c> if item was successfully removed from the <see cref="ObservableCollectionBase{T}"/>; otherwise, <c>false</c>. This method also returns false if item is not found in the <see cref="ObservableCollectionBase{T}"/>.
        /// </returns>
        /// <exception cref="NotSupportedException">
        /// The <see cref="ObservableCollectionBase{T}"/> is read-only.
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

            return LockRead(() =>
            {
                int index = Items.IndexOf(item);
                if (index == -1)
                {
                    return false;
                }
                return RemoveItem(index);
            });
        }

        /// <summary>
        /// Removes the <see cref="ObservableCollectionBase{T}"/> item at the specified index.
        /// </summary>
        /// <param name="index">
        /// The zero-based index of the item to remove.
        /// </param>
        /// <exception cref="ArgumentOutOfRangeException">
        /// <paramref name="index"/> is not a valid index in the <see cref="ObservableCollectionBase{T}"/>.
        /// </exception>
        /// <exception cref="NotSupportedException">
        /// The <see cref="ObservableCollectionBase{T}"/> is read-only.
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

            LockRead(() =>
            {
                if (index < 0 || index > Items.Count)
                {
                    throw new ArgumentOutOfRangeException(nameof(index));
                }

                RemoveItem(index);
            });
        }

        /// <summary>
        /// Locks read operations of the <see cref="ObservableCollectionBase{T}"/> while executing the <paramref name="block"/> action.
        /// </summary>
        /// <param name="block">
        /// The action to be executed inside the lock block.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="block"/> is a null reference.
        /// </exception>
        protected void LockRead(Action block)
        {
            if (block == null)
            {
                throw new ArgumentNullException(nameof(block));
            }

            LockRead(() =>
            {
                block();
                return 0;
            });
        }

        /// <summary>
        /// Locks read operations of the <see cref="ObservableCollectionBase{T}"/> while executing the <paramref name="block"/> function.
        /// </summary>
        /// <typeparam name="TReturn">
        /// The object type returned by the <paramref name="block"/> function.
        /// </typeparam>
        /// <param name="block">
        /// The function to be executed inside the lock block.
        /// </param>
        /// <returns>
        /// The object returned by the <paramref name="block"/> function.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="block"/> is a null reference.
        /// </exception>
        protected TReturn LockRead<TReturn>(Func<TReturn> block)
        {
            if (block == null)
            {
                throw new ArgumentNullException(nameof(block));
            }

            try
            {
                rwLock.EnterUpgradeableReadLock();
                return block();
            }
            finally
            {
                rwLock.ExitUpgradeableReadLock();
            }
        }

        /// <summary>
        /// Locks write operations of the <see cref="ObservableCollectionBase{T}"/> while executing the <paramref name="block"/> action.
        /// </summary>
        /// <param name="block">
        /// The action to be executed inside the lock block.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="block"/> is a null reference.
        /// </exception>
        protected void LockWrite(Action block)
        {
            if (block == null)
            {
                throw new ArgumentNullException(nameof(block));
            }

            LockWrite(() =>
            {
                block();
                return 0;
            });
        }

        /// <summary>
        /// Locks write operations of the <see cref="ObservableCollectionBase{T}"/> while executing the <paramref name="block"/> function.
        /// </summary>
        /// <typeparam name="TReturn">
        /// The object type returned by the <paramref name="block"/> function.
        /// </typeparam>
        /// <param name="block">
        /// The function to be executed inside the lock block.
        /// </param>
        /// <returns>
        /// The object returned by the <paramref name="block"/> function.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="block"/> is a null reference.
        /// </exception>
        protected TReturn LockWrite<TReturn>(Func<TReturn> block)
        {
            if (block == null)
            {
                throw new ArgumentNullException(nameof(block));
            }

            try
            {
                rwLock.EnterWriteLock();
                return block();
            }
            finally
            {
                rwLock.ExitWriteLock();
            }
        }

        /// <summary>
        /// Removes all elements from the <see cref="ObservableCollectionBase{T}"/> and notify the observers.
        /// </summary>
        /// <returns>
        /// <c>true</c> if operation was executed; otherwise <c>false</c>.
        /// </returns>
        protected bool ClearItems()
        {
            return LockWrite(() =>
            {
                if (InternalClearItems(out _))
                {
                    OnPropertyChanged(nameof(Count));
                    OnPropertyChanged(IndexerName);
                    OnCollectionReset();
                    return true;
                }
                return false;
            });
        }

        /// <summary>
        /// Inserts an element into the <see cref="ObservableCollectionBase{T}"/> at the specified <paramref name="index"/> and notify the observers.
        /// </summary>
        /// <param name="index">
        /// The zero-based index at which item should be inserted.
        /// </param>
        /// <param name="item">
        /// The element to insert. The value can be null for reference types.
        /// </param>
        /// <returns>
        /// <c>true</c> if operation was executed; otherwise <c>false</c>.
        /// </returns>
        /// <exception cref="ArgumentOutOfRangeException">
        /// <paramref name="index"/> is less than zero. -or- index is greater than <see cref="ObservableCollectionBase{T}.Count"/>.
        /// </exception>
        protected bool InsertItem(int index, T item)
        {
            return LockWrite(() =>
            {
                if (index < 0 || index > Items.Count)
                {
                    throw new ArgumentOutOfRangeException(nameof(index));
                }

                if (InternalInsertItem(index, item, out _))
                {
                    OnPropertyChanged(nameof(Count));
                    OnPropertyChanged(IndexerName);
                    OnCollectionAdd(item, index);
                    return true;
                }
                return false;
            });
        }

        /// <summary>
        /// Moves an element at the specified <paramref name="oldIndex"/> to the specified <paramref name="newIndex"/> of the <see cref="ObservableCollectionBase{T}"/> and notify the observers.
        /// </summary>
        /// <param name="oldIndex">
        /// The index of the element to be moved.
        /// </param>
        /// <param name="newIndex">
        /// The new index of the element to move to.
        /// </param>
        /// <returns>
        /// <c>true</c> if operation was executed; otherwise <c>false</c>.
        /// </returns>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Either or both <paramref name="oldIndex"/> or <paramref name="newIndex"/> are less than zero. -or- is greater than <see cref="ObservableCollectionBase{T}.Count"/>.
        /// </exception>
        protected bool MoveItem(int oldIndex, int newIndex)
        {
            return LockWrite(() =>
            {
                if (InternalMoveItem(oldIndex, newIndex, out T movedItem))
                {
                    OnPropertyChanged(IndexerName);
                    OnCollectionMove(movedItem, newIndex, oldIndex);
                    return true;
                }
                return false;
            });
        }

        /// <summary>
        /// Removes the element at the specified <paramref name="index"/> of the <see cref="ObservableCollectionBase{T}"/> and notify the observers.
        /// </summary>
        /// <param name="index">
        /// The zero-based index of the element to remove.
        /// </param>
        /// <returns>
        /// <c>true</c> if operation was executed; otherwise <c>false</c>.
        /// </returns>
        /// <exception cref="ArgumentOutOfRangeException">
        /// <paramref name="index"/> is less than zero. -or- is greater than <see cref="ObservableCollectionBase{T}.Count"/>.
        /// </exception>
        protected bool RemoveItem(int index)
        {
            return LockWrite(() =>
            {
                if (InternalRemoveItem(index, out T removedItem))
                {
                    OnPropertyChanged(nameof(Count));
                    OnPropertyChanged(IndexerName);
                    OnCollectionRemove(removedItem, index);
                    return true;
                }
                return false;
            });
        }

        /// <summary>
        /// Replaces the element at the specified index and notify the observers.
        /// </summary>
        /// <param name="index">
        /// The zero-based index of the element to replace.
        /// </param>
        /// <param name="item">
        /// The new value for the element at the specified <paramref name="index"/>. The value can be <c>null</c> for reference types.
        /// </param>
        /// <returns>
        /// <c>true</c> if operation was executed; otherwise <c>false</c>.
        /// </returns>
        /// <exception cref="ArgumentOutOfRangeException">
        /// <paramref name="index"/> is less than zero. -or- is greater than <see cref="ObservableCollectionBase{T}.Count"/>.
        /// </exception>
        protected bool SetItem(int index, T item)
        {
            return LockWrite(() =>
            {
                if (InternalSetItem(index, item, out T originalItem))
                {
                    OnPropertyChanged(IndexerName);
                    OnCollectionReplace(originalItem, item, index);
                    return true;
                }
                return false;
            });
        }

        /// <summary>
        /// Removes all elements from the <see cref="ObservableCollectionBase{T}"/>.
        /// </summary>
        /// <param name="lastCount">
        /// The last count of the <see cref="ObservableCollectionBase{T}"/> before modification.
        /// </param>
        /// <returns>
        /// <c>true</c> if operation was executed; otherwise <c>false</c>.
        /// </returns>
        protected virtual bool InternalClearItems(out int lastCount)
        {
            lastCount = Items.Count;

            Items.Clear();

            return true;
        }

        /// <summary>
        /// Inserts an element into the <see cref="ObservableCollectionBase{T}"/> at the specified <paramref name="index"/>.
        /// </summary>
        /// <param name="index">
        /// The zero-based index at which item should be inserted.
        /// </param>
        /// <param name="item">
        /// The element to insert. The value can be null for reference types.
        /// </param>
        /// <param name="lastCount">
        /// The last count of the <see cref="ObservableCollectionBase{T}"/> before modification.
        /// </param>
        /// <returns>
        /// <c>true</c> if operation was executed; otherwise <c>false</c>.
        /// </returns>
        /// <exception cref="ArgumentOutOfRangeException">
        /// <paramref name="index"/> is less than zero. -or- index is greater than <see cref="ObservableCollectionBase{T}.Count"/>.
        /// </exception>
        protected virtual bool InternalInsertItem(int index, T item, out int lastCount)
        {
            if (index < 0 || index > Items.Count)
            {
                throw new ArgumentOutOfRangeException(nameof(index));
            }

            lastCount = Items.Count;

            Items.Insert(index, item);

            return true;
        }

        /// <summary>
        /// Moves an element at the specified <paramref name="oldIndex"/> to the specified <paramref name="newIndex"/> of the <see cref="ObservableCollectionBase{T}"/>.
        /// </summary>
        /// <param name="oldIndex">
        /// The index of the element to be moved.
        /// </param>
        /// <param name="newIndex">
        /// The new index of the element to move to.
        /// </param>
        /// <param name="movedItem">
        /// The moved element at the specified <paramref name="oldIndex"/> to the specified <paramref name="newIndex"/> from the <see cref="ObservableCollectionBase{T}"/>.
        /// </param>
        /// <returns>
        /// <c>true</c> if operation was executed; otherwise <c>false</c>.
        /// </returns>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Either or both <paramref name="oldIndex"/> or <paramref name="newIndex"/> are less than zero. -or- is greater than <see cref="ObservableCollectionBase{T}.Count"/>.
        /// </exception>
        protected virtual bool InternalMoveItem(int oldIndex, int newIndex, out T movedItem)
        {
            if (oldIndex < 0 || oldIndex >= Items.Count)
            {
                throw new ArgumentOutOfRangeException(nameof(oldIndex));
            }

            if (newIndex < 0 || newIndex >= Items.Count)
            {
                throw new ArgumentOutOfRangeException(nameof(newIndex));
            }

            movedItem = Items[oldIndex];

            Items.RemoveAt(oldIndex);
            Items.Insert(newIndex, movedItem);

            return true;
        }

        /// <summary>
        /// Removes the element at the specified <paramref name="index"/> of the <see cref="ObservableCollectionBase{T}"/>.
        /// </summary>
        /// <param name="index">
        /// The zero-based index of the element to remove.
        /// </param>
        /// <param name="oldItem">
        /// The removed element at the specified <paramref name="index"/> from the <see cref="ObservableCollectionBase{T}"/>.
        /// </param>
        /// <returns>
        /// <c>true</c> if operation was executed; otherwise <c>false</c>.
        /// </returns>
        /// <exception cref="ArgumentOutOfRangeException">
        /// <paramref name="index"/> is less than zero. -or- is greater than <see cref="ObservableCollectionBase{T}.Count"/>.
        /// </exception>
        protected virtual bool InternalRemoveItem(int index, out T oldItem)
        {
            if (index < 0 || index >= Items.Count)
            {
                throw new ArgumentOutOfRangeException(nameof(index));
            }

            oldItem = Items[index];

            Items.RemoveAt(index);

            return true;
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
        /// <param name="originalItem">
        /// The replaced original element at the specified <paramref name="index"/> from the <see cref="ObservableCollectionBase{T}"/>.
        /// </param>
        /// <returns>
        /// <c>true</c> if operation was executed; otherwise <c>false</c>.
        /// </returns>
        /// <exception cref="ArgumentOutOfRangeException">
        /// <paramref name="index"/> is less than zero. -or- is greater than <see cref="ObservableCollectionBase{T}.Count"/>.
        /// </exception>
        protected virtual bool InternalSetItem(int index, T item, out T originalItem)
        {
            if (index < 0 || index >= Items.Count)
            {
                throw new ArgumentOutOfRangeException(nameof(index));
            }

            originalItem = Items[index];

            Items[index] = item;

            return true;
        }

        private protected ArgumentException WrongTypeException(string propertyName, Type providedType)
        {
            return new ArgumentException("Expected value type is \"" + typeof(T).FullName + "\" but collection was provided with \"" + providedType.FullName + "\" value type.", propertyName);
        }

        private protected NotSupportedException ReadOnlyException(string operationName)
        {
            return new NotSupportedException("Operation \"" + operationName + "\" is not supported. Collection is read-only.");
        }

        #endregion

        #region ObservableSyncContext Members

        /// <inheritdoc/>
        public override bool IsNull()
        {
            if (IsDisposed)
            {
                return default;
            }

            return LockRead(() => Items.Count == 0);
        }

        /// <inheritdoc/>
        public override bool SetNull()
        {
            if (IsDisposed)
            {
                return default;
            }

            return LockRead(() =>
            {
                bool isNull = Items.Count == 0;
                Clear();
                return !isNull;
            });
        }

        #endregion

        #region IReadOnlyList<T> Members

        T IReadOnlyList<T>.this[int index] => this[index];

        #endregion

        #region IReadOnlyCollection<T> Members

        int IReadOnlyCollection<T>.Count => Count;

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

            return LockRead(() =>
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

        #region ICollection Members

        int ICollection.Count => Count;

        bool ICollection.IsSynchronized => true;

        object ICollection.SyncRoot => SyncRoot;

        void ICollection.CopyTo(Array array, int index) => CopyTo(array, index);

        #endregion

        #region IEnumerable<T> Members

        IEnumerator<T> IEnumerable<T>.GetEnumerator() => GetEnumerator();

        #endregion

        #region IEnumerable Members

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        #endregion

        #region Helper Classes

        /// <summary>
        /// Provides a filter observable collection from <see cref="ObservableCollectionBase{T}"/> used for data binding.
        /// </summary>
        public class ObservableCollectionFilter : ObservableCollectionBase<T>
        {
            internal ObservableCollectionFilter(IEnumerable<T> initialItems)
                : base(() => new List<T>(initialItems))
            {
                IsReadOnly = true; 
            }
        }

        #endregion
    }
}
