using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ObservableHelpers
{
    public class SyncOperation
    {
        private Action<(Action callback, object[] parameters)> contextPost;
        private Action<(Action callback, object[] parameters)> contextSend;

        public SyncOperation()
        {
            SetContext();
        }

        public void SetContext()
        {
            SetContext(AsyncOperationManager.SynchronizationContext);
        }

        public void SetContext(Action<(Action callback, object[] parameters)> contextPost, Action<(Action callback, object[] parameters)> contextSend)
        {
            this.contextPost = contextPost;
            this.contextSend = contextSend;
        }

        public void SetContext(SynchronizationContext context)
        {
            SetContext(
                action => context.Post(s => action.callback(), null),
                action => context.Send(s => action.callback(), null));
        }

        public void SetContext(SyncOperation syncOperation)
        {
            SetContext(
                action => syncOperation.contextPost(action),
                action => syncOperation.contextSend(action));
        }

        public void SetContext(ISyncObject syncObject)
        {
            SetContext(
                action => syncObject.SyncOperation.contextPost(action),
                action => syncObject.SyncOperation.contextSend(action));
        }

        public void ContextPost(Action action, params object[] parameters)
        {
            contextPost((action, parameters));
        }

        public void ContextSend(Action action, params object[] parameters)
        {
            contextSend((action, parameters));
        }

        public async Task ContextSendAsync(Action action, params object[] parameters)
        {
            await Task.Run(delegate
            {
                contextSend((action, parameters));
            });
        }

        public async Task ContextSendAsync(Func<Task> func, params object[] parameters)
        {
            await Task.Run(delegate
            {
                contextSend((async () => await func(), parameters));
            });
        }
    }
}
