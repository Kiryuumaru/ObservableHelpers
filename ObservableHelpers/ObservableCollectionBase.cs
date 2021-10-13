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
    /// <typeparam name="TCollectionWrapper">
    /// Specifies the type of the collection wrapper.
    /// </typeparam>
    public abstract class ObservableCollectionBase<T, TCollectionWrapper> :
        ObservableCollectionSyncContext,
        IReadOnlyCollection<T>,
        ICollection<T>,
        ICollection
        where TCollectionWrapper : ICollection<T>
    {
        #region Properties

        /// <summary>
        /// Gets the number of elements contained in the <see cref="ObservableCollectionBase{T, TCollectionWrapper}"/> collection.
        /// </summary>
        public int Count
        {
            get
            {
                if (IsDisposed)
                {
                    return default;
                }

                return LockRead(() => Items.Count);
            }
        }

        /// <summary>
        /// Gets a value indicating whether the <see cref="ObservableCollectionBase{T, TCollectionWrapper}"/> is read-only.
        /// </summary>
        public bool IsReadOnly { get; protected set; }

        /// <summary>
        /// Gets an object that can be used to synchronize access to the <see cref="ObservableCollectionBase{T, TCollectionWrapper}"/>.
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
        /// Gets a <see cref="ICollection{T}"/> wrapper around the <see cref="ObservableCollectionBase{T, TCollectionWrapper}"/>.
        /// </summary>
        protected virtual TCollectionWrapper Items { get; set; }

        // This must agree with Binding.IndexerName. It is declared separately
        // here so as to avoid a dependency on PresentationFramework.dll.
        private protected const string IndexerName = "Item[]";

        private readonly ReaderWriterLockSlim rwLock = new ReaderWriterLockSlim();

        private object syncRoot;

        #endregion

        #region Initializers

        /// <summary>
        /// Initializes a new instance of the <see cref="ObservableCollectionBase{T, TCollectionWrapper}"/> class.
        /// </summary>
        /// <param name="collectionWrapperFactory">
        /// The function used to create the <typeparamref name="TCollectionWrapper"/>.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="collectionWrapperFactory"/> is a null reference.
        /// </exception>
        public ObservableCollectionBase(Func<TCollectionWrapper> collectionWrapperFactory)
        {
            Items = collectionWrapperFactory.Invoke();
        }

        #endregion

        #region Members

        /// <summary>
        /// Adds an item to the <see cref="ObservableCollectionBase{T, TCollectionWrapper}"/>.
        /// </summary>
        /// <param name="item">
        /// The item to add to the <see cref="ObservableCollectionBase{T, TCollectionWrapper}"/>.
        /// </param>
        /// <exception cref="NotSupportedException">
        /// The <see cref="ObservableCollectionBase{T, TCollectionWrapper}"/> is read-only.
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

            AddItem(item);
        }

        /// <summary>
        /// Removes all elements from the <see cref="ObservableCollectionBase{T, TCollectionWrapper}"/>.
        /// </summary>
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
        /// Determines whether the <see cref="ObservableCollectionBase{T, TCollectionWrapper}"/> contains a specific <paramref name="item"/>.
        /// </summary>
        /// <param name="item">
        /// The item to locate in the <see cref="ObservableCollectionBase{T, TCollectionWrapper}"/>.
        /// </param>
        /// <returns>
        /// <c>true</c> if item is found in the <see cref="ObservableCollectionBase{T, TCollectionWrapper}"/>; otherwise, <c>false</c>.
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
        /// Copies the elements of the <see cref="ObservableCollectionBase{T, TCollectionWrapper}"/> to an <see cref="Array"/>, starting at a particular <see cref="Array"/> index.
        /// </summary>
        /// <param name="array">
        /// The one-dimensional <see cref="Array"/> that is the destination of the elements copied from <see cref="ObservableCollectionBase{T, TCollectionWrapper}"/>. The <see cref="Array"/> must have zero-based indexing.
        /// </param>
        /// <param name="arrayIndex">
        /// The zero-based index in array at which copying begins.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="array"/> is a null reference.
        /// </exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// <paramref name="arrayIndex"/> is less than 0 or the number of elements in the source <see cref="ObservableCollectionBase{T, TCollectionWrapper}"/> is greater than the available space from <paramref name="arrayIndex"/> to the end of the destination <paramref name="array"/>.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// The number of elements in the source <see cref="ObservableCollectionBase{T, TCollectionWrapper}"/> is greater than the available space from <paramref name="arrayIndex"/> to the end of the destination <paramref name="array"/>.
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
        /// Copies the elements of the <see cref="ObservableCollectionBase{T, TCollectionWrapper}"/> to an <see cref="Array"/>, starting at a particular <see cref="Array"/> index.
        /// </summary>
        /// <param name="array">
        /// The one-dimensional <see cref="Array"/> that is the destination of the elements copied from <see cref="ObservableCollectionBase{T, TCollectionWrapper}"/>. The <see cref="Array"/> must have zero-based indexing.
        /// </param>
        /// <param name="arrayIndex">
        /// The zero-based index in array at which copying begins.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="array"/> is a null reference.
        /// </exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// <paramref name="arrayIndex"/> is less than 0 or the number of elements in the source <see cref="ObservableCollectionBase{T, TCollectionWrapper}"/> is greater than the available space from <paramref name="arrayIndex"/> to the end of the destination <paramref name="array"/>.
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
        /// Removes the first occurrence of a specific object from the <see cref="ObservableCollectionBase{T, TCollectionWrapper}"/>.
        /// </summary>
        /// <param name="item">
        /// The object to remove from the <see cref="ObservableCollectionBase{T, TCollectionWrapper}"/>.
        /// </param>
        /// <returns>
        /// <c>true</c> if item was successfully removed from the <see cref="ObservableCollectionBase{T, TCollectionWrapper}"/>; otherwise, <c>false</c>. This method also returns false if item is not found in the <see cref="ObservableCollectionBase{T, TCollectionWrapper}"/>.
        /// </returns>
        /// <exception cref="NotSupportedException">
        /// The <see cref="ObservableCollectionBase{T, TCollectionWrapper}"/> is read-only.
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

            return RemoveItem(item);
        }

        /// <summary>
        /// Adds an item to the <see cref="ObservableCollectionBase{T, TCollectionWrapper}"/>.
        /// </summary>
        /// <param name="item">
        /// The item to add to the <see cref="ObservableCollectionBase{T, TCollectionWrapper}"/>.
        /// </param>
        protected virtual void AddItem(T item)
        {
            if (IsDisposed)
            {
                return;
            }

            LockWrite(() =>
            {
                Items.Add(item);

                OnPropertyChanged(nameof(Count));
                OnPropertyChanged(IndexerName);
                OnCollectionAdd(item, Items.Count - 1);
            });
        }

        /// <summary>
        /// Removes the first occurrence of a specific object from the <see cref="ObservableCollectionBase{T, TCollectionWrapper}"/>.
        /// </summary>
        /// <param name="item">
        /// The object to remove from the <see cref="ObservableCollectionBase{T, TCollectionWrapper}"/>.
        /// </param>
        /// <returns>
        /// <c>true</c> if item was successfully removed from the <see cref="ObservableCollectionBase{T, TCollectionWrapper}"/>; otherwise, <c>false</c>. This method also returns false if item is not found in the <see cref="ObservableCollectionBase{T, TCollectionWrapper}"/>.
        /// </returns>
        protected virtual bool RemoveItem(T item)
        {
            if (IsDisposed)
            {
                return default;
            }

            return LockWrite(() =>
            {
                int index = 0;
                foreach (T i in Items)
                {
                    if (EqualityComparer<T>.Default.Equals(i, item))
                    {
                        break;
                    }
                    index++;
                }

                if (index == -1)
                {
                    return false;
                }

                if (Items.Remove(item))
                {
                    OnPropertyChanged(nameof(Count));
                    OnPropertyChanged(IndexerName);
                    OnCollectionRemove(item, index);
                    return true;
                }
                else
                {
                    return false;
                }
            });
        }

        /// <summary>
        /// Removes all elements from the <see cref="ObservableCollectionBase{T, TCollectionWrapper}"/>.
        /// </summary>
        protected virtual void ClearItems()
        {
            if (IsDisposed)
            {
                return;
            }

            LockWrite(() =>
            {
                Items.Clear();

                OnPropertyChanged(nameof(Count));
                OnPropertyChanged(IndexerName);
                OnCollectionReset();
            });
        }

        private protected void LockRead(Action block)
        {
            if (IsDisposed)
            {
                return;
            }
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

        private protected TReturn LockRead<TReturn>(Func<TReturn> block)
        {
            if (IsDisposed)
            {
                return default;
            }

            if (block == null)
            {
                throw new ArgumentNullException(nameof(block));
            }

            try
            {
                rwLock.EnterUpgradeableReadLock();
                return block();
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

        private protected void LockWrite(Action block)
        {
            if (IsDisposed)
            {
                return;
            }
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

        private protected TReturn LockWrite<TReturn>(Func<TReturn> block)
        {
            if (IsDisposed)
            {
                return default;
            }

            if (block == null)
            {
                throw new ArgumentNullException(nameof(block));
            }

            try
            {
                rwLock.EnterWriteLock();
                return block();
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

            return Count == 0;
        }

        /// <inheritdoc/>
        public override bool SetNull()
        {
            if (IsDisposed)
            {
                return default;
            }

            bool isNull = Count == 0;
            Clear();
            return !isNull;
        }

        #endregion

        #region IReadOnlyCollection<T> Members

        int IReadOnlyCollection<T>.Count => Count;

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
        /// Provides a filter observable collection from <see cref="ObservableCollectionBase{T, TCollectionWrapper}"/> used for data binding.
        /// </summary>
        public class ObservableCollectionFilter : ObservableCollectionBase<T, ICollection<T>>
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
