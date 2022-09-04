using ObservableHelpers.Utilities;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace ObservableHelpers.Collections;

/// <summary>
/// Provides a thread-safe observable collection used for data binding.
/// </summary>
/// <typeparam name="T">
/// Specifies the type of the items in this collection.
/// </typeparam>
public class ObservableConcurrentStack<T> :
    ObservableConcurrentCollectionBase<T>,
    IProducerConsumerCollection<T>
{
    #region Properties

    /// <summary>
    /// Gets a value that indicates whether the <see cref="ObservableConcurrentStack{T}"/> is empty.
    /// </summary>
    public bool IsEmpty  => Count == 0;

    #endregion

    #region Initializers

    /// <summary>
    /// Initializes a new instance of the <see cref="ObservableConcurrentStack{T}"/> class that contains empty elements and has sufficient capacity to accommodate the number of elements copied.
    /// </summary>
    public ObservableConcurrentStack()
        : base()
    {

    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ObservableConcurrentStack{T}"/> class that contains elements copied from the specified collection and has sufficient capacity to accommodate the number of elements copied.
    /// </summary>
    /// <param name="enumerable">
    /// The collection whose elements are copied to the new list.
    /// </param>
    /// <remarks>
    /// The elements are copied onto the <see cref="ObservableConcurrentStack{T}"/> in the same order they are read by the enumerator of the collection.
    /// </remarks>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="enumerable"/> is a null reference.
    /// </exception>
    public ObservableConcurrentStack(IEnumerable<T> enumerable)
        : base(_ => new List<T>(enumerable.Reverse()))
    {

    }

    #endregion

    #region Methods

    /// <summary>
    /// Removes all objects from the <see cref="ObservableConcurrentStack{T}"/>.
    /// </summary>
    /// <exception cref="NotSupportedException">
    /// The <see cref="ObservableConcurrentQueue{T}"/> is read-only.
    /// </exception>
    public void Clear()
    {
        ThrowIfReadOnly(nameof(Clear));

        ClearItems(out _);
    }

    /// <summary>
    /// Returns the object at the top of the <see cref="ObservableConcurrentStack{T}"/> without removing it.
    /// </summary>
    /// <returns>
    /// The object at the top of the <see cref="ObservableConcurrentStack{T}"/>.
    /// </returns>
    /// <exception cref="InvalidOperationException">
    /// The <see cref="ObservableConcurrentStack{T}"/> is empty.
    /// </exception>
    public T Peek()
    {
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
    /// Removes and returns the object at the top of the <see cref="ObservableConcurrentStack{T}"/>.
    /// </summary>
    /// <returns>
    /// The object removed from the top of the <see cref="ObservableConcurrentStack{T}"/>.
    /// </returns>
    /// <exception cref="InvalidOperationException">
    /// The <see cref="ObservableConcurrentStack{T}"/> is empty.
    /// </exception>
    /// <exception cref="NotSupportedException">
    /// The <see cref="ObservableConcurrentQueue{T}"/> is read-only.
    /// </exception>
    public T Pop()
    {
        ThrowIfReadOnly(nameof(Pop));

        return RWLock.LockUpgradeableRead(() =>
        {
            if (Count == 0)
            {
                throw new InvalidOperationException("The stack is empty");
            }
            else
            {
                if (!RemoveItem(0, out T? item) || item == null)
                {
                    throw new InvalidOperationException("The stack is empty");
                }
                return item;
            }
        });
    }

    /// <summary>
    /// Inserts an object at the top of the <see cref="ObservableConcurrentStack{T}"/>.
    /// </summary>
    /// <param name="item">
    /// The object to push onto the <see cref="ObservableConcurrentStack{T}"/>. The value can be null for reference types.
    /// </param>
    /// <exception cref="NotSupportedException">
    /// The <see cref="ObservableConcurrentQueue{T}"/> is read-only.
    /// </exception>
    public void Push(T item)
    {
        ThrowIfReadOnly(nameof(Push));

        InsertItem(0, item, out _);
    }

    /// <summary>
    /// Inserts multiple objects at the top of the <see cref="ObservableConcurrentStack{T}"/> atomically.
    /// </summary>
    /// <param name="items">
    /// The objects to push onto the <see cref="ObservableConcurrentStack{T}"/>.
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="items"/> is a null reference.
    /// </exception>
    /// <exception cref="NotSupportedException">
    /// The <see cref="ObservableConcurrentQueue{T}"/> is read-only.
    /// </exception>
    public void PushRange(T[] items)
    {
        ThrowIfReadOnly(nameof(PushRange));

        InsertItems(0, items.Reverse(), out _);
    }

    /// <summary>
    /// Inserts multiple objects at the top of the <see cref="ObservableConcurrentStack{T}"/> atomically.
    /// </summary>
    /// <param name="items">
    /// The objects to push onto the <see cref="ObservableConcurrentStack{T}"/>.
    /// </param>
    /// <param name="startIndex">
    /// The zero-based offset in items at which to begin inserting elements onto the top of the <see cref="ObservableConcurrentStack{T}"/>.
    /// </param>
    /// <param name="count">
    /// The number of elements to be inserted onto the top of the <see cref="ObservableConcurrentStack{T}"/>.
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
    /// The <see cref="ObservableConcurrentQueue{T}"/> is read-only.
    /// </exception>
    public void PushRange(T[] items, int startIndex, int count)
    {
        ArgumentNullException.ThrowIfNull(items);
        ThrowIfReadOnly(nameof(PushRange));
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
    /// Copies the <see cref="ObservableConcurrentStack{T}"/> to a new array.
    /// </summary>
    /// <returns>
    /// A new array containing copies of the elements of the <see cref="ObservableConcurrentStack{T}"/>.
    /// </returns>
    public T[] ToArray()
    {
        return RWLock.LockRead(() => Items.ToArray());
    }

    /// <summary>
    /// Sets the capacity to the actual number of elements in the <see cref="ObservableConcurrentStack{T}"/>, if that number is less than 90 percent of current capacity.
    /// </summary>
    public void TrimExcess()
    {
        Items.TrimExcess();
    }

    /// <summary>
    /// Attempts to return an object from the top of the <see cref="ObservableConcurrentStack{T}"/> without removing it.
    /// </summary>
    /// <param name="result">
    /// When this method returns, result contains an object from the top of the <see cref="ObservableConcurrentStack{T}"/> or an unspecified value if the operation failed.
    /// </param>
    /// <returns>
    /// <c>true</c> if and object was returned successfully; otherwise, <c>false</c>.
    /// </returns>
    public bool TryPeek([MaybeNullWhen(false)] out T result)
    {
        result = default;

        T? proxy = default;
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
    /// Attempts to pop and return the object at the top of the <see cref="ObservableConcurrentStack{T}"/>.
    /// </summary>
    /// <param name="result">
    /// When this method returns, if the operation was successful, result contains the object removed. If no object was available to be removed, the value is unspecified.
    /// </param>
    /// <returns>
    /// <c>true</c> if an element was removed and returned from the top of the <see cref="ObservableConcurrentStack{T}"/> successfully; otherwise, <c>false</c>.
    /// </returns>
    /// <exception cref="NotSupportedException">
    /// The <see cref="ObservableConcurrentQueue{T}"/> is read-only.
    /// </exception>
    public bool TryPop([MaybeNullWhen(false)] out T result)
    {
        result = default;

        ThrowIfReadOnly(nameof(TryPop));

        T? proxy = default;
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
    /// Attempts to pop and return multiple objects from the top of the <see cref="ObservableConcurrentStack{T}"/> atomically.
    /// </summary>
    /// <param name="items">
    /// The <see cref="Array"/> to which objects popped from the top of the <see cref="ObservableConcurrentStack{T}"/> will be added.
    /// </param>
    /// <returns>
    /// The number of objects successfully popped from the top of the <see cref="ObservableConcurrentStack{T}"/> and inserted in items.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="items"/> is a null argument.
    /// </exception>
    /// <exception cref="NotSupportedException">
    /// The <see cref="ObservableConcurrentQueue{T}"/> is read-only.
    /// </exception>
    public int TryPopRange(T[] items)
    {
        return TryPopRange(items, 0, items?.Length ?? 0);
    }

    /// <summary>
    /// Attempts to pop and return multiple objects from the top of the <see cref="ObservableConcurrentStack{T}"/> atomically.
    /// </summary>
    /// <param name="items">
    /// The <see cref="Array"/> to which objects popped from the top of the <see cref="ObservableConcurrentStack{T}"/> will be added.
    /// </param>
    /// <param name="startIndex">
    /// The zero-based offset in <paramref name="items"/> at which to begin inserting elements from the top of the <see cref="ObservableConcurrentStack{T}"/>.
    /// </param>
    /// <param name="count">
    /// The number of elements to be popped from top of the <see cref="ObservableConcurrentStack{T}"/> and inserted into items.
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
    /// The <see cref="ObservableConcurrentQueue{T}"/> is read-only.
    /// </exception>
    public int TryPopRange(T[] items, int startIndex, int count)
    {
        ArgumentNullException.ThrowIfNull(items);
        ThrowIfReadOnly(nameof(TryPopRange));

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
                RemoveItems(0, lastCount, out IEnumerable<T>? removedItems);
                if (removedItems != null)
                {
                    foreach (T item in removedItems)
                    {
                        items[startIndex++] = item;
                    }
                }
                return lastCount;
            }
            else
            {
                RemoveItems(0, count, out IEnumerable<T>? removedItems);
                if (removedItems != null)
                {
                    foreach (T item in removedItems)
                    {
                        items[startIndex++] = item;
                    }
                }
                return count;
            }
        });
    }

    #endregion

    #region ObservableCollectionBase<T> Members

    /// <summary>
    /// Returns an enumerator for the <see cref="ObservableConcurrentStack{T}"/>.
    /// </summary>
    /// <returns>
    /// An <see cref="ObservableConcurrentStack{T}.StackEnumerator"/> for the <see cref="ObservableConcurrentStack{T}"/>.
    /// </returns>
    public override IEnumerator<T> GetEnumerator()
    {
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
#pragma warning disable CS8769 // Nullability of reference types in type of parameter doesn't match implemented member (possibly because of nullability attributes).
    bool IProducerConsumerCollection<T>.TryTake([MaybeNullWhen(false)] out T item)
#pragma warning restore CS8769 // Nullability of reference types in type of parameter doesn't match implemented member (possibly because of nullability attributes).
    {
        return TryPop(out item);
    }

#endregion

    #region Helper Classes

    /// <summary>
    /// Enumerates the elements of a <see cref="ObservableConcurrentStack{T}"/>.
    /// </summary>
    public struct StackEnumerator : IEnumerator<T>
    {
        #region Properties

        /// <inheritdoc/>
        public T Current => enumerator.Current;

        private readonly IEnumerator<T> enumerator;

        #endregion

        #region Initializers

        internal StackEnumerator(ObservableConcurrentStack<T> stack)
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

        object? IEnumerator.Current => enumerator.Current;

        #endregion
    }

    #endregion
}
