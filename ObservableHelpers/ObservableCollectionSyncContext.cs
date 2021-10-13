using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;

namespace ObservableHelpers
{
    /// <summary>
    /// Contains all implementations for performing observable operations.
    /// </summary>
    public abstract class ObservableCollectionSyncContext :
        ObservableSyncContext,
        INotifyCollectionChanged
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
        /// Raises <see cref="OnCollectionChanged(NotifyCollectionChangedEventArgs)"/> event with <see cref="NotifyCollectionChangedAction.Add"/> action to any listeners.
        /// </summary>
        /// <param name="item">
        /// The item that is affected by the change.
        /// </param>
        /// <param name="index">
        /// The index where the change occurred.
        /// </param>
        protected void OnCollectionAdd(object item, int index)
        {
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, item, index));
        }

        /// <summary>
        /// Raises <see cref="OnCollectionChanged(NotifyCollectionChangedEventArgs)"/> event with <see cref="NotifyCollectionChangedAction.Add"/> action to any listeners.
        /// </summary>
        /// <param name="items">
        /// The items that is affected by the change.
        /// </param>
        /// <param name="index">
        /// The index where the change occurred.
        /// </param>
        protected void OnCollectionAdd(IList items, int index)
        {
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, items, index));
        }

        /// <summary>
        /// Raises <see cref="OnCollectionChanged(NotifyCollectionChangedEventArgs)"/> event with <see cref="NotifyCollectionChangedAction.Remove"/> action to any listeners.
        /// </summary>
        /// <param name="item">
        /// The item that is affected by the change.
        /// </param>
        /// <param name="index">
        ///  The index where the change occurred.
        /// </param>
        protected void OnCollectionRemove(object item, int index)
        {
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, item, index));
        }

        /// <summary>
        /// Raises <see cref="OnCollectionChanged(NotifyCollectionChangedEventArgs)"/> event with <see cref="NotifyCollectionChangedAction.Replace"/> action to any listeners.
        /// </summary>
        /// <param name="oldItem">
        /// The new item that is replacing the original item.
        /// </param>
        /// <param name="newItem">
        ///  The original item that is replaced.
        /// </param>
        /// <param name="index">
        /// The index of the item being replaced.
        /// </param>
        protected void OnCollectionReplace(object oldItem, object newItem, int index)
        {
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Replace, newItem, oldItem, index));
        }

        /// <summary>
        /// Raises <see cref="OnCollectionChanged(NotifyCollectionChangedEventArgs)"/> event with <see cref="NotifyCollectionChangedAction.Move"/> action to any listeners.
        /// </summary>
        /// <param name="item">
        /// The item affected by the change.
        /// </param>
        /// <param name="index">
        /// The new index for the changed item.
        /// </param>
        /// <param name="oldIndex">
        /// The old index for the changed item.
        /// </param>
        protected void OnCollectionMove(object item, int index, int oldIndex)
        {
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Move, item, index, oldIndex));
        }

        /// <summary>
        /// Raises <see cref="OnCollectionChanged(NotifyCollectionChangedEventArgs)"/> event with <see cref="NotifyCollectionChangedAction.Reset"/> action to any listeners.
        /// </summary>
        protected void OnCollectionReset()
        {
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        }

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
