using System.ComponentModel;
using System.Threading;
using LockerHelpers;
using ObservableHelpers.Abstraction;
using SynchronizationContextHelpers;

namespace ObservableHelpers.Utilities;

/// <summary>
/// Contains all implementations for performing observable operations.
/// </summary>
public abstract class ObservableSyncContext :
    ISynchronizedObject
{
    #region Properties

    /// <inheritdoc/>
    public RWLock RWLock { get; } = new RWLock(LockRecursionPolicy.SupportsRecursion);

    /// <inheritdoc/>
    public SyncOperation SyncOperation { get; } = new SyncOperation();

    #endregion

    #region Events

    /// <inheritdoc/>
    public event PropertyChangedEventHandler? PropertyChanged;

    /// <inheritdoc/>
    public event PropertyChangedEventHandler? SynchronizedPropertyChanged;

    /// <inheritdoc/>
    public event PropertyChangedEventHandler? UnsynchronizedPropertyChanged;

    /// <summary>
    /// Gets or sets <c>true</c> if the <see cref="PropertyChanged"/> event will invoke on the synchronized context.
    /// </summary>
    public bool SynchronizePropertyChangedEvent { get; set; }

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
    /// Invokes <see cref="PropertyChanged"/> and <see cref="SynchronizedPropertyChanged"/>.
    /// </summary>
    /// <param name="args">
    /// The <see cref="PropertyChangedEventArgs"/> event argument.
    /// </param>
    protected virtual void OnPropertyChanged(PropertyChangedEventArgs args)
    {
        UnsynchronizedPropertyChanged?.Invoke(this, args);
        SyncOperation.ContextPost(delegate
        {
            SynchronizedPropertyChanged?.Invoke(this, args);
        });

        if (SynchronizePropertyChangedEvent)
        {
            SyncOperation.ContextPost(delegate
            {
                PropertyChanged?.Invoke(this, args);
            });
        }
        else
        {
            PropertyChanged?.Invoke(this, args);
        }
    }

    #endregion
}
