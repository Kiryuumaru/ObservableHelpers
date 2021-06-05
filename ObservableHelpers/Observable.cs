using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using System.Threading;

namespace ObservableHelpers
{
    public abstract class Observable : SyncContext, IObservable
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public event EventHandler<Exception> PropertyError;

        protected virtual void InvokeOnChanged(PropertyChangedEventArgs args)
        {
            SynchronizationContextPost(delegate
            {
                PropertyChanged?.Invoke(this, args);
            });
        }

        protected virtual void InvokeOnChanged(string propertyName)
        {
            InvokeOnChanged(new PropertyChangedEventArgs(propertyName));
        }

        protected virtual void InvokeOnError(Exception exception)
        {
            SynchronizationContextPost(delegate
            {
                PropertyError?.Invoke(this, exception);
            });
        }

        protected virtual void InvokeOnError(string message)
        {
            InvokeOnError(new Exception(message));
        }
    }
}
