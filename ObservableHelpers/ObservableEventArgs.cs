using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace ObservableHelpers
{
    public class ObservableObjectChangesEventArgs : PropertyChangedEventArgs
    {
        public string Key { get; }
        public string Group { get; }
        public ObservableObjectChangesEventArgs(
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
