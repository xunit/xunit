using System;
using System.Reflection;

static class ExceptionExtensions
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
#if NET35
        FieldInfo remoteStackTraceString =
            typeof(Exception).GetField("_remoteStackTraceString", BindingFlags.Instance | BindingFlags.NonPublic) ??
            typeof(Exception).GetField("remote_stack_trace", BindingFlags.Instance | BindingFlags.NonPublic);

        remoteStackTraceString.SetValue(ex, ex.StackTrace + RETHROW_MARKER);
        throw ex;
#else
        System.Runtime.ExceptionServices.ExceptionDispatchInfo.Capture(ex).Throw();
#endif
    }

    /// <summary>
    /// Unwraps an exception to remove any wrappers, like <see cref="TargetInvocationException"/>.
    /// </summary>
    /// <param name="ex">The exception to unwrap.</param>
    /// <returns>The unwrapped exception.</returns>
    public static Exception Unwrap(this Exception ex)
    {
        while (true)
        {
            if (!(ex is TargetInvocationException tiex))
                return ex;

            ex = tiex.InnerException;
        }
    }
}
