using System;
using System.Collections.Generic;
using System.Text;

namespace ObservableHelpers.Exceptions
{
    /// <summary>
    /// Exception represents error that occurs if the property already exists.
    /// </summary>
    public class PropertyKeyAndNameNullException : ObservableHelpersException
    {
        internal PropertyKeyAndNameNullException()
            : base("Neither property key nor name are provided.")
        {
        }
    }
}
