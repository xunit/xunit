using System;
using System.Reflection;

/// <summary>
/// Extension methods for <see cref="Exception"/>.
/// </summary>
public static class ExceptionExtensions
{
    const string RETHROW_MARKER = "$$RethrowMarker$$";

    /// <summary>
    /// Rethrows an exception object without losing the existing stack trace information
    /// </summary>
    /// <param name="ex">The exception to re-throw.</param>
    /// <remarks>
    /// For more information on this technique, see
    /// http://www.dotnetjunkies.com/WebLog/chris.taylor/archive/2004/03/03/8353.aspx.
    /// The remote_stack_trace string is here to support Mono.
    /// </remarks>
    public static void RethrowWithNoStackTraceLoss(this Exception ex)
    {
        // TODO: Is there code from ASP.NET Web Stack that we can borrow, that helps us do better things in 4.5?

        FieldInfo remoteStackTraceString =
            typeof(Exception).GetField("_remoteStackTraceString", BindingFlags.Instance | BindingFlags.NonPublic) ??
            typeof(Exception).GetField("remote_stack_trace", BindingFlags.Instance | BindingFlags.NonPublic);

        remoteStackTraceString.SetValue(ex, ex.StackTrace + RETHROW_MARKER);
        throw ex;
    }
}