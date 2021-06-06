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
        public event EventHandler<Exception> Error;

        protected virtual void OnPropertyChanged(PropertyChangedEventArgs args)
        {
            SynchronizationContextPost(delegate
            {
                PropertyChanged?.Invoke(this, args);
            });
        }

        protected virtual void OnPropertyChanged(string propertyName)
        {
            OnPropertyChanged(new PropertyChangedEventArgs(propertyName));
        }

        protected virtual void OnError(Exception exception)
        {
            SynchronizationContextPost(delegate
            {
                Error?.Invoke(this, exception);
            });
        }

        protected virtual void OnError(string message)
        {
            OnError(new Exception(message));
        }
    }
}
