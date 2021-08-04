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
        /// Event raised on the current context when a property is changed.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <inheritdoc/>
        public abstract bool SetNull();

        /// <inheritdoc/>
        public abstract bool IsNull();

        /// <summary>
        /// Invokes <see cref="PropertyChanged"/> into current context.
        /// </summary>
        /// <param name="args">
        /// The <see cref="PropertyChangedEventArgs"/> event argument.
        /// </param>
        protected virtual void OnPropertyChanged(PropertyChangedEventArgs args)
        {
            ContextPost(delegate
            {
                PropertyChanged?.Invoke(this, args);
            });
        }
    }
}
