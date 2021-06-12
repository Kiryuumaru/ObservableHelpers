using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using System.Threading;

namespace ObservableHelpers
{
    public abstract class Observable : Disposable, INullableObject, ISynchronizationObject, INotifyPropertyChanged
    {
        public SynchronizationOperation SynchronizationOperation { get; private set; }

        public event PropertyChangedEventHandler PropertyChanged;

        protected Observable()
        {
            SynchronizationOperation = new SynchronizationOperation();
        }

        public abstract bool SetNull();

        public abstract bool IsNull();

        protected virtual void OnPropertyChanged(PropertyChangedEventArgs args)
        {
            SynchronizationOperation.ContextPost(delegate
            {
                PropertyChanged?.Invoke(this, args);
            });
        }

        protected virtual void OnPropertyChanged(string propertyName)
        {
            OnPropertyChanged(new PropertyChangedEventArgs(propertyName));
        }
    }
}
