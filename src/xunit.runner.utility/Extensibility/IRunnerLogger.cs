namespace Xunit
{
    /// <summary>
    /// Interface implemented by runners, passed to <see cref="IRunnerReporter"/>, so that the
    /// report can log lines of text to the output device.
    /// </summary>
    public interface IRunnerLogger
    {
        /// <summary>
        /// Gets a lock object that can be used to ensure that multiple calls to
        /// log messages will always be grouped together.
        /// </summary>
        object LockObject { get; }

        /// <summary>
        /// Logs a normal-priority message with stack frame.
        /// </summary>
        /// <param name="stackFrame">The stack frame information</param>
        /// <param name="message">The message to be logged</param>
        void LogMessage(StackFrameInfo stackFrame, string message);

        /// <summary>
        /// Logs a high-priority message with stack frame.
        /// </summary>
        /// <param name="stackFrame">The stack frame information</param>
        /// <param name="message">The message to be logged</param>
        void LogImportantMessage(StackFrameInfo stackFrame, string message);

        /// <summary>
        /// Logs a warning message with stack frame.
        /// </summary>
        /// <param name="stackFrame">The stack frame information</param>
        /// <param name="message">The message to be logged</param>
        void LogWarning(StackFrameInfo stackFrame, string message);

        /// <summary>
        /// Logs an error message with stack frame.
        /// </summary>
        /// <param name="stackFrame">The stack frame information</param>
        /// <param name="message">The message to be logged</param>
        void LogError(StackFrameInfo stackFrame, string message);

        /// <summary>
        /// Logs a messages with as little processing as possible. For example, the console runner will
        /// not attempt to set the color of the text that's being logged. This is most useful when attempting
        /// to render text lines that will be processed, like for TeamCity.
        /// </summary>
        /// <param name="message">The message to be logged.</param>
        void LogRaw(string message);
    }
}
