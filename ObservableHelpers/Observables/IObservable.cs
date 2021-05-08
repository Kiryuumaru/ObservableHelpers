using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace ObservableHelpers.Observables
{
    public interface IObservable : IAttributed, INotifyPropertyChanged
    {
        void OnError(Exception exception, bool defaultIgnoreAndContinue = true);
        void OnError(ContinueExceptionEventArgs args);
    }
}
