using System.Collections.Specialized;
using System.ComponentModel;

namespace ObservableHelpers
{
    /// <summary>
    /// Contains all implementations for performing observable operations.
    /// </summary>
    public abstract class ObservableCollectionSyncContext : ObservableSyncContext, INotifyCollectionChanged
    {
        /// <summary>
        /// Event raised on the current syncronization context when the collection changes.
        /// </summary>
        public virtual event NotifyCollectionChangedEventHandler CollectionChanged;

        /// <summary>
        /// Event raised on the callers thread instead of the current syncronization context thread when the collection changes.
        /// </summary>
        public virtual event NotifyCollectionChangedEventHandler ImmediateCollectionChanged;

        /// <summary>
        /// Invokes <see cref="CollectionChanged"/> into current synchronization context.
        /// </summary>
        /// <param name="e">
        /// The <see cref="NotifyCollectionChangedEventArgs"/> event argument.
        /// </param>
        protected virtual void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
        {
            ImmediateCollectionChanged?.Invoke(this, e);
            ContextPost(delegate
            {
                CollectionChanged?.Invoke(this, e);
            });
        }
    }
}
