using System;
using System.Collections.Generic;
using System.Text;

namespace ObservableHelpers
{
    /// <summary>
    /// Exception represents error that occurs if the property already exists.
    /// </summary>
    public class PropertyKeyAndNameNullException : ArgumentNullException
    {
        /// <inheritdoc/>
        public PropertyKeyAndNameNullException(string message)
            : base(message)
        {
        }

        /// <inheritdoc/>
        public PropertyKeyAndNameNullException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        /// <inheritdoc/>
        public PropertyKeyAndNameNullException()
            : base("Neither property key nor name are provided.")
        {
        }
    }
}
