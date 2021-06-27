﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ObservableHelpers
{
    /// <summary>
    /// Provides operations for <see cref="ObservableHelpers.SyncOperation"/> with proper disposable implementations.
    /// </summary>
    public class SyncContext : Disposable, ISyncObject
    {
        /// <inheritdoc/>
        public SyncOperation SyncOperation { get; private set; }

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

        /// <summary>
        /// Post action to the current context.
        /// </summary>
        /// <param name="action">
        /// The action to be executed at the current context.
        /// </param>
        /// <param name="parameters">
        /// The parameters to be pass at the current context.
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
        /// Send action to the current context.
        /// </summary>
        /// <param name="action">
        /// The action to be executed at the current context.
        /// </param>
        /// <param name="parameters">
        /// The parameters to be pass at the current context.
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
        /// Send action to the current context asynchronously.
        /// </summary>
        /// <param name="action">
        /// The action to be executed at the current context.
        /// </param>
        /// <param name="parameters">
        /// The parameters to be pass at the current context.
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
        /// Send action to the current context asynchronously.
        /// </summary>
        /// <param name="func">
        /// The action to be executed at the current context.
        /// </param>
        /// <param name="parameters">
        /// The parameters to be pass at the current context.
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
    }
}
