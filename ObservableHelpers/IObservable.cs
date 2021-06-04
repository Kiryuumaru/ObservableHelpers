using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using System.Threading;

namespace ObservableHelpers
{
    public interface IObservable : INotifyPropertyChanged
    {
        event PropertyChangedEventHandler PropertyChangedInternal;
        event EventHandler<Exception> PropertyError;
        event EventHandler<Exception> PropertyErrorInternal;
    }
}
