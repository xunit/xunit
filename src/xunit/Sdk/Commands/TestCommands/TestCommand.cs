using System.Diagnostics.CodeAnalysis;
using System.Xml;

namespace Xunit.Sdk
{
    /// <summary>
    /// Represents an xUnit.net test command.
    /// </summary>
    public abstract class TestCommand : ITestCommand
    {
        private int timeout;

        /// <summary>
        /// The method under test.
        /// </summary>
        [SuppressMessage("Microsoft.Design", "CA1051:DoNotDeclareVisibleInstanceFields", Justification = "Would be a breaking change")]
        protected IMethodInfo testMethod;

        /// <summary>
        /// Initializes a new instance of the <see cref="TestCommand"/> class.
        /// </summary>
        /// <param name="method">The method under test.</param>
        /// <param name="displayName">The display name of the test.</param>
        /// <param name="timeout">The timeout, in milliseconds.</param>
        [SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0", Justification = "This parameter is verified elsewhere.")]
        protected TestCommand(IMethodInfo method, string displayName, int timeout)
        {
            testMethod = method;
            MethodName = method.Name;
            TypeName = method.TypeName;
            DisplayName = displayName ?? TypeName + "." + MethodName;
            this.timeout = timeout;
        }

        /// <inheritdoc/>
        public string DisplayName { get; protected set; }

        /// <summary>
        /// Gets the name of the method under test.
        /// </summary>
        public string MethodName { get; protected set; }

        /// <inheritdoc/>
        public virtual bool ShouldCreateInstance
        {
            get { return !testMethod.IsStatic; }
        }

        /// <inheritdoc/>
        public virtual int Timeout
        {
            get { return timeout; }
        }

        /// <summary>
        /// Gets the name of the type under test.
        /// </summary>
        public string TypeName { get; protected set; }

        /// <inheritdoc/>
        public abstract MethodResult Execute(object testClass);

        /// <inheritdoc/>
        public virtual XmlNode ToStartXml()
        {
            XmlDocument doc = new XmlDocument();
            doc.LoadXml("<dummy/>");
            XmlNode testNode = XmlUtility.AddElement(doc.ChildNodes[0], "start");

            XmlUtility.AddAttribute(testNode, "name", DisplayName);
            XmlUtility.AddAttribute(testNode, "type", TypeName);
            XmlUtility.AddAttribute(testNode, "method", MethodName);

            return testNode;
        }
    }
}