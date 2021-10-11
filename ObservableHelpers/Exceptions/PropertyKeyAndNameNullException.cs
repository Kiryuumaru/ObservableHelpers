namespace ObservableHelpers.Exceptions
{
    /// <summary>
    /// Occurs when neither property key nor name are provided.
    /// </summary>
    public class PropertyKeyAndNameNullException : ObservableHelpersException
    {
        internal PropertyKeyAndNameNullException()
            : base("Neither property key nor name are provided.")
        {
        }
    }
}
