using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ObservableHelpers
{
    /// <summary>
    /// <para>Contains helpers for <see cref="SynchronizationContext"/> operations.</para>
    /// <para>Can be use as UI safe operations.</para>
    /// </summary>
    public class SyncOperation
    {
        private Action<(Action callback, object[] parameters)> contextPost;
        private Action<(Action callback, object[] parameters)> contextSend;

        /// <summary>
        /// Creates new instance of the <c>SyncOperation</c> class.
        /// </summary>
        /// <remarks>
        /// <para>This class used the current thread as base context for all operations upon creation.</para>
        /// <para>Use <see cref="SetContext()"/> method to change the current context.</para>
        /// </remarks>
        public SyncOperation()
        {
            SetContext();
        }

        /// <summary>
        /// Sets the current context to the current thread.
        /// </summary>
        public void SetContext()
        {
            SetContext(AsyncOperationManager.SynchronizationContext);
        }

        /// <summary>
        /// Sets the current context to use the custom operations.
        /// </summary>
        /// <param name="contextPost">
        /// Operation implementation for post.
        /// </param>
        /// <param name="contextSend">
        /// Operation implementation for send.
        /// </param>
        public void SetContext(Action<(Action callback, object[] parameters)> contextPost, Action<(Action callback, object[] parameters)> contextSend)
        {
            this.contextPost = contextPost;
            this.contextSend = contextSend;
        }

        /// <summary>
        /// Sets the current context to the specified <see cref="SynchronizationContext"/>.
        /// </summary>
        /// <param name="context">
        /// The provided <see cref="SynchronizationContext"/> for context operations.
        /// </param>
        public void SetContext(SynchronizationContext context)
        {
            SetContext(
                action => context.Post(s => action.callback(), null),
                action => context.Send(s => action.callback(), null));
        }

        /// <summary>
        /// Sets the current context to use another <see cref="SyncOperation"/> as base context operation.
        /// </summary>
        /// <param name="syncOperation">
        /// The <see cref="SyncOperation"/> to be use as base context operation.
        /// </param>
        public void SetContext(SyncOperation syncOperation)
        {
            SetContext(
                action => syncOperation.contextPost(action),
                action => syncOperation.contextSend(action));
        }

        /// <summary>
        /// Sets the current context to use the <see cref="ISyncObject"/> implementations as base context operation.
        /// </summary>
        /// <param name="syncObject">
        /// The <see cref="ISyncObject"/> implementations to be use as base context operation.
        /// </param>
        public void SetContext(ISyncObject syncObject)
        {
            SetContext(
                action => syncObject.SyncOperation.contextPost(action),
                action => syncObject.SyncOperation.contextSend(action));
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
        public void ContextPost(Action action, params object[] parameters)
        {
            contextPost((action, parameters));
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
        public void ContextSend(Action action, params object[] parameters)
        {
            contextSend((action, parameters));
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
        public async Task ContextSendAsync(Action action, params object[] parameters)
        {
            await Task.Run(delegate
            {
                contextSend((action, parameters));
            });
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
        public async Task ContextSendAsync(Func<Task> func, params object[] parameters)
        {
            await Task.Run(delegate
            {
                contextSend((async () => await func(), parameters));
            });
        }
    }
}
