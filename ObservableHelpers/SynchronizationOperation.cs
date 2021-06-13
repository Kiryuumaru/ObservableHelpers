using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ObservableHelpers
{
    public class SynchronizationOperation
    {
        private Action<Action> contextPost;
        private Action<Action> contextSend;

        public SynchronizationOperation()
        {
            SetContext();
        }

        public void SetContext(Action<Action> contextPost, Action<Action> contextSend)
        {
            this.contextPost = contextPost;
            this.contextSend = contextSend;
        }

        public void SetContext(SynchronizationContext context)
        {
            SetContext(
                callback => context.Post(s => callback(), null),
                callback => context.Send(s => callback(), null));
        }

        public void SetContext()
        {
            SetContext(AsyncOperationManager.SynchronizationContext);
        }

        public void SetContext(SynchronizationOperation syncContext)
        {
            SetContext(
                callback => syncContext.contextPost(callback),
                callback => syncContext.contextSend(callback));
        }

        public void SetContext(ISynchronizationObject syncObject)
        {
            SetContext(
                callback => syncObject.SynchronizationOperation.contextPost(callback),
                callback => syncObject.SynchronizationOperation.contextSend(callback));
        }

        public void ContextPost(Action action)
        {
            contextPost(action);
        }

        public void ContextSend(Action action)
        {
            contextSend(action);
        }

        public async Task ContextSendAsync(Action action)
        {
            await Task.Run(delegate
            {
                contextSend(action);
            });
        }

        public async Task ContextSendAsync(Func<Task> func)
        {
            await Task.Run(delegate
            {
                contextSend(async () => await func());
            });
        }
    }
}
