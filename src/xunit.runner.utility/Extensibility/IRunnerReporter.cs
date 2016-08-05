using Xunit.Abstractions;

namespace Xunit
{
    /// <summary>
    /// This interface represents a reporter which is invoked by a test runner
    /// during test execution. The report can be explicitly invoked by a command
    /// line switch or implicitly invoked by being environmentally enabled (for
    /// example, a reporter that emits messages for TeamCity).
    /// </summary>
    public interface IRunnerReporter
    {
        /// <summary>
        /// Gets the description of the reporter. This is typically used when showing
        /// the user the invocation option for the reporter.
        /// </summary>
        string Description { get; }

        /// <summary>
        /// Gets a value which indicates whether the reporter should be
        /// environmentally enabled.
        /// </summary>
        bool IsEnvironmentallyEnabled { get; }

        /// <summary>
        /// Gets a value which indicates a runner switch which can be used
        /// to explicitly enable the runner. If the return value is <c>null</c>,
        /// then the reported can only be environmentally enabled (implicitly).
        /// This value is used either as a command line switch (with the console or
        /// .NET CLI runner) or as a runner configuration value (with the MSBuild runner).
        /// </summary>
        string RunnerSwitch { get; }

        /// <summary>
        /// Creates a message handler that will report messages for the given
        /// test assembly. Ideally, the handler should also implement <see cref="IMessageSinkWithTypes"/>
        /// for optimal performance, but plain implementations of <see cref="IMessageSink"/> are supported
        /// for backward compatibility reasons.
        /// </summary>
        /// <param name="logger">The logger used to send result messages to</param>
        /// <returns>The message handler that handles the messages</returns>
        IMessageSink CreateMessageHandler(IRunnerLogger logger);
    }
}
