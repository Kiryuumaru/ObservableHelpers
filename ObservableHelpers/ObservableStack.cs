using ObservableHelpers.Utilities;
using System;
using System.Collections;
using System.Collections.Concurrent;
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
    public class ObservableStack<T> :
        ObservableCollectionBase<T>,
        IProducerConsumerCollection<T>
    {
        #region Properties

        /// <summary>
        /// Gets a value that indicates whether the <see cref="ObservableStack{T}"/> is empty.
        /// </summary>
        public bool IsEmpty  => Count == 0;

        #endregion

        #region Initializers

        /// <summary>
        /// Initializes a new instance of the <see cref="ObservableStack{T}"/> class that contains empty elements and has sufficient capacity to accommodate the number of elements copied.
        /// </summary>
        public ObservableStack()
            : base()
        {

        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ObservableStack{T}"/> class that contains elements copied from the specified collection and has sufficient capacity to accommodate the number of elements copied.
        /// </summary>
        /// <param name="enumerable">
        /// The collection whose elements are copied to the new list.
        /// </param>
        /// <remarks>
        /// The elements are copied onto the <see cref="ObservableStack{T}"/> in the same order they are read by the enumerator of the collection.
        /// </remarks>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="enumerable"/> is a null reference.
        /// </exception>
        public ObservableStack(IEnumerable<T> enumerable)
            : base(_ => new List<T>(enumerable.Reverse()))
        {

        }

        #endregion

        #region Methods

        /// <summary>
        /// Removes all objects from the <see cref="ObservableStack{T}"/>.
        /// </summary>
        /// <exception cref="NotSupportedException">
        /// The <see cref="ObservableQueue{T}"/> is read-only.
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
        /// Returns the object at the top of the <see cref="ObservableStack{T}"/> without removing it.
        /// </summary>
        /// <returns>
        /// The object at the top of the <see cref="ObservableStack{T}"/>.
        /// </returns>
        /// <exception cref="InvalidOperationException">
        /// The <see cref="ObservableStack{T}"/> is empty.
        /// </exception>
        public T Peek()
        {
            if (IsDisposed)
            {
                return default;
            }

            return RWLock.LockRead(() =>
            {
                if (Count == 0)
                {
                    throw new InvalidOperationException("The stack is empty");
                }
                else
                {
                    return this[0];
                }
            });
        }

        /// <summary>
        /// Removes and returns the object at the top of the <see cref="ObservableStack{T}"/>.
        /// </summary>
        /// <returns>
        /// The object removed from the top of the <see cref="ObservableStack{T}"/>.
        /// </returns>
        /// <exception cref="InvalidOperationException">
        /// The <see cref="ObservableStack{T}"/> is empty.
        /// </exception>
        /// <exception cref="NotSupportedException">
        /// The <see cref="ObservableQueue{T}"/> is read-only.
        /// </exception>
        public T Pop()
        {
            if (IsDisposed)
            {
                return default;
            }
            if (IsReadOnly)
            {
                throw ReadOnlyException(nameof(Pop));
            }

            return RWLock.LockUpgradeableRead(() =>
            {
                if (Count == 0)
                {
                    throw new InvalidOperationException("The stack is empty");
                }
                else
                {
                    RemoveItem(0, out T item);
                    return item;
                }
            });
        }

        /// <summary>
        /// Inserts an object at the top of the <see cref="ObservableStack{T}"/>.
        /// </summary>
        /// <param name="item">
        /// The object to push onto the <see cref="ObservableStack{T}"/>. The value can be null for reference types.
        /// </param>
        /// <exception cref="NotSupportedException">
        /// The <see cref="ObservableQueue{T}"/> is read-only.
        /// </exception>
        public void Push(T item)
        {
            if (IsDisposed)
            {
                return;
            }
            if (IsReadOnly)
            {
                throw ReadOnlyException(nameof(Push));
            }

            InsertItem(0, item, out _);
        }

        /// <summary>
        /// Inserts multiple objects at the top of the <see cref="ObservableStack{T}"/> atomically.
        /// </summary>
        /// <param name="items">
        /// The objects to push onto the <see cref="ObservableStack{T}"/>.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="items"/> is a null reference.
        /// </exception>
        /// <exception cref="NotSupportedException">
        /// The <see cref="ObservableQueue{T}"/> is read-only.
        /// </exception>
        public void PushRange(T[] items)
        {
            if (IsDisposed)
            {
                return;
            }
            if (IsReadOnly)
            {
                throw ReadOnlyException(nameof(PushRange));
            }

            InsertItems(0, items.Reverse(), out _);
        }

        /// <summary>
        /// Inserts multiple objects at the top of the <see cref="ObservableStack{T}"/> atomically.
        /// </summary>
        /// <param name="items">
        /// The objects to push onto the <see cref="ObservableStack{T}"/>.
        /// </param>
        /// <param name="startIndex">
        /// The zero-based offset in items at which to begin inserting elements onto the top of the <see cref="ObservableStack{T}"/>.
        /// </param>
        /// <param name="count">
        /// The number of elements to be inserted onto the top of the <see cref="ObservableStack{T}"/>.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="items"/> is a null reference.
        /// </exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// <paramref name="startIndex"/> or <paramref name="count"/> is negative. Or <paramref name="startIndex"/> is greater than or equal to the length of <paramref name="items"/>.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// <paramref name="startIndex"/> + <paramref name="count"/> is greater than the length of <paramref name="items"/>.
        /// </exception>
        /// <exception cref="NotSupportedException">
        /// The <see cref="ObservableQueue{T}"/> is read-only.
        /// </exception>
        public void PushRange(T[] items, int startIndex, int count)
        {
            if (IsDisposed)
            {
                return;
            }
            if (IsReadOnly)
            {
                throw ReadOnlyException(nameof(PushRange));
            }
            if (items == null)
            {
                throw new ArgumentNullException(nameof(items));
            }
            if (startIndex < 0 || startIndex >= items.Length)
            {
                throw new ArgumentOutOfRangeException(nameof(startIndex));
            }
            if (count < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(count));
            }
            if (startIndex + count > items.Length)
            {
                throw new ArgumentException(nameof(startIndex) + nameof(count) + "is greater than the length of " + nameof(items));
            }

            T[] ranged = new T[count];

            Array.Copy(items, startIndex, ranged, 0, count);

            InsertItems(0, ranged.Reverse(), out _);
        }

        /// <summary>
        /// Copies the <see cref="ObservableStack{T}"/> to a new array.
        /// </summary>
        /// <returns>
        /// A new array containing copies of the elements of the <see cref="ObservableStack{T}"/>.
        /// </returns>
        public T[] ToArray()
        {
            if (IsDisposed)
            {
                return default;
            }

            return RWLock.LockRead(() => Items.ToArray());
        }

        /// <summary>
        /// Sets the capacity to the actual number of elements in the <see cref="ObservableStack{T}"/>, if that number is less than 90 percent of current capacity.
        /// </summary>
        public void TrimExcess()
        {
            if (IsDisposed)
            {
                return;
            }

            Items.TrimExcess();
        }

        /// <summary>
        /// Attempts to return an object from the top of the <see cref="ObservableStack{T}"/> without removing it.
        /// </summary>
        /// <param name="result">
        /// When this method returns, result contains an object from the top of the <see cref="ObservableStack{T}"/> or an unspecified value if the operation failed.
        /// </param>
        /// <returns>
        /// <c>true</c> if and object was returned successfully; otherwise, <c>false</c>.
        /// </returns>
        public bool TryPeek(out T result)
        {
            result = default;

            if (IsDisposed)
            {
                return default;
            }

            T proxy = default;
            bool ret = RWLock.LockRead(() =>
            {
                if (Count == 0)
                {
                    return false;
                }
                else
                {
                    proxy = this[0];
                    return true;
                }
            });
            result = proxy;
            return ret;
        }
        /// <summary>
        /// Attempts to pop and return the object at the top of the <see cref="ObservableStack{T}"/>.
        /// </summary>
        /// <param name="result">
        /// When this method returns, if the operation was successful, result contains the object removed. If no object was available to be removed, the value is unspecified.
        /// </param>
        /// <returns>
        /// <c>true</c> if an element was removed and returned from the top of the <see cref="ObservableStack{T}"/> successfully; otherwise, <c>false</c>.
        /// </returns>
        /// <exception cref="NotSupportedException">
        /// The <see cref="ObservableQueue{T}"/> is read-only.
        /// </exception>
        public bool TryPop(out T result)
        {
            result = default;

            if (IsDisposed)
            {
                return default;
            }
            if (IsReadOnly)
            {
                throw ReadOnlyException(nameof(TryPop));
            }

            T proxy = default;
            bool ret = RWLock.LockUpgradeableRead(() =>
            {
                if (Count == 0)
                {
                    return false;
                }
                else
                {
                    return RemoveItem(0, out proxy);
                }
            });
            result = proxy;
            return ret;
        }

        /// <summary>
        /// Attempts to pop and return multiple objects from the top of the <see cref="ObservableStack{T}"/> atomically.
        /// </summary>
        /// <param name="items">
        /// The <see cref="Array"/> to which objects popped from the top of the <see cref="ObservableStack{T}"/> will be added.
        /// </param>
        /// <returns>
        /// The number of objects successfully popped from the top of the <see cref="ObservableStack{T}"/> and inserted in items.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="items"/> is a null argument.
        /// </exception>
        /// <exception cref="NotSupportedException">
        /// The <see cref="ObservableQueue{T}"/> is read-only.
        /// </exception>
        public int TryPopRange(T[] items)
        {
            return TryPopRange(items, 0, items?.Length ?? 0);
        }

        /// <summary>
        /// Attempts to pop and return multiple objects from the top of the <see cref="ObservableStack{T}"/> atomically.
        /// </summary>
        /// <param name="items">
        /// The <see cref="Array"/> to which objects popped from the top of the <see cref="ObservableStack{T}"/> will be added.
        /// </param>
        /// <param name="startIndex">
        /// The zero-based offset in <paramref name="items"/> at which to begin inserting elements from the top of the <see cref="ObservableStack{T}"/>.
        /// </param>
        /// <param name="count">
        /// The number of elements to be popped from top of the <see cref="ObservableStack{T}"/> and inserted into items.
        /// </param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="items"/> is a null reference.
        /// </exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// <paramref name="startIndex"/> or <paramref name="count"/> is negative. Or <paramref name="startIndex"/> is greater than or equal to the length of items.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// <paramref name="startIndex"/> + <paramref name="count"/> is greater than the length of <paramref name="items"/>.
        /// </exception>
        /// <exception cref="NotSupportedException">
        /// The <see cref="ObservableQueue{T}"/> is read-only.
        /// </exception>
        public int TryPopRange(T[] items, int startIndex, int count)
        {
            if (IsDisposed)
            {
                return default;
            }
            if (IsReadOnly)
            {
                throw ReadOnlyException(nameof(TryPopRange));
            }
            if (items == null)
            {
                throw new ArgumentNullException(nameof(items));
            }
            if (startIndex < 0 || startIndex >= items.Length)
            {
                throw new ArgumentOutOfRangeException(nameof(startIndex));
            }
            if (count < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(count));
            }
            if (startIndex + count > items.Length)
            {
                throw new ArgumentException(nameof(startIndex) + nameof(count) + "is greater than the length of " + nameof(items));
            }

            return RWLock.LockUpgradeableRead(() =>
            {
                if (Count == 0)
                {
                    return 0;
                }
                else if (Count <= count)
                {
                    int lastCount = Count;
                    RemoveItems(0, lastCount, out IEnumerable<T> removedItems);
                    foreach (T item in removedItems)
                    {
                        items[startIndex++] = item;
                    }
                    return lastCount;
                }
                else
                {
                    RemoveItems(0, count, out IEnumerable<T> removedItems);
                    foreach (T item in removedItems)
                    {
                        items[startIndex++] = item;
                    }
                    return count;
                }
            });
        }

        #endregion

        #region ObservableCollectionBase<T> Members

        /// <summary>
        /// Returns an enumerator for the <see cref="ObservableStack{T}"/>.
        /// </summary>
        /// <returns>
        /// An <see cref="ObservableStack{T}.StackEnumerator"/> for the <see cref="ObservableStack{T}"/>.
        /// </returns>
        public override IEnumerator<T> GetEnumerator()
        {
            if (IsDisposed)
            {
                return default;
            }

            return new StackEnumerator(this);
        }

        #endregion

        #region IProducerConsumerCollection<T> Members

        /// <inheritdoc/>
        bool IProducerConsumerCollection<T>.TryAdd(T item)
        {
            Push(item);
            return true;
        }

        /// <inheritdoc/>
        bool IProducerConsumerCollection<T>.TryTake(out T item)
        {
            return TryPop(out item);
        }

        #endregion

        #region Helper Classes

        /// <summary>
        /// Enumerates the elements of a <see cref="ObservableStack{T}"/>.
        /// </summary>
        public struct StackEnumerator : IEnumerator<T>
        {
            #region Properties

            /// <inheritdoc/>
            public T Current => enumerator.Current;

            private readonly IEnumerator<T> enumerator;

            #endregion

            #region Initializers

            internal StackEnumerator(ObservableStack<T> stack)
            {
                enumerator = stack.Items.GetEnumerator();
            }

            #endregion

            #region Methods

            /// <inheritdoc/>
            public bool MoveNext() => enumerator.MoveNext();

            /// <inheritdoc/>
            public void Reset() => enumerator.Reset();

            /// <inheritdoc/>
            public void Dispose() => enumerator.Dispose();

            #endregion

            #region IEnumerator Members

            object IEnumerator.Current => enumerator.Current;

            #endregion
        }

        #endregion
    }
}
