using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace ObservableHelpers
{
    public interface IObservable : INotifyPropertyChanged
    {
        void OnError(Exception exception);
    }
}
