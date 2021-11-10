using ObservableHelpers.Abstraction;
using ObservableHelpers.Utilities;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Threading;

namespace ObservableHelpers.Utilities
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
        ICollection
    {
        #region Properties

        /// <summary>
        /// Gets the element at the specified index.
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
        public T this[int index]
        {
            get
            {
                if (IsDisposed)
                {
                    return default;
                }

                return RWLock.LockRead(() =>
                {
                    if (index < 0 || index >= Items.Count)
                    {
                        throw new ArgumentOutOfRangeException(nameof(index));
                    }
                    return Items[index];
                });
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

                return Items.Count;
            }
        }

        /// <summary>
        /// Gets a value indicating whether the <see cref="ObservableCollectionBase{T}"/> is read-only.
        /// </summary>
        public bool IsReadOnly { get; protected set; }

        /// <summary>
        /// Gets the read-write lock for concurrency.
        /// </summary>
        protected RWLock RWLock { get; } = new RWLock(LockRecursionPolicy.SupportsRecursion);

        /// <summary>
        /// Gets a <see cref="List{T}"/> wrapper around the <see cref="ObservableCollectionBase{T}"/>.
        /// </summary>
        protected virtual List<T> Items { get; set; }

        // This must agree with Binding.IndexerName. It is declared separately
        // here so as to avoid a dependency on PresentationFramework.dll.
        private protected const string IndexerName = "Item[]";

        #endregion

        #region Initializers

        /// <summary>
        /// Initializes a new instance of the <see cref="ObservableCollectionBase{T}"/> class that contains empty elements and has sufficient capacity to accommodate the number of elements copied.
        /// </summary>
        public ObservableCollectionBase()
            : this(instance => new List<T>())
        {

        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ObservableCollectionBase{T}"/> class.
        /// </summary>
        /// <param name="collectionWrapperFactory">
        /// The function used to create the <see cref="ObservableCollectionBase{T}.Items"/>.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="collectionWrapperFactory"/> is a null reference.
        /// </exception>
        public ObservableCollectionBase(Func<ObservableCollectionBase<T>, List<T>> collectionWrapperFactory)
        {
            if (collectionWrapperFactory == null)
            {
                throw new ArgumentNullException(nameof(collectionWrapperFactory));
            }
            Items = collectionWrapperFactory.Invoke(this);
        }

        #endregion

        #region Members

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

            return RWLock.LockRead(() => Items.Contains(item));
        }

        /// <summary>
        /// Copies the entire <see cref="ObservableCollectionBase{T}"/> to a compatible one-dimensional <paramref name="array"/>, starting at the beginning of the specified target <paramref name="array"/>.
        /// </summary>
        /// <param name="array">
        /// The one-dimensional <see cref="Array"/> that is the destination of the elements copied from <see cref="ObservableCollectionBase{T}"/>. The <see cref="Array"/> must have zero-based indexing.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="array"/> is a null reference.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// The number of elements in the source <see cref="ObservableCollectionBase{T}"/> is greater than the number of elements that the destination <paramref name="array"/> can contain.
        /// </exception>
        public void CopyTo(T[] array)
        {
            if (IsDisposed)
            {
                return;
            }

            RWLock.LockRead(() => Items.CopyTo(array));
        }

        /// <summary>
        /// Copies the entire <see cref="ObservableCollectionBase{T}"/> to a compatible one-dimensional <paramref name="array"/>, starting at the specified <paramref name="arrayIndex"/> of the target <paramref name="array"/>.
        /// </summary>
        /// <param name="array">
        /// The one-dimensional <see cref="Array"/> that is the destination of the elements copied from <see cref="ObservableCollectionBase{T}"/>. The <see cref="Array"/> must have zero-based indexing.
        /// </param>
        /// <param name="arrayIndex">
        /// The zero-based index in <paramref name="array"/> at which copying begins.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="array"/> is a null reference.
        /// </exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// <paramref name="arrayIndex"/> is less than 0.
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

            RWLock.LockRead(() => Items.CopyTo(array, arrayIndex));
        }

        /// <summary>
        /// Copies the entire <see cref="ObservableCollectionBase{T}"/> to a compatible one-dimensional <paramref name="array"/>, starting at the specified <paramref name="arrayIndex"/> of the target <paramref name="array"/>.
        /// </summary>
        /// <param name="array">
        /// The one-dimensional <see cref="Array"/> that is the destination of the elements copied from <see cref="ObservableCollectionBase{T}"/>. The <see cref="Array"/> must have zero-based indexing.
        /// </param>
        /// <param name="arrayIndex">
        /// The zero-based index in <paramref name="array"/> at which copying begins.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="array"/> is a null reference.
        /// </exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// <paramref name="arrayIndex"/> is less than 0.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// The number of elements in the source <see cref="ObservableCollectionBase{T}"/> is greater than the available space from <paramref name="arrayIndex"/> to the end of the destination <paramref name="array"/>.
        /// </exception>
        public void CopyTo(Array array, int arrayIndex)
        {
            if (IsDisposed)
            {
                return;
            }

            RWLock.LockRead(() => (Items as ICollection).CopyTo(array, arrayIndex));
        }

        /// <summary>
        /// Copies a range of elements from the <see cref="ObservableCollectionBase{T}"/> to a compatible one-dimensional <paramref name="array"/>, starting at the specified <paramref name="arrayIndex"/> of the target <paramref name="array"/>.
        /// </summary>
        /// <param name="index">
        /// The zero-based index in the source <see cref="ObservableCollectionBase{T}"/> at which copying begins.
        /// </param>
        /// <param name="array">
        /// The one-dimensional <see cref="Array"/> that is the destination of the elements copied from <see cref="ObservableCollectionBase{T}"/>. The <see cref="Array"/> must have zero-based indexing.
        /// </param>
        /// <param name="arrayIndex">
        /// The zero-based index in array at which copying begins.
        /// </param>
        /// <param name="count">
        /// The number of elements to copy.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="array"/> is a null reference.
        /// </exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// <paramref name="index"/> is less than 0. -or- <paramref name="arrayIndex"/> is less than 0. -or- <paramref name="count"/> is less than 0.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// <paramref name="index"/> is equal to or greater than the <see cref="ObservableCollectionBase{T}.Count"/> of the source <see cref="ObservableCollectionBase{T}"/>. -or- The number of elements from <paramref name="index"/> to the end of the source <see cref="ObservableCollectionBase{T}"/> is greater than the available space from <paramref name="arrayIndex"/> to the end of the destination <paramref name="array"/>.
        /// </exception>
        public void CopyTo(int index, T[] array, int arrayIndex, int count)
        {
            if (IsDisposed)
            {
                return;
            }

            RWLock.LockRead(() => Items.CopyTo(index, array, arrayIndex, count));
        }

        /// <summary>
        /// Returns an enumerator that iterates through the collection.
        /// </summary>
        /// <returns>
        /// An enumerator that can be used to iterate through the collection.
        /// </returns>
        public virtual IEnumerator<T> GetEnumerator()
        {
            if (IsDisposed)
            {
                return default;
            }

            return RWLock.LockRead(() => Items.GetEnumerator());
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

            return RWLock.LockRead(() => Items.IndexOf(value));
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
        public ObservableCollectionBaseFilter ObservableFilter(Predicate<T> predicate)
        {
            if (IsDisposed)
            {
                return default;
            }
            if (predicate == null)
            {
                throw new ArgumentNullException(nameof(predicate));
            }

            ObservableCollectionBaseFilter filter = RWLock.LockRead(() =>
            {
                filter = new ObservableCollectionBaseFilter(Items.Where(i => predicate.Invoke(i)));
                filter.SyncOperation.SetContext(this);
                return filter;
            });

            void Filter_ImmediateCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
            {
                filter.RWLock.LockWrite(() =>
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
        /// Removes all elements from the <see cref="ObservableCollectionBase{T}"/> and notify the observers.
        /// </summary>
        /// <param name="lastCount">
        /// The last <see cref="Count"/> of the <see cref="ObservableCollectionBase{T}"/> before the operation.
        /// </param>
        /// <returns>
        /// <c>true</c> if operation was executed; otherwise <c>false</c>.
        /// </returns>
        protected bool ClearItems(out int lastCount)
        {
            int proxy = default;
            bool ret = RWLock.LockRead(() =>
            {
                return RWLock.LockWrite(() =>
                {
                    if (InternalClearItems(out proxy))
                    {
                        OnPropertyChanged(nameof(Count));
                        OnPropertyChanged(IndexerName);
                        OnCollectionReset();
                        return true;
                    }
                    return false;
                });
            });
            lastCount = proxy;
            return ret;
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
        /// <param name="lastCount">
        /// The last <see cref="Count"/> of the <see cref="ObservableCollectionBase{T}"/> before the operation.
        /// </param>
        /// <returns>
        /// <c>true</c> if operation was executed; otherwise <c>false</c>.
        /// </returns>
        /// <exception cref="ArgumentOutOfRangeException">
        /// <paramref name="index"/> is less than zero. -or- index is greater than <see cref="ObservableCollectionBase{T}.Count"/>.
        /// </exception>
        protected bool InsertItem(int index, T item, out int lastCount)
        {
            int proxy = default;
            bool ret = RWLock.LockRead(() =>
            {
                if (index < 0 || index > Items.Count)
                {
                    throw new ArgumentOutOfRangeException(nameof(index));
                }

                return RWLock.LockWrite(() =>
                {
                    if (InternalInsertItems(index, new T[] { item }, out proxy))
                    {
                        if (item is ISyncObject sync)
                        {
                            sync.SyncOperation.SetContext(this);
                        }
                        OnPropertyChanged(nameof(Count));
                        OnPropertyChanged(IndexerName);
                        OnCollectionAdd(item, index);
                        return true;
                    }
                    return false;
                });
            });
            lastCount = proxy;
            return ret;
        }

        /// <summary>
        /// Inserts an elements into the <see cref="ObservableCollectionBase{T}"/> at the specified <paramref name="index"/> and notify the observers.
        /// </summary>
        /// <param name="index">
        /// The zero-based index at which item should be inserted.
        /// </param>
        /// <param name="items">
        /// The elements to insert. The value can be null for reference types.
        /// </param>
        /// <param name="lastCount">
        /// The last <see cref="Count"/> of the <see cref="ObservableCollectionBase{T}"/> before the operation.
        /// </param>
        /// <returns>
        /// <c>true</c> if operation was executed; otherwise <c>false</c>.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="items"/> is a null reference.
        /// </exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// <paramref name="index"/> is less than zero. -or- index is greater than <see cref="ObservableCollectionBase{T}.Count"/>.
        /// </exception>
        protected bool InsertItems(int index, IEnumerable<T> items, out int lastCount)
        {
            int proxy = default;
            bool ret = RWLock.LockRead(() =>
            {
                if (items == null)
                {
                    throw new ArgumentNullException(nameof(items));
                }
                if (index < 0 || index > Items.Count)
                {
                    throw new ArgumentOutOfRangeException(nameof(index));
                }

                return RWLock.LockWrite(() =>
                {
                    if (InternalInsertItems(index, items, out proxy))
                    {
                        foreach (T item in items)
                        {
                            if (item is ISyncObject sync)
                            {
                                sync.SyncOperation.SetContext(this);
                            }
                        }
                        OnPropertyChanged(nameof(Count));
                        OnPropertyChanged(IndexerName);
                        if (items is IList list)
                        {
                            OnCollectionAdd(list, index);
                        }
                        else
                        {
                            OnCollectionAdd(items.ToList(), index);
                        }
                        return true;
                    }
                    return false;
                });
            });
            lastCount = proxy;
            return ret;
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
        /// <param name="movedItem">
        /// The moved element at the specified <paramref name="oldIndex"/> to the specified <paramref name="newIndex"/> from the <see cref="ObservableCollectionBase{T}"/>.
        /// </param>
        /// <returns>
        /// <c>true</c> if operation was executed; otherwise <c>false</c>.
        /// </returns>
        /// <exception cref="ArgumentOutOfRangeException">
        /// <paramref name="oldIndex"/> or <paramref name="newIndex"/> is less than zero. -or- is greater than or equal to <see cref="ObservableCollectionBase{T}.Count"/>.
        /// </exception>
        protected bool MoveItem(int oldIndex, int newIndex, out T movedItem)
        {
            T proxy = default;
            bool ret = RWLock.LockRead(() =>
            {
                if (oldIndex < 0 || oldIndex >= Items.Count)
                {
                    throw new ArgumentOutOfRangeException(nameof(oldIndex));
                }

                if (newIndex < 0 || newIndex >= Items.Count)
                {
                    throw new ArgumentOutOfRangeException(nameof(newIndex));
                }

                return RWLock.LockWrite(() =>
                {
                    if (InternalMoveItem(oldIndex, newIndex, out proxy))
                    {
                        OnPropertyChanged(IndexerName);
                        OnCollectionMove(proxy, oldIndex, newIndex);
                        return true;
                    }
                    return false;
                });
            });
            movedItem = proxy;
            return ret;
        }

        /// <summary>
        /// Removes the element at the specified <paramref name="index"/> of the <see cref="ObservableCollectionBase{T}"/> and notify the observers.
        /// </summary>
        /// <param name="index">
        /// The zero-based index of the element to remove.
        /// </param>
        /// <param name="removedItem">
        /// The removed element at the specified <paramref name="index"/> from the <see cref="ObservableCollectionBase{T}"/>.
        /// </param>
        /// <returns>
        /// <c>true</c> if operation was executed; otherwise <c>false</c>.
        /// </returns>
        /// <exception cref="ArgumentOutOfRangeException">
        /// <paramref name="index"/> is less than zero. -or- is greater than <see cref="ObservableCollectionBase{T}.Count"/>.
        /// </exception>
        protected bool RemoveItem(int index, out T removedItem)
        {
            T proxy = default;
            bool ret = RWLock.LockRead(() =>
            {
                if (index < 0 || index >= Items.Count)
                {
                    throw new ArgumentOutOfRangeException(nameof(index));
                }

                return RWLock.LockWrite(() =>
                {
                    if (InternalRemoveItems(index, 1, out IEnumerable<T> removedItems))
                    {
                        proxy = removedItems.FirstOrDefault();
                        OnPropertyChanged(nameof(Count));
                        OnPropertyChanged(IndexerName);
                        if (removedItems is IList list)
                        {
                            OnCollectionRemove(list, index);
                        }
                        else
                        {
                            OnCollectionRemove(removedItems.ToList(), index);
                        }
                        return true;
                    }
                    return false;
                });
            });
            removedItem = proxy;
            return ret;
        }

        /// <summary>
        /// Removes the elements at the specified <paramref name="index"/> of the <see cref="ObservableCollectionBase{T}"/> and notify the observers.
        /// </summary>
        /// <param name="index">
        /// The zero-based starting index of the elements to remove.
        /// </param>
        /// <param name="count">
        /// The count of elements to remove.
        /// </param>
        /// <param name="removedItems">
        /// The removed elements at the specified <paramref name="index"/> from the <see cref="ObservableCollectionBase{T}"/>.
        /// </param>
        /// <returns>
        /// <c>true</c> if operation was executed; otherwise <c>false</c>.
        /// </returns>
        /// <exception cref="ArgumentException">
        /// <paramref name="index"/> and <paramref name="count"/> do not denote a valid range of elements in the <see cref="ObservableCollectionBase{T}"/>.
        /// </exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// <paramref name="index"/> or <paramref name="count"/> is less than zero.
        /// </exception>
        protected bool RemoveItems(int index, int count, out IEnumerable<T> removedItems)
        {
            IEnumerable<T> proxy = default;
            bool ret = RWLock.LockRead(() =>
            {
                if (index < 0)
                {
                    throw new ArgumentOutOfRangeException(nameof(index));
                }
                if (count < 0)
                {
                    throw new ArgumentOutOfRangeException(nameof(count));
                }
                if (index + count > Items.Count)
                {
                    throw new ArgumentException("Index and count do not denote a valid range of elements in the " + GetType().FullName);
                }

                return RWLock.LockWrite(() =>
                {
                    if (InternalRemoveItems(index, count, out proxy))
                    {
                        OnPropertyChanged(nameof(Count));
                        OnPropertyChanged(IndexerName);
                        if (proxy is IList list)
                        {
                            OnCollectionRemove(list, index);
                        }
                        else
                        {
                            OnCollectionRemove(proxy.ToList(), index);
                        }
                        return true;
                    }
                    return false;
                });
            });
            removedItems = proxy;
            return ret;
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
        /// <param name="originalItem">
        /// The replaced original element at the specified <paramref name="index"/> from the <see cref="ObservableCollectionBase{T}"/>.
        /// </param>
        /// <returns>
        /// <c>true</c> if operation was executed; otherwise <c>false</c>.
        /// </returns>
        /// <exception cref="ArgumentOutOfRangeException">
        /// <paramref name="index"/> is less than zero. -or- is greater than or equal to <see cref="ObservableCollectionBase{T}.Count"/>.
        /// </exception>
        protected bool SetItem(int index, T item, out T originalItem)
        {
            T proxy = default;
            bool ret = RWLock.LockRead(() =>
            {
                if (index < 0 || index >= Items.Count)
                {
                    throw new ArgumentOutOfRangeException(nameof(index));
                }

                return RWLock.LockWrite(() =>
                {
                    if (InternalSetItem(index, item, out proxy))
                    {
                        if (item is ISyncObject sync)
                        {
                            sync.SyncOperation.SetContext(this);
                        }
                        OnPropertyChanged(IndexerName);
                        OnCollectionReplace(proxy, item, index);
                        return true;
                    }
                    return false;
                });
            });
            originalItem = proxy;
            return ret;
        }

        /// <summary>
        /// Provides an overridable internal operation for <see cref="ClearItems"/>.
        /// </summary>
        /// <param name="lastCount">
        /// The last <see cref="Count"/> of the <see cref="ObservableCollectionBase{T}"/> before the operation.
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
        /// Provides an overridable internal operation for <see cref="InsertItem(int, T, out int)"/> and <see cref="InsertItems(int, IEnumerable{T}, out int)"/>.
        /// </summary>
        /// <param name="index">
        /// The zero-based starting index at which elements should be inserted.
        /// </param>
        /// <param name="items">
        /// The elements to insert. The value can be null for reference types.
        /// </param>
        /// <param name="lastCount">
        /// The last <see cref="Count"/> of the <see cref="ObservableCollectionBase{T}"/> before the operation.
        /// </param>
        /// <returns>
        /// <c>true</c> if operation was executed; otherwise <c>false</c>.
        /// </returns>
        protected virtual bool InternalInsertItems(int index, IEnumerable<T> items, out int lastCount)
        {
            lastCount = Items.Count;

            int insertCount = items.Count();

            if (insertCount == 1)
            {
                Items.Insert(index, items.First());

                return true;
            }
            else if (insertCount > 1)
            {
                Items.InsertRange(index, items);

                return true;
            }
            else
            {
                return false;
            }

        }

        /// <summary>
        /// Provides an overridable internal operation for <see cref="MoveItem(int, int, out T)"/>.
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
        protected virtual bool InternalMoveItem(int oldIndex, int newIndex, out T movedItem)
        {
            movedItem = Items[oldIndex];

            Items.RemoveAt(oldIndex);
            Items.Insert(newIndex, movedItem);

            return true;
        }

        /// <summary>
        /// Provides an overridable internal operation for <see cref="RemoveItem(int, out T)"/> and <see cref="RemoveItems(int, int, out IEnumerable{T})"/>.
        /// </summary>
        /// <param name="index">
        /// The zero-based index of the element to remove.
        /// </param>
        /// <param name="count">
        /// The count of elements to remove.
        /// </param>
        /// <param name="oldItems">
        /// The removed elements at the specified <paramref name="index"/> from the <see cref="ObservableCollectionBase{T}"/>.
        /// </param>
        /// <returns>
        /// <c>true</c> if operation was executed; otherwise <c>false</c>.
        /// </returns>
        protected virtual bool InternalRemoveItems(int index, int count, out IEnumerable<T> oldItems)
        {
            if (count == 1)
            {
                oldItems = new T[] { Items[index] };

                Items.RemoveAt(index);

                return true;
            }
            else if (count > 1)
            {
                oldItems = Items.GetRange(index, count);

                Items.RemoveRange(index, count);

                return true;
            }
            else
            {
                oldItems = null;

                return false;
            }

        }

        /// <summary>
        /// Provides an overridable internal operation for <see cref="SetItem(int, T, out T)"/>.
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
        protected virtual bool InternalSetItem(int index, T item, out T originalItem)
        {
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

            return RWLock.LockRead(() => Items.Count == 0);
        }

        /// <inheritdoc/>
        public override bool SetNull()
        {
            throw ReadOnlyException(nameof(SetNull));
        }

        #endregion

        #region IReadOnlyList<T> Members

        T IReadOnlyList<T>.this[int index] => this[index];

        #endregion

        #region IReadOnlyCollection<T> Members

        int IReadOnlyCollection<T>.Count => Count;

        #endregion

        #region ICollection Members

        int ICollection.Count => Count;

        bool ICollection.IsSynchronized => true;

        object ICollection.SyncRoot
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

        void ICollection.CopyTo(Array array, int index) => CopyTo(array, index);

        private object syncRoot;

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
        public class ObservableCollectionBaseFilter : ObservableCollectionBase<T>
        {
            internal ObservableCollectionBaseFilter(IEnumerable<T> initialItems)
                : base(_ => initialItems.ToList())
            {

            }
        }

        #endregion
    }
}
