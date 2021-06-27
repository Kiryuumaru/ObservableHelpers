using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ObservableHelpers
{
    /// <summary>
    /// Contains proper declarations for disposable operations.
    /// </summary>
    public interface IDisposableObject : IDisposable
    {
        /// <summary>
        /// Gets a value indicating whether this object is in the process of disposing.
        /// </summary>
        bool IsDisposing { get; }

        /// <summary>
        /// Gets a value indicating whether this object has been disposed.
        /// </summary>
        bool IsDisposed { get; }

        /// <summary>
        /// Gets a value indicating whether this object has been disposed or is in the process of being disposed.
        /// </summary>
        bool IsDisposedOrDisposing { get; }
    }
}
