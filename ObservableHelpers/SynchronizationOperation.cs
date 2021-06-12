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
        private Func<SynchronizationContext> contextFactory;

        public SynchronizationOperation()
        {
            SetContext();
        }

        public void SetContext()
        {
            SynchronizationContext context = AsyncOperationManager.SynchronizationContext;
            contextFactory = new Func<SynchronizationContext>(() => context);
        }

        public void SetContext(SynchronizationContext context)
        {
            contextFactory = new Func<SynchronizationContext>(() => context);
        }

        public void SetContext(SynchronizationOperation syncContext)
        {
            contextFactory = new Func<SynchronizationContext>(() => syncContext.contextFactory.Invoke());
        }

        public void SetContext(ISynchronizationObject syncObject)
        {
            contextFactory = new Func<SynchronizationContext>(() => syncObject.SynchronizationOperation.contextFactory.Invoke());
        }

        public void SetContext(Func<SynchronizationContext> contextFactory)
        {
            this.contextFactory = new Func<SynchronizationContext>(() => contextFactory.Invoke());
        }

        public void SetContext(Func<SynchronizationOperation> contextFactory)
        {
            this.contextFactory = new Func<SynchronizationContext>(() => contextFactory.Invoke().contextFactory.Invoke());
        }

        public void SetContext(Func<ISynchronizationObject> contextFactory)
        {
            this.contextFactory = new Func<SynchronizationContext>(() => contextFactory.Invoke().SynchronizationOperation.contextFactory.Invoke());
        }

        public void ContextPost(Action action)
        {
            contextFactory.Invoke().Post(s => action(), null);
        }

        public void ContextSend(Action action)
        {
            contextFactory.Invoke().Send(s => action(), null);
        }

        public async Task ContextSendAsync(Action action)
        {
            await Task.Run(delegate
            {
                contextFactory.Invoke().Send(s => action(), null);
            });
        }

        public async Task ContextSendAsync(Func<Task> func)
        {
            await Task.Run(delegate
            {
                contextFactory.Invoke().Send(async s => await func(), null);
            });
        }
    }
}
