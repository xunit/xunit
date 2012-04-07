using System.Xml;

namespace Xunit.Sdk
{
    /// <summary>
    /// Base class used by commands which delegate to inner commands.
    /// </summary>
    public abstract class DelegatingTestCommand : ITestCommand
    {
        readonly ITestCommand innerCommand;

        /// <summary>
        /// Creates a new instance of the <see cref="DelegatingTestCommand"/> class.
        /// </summary>
        /// <param name="innerCommand">The inner command to delegate to.</param>
        protected DelegatingTestCommand(ITestCommand innerCommand)
        {
            this.innerCommand = innerCommand;
        }

        /// <inheritdoc/>
        public ITestCommand InnerCommand
        {
            get { return innerCommand; }
        }

        /// <inheritdoc/>
        public string DisplayName
        {
            get { return innerCommand.DisplayName; }
        }

        /// <inheritdoc/>
        public bool ShouldCreateInstance
        {
            get { return innerCommand.ShouldCreateInstance; }
        }

        /// <inheritdoc/>
        public virtual int Timeout
        {
            get { return innerCommand.Timeout; }
        }

        /// <inheritdoc/>
        public abstract MethodResult Execute(object testClass);

        /// <inheritdoc/>
        public XmlNode ToStartXml()
        {
            return innerCommand.ToStartXml();
        }
    }
}