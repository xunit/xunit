using System;
using System.Diagnostics.CodeAnalysis;
using System.Xml;

namespace Xunit.Sdk
{
    /// <summary>
    /// Represents a failed test result.
    /// </summary>
    [Serializable]
    public class FailedResult : MethodResult
    {
        /// <summary>
        /// Creates a new instance of the <see cref="FailedResult"/> class.
        /// </summary>
        /// <param name="method">The method under test</param>
        /// <param name="exception">The exception throw by the test</param>
        /// <param name="displayName">The display name for the test. If null, the fully qualified
        /// type name is used.</param>
        [SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "1", Justification = "This parameter is verified elsewhere.")]
        public FailedResult(IMethodInfo method, Exception exception, string displayName)
            : base(method, displayName)
        {
            ExceptionType = exception.GetType().FullName;
            Message = ExceptionUtility.GetMessage(exception);
            StackTrace = ExceptionUtility.GetStackTrace(exception);
        }

        /// <summary>
        /// Creates a new instance of the <see cref="FailedResult"/> class.
        /// </summary>
        /// <param name="methodName">The name of the method under test</param>
        /// <param name="typeName">The name of the type under test</param>
        /// <param name="displayName">The display name of the test</param>
        /// <param name="traits">The custom properties attached to the test method</param>
        /// <param name="exceptionType">The full type name of the exception throw</param>
        /// <param name="message">The exception message</param>
        /// <param name="stackTrace">The exception stack trace</param>
        public FailedResult(string methodName, string typeName, string displayName, MultiValueDictionary<string, string> traits, string exceptionType, string message, string stackTrace)
            : base(methodName, typeName, displayName, traits)
        {
            ExceptionType = exceptionType;
            Message = message;
            StackTrace = stackTrace;
        }

        /// <summary>
        /// Gets the exception type thrown by the test method.
        /// </summary>
        public string ExceptionType { get; private set; }

        /// <summary>
        /// Gets the exception message thrown by the test method.
        /// </summary>
        public string Message { get; private set; }

        /// <summary>
        /// Gets the stack trace of the exception thrown by the test method.
        /// </summary>
        public string StackTrace { get; private set; }

        /// <summary>
        /// Converts the test result into XML that is consumed by the test runners.
        /// </summary>
        /// <param name="parentNode">The parent node.</param>
        /// <returns>The newly created XML node.</returns>
        public override XmlNode ToXml(XmlNode parentNode)
        {
            XmlNode testNode = base.ToXml(parentNode);

            XmlUtility.AddAttribute(testNode, "result", "Fail");
            AddTime(testNode);

            XmlNode failureNode = XmlUtility.AddElement(testNode, "failure");
            XmlUtility.AddAttribute(failureNode, "exception-type", ExceptionType);

            XmlNode messageNode = XmlUtility.AddElement(failureNode, "message");
            messageNode.InnerText = Message;

            if (!string.IsNullOrEmpty(StackTrace))
            {
                XmlNode stackTraceNode = XmlUtility.AddElement(failureNode, "stack-trace");
                stackTraceNode.InnerText = StackTrace;
            }

            return testNode;
        }
    }
}