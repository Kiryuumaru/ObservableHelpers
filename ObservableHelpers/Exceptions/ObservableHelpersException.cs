using System;

namespace ObservableHelpers.Exceptions;

/// <summary>
/// Occurs when there`s an observable helpers error.
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
