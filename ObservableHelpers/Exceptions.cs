using System;
using System.Collections.Generic;
using System.Text;

namespace ObservableHelpers
{
    /// <summary>
    /// Exception represents error that occurs if the property key and name is not provided.
    /// </summary>
    public class PropertyKeyAndNameNotProvided : Exception
    {
        /// <inheritdoc/>
        public PropertyKeyAndNameNotProvided(string message)
            : base(message)
        {
        }

        /// <inheritdoc/>
        public PropertyKeyAndNameNotProvided(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        /// <inheritdoc/>
        public PropertyKeyAndNameNotProvided()
            : base()
        {
        }
    }

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
            : base()
        {
        }
    }
}
