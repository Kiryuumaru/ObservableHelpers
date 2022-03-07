using ObservableHelpers.Abstraction;
using System.ComponentModel;
using System.Threading;
using SynchronizationContextHelpers;
using LockerHelpers;

namespace ObservableHelpers.Utilities;

/// <summary>
/// Contains all implementations for performing observable operations.
/// </summary>
public abstract class ObservableSyncContext :
    SyncContext,
    IObservable
{
    #region Properties

    /// <summary>
    /// Gets the read-write lock for concurrency.
    /// </summary>
    public RWLock RWLock { get; } = new RWLock(LockRecursionPolicy.SupportsRecursion);

    #endregion

    #region Events

    /// <summary>
    /// Event raised on the current synchronizatiob context when a property is changed.
    /// </summary>
    public event PropertyChangedEventHandler? PropertyChanged;

    /// <inheritdoc/>
    public event PropertyChangedEventHandler? ImmediatePropertyChanged;

    #endregion

    #region Abstract Methods

    /// <inheritdoc/>
    public abstract bool SetNull();

    /// <inheritdoc/>
    public abstract bool IsNull();

    #endregion

    #region Methods

    /// <summary>
    /// Raises <see cref="OnPropertyChanged(PropertyChangedEventArgs)"/> with the specified <paramref name="propertyName"/>.
    /// </summary>
    /// <param name="propertyName">
    /// The name of the changed property.
    /// </param>
    protected void OnPropertyChanged(string propertyName)
    {
        OnPropertyChanged(new PropertyChangedEventArgs(propertyName));
    }

    /// <summary>
    /// Invokes <see cref="PropertyChanged"/> into current synchronization context.
    /// </summary>
    /// <param name="args">
    /// The <see cref="PropertyChangedEventArgs"/> event argument.
    /// </param>
    protected virtual void OnPropertyChanged(PropertyChangedEventArgs args)
    {
        if (IsDisposed)
        {
            return;
        }
        ImmediatePropertyChanged?.Invoke(this, args);
        ContextPost(delegate
        {
            PropertyChanged?.Invoke(this, args);
        });
    }

    /// <inheritdoc/>
    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            PropertyChanged = null;
            ImmediatePropertyChanged = null;
        }
        base.Dispose(disposing);
    }

    #endregion
}
