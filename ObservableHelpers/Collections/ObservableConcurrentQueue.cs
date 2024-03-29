﻿using ObservableHelpers.Utilities;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace ObservableHelpers.Collections;

/// <summary>
/// Provides a thread-safe observable first-in, first-out collection of objects used for data binding.
/// </summary>
/// <typeparam name="T">
/// Specifies the type of the items in this queue.
/// </typeparam>
public class ObservableConcurrentQueue<T> :
    ObservableConcurrentCollectionBase<T>,
    IProducerConsumerCollection<T>,
    IEnumerable<T>,
    ICollection
{
    #region Properties

    /// <summary>
    /// Gets a value that indicates whether the <see cref="ObservableConcurrentQueue{T}"/> is empty.
    /// </summary>
    public bool IsEmpty => Count == 0;

    #endregion

    #region Initializers

    /// <summary>
    /// Initializes a new instance of the <see cref="ObservableConcurrentQueue{T}"/> class that contains empty elements and has sufficient capacity to accommodate the number of elements copied.
    /// </summary>
    public ObservableConcurrentQueue()
        : base()
    {

    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ObservableConcurrentQueue{T}"/> class that contains elements copied from the specified collection and has sufficient capacity to accommodate the number of elements copied.
    /// </summary>
    /// <param name="enumerable">
    /// The collection whose elements are copied to the new list.
    /// </param>
    /// <remarks>
    /// The elements are copied onto the <see cref="ObservableConcurrentQueue{T}"/> in the same order they are read by the enumerator of the collection.
    /// </remarks>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="enumerable"/> is a null reference.
    /// </exception>
    public ObservableConcurrentQueue(IEnumerable<T> enumerable)
        : base(_ => new List<T>(enumerable))
    {

    }

    #endregion

    #region Methods

    /// <summary>
    /// Removes all objects from the <see cref="ObservableConcurrentQueue{T}"/>.
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
    /// Removes and returns the object at the beginning of the <see cref="ObservableConcurrentQueue{T}"/>.
    /// </summary>
    /// <returns>
    /// The object that is removed from the beginning of the <see cref="ObservableConcurrentQueue{T}"/>.
    /// </returns>
    /// <exception cref="InvalidOperationException">
    /// The <see cref="ObservableConcurrentQueue{T}"/> is empty.
    /// </exception>
    /// <exception cref="NotSupportedException">
    /// The <see cref="ObservableConcurrentQueue{T}"/> is read-only.
    /// </exception>
    public T Dequeue()
    {
        ThrowIfReadOnly(nameof(Dequeue));

        return RWLock.LockUpgradeableRead(() =>
        {
            if (Count == 0)
            {
                throw new InvalidOperationException("The queue is empty");
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
    /// Adds an object to the end of the <see cref="ObservableConcurrentQueue{T}"/>.
    /// </summary>
    /// <param name="item">
    /// The object to add to the end of the <see cref="ObservableConcurrentQueue{T}"/>. The value can be a null reference (Nothing in Visual Basic) for reference types.
    /// </param>
    /// <exception cref="NotSupportedException">
    /// The <see cref="ObservableConcurrentQueue{T}"/> is read-only.
    /// </exception>
    public void Enqueue(T item)
    {
        ThrowIfReadOnly(nameof(Enqueue));

        InsertItem(Count, item, out _);
    }

    /// <summary>
    /// Adds multiple objects at the top of the <see cref="ObservableConcurrentQueue{T}"/> atomically.
    /// </summary>
    /// <param name="items">
    /// The objects to add onto the <see cref="ObservableConcurrentQueue{T}"/>.
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="items"/> is a null reference.
    /// </exception>
    /// <exception cref="NotSupportedException">
    /// The <see cref="ObservableConcurrentQueue{T}"/> is read-only.
    /// </exception>
    public void EnqueueRange(T[] items)
    {
        ThrowIfReadOnly(nameof(EnqueueRange));

        InsertItems(Count, items, out _);
    }

    /// <summary>
    /// Adds multiple objects at the top of the <see cref="ObservableConcurrentQueue{T}"/> atomically.
    /// </summary>
    /// <param name="items">
    /// The objects to add onto the <see cref="ObservableConcurrentQueue{T}"/>.
    /// </param>
    /// <param name="startIndex">
    /// The zero-based offset in items at which to begin inserting elements onto the top of the <see cref="ObservableConcurrentQueue{T}"/>.
    /// </param>
    /// <param name="count">
    /// The number of elements to be inserted onto the top of the <see cref="ObservableConcurrentQueue{T}"/>.
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
    public void EnqueueRange(T[] items, int startIndex, int count)
    {
        ArgumentNullException.ThrowIfNull(items);
        ThrowIfReadOnly(nameof(EnqueueRange));
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

        InsertItems(Count, ranged, out _);
    }

    /// <summary>
    /// Returns the object at the beginning of the <see cref="ObservableConcurrentQueue{T}"/> without removing it.
    /// </summary>
    /// <returns>
    /// The object at the beginning of the <see cref="ObservableConcurrentQueue{T}"/>.
    /// </returns>
    /// <exception cref="InvalidOperationException">
    /// The <see cref="ObservableConcurrentQueue{T}"/> is empty.
    /// </exception>
    public T Peek()
    {
        return RWLock.LockRead(() =>
        {
            if (Count == 0)
            {
                throw new InvalidOperationException("The queue is empty");
            }
            else
            {
                return this[0];
            }
        });
    }

    /// <summary>
    /// Copies the elements stored in the <see cref="ObservableConcurrentQueue{T}"/> to a new array.
    /// </summary>
    /// <returns>
    /// A new array containing a snapshot of elements copied from the <see cref="ObservableConcurrentQueue{T}"/>.
    /// </returns>
    public T[] ToArray()
    {
        return RWLock.LockRead(() => Items.ToArray());
    }

    /// <summary>
    /// Sets the capacity to the actual number of elements in the <see cref="ObservableConcurrentQueue{T}"/>, if that number is less than 90 percent of current capacity.
    /// </summary>
    public void TrimExcess()
    {
        Items.TrimExcess();
    }

    /// <summary>
    /// Tries to remove and return the object at the beginning of the concurrent queue.
    /// </summary>
    /// <param name="result">
    /// When this method returns, if the operation was successful, result contains the object removed. If no object was available to be removed, the value is unspecified.
    /// </param>
    /// <returns>
    /// <c>true</c> if an element was removed and returned from the beginning of the <see cref="ObservableConcurrentQueue{T}"/> successfully; otherwise, <c>false</c>.
    /// </returns>
    /// <exception cref="NotSupportedException">
    /// The <see cref="ObservableConcurrentQueue{T}"/> is read-only.
    /// </exception>
    public bool TryDequeue([MaybeNullWhen(false)] out T result)
    {
        result = default;

        ThrowIfReadOnly(nameof(TryDequeue));

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
    /// Attempts to dequeue and return multiple objects from the beginning of the <see cref="ObservableConcurrentQueue{T}"/> atomically.
    /// </summary>
    /// <param name="items">
    /// The <see cref="Array"/> to which objects dequeued from the beginning of the <see cref="ObservableConcurrentQueue{T}"/> will be added.
    /// </param>
    /// <returns>
    /// The number of objects successfully dequeued from the beginning of the <see cref="ObservableConcurrentQueue{T}"/> and inserted in items.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="items"/> is a null argument.
    /// </exception>
    /// <exception cref="NotSupportedException">
    /// The <see cref="ObservableConcurrentQueue{T}"/> is read-only.
    /// </exception>
    public int TryDequeueRange(T[] items)
    {
        return TryDequeueRange(items, 0, items?.Length ?? 0);
    }

    /// <summary>
    /// Attempts to pop and return multiple objects from the beginning of the <see cref="ObservableConcurrentQueue{T}"/> atomically.
    /// </summary>
    /// <param name="items">
    /// The <see cref="Array"/> to which objects dequeued from the beginning of the <see cref="ObservableConcurrentQueue{T}"/> will be added.
    /// </param>
    /// <param name="startIndex">
    /// The zero-based offset in <paramref name="items"/> at which to begin inserting elements from the beginning of the <see cref="ObservableConcurrentQueue{T}"/>.
    /// </param>
    /// <param name="count">
    /// The number of elements to be dequeued from beginning of the <see cref="ObservableConcurrentQueue{T}"/> and inserted into items.
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
    public int TryDequeueRange(T[] items, int startIndex, int count)
    {
        ArgumentNullException.ThrowIfNull(items);
        ThrowIfReadOnly(nameof(TryDequeueRange));
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

    /// <summary>
    /// Tries to return an object from the beginning of the <see cref="ObservableConcurrentQueue{T}"/> without removing it.
    /// </summary>
    /// <param name="result">
    /// When this method returns, result contains an object from the beginning of the <see cref="ObservableConcurrentQueue{T}"/> or an unspecified value if the operation failed.
    /// </param>
    /// <returns>
    /// <c>true</c> if an object was returned successfully; otherwise, <c>false</c>.
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

    #endregion

    #region ObservableCollection<T> Members

    /// <inheritdoc/>
    public override IEnumerator<T> GetEnumerator()
    {
        return new QueueEnumerator(this);
    }

    #endregion

    #region IProducerConsumerCollection<T>

    /// <inheritdoc/>
    bool IProducerConsumerCollection<T>.TryAdd(T item)
    {
        Enqueue(item);

        return true;
    }

    /// <inheritdoc/>
#pragma warning disable CS8769 // Nullability of reference types in type of parameter doesn't match implemented member (possibly because of nullability attributes).
    bool IProducerConsumerCollection<T>.TryTake([MaybeNullWhen(false)] out T item)
#pragma warning restore CS8769 // Nullability of reference types in type of parameter doesn't match implemented member (possibly because of nullability attributes).
    {
        return TryDequeue(out item);
    }

#endregion

    #region Helper Classes

    /// <summary>
    /// Enumerates the elements of a <see cref="ObservableConcurrentQueue{T}"/>.
    /// </summary>
    public struct QueueEnumerator : IEnumerator<T>
    {
        #region Properties

        /// <inheritdoc/>
        public T Current => enumerator.Current;

        private readonly IEnumerator<T> enumerator;

        #endregion

        #region Initializers

        internal QueueEnumerator(ObservableConcurrentQueue<T> queue)
        {
            enumerator = queue.Items.GetEnumerator();
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
