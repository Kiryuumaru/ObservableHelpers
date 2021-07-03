using System;
using System.Collections.Generic;
using System.Text;

namespace ObservableHelpers.Exceptions
{
    /// <summary>
    /// Occurs when the specified property already exists.
    /// </summary>
    public class PropertyAlreadyExistsException : ObservableHelpersException
    {
        internal PropertyAlreadyExistsException(string propertyKey, string propertyName)
            : base("The provided property (key:" + propertyKey + " name:" + propertyName + ") already exists.")
        {
        }
    }
}
