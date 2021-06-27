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
    /// Contains declaration for <see cref="ObservableHelpers.SyncOperation"/>.
    /// </summary>
    public interface ISyncObject : IDisposableObject
    {
        /// <summary>
        /// Gets the <see cref="ObservableHelpers.SyncOperation"/> used by this object.
        /// </summary>
        SyncOperation SyncOperation { get; }
    }
}
