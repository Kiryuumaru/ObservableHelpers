using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ObservableHelpers
{
    public class SyncContext : Disposable, ISyncObject
    {
        public SyncOperation SyncOperation { get; private set; }

        public SyncContext()
        {
            SyncOperation = new SyncOperation();
        }

        protected void ContextPost(Action action)
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
            });
        }

        protected void ContextSend(Action action)
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
            });
        }

        protected async Task ContextSendAsync(Action action)
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
            });
        }

        protected async Task ContextSendAsync(Func<Task> func)
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
            });
        }
    }
}
