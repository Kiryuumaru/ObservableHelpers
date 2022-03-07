using System;

namespace ObservableHelpers.Exceptions;

/// <summary>
/// Occurs when there`s an observable helpers error.
/// </summary>
public class ObservableHelpersException : Exception
{
    private const string ExceptionMessage =
        "An observable helpers error has occured.";

    /// <summary>
    /// Creates an instance of <see cref="ObservableHelpersException"/>.
    /// </summary>
    public ObservableHelpersException()
        : this(ExceptionMessage)
    {
    }

    /// <summary>
    /// Creates an instance of <see cref="ObservableHelpersException"/> with provided <paramref name="message"/>.
    /// </summary>
    /// <param name="message">
    /// The message of the exception.
    /// </param>
    public ObservableHelpersException(string message)
        : base(message)
    {
    }

    /// <summary>
    /// Creates an instance of <see cref="ObservableHelpersException"/> with provided <paramref name="innerException"/>.
    /// </summary>
    /// <param name="innerException">
    /// The inner exception occured.
    /// </param>
    public ObservableHelpersException(Exception innerException)
        : base(ExceptionMessage, innerException)
    {
    }

    /// <summary>
    /// Creates an instance of <see cref="ObservableHelpersException"/> with provided <paramref name="message"/> and <paramref name="innerException"/>.
    /// </summary>
    /// <param name="message">
    /// The message of the exception.
    /// </param>
    /// <param name="innerException">
    /// The inner exception occured.
    /// </param>
    public ObservableHelpersException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}
