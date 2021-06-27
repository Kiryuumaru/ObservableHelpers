using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace ObservableHelpers
{
    /// <summary>
    /// The <see cref="PropertyChangedEventArgs"/> derived implementations for <see cref="INotifyPropertyChanged.PropertyChanged"/> events
    /// </summary>
    public class ObjectPropertyChangesEventArgs : PropertyChangedEventArgs
    {
        /// <summary>
        /// Gets the key of the property that changed.
        /// </summary>
        public string Key { get; }

        /// <summary>
        /// Gets the group of the property that changed.
        /// </summary>
        public string Group { get; }

        /// <summary>
        /// Creates new instance of the <see cref="ObjectPropertyChangesEventArgs"/> class.
        /// </summary>
        /// <param name="key">
        /// Key of the property that changed.
        /// </param>
        /// <param name="propertyName">
        /// Name of the property that changed.
        /// </param>
        /// <param name="group">
        /// Group of the property that changed.
        /// </param>
        public ObjectPropertyChangesEventArgs(
            string key,
            string propertyName,
            string group)
            : base(propertyName)
        {
            Key = key;
            Group = group;
        }
    }
}
