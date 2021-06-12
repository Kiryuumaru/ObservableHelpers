using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ObservableHelpers
{
    public abstract class Observable : SyncContext, INullableObject, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public abstract bool SetNull();

        public abstract bool IsNull();

        protected virtual void OnPropertyChanged(PropertyChangedEventArgs args)
        {
            ContextPost(delegate
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
