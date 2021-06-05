using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using System.Threading;

namespace ObservableHelpers
{
    public abstract class SyncContext : Disposable
    {
        private SynchronizationContext context;

        protected SyncContext()
        {
            SetSynchronizationContext();
        }

        protected void SynchronizationContextPost(Action action)
        {
            context.Post(s => action(), null);
        }

        public void SetSynchronizationContext()
        {
            context = AsyncOperationManager.SynchronizationContext;
        }

        public void SetSynchronizationContext(SynchronizationContext context)
        {
            this.context = context;
        }

        public void SetSynchronizationContext(SyncContext syncContext)
        {
            context = syncContext.context;
        }
    }
}
