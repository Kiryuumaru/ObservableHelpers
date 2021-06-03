using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace ObservableHelpers
{
    public interface IObservable : INotifyPropertyChanged
    {
        event PropertyChangedEventHandler PropertyChangedInternal;
        event EventHandler<Exception> PropertyError;
        event EventHandler<Exception> PropertyErrorInternal;
    }
}
