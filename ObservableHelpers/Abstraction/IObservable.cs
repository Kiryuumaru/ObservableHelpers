using System.ComponentModel;

namespace ObservableHelpers.Abstraction;

/// <summary>
/// Contains bundle declarations for observable operations.
/// </summary>
public interface IObservable :
    INullableObject,
    INotifyPropertyChanged
{
    /// <summary>
    /// Event raised on the current synchronization context when a property is changed.
    /// </summary>
    event PropertyChangedEventHandler SynchronizedPropertyChanged;
}
