using ObservableHelpers.Abstraction;
using System.ComponentModel;
using System.Threading;

namespace ObservableHelpers.Utilities
{
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
        protected RWLock RWLock { get; } = new RWLock(LockRecursionPolicy.SupportsRecursion);

        #endregion

        #region Events

        /// <summary>
        /// Event raised on the current synchronizatiob context when a property is changed.
        /// </summary>
        public virtual event PropertyChangedEventHandler PropertyChanged;

        /// <inheritdoc/>
        public virtual event PropertyChangedEventHandler ImmediatePropertyChanged;

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
            ContextSend(delegate
            {
                PropertyChanged?.Invoke(this, args);
            });
        }

        #endregion
    }
}
