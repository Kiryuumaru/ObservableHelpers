namespace ObservableHelpers.Exceptions;

/// <summary>
/// Occurs when the specified property already exists.
/// </summary>
public class PropertyAlreadyExistsException : ObservableHelpersException
{
    /// <summary>
    /// Creates an instance of <see cref="PropertyAlreadyExistsException"/> with provided <paramref name="propertyKey"/> and <paramref name="propertyName"/>.
    /// </summary>
    /// <param name="propertyKey">
    /// The key of the property.
    /// </param>
    /// <param name="propertyName">
    /// The name of the property.
    /// </param>
    public PropertyAlreadyExistsException(string propertyKey, string propertyName)
        : base("The provided property (key:" + propertyKey + " name:" + propertyName + ") already exists.")
    {
    }
}
