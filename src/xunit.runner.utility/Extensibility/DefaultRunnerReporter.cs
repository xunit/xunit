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
        {
            get { return null; }
        }

        /// <inheritdoc/>
        public virtual bool IsEnvironmentallyEnabled
        {
            get { return false; }
        }

        /// <inheritdoc/>
        public virtual string RunnerSwitch
        {
            get { return null; }
        }

        /// <inheritdoc/>
        public virtual IMessageSink CreateMessageHandler(IRunnerLogger logger)
        {
            return new DefaultRunnerReporterMessageHandler(logger);
        }
    }
}
