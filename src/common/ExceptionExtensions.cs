using System;
using System.Reflection;
using System.Runtime.ExceptionServices;

static class ExceptionExtensions
{
    /// <summary>
    /// Rethrows an exception object without losing the existing stack trace information
    /// </summary>
    /// <param name="ex">The exception to re-throw.</param>
    public static void RethrowWithNoStackTraceLoss(this Exception ex) =>
        ExceptionDispatchInfo.Capture(ex).Throw();

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

            ex = tiex.InnerException!;
        }
    }
}
