using System;
using System.ComponentModel;

namespace ObservableHelpers
{
    /// <summary>
    /// The <see cref="PropertyChangedEventArgs"/> derived implementations for <see cref="INotifyPropertyChanged.PropertyChanged"/> events
    /// </summary>
    public class ObjectPropertyChangesEventArgs :
        PropertyChangedEventArgs
    {
        /// <summary>
        /// Gets the key of the property that changed.
        /// </summary>
        public string Key { get; }

        /// <summary>
        /// Gets the group of the property that changed.
        /// </summary>
        public string Group { get; }

        internal ObjectPropertyChangesEventArgs(
            string key,
            string propertyName,
            string group)
            : base(propertyName)
        {
            Key = key;
            Group = group;
        }
    }

    /// <summary>
    /// The property set event arguments for <see cref="ObservableObject"/>.
    /// </summary>
    public class ObjectPropertySetEventArgs<T> : EventArgs
    {
        /// <summary>
        /// Gets the key of the property that sets.
        /// </summary>
        public string Key { get; }

        /// <summary>
        /// Gets the property name of the property that sets.
        /// </summary>
        public string PropertyName { get; }

        /// <summary>
        /// Gets the group of the property that sets.
        /// </summary>
        public string Group { get; }

        /// <summary>
        /// Gets the old value of the property that sets.
        /// </summary>
        public T OldValue { get; }

        /// <summary>
        /// Gets the new value of the property that sets.
        /// </summary>
        public T NewValue { get; }

        /// <summary>
        /// Gets <c>true</c> if the property sets; otherwise, <c>false</c>.
        /// </summary>
        public bool HasChanges { get; }

        internal ObjectPropertySetEventArgs(
            string key,
            string propertyName,
            string group,
            T oldValue,
            T newValue,
            bool hasChanges)
        {
            Key = key;
            PropertyName = propertyName;
            Group = group;
            OldValue = oldValue;
            NewValue = newValue;
            HasChanges = hasChanges;
        }
    }
}
