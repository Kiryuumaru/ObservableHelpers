using System;
using System.Collections.Generic;
using System.Text;

namespace ObservableHelpers
{
    /// <summary>
    /// Exception represents error that occurs if the property already exists.
    /// </summary>
    public class PropertyAlreadyExistsException : Exception
    {
        /// <inheritdoc/>
        public PropertyAlreadyExistsException(string message)
            : base(message)
        {
        }

        /// <inheritdoc/>
        public PropertyAlreadyExistsException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        /// <inheritdoc/>
        public PropertyAlreadyExistsException()
            : base("Provided property already exists.")
        {
        }

        /// <inheritdoc/>
        public PropertyAlreadyExistsException(string propertyKey, string propertyName)
            : base("The provided property (key:" + propertyKey + " name:" + propertyName + ") already exists.")
        {
        }
    }
}
