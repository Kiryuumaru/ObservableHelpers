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
        private Func<SynchronizationContext> contextFactory;

        protected SyncContext()
        {
            SetSynchronizationContext();
        }

        public void SetSynchronizationContext()
        {
            VerifyNotDisposed();

            SynchronizationContext context = AsyncOperationManager.SynchronizationContext;
            contextFactory = new Func<SynchronizationContext>(() => context);
        }

        public void SetSynchronizationContext(SynchronizationContext context)
        {
            VerifyNotDisposed();

            contextFactory = new Func<SynchronizationContext>(() => context);
        }

        public void SetSynchronizationContext(SyncContext syncContext)
        {
            VerifyNotDisposed();

            contextFactory = new Func<SynchronizationContext>(() => syncContext.contextFactory.Invoke());
        }

        public void SetSynchronizationContext(Func<SynchronizationContext> contextFactory)
        {
            VerifyNotDisposed();

            this.contextFactory = new Func<SynchronizationContext>(() => contextFactory.Invoke());
        }

        protected void SynchronizationContextPost(Action action)
        {
            if (IsDisposedOrDisposing)
            {
                return;
            }

            contextFactory.Invoke().Post(s => action(), null);
        }
    }
}
