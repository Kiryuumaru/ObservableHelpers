using System;
using System.Collections.Generic;
using System.Text;

namespace ObservableHelpers.Exceptions
{
    /// <summary>
    /// Exception represents error that occurs if the property already exists.
    /// </summary>
    public abstract class ObservableHelpersException : Exception
    {
        private protected ObservableHelpersException()
            : this("An observable helpers error has occured.")
        {
        }

        private protected ObservableHelpersException(string message)
            : base(message)
        {
        }

        private protected ObservableHelpersException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
