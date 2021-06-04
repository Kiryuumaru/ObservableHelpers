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
        private readonly SynchronizationContext context = AsyncOperationManager.SynchronizationContext;

        protected virtual void SynchronizationContextPost(Action action)
        {
            context.Post(s => action(), null);
        }
    }
}
