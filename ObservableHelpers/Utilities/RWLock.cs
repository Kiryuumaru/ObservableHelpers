﻿using System;
using System.Threading;

namespace ObservableHelpers.Utilities
{
    /// <summary>
    /// Provides wrapped <see cref="ReaderWriterLockSlim"/> with convenient methods.
    /// </summary>
    public class RWLock
    {
        #region Properties

        /// <summary>
        /// Gets the wrapped <see cref="ReaderWriterLockSlim"/>.
        /// </summary>
        public ReaderWriterLockSlim ReaderWriterLockSlim { get; }

        #endregion

        #region Initializers

        /// <summary>
        /// Creates new instance of <see cref="RWLock"/> with default <see cref="LockRecursionPolicy.NoRecursion"/>.
        /// </summary>
        public RWLock()
        {
            ReaderWriterLockSlim = new ReaderWriterLockSlim(LockRecursionPolicy.NoRecursion);
        }

        /// <summary>
        /// Creates new instance of <see cref="RWLock"/>, specifying the <see cref="LockRecursionPolicy"/>.
        /// </summary>
        public RWLock(LockRecursionPolicy recursionPolicy)
        {
            ReaderWriterLockSlim = new ReaderWriterLockSlim(recursionPolicy);
        }

        #endregion

        #region Methods

        /// <summary>
        /// Locks read operations while executing the <paramref name="block"/> action.
        /// </summary>
        /// <param name="block">
        /// The action to be executed inside the lock block.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="block"/> is a null reference.
        /// </exception>
        public void LockRead(Action block)
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
        /// Locks read operations while executing the <paramref name="block"/> function.
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
        public TReturn LockRead<TReturn>(Func<TReturn> block)
        {
            if (block == null)
            {
                throw new ArgumentNullException(nameof(block));
            }

            try
            {
                ReaderWriterLockSlim.EnterUpgradeableReadLock();
                return block();
            }
            finally
            {
                ReaderWriterLockSlim.ExitUpgradeableReadLock();
            }
        }

        /// <summary>
        /// Locks write operations while executing the <paramref name="block"/> action.
        /// </summary>
        /// <param name="block">
        /// The action to be executed inside the lock block.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="block"/> is a null reference.
        /// </exception>
        public void LockWrite(Action block)
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
        /// Locks write operations while executing the <paramref name="block"/> function.
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
        public TReturn LockWrite<TReturn>(Func<TReturn> block)
        {
            if (block == null)
            {
                throw new ArgumentNullException(nameof(block));
            }

            try
            {
                ReaderWriterLockSlim.EnterWriteLock();
                return block();
            }
            finally
            {
                ReaderWriterLockSlim.ExitWriteLock();
            }
        }

        #endregion
    }
}
