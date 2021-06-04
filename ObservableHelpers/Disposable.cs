using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using System.Threading;

namespace ObservableHelpers
{
    public abstract class Observable : Disposable, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public event PropertyChangedEventHandler PropertyChangedInternal;
        public event EventHandler<Exception> PropertyError;
        public event EventHandler<Exception> PropertyErrorInternal;

        private readonly SynchronizationContext context = AsyncOperationManager.SynchronizationContext;

        protected virtual void InvokeOnChanged(PropertyChangedEventArgs args)
        {
            PropertyChangedInternal?.Invoke(this, args);
            SynchronizationContextPost(delegate { PropertyChanged?.Invoke(this, args); });
        }

        protected virtual void InvokeOnChanged(string propertyName)
        {
            InvokeOnChanged(new PropertyChangedEventArgs(propertyName));
        }

        protected virtual void InvokeOnError(Exception exception)
        {
            PropertyErrorInternal?.Invoke(this, exception);
            SynchronizationContextPost(delegate { PropertyError?.Invoke(this, exception); });
        }

        protected virtual void InvokeOnError(string message)
        {
            InvokeOnError(new Exception(message));
        }

        protected virtual void SynchronizationContextPost(Action action)
        {
            context.Post(s => action(), null);
        }
    }
}
