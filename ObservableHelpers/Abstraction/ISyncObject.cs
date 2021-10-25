using ObservableHelpers.Utilities;

namespace ObservableHelpers.Abstraction
{
    /// <summary>
    /// Contains declaration for <see cref="Utilities.SyncOperation"/>.
    /// </summary>
    public interface ISyncObject :
        IDisposableObject
    {
        /// <summary>
        /// Gets the <see cref="Utilities.SyncOperation"/> used by this object.
        /// </summary>
        SyncOperation SyncOperation { get; }
    }
}
