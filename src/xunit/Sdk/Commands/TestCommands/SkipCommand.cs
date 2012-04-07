using System.Xml;

namespace Xunit.Sdk
{
    /// <summary>
    /// Implementation of <see cref="ITestCommand"/> that represents a skipped test.
    /// </summary>
    public class SkipCommand : TestCommand
    {
        /// <summary>
        /// Creates a new instance of the <see cref="SkipCommand"/> class.
        /// </summary>
        /// <param name="testMethod">The method that is being skipped</param>
        /// <param name="displayName">The display name for the test. If null, the fully qualified
        /// type name is used.</param>
        /// <param name="reason">The reason the test was skipped.</param>
        public SkipCommand(IMethodInfo testMethod, string displayName, string reason)
            : base(testMethod, displayName, 0)
        {
            Reason = reason;
        }

        /// <summary>
        /// Gets the skip reason.
        /// </summary>
        public string Reason { get; private set; }

        /// <inheritdoc/>
        public override bool ShouldCreateInstance
        {
            get { return false; }
        }

        /// <inheritdoc/>
        public override MethodResult Execute(object testClass)
        {
            return new SkipResult(testMethod, DisplayName, Reason);
        }

        /// <inheritdoc/>
        public override XmlNode ToStartXml()
        {
            return null;
        }
    }
}