using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ObservableHelpers
{
    public class SyncContext : Disposable, ISynchronizationObject
    {
        public SynchronizationOperation SynchronizationOperation { get; private set; }

        public SyncContext()
        {
            SynchronizationOperation = new SynchronizationOperation();
        }

        protected void ContextPost(Action action)
        {
            if (IsDisposed)
            {
                return;
            }
            SynchronizationOperation.ContextPost(() =>
            {
                if (IsDisposed)
                {
                    return;
                }
                action();
            });
        }

        protected void ContextSend(Action action)
        {
            if (IsDisposed)
            {
                return;
            }
            SynchronizationOperation.ContextSend(() =>
            {
                if (IsDisposed)
                {
                    return;
                }
                action();
            });
        }

        protected async Task ContextSendAsync(Action action)
        {
            if (IsDisposed)
            {
                return;
            }
            await SynchronizationOperation.ContextSendAsync(() =>
            {
                if (IsDisposed)
                {
                    return;
                }
                action();
            });
        }

        protected async Task ContextSendAsync(Func<Task> func)
        {
            if (IsDisposed)
            {
                return;
            }
            await SynchronizationOperation.ContextSendAsync(async () =>
            {
                if (IsDisposed)
                {
                    return;
                }
                await func();
            });
        }
    }
}
