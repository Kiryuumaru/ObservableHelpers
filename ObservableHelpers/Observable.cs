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
    /// Contains all implementations for performing observable operations.
    /// </summary>
    public abstract class Observable : SyncContext, IObservable
    {
        /// <summary>
        /// Event raised on the current synchronizatiob context when a property is changed.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <inheritdoc/>
        public event PropertyChangedEventHandler ImmediatePropertyChanged;

        /// <inheritdoc/>
        public abstract bool SetNull();

        /// <inheritdoc/>
        public abstract bool IsNull();

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
