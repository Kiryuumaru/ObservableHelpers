using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

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
        protected ReaderWriterLockSlim ReaderWriterLockSlim { get; }

        private readonly ReaderWriterLockSlim lockLocker = new ReaderWriterLockSlim(LockRecursionPolicy.SupportsRecursion);

        private event Action OnLockFree;
        private event Action OnWriteLockFree;
        private event Action OnReadLockFree;
        private event Action OnUpgradeableReadLockFree;

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
        /// Locks read operations while executing the <paramref name="block"/> action.
        /// </summary>
        /// <param name="block">
        /// The action to be executed inside the lock block.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="block"/> is a null reference.
        /// </exception>
        public void LockReadAndForget(Action block)
        {
            if (block == null)
            {
                throw new ArgumentNullException(nameof(block));
            }

            bool isLocked = false;

            Task.Run(() =>
            {
                LockRead(() =>
                {
                    isLocked = true;
                    block();
                    return 0;
                });
            });

            while (!isLocked)
            {
                Thread.Sleep(1);
            }
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
                ReaderWriterLockSlim.EnterReadLock();
                return block();
            }
            finally
            {
                ReaderWriterLockSlim.ExitReadLock();
                TryInvokeOnReadLockExit();
                TryInvokeOnLockExit();
            }
        }

        /// <summary>
        /// Locks read operations while executing the <paramref name="block"/> action while having an option to upgrade to write mode.
        /// </summary>
        /// <param name="block">
        /// The action to be executed inside the lock block.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="block"/> is a null reference.
        /// </exception>
        public void LockUpgradeableRead(Action block)
        {
            if (block == null)
            {
                throw new ArgumentNullException(nameof(block));
            }

            LockUpgradeableRead(() =>
            {
                block();
                return 0;
            });
        }

        /// <summary>
        /// Locks read operations while executing the <paramref name="block"/> action while having an option to upgrade to write mode.
        /// </summary>
        /// <param name="block">
        /// The action to be executed inside the lock block.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="block"/> is a null reference.
        /// </exception>
        public void LockUpgradeableReadAndForget(Action block)
        {
            if (block == null)
            {
                throw new ArgumentNullException(nameof(block));
            }

            bool isLocked = false;

            LockUpgradeableRead(() =>
            {
                isLocked = true;
                block();
                return 0;
            });

            while (!isLocked)
            {
                Thread.Sleep(1);
            }
        }

        /// <summary>
        /// Locks read operations while executing the <paramref name="block"/> function while having an option to upgrade to write mode.
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
        public TReturn LockUpgradeableRead<TReturn>(Func<TReturn> block)
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
                TryInvokeOnUpgradeableReadLockExit();
                TryInvokeOnLockExit();
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
        /// Locks write operations while executing the <paramref name="block"/> action.
        /// </summary>
        /// <param name="block">
        /// The action to be executed inside the lock block.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="block"/> is a null reference.
        /// </exception>
        public void LockWriteAndForget(Action block)
        {
            if (block == null)
            {
                throw new ArgumentNullException(nameof(block));
            }

            bool isLocked = false;

            LockWrite(() =>
            {
                isLocked = true;
                block();
                return 0;
            });

            while (!isLocked)
            {
                Thread.Sleep(1);
            }
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
                TryInvokeOnWriteLockExit();
                TryInvokeOnLockExit();
            }
        }

        /// <summary>
        /// Invoke <see cref="Action"/> on read lock exit.
        /// </summary>
        /// <param name="action">
        /// The <see cref="Action"/> to invoke on read lock exit.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="action"/> is a null reference.
        /// </exception>
        public void InvokeOnReadLockExit(Action action)
        {
            if (action == null)
            {
                throw new ArgumentNullException(nameof(action));
            }

            bool isFree = true;

            try
            {
                lockLocker.EnterUpgradeableReadLock();
                if (ReaderWriterLockSlim.IsReadLockHeld)
                {
                    try
                    {
                        lockLocker.EnterWriteLock();
                        OnReadLockFree += action;
                    }
                    finally
                    {
                        lockLocker.ExitWriteLock();
                    }
                    isFree = false;
                }
            }
            finally
            {
                lockLocker.ExitUpgradeableReadLock();
            }

            if (isFree)
            {
                action.Invoke();
            }
        }

        /// <summary>
        /// Invoke <see cref="Action"/> on upgradeable read lock exit.
        /// </summary>
        /// <param name="action">
        /// The <see cref="Action"/> to invoke on upgradeable read lock exit.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="action"/> is a null reference.
        /// </exception>
        public void InvokeOnUpgradeableReadLockExit(Action action)
        {
            if (action == null)
            {
                throw new ArgumentNullException(nameof(action));
            }

            bool isFree = true;

            try
            {
                lockLocker.EnterUpgradeableReadLock();
                if (ReaderWriterLockSlim.IsUpgradeableReadLockHeld)
                {
                    try
                    {
                        lockLocker.EnterWriteLock();
                        OnUpgradeableReadLockFree += action;
                    }
                    finally
                    {
                        lockLocker.ExitWriteLock();
                    }
                    isFree = false;
                }
            }
            finally
            {
                lockLocker.ExitUpgradeableReadLock();
            }

            if (isFree)
            {
                action.Invoke();
            }
        }

        /// <summary>
        /// Invoke <see cref="Action"/> on write lock exit.
        /// </summary>
        /// <param name="action">
        /// The <see cref="Action"/> to invoke on write lock exit.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="action"/> is a null reference.
        /// </exception>
        public void InvokeOnWriteLockExit(Action action)
        {
            if (action == null)
            {
                throw new ArgumentNullException(nameof(action));
            }

            bool isFree = true;

            try
            {
                lockLocker.EnterUpgradeableReadLock();
                if (ReaderWriterLockSlim.IsWriteLockHeld)
                {
                    try
                    {
                        lockLocker.EnterWriteLock();
                        OnWriteLockFree += action;
                    }
                    finally
                    {
                        lockLocker.ExitWriteLock();
                    }
                    isFree = false;
                }
            }
            finally
            {
                lockLocker.ExitUpgradeableReadLock();
            }

            if (isFree)
            {
                action.Invoke();
            }
        }

        /// <summary>
        /// Invoke <see cref="Action"/> on lock exit.
        /// </summary>
        /// <param name="action">
        /// The <see cref="Action"/> to invoke on lock exit.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="action"/> is a null reference.
        /// </exception>
        public void InvokeOnLockExit(Action action)
        {
            if (action == null)
            {
                throw new ArgumentNullException(nameof(action));
            }

            bool isFree = true;

            try
            {
                lockLocker.EnterUpgradeableReadLock();
                if (ReaderWriterLockSlim.IsReadLockHeld ||
                    ReaderWriterLockSlim.IsUpgradeableReadLockHeld ||
                    ReaderWriterLockSlim.IsWriteLockHeld)
                    
                {
                    try
                    {
                        lockLocker.EnterWriteLock();
                        OnLockFree += action;
                    }
                    finally
                    {
                        lockLocker.ExitWriteLock();
                    }
                    isFree = false;
                }
            }
            finally
            {
                lockLocker.ExitUpgradeableReadLock();
            }

            if (isFree)
            {
                action.Invoke();
            }
        }

        private void TryInvokeOnReadLockExit()
        {
            Action action = null;

            try
            {
                lockLocker.EnterUpgradeableReadLock();
                if (!ReaderWriterLockSlim.IsReadLockHeld)
                {
                    action = OnReadLockFree;
                    if (action != null)
                    {
                        try
                        {
                            lockLocker.EnterWriteLock();
                            OnReadLockFree = null;
                        }
                        finally
                        {
                            lockLocker.ExitWriteLock();
                        }
                    }
                }
            }
            finally
            {
                lockLocker.ExitUpgradeableReadLock();
            }

            if (action != null)
            {
                action.Invoke();
            }
        }

        private void TryInvokeOnUpgradeableReadLockExit()
        {
            Action action = null;

            try
            {
                lockLocker.EnterUpgradeableReadLock();
                if (!ReaderWriterLockSlim.IsUpgradeableReadLockHeld)
                {
                    action = OnUpgradeableReadLockFree;
                    if (action != null)
                    {
                        try
                        {
                            lockLocker.EnterWriteLock();
                            OnUpgradeableReadLockFree = null;
                        }
                        finally
                        {
                            lockLocker.ExitWriteLock();
                        }
                    }
                }
            }
            finally
            {
                lockLocker.ExitUpgradeableReadLock();
            }

            if (action != null)
            {
                action.Invoke();
            }
        }

        private void TryInvokeOnWriteLockExit()
        {
            Action action = null;

            try
            {
                lockLocker.EnterUpgradeableReadLock();
                if (!ReaderWriterLockSlim.IsWriteLockHeld)
                {
                    action = OnWriteLockFree;
                    if (action != null)
                    {
                        try
                        {
                            lockLocker.EnterWriteLock();
                            OnWriteLockFree = null;
                        }
                        finally
                        {
                            lockLocker.ExitWriteLock();
                        }
                    }
                }
            }
            finally
            {
                lockLocker.ExitUpgradeableReadLock();
            }

            if (action != null)
            {
                action.Invoke();
            }
        }

        private void TryInvokeOnLockExit()
        {
            Action action = null;

            try
            {
                lockLocker.EnterUpgradeableReadLock();
                if (!ReaderWriterLockSlim.IsReadLockHeld &&
                    !ReaderWriterLockSlim.IsUpgradeableReadLockHeld &&
                    !ReaderWriterLockSlim.IsWriteLockHeld)
                {
                    action = OnLockFree;
                    if (action != null)
                    {
                        try
                        {
                            lockLocker.EnterWriteLock();
                            OnLockFree = null;
                        }
                        finally
                        {
                            lockLocker.ExitWriteLock();
                        }
                    }
                }
            }
            finally
            {
                lockLocker.ExitUpgradeableReadLock();
            }

            if (action != null)
            {
                action.Invoke();
            }
        }

        #endregion
    }
}
