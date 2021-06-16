using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace ObservableHelpers
{
    public class ObjectPropertyChangesEventArgs : PropertyChangedEventArgs
    {
        public string Key { get; }
        public string Group { get; }
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
