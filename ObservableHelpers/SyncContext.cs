using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ObservableHelpers
{
    public abstract class SyncContext : Disposable
    {
        private Func<SynchronizationContext> contextFactory;

        private readonly Queue<Action> actionQueue = new Queue<Action>();
        private bool isActionQueueRunning;

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
            VerifyNotDisposed();

            contextFactory.Invoke().Post(s => action(), null);
        }

        protected void SynchronizationContextSend(Action action)
        {
            VerifyNotDisposed();

            contextFactory.Invoke().Send(s => action(), null);
        }

        protected void SynchronizationContextQueue(Action action)
        {
            if (IsDisposed)
            {
                return;
            }

            Task.Run(delegate
            {
                lock (actionQueue)
                {
                    actionQueue.Enqueue(action);
                }

                if (isActionQueueRunning) return;
                isActionQueueRunning = true;
                while (true)
                {
                    if (IsDisposed)
                    {
                        break;
                    }

                    Action actionToInvoke = null;
                    lock (actionQueue)
                    {
                        try
                        {
                            actionToInvoke = actionQueue.Dequeue();
                        }
                        catch { }
                    }

                    if (IsDisposed || actionToInvoke == null)
                    {
                        break;
                    }

                    contextFactory.Invoke().Send(s => actionToInvoke(), null);
                }
                isActionQueueRunning = false;
            });
        }
    }
}
