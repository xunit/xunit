using Xunit.Abstractions;

namespace Xunit
{
    /// <summary>
    /// The default implementation of <see cref="IRunnerReporter"/>, used
    /// by runners when there is no other overridden reporter. It returns
    /// an instance of <see cref="DefaultRunnerReporterMessageHandler"/>.
    /// </summary>
    public class DefaultRunnerReporter : IRunnerReporter
    {
        /// <inheritdoc/>
        public virtual string Description
            => null;

        /// <inheritdoc/>
        public virtual bool IsEnvironmentallyEnabled
            => false;

        /// <inheritdoc/>
        public virtual string RunnerSwitch
            => null;

        /// <inheritdoc/>
        public virtual IMessageSinkWithTypes CreateMessageHandler(IRunnerLogger logger)
            => new DefaultRunnerReporterMessageHandler(logger);
    }
}
