using System;
using System.Xml;

namespace Xunit.Sdk
{
    /// <summary>
    /// Contains the test results from a test class.
    /// </summary>
    [Serializable]
    public class ClassResult : CompositeResult
    {
        string exceptionType;
        int failCount;
        string message;
        int passCount;
        int skipCount;
        string stackTrace;
        readonly string typeFullName;
        readonly string typeName;
        readonly string typeNamespace;

        /// <summary>
        /// Creates a new instance of the <see cref="ClassResult"/> class.
        /// </summary>
        /// <param name="type">The type under test</param>
        public ClassResult(Type type)
        {
            if (type == null)
                throw new ArgumentNullException("type");

            typeName = type.Name;
            typeFullName = type.FullName;
            typeNamespace = type.Namespace;
        }

        /// <summary>
        /// Creates a new instance of the <see cref="ClassResult"/> class.
        /// </summary>
        /// <param name="typeName">The simple name of the type under test</param>
        /// <param name="typeFullName">The fully qualified name of the type under test</param>
        /// <param name="typeNamespace">The namespace of the type under test</param>
        public ClassResult(string typeName,
                           string typeFullName,
                           string typeNamespace)
        {
            this.typeName = typeName;
            this.typeFullName = typeFullName;
            this.typeNamespace = typeNamespace;
        }

        /// <summary>
        /// Gets the fully qualified test fixture exception type, when an exception has occurred.
        /// </summary>
        public string ExceptionType
        {
            get { return exceptionType; }
        }

        /// <summary>
        /// Gets the number of tests which failed.
        /// </summary>
        public int FailCount
        {
            get { return failCount; }
        }

        /// <summary>
        /// Gets the fully qualified name of the type under test.
        /// </summary>
        public string FullyQualifiedName
        {
            get { return typeFullName; }
        }

        /// <summary>
        /// Gets the test fixture exception message, when an exception has occurred.
        /// </summary>
        public string Message
        {
            get { return message; }
        }

        /// <summary>
        /// Gets the simple name of the type under test.
        /// </summary>
        public string Name
        {
            get { return typeName; }
        }

        /// <summary>
        /// Gets the namespace of the type under test.
        /// </summary>
        public string Namespace
        {
            get { return typeNamespace; }
        }

        /// <summary>
        /// Gets the number of tests which passed.
        /// </summary>
        public int PassCount
        {
            get { return passCount; }
        }

        /// <summary>
        /// Gets the number of tests which were skipped.
        /// </summary>
        public int SkipCount
        {
            get { return skipCount; }
        }

        /// <summary>
        /// Gets the test fixture exception stack trace, when an exception has occurred.
        /// </summary>
        public string StackTrace
        {
            get { return stackTrace; }
        }

        /// <summary>
        /// Sets the exception thrown by the test fixture.
        /// </summary>
        /// <param name="ex">The thrown exception</param>
        public void SetException(Exception ex)
        {
            if (ex == null)
            {
                exceptionType = null;
                message = null;
                stackTrace = null;
            }
            else
            {
                exceptionType = ex.GetType().FullName;
                message = ExceptionUtility.GetMessage(ex);
                stackTrace = ExceptionUtility.GetStackTrace(ex);
            }
        }

        /// <summary>
        /// Converts the test result into XML that is consumed by the test runners.
        /// </summary>
        /// <param name="parentNode">The parent node.</param>
        /// <returns>The newly created XML node.</returns>
        public override XmlNode ToXml(XmlNode parentNode)
        {
            failCount = 0;
            passCount = 0;
            skipCount = 0;
            ExecutionTime = 0.0;

            XmlNode classNode = XmlUtility.AddElement(parentNode, "class");

            if (Message != null)
            {
                failCount += 1;

                XmlNode failureNode = XmlUtility.AddElement(classNode, "failure");
                XmlUtility.AddAttribute(failureNode, "exception-type", ExceptionType);
                XmlNode messageNode = XmlUtility.AddElement(failureNode, "message");
                XmlUtility.SetInnerText(messageNode, Message);
                XmlNode stackTraceNode = XmlUtility.AddElement(failureNode, "stack-trace");
                XmlUtility.SetInnerText(stackTraceNode, StackTrace);
            }

            foreach (ITestResult testResult in Results)
            {
                testResult.ToXml(classNode);

                if (testResult is PassedResult)
                    passCount++;
                else if (testResult is FailedResult)
                    failCount++;
                else if (testResult is SkipResult)
                    skipCount++;

                ExecutionTime += testResult.ExecutionTime;
            }

            AddTime(classNode);
            XmlUtility.AddAttribute(classNode, "name", FullyQualifiedName);
            XmlUtility.AddAttribute(classNode, "total", classNode.ChildNodes.Count);
            XmlUtility.AddAttribute(classNode, "passed", passCount);
            XmlUtility.AddAttribute(classNode, "failed", failCount);
            XmlUtility.AddAttribute(classNode, "skipped", skipCount);

            return classNode;
        }
    }
}