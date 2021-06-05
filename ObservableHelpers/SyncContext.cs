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
        private readonly AsyncOperation context;

        protected SyncContext()
        {
            context = AsyncOperationManager.CreateOperation(null);
        }

        protected void SynchronizationContextPost(Action action)
        {
            context.Post(s => action(), null);
        }
    }
}
