using ObservableHelpers.Abstraction;
using System;
using System.Threading.Tasks;

namespace ObservableHelpers
{
    /// <summary>
    /// Provides operations for <see cref="ObservableHelpers.SyncOperation"/> with proper disposable implementations.
    /// </summary>
    public class SyncContext :
        Disposable,
        ISyncObject
    {
        #region Properties

        /// <inheritdoc/>
        public SyncOperation SyncOperation { get; private set; }

        #endregion

        #region Initializers

        /// <summary>
        /// Creates new instance of the <c>SyncContext</c> class.
        /// </summary>
        /// <remarks>
        /// <para>To use safely in UI operations, create the instance in UI thread.</para>
        /// <para>See <see cref="ObservableHelpers.SyncOperation"/></para>
        /// </remarks>
        public SyncContext()
        {
            SyncOperation = new SyncOperation();
        }

        #endregion

        #region Methods

        /// <summary>
        /// Executes <paramref name="action"/> to the current synchronization context without blocking the executing thread.
        /// </summary>
        /// <param name="action">
        /// The action to be executed at the current synchronization context.
        /// </param>
        /// <param name="parameters">
        /// The parameters to be pass at the current synchronization context.
        /// </param>
        protected void ContextPost(Action action, params object[] parameters)
        {
            if (IsDisposed)
            {
                return;
            }
            SyncOperation.ContextPost(() =>
            {
                if (IsDisposed)
                {
                    return;
                }
                action();
            }, parameters);
        }

        /// <summary>
        /// Executes <paramref name="action"/> to the current synchronization context and blocking the executing thread until the <paramref name="action"/> ended.
        /// </summary>
        /// <param name="action">
        /// The action to be executed at the current synchronization context.
        /// </param>
        /// <param name="parameters">
        /// The parameters to be pass at the current synchronization context.
        /// </param>
        protected void ContextSend(Action action, params object[] parameters)
        {
            if (IsDisposed)
            {
                return;
            }
            SyncOperation.ContextSend(() =>
            {
                if (IsDisposed)
                {
                    return;
                }
                action();
            }, parameters);
        }

        /// <summary>
        /// Executes <paramref name="action"/> to the current synchronization context asynchronously.
        /// </summary>
        /// <param name="action">
        /// The action to be executed at the current synchronization context.
        /// </param>
        /// <param name="parameters">
        /// The parameters to be pass at the current synchronization context.
        /// </param>
        /// <returns>
        /// A <see cref="Task"/> that represents a proxy for the task returned by <paramref name="action"/>.
        /// </returns>
        protected async Task ContextSendAsync(Action action, params object[] parameters)
        {
            if (IsDisposed)
            {
                return;
            }
            await SyncOperation.ContextSendAsync(() =>
            {
                if (IsDisposed)
                {
                    return;
                }
                action();
            }, parameters);
        }

        /// <summary>
        /// Executes <paramref name="func"/> to the current synchronization context asynchronously.
        /// </summary>
        /// <param name="func">
        /// The action to be executed at the current synchronization context.
        /// </param>
        /// <param name="parameters">
        /// The parameters to be pass at the current synchronization context.
        /// </param>
        /// <returns>
        /// A <see cref="Task"/> that represents a proxy for the task returned by <paramref name="func"/>.
        /// </returns>
        protected async Task ContextSendAsync(Func<Task> func, params object[] parameters)
        {
            if (IsDisposed)
            {
                return;
            }
            await SyncOperation.ContextSendAsync(async () =>
            {
                if (IsDisposed)
                {
                    return;
                }
                await func();
            }, parameters);
        }

        #endregion
    }
}
