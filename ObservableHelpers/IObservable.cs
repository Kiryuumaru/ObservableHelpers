using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ObservableHelpers
{
    /// <summary>
    /// Contains bundle declarations for observable operations.
    /// </summary>
    public interface IObservable : IDisposableObject, ISyncObject, INullableObject, INotifyPropertyChanged
    {
        /// <summary>
        /// Occurs right after the property value changes and will invoke on the caller thread instead of the current synchronization context thread. Advisable for non-UI related events.
        /// </summary>
        event PropertyChangedEventHandler ImmediatePropertyChanged;
    }
}
