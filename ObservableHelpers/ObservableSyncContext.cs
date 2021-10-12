﻿using ObservableHelpers.Abstraction;
using System.ComponentModel;

namespace ObservableHelpers
{
    /// <summary>
    /// Contains all implementations for performing observable operations.
    /// </summary>
    public abstract class ObservableSyncContext :
        SyncContext,
        IObservable
    {
        /// <summary>
        /// Event raised on the current synchronizatiob context when a property is changed.
        /// </summary>
        public virtual event PropertyChangedEventHandler PropertyChanged;

        /// <inheritdoc/>
        public virtual event PropertyChangedEventHandler ImmediatePropertyChanged;

        /// <inheritdoc/>
        public abstract bool SetNull();

        /// <inheritdoc/>
        public abstract bool IsNull();

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
            ImmediatePropertyChanged?.Invoke(this, args);
            ContextPost(delegate
            {
                PropertyChanged?.Invoke(this, args);
            });
        }
    }
}
