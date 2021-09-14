using System;
using System.Reflection;
using System.Xml;
using Xunit;
using Xunit.Sdk;

namespace Xunit1
{
    public class FailedResultTests
    {
        [Fact]
        public void InitializeFailedResult()
        {
            Type stubType = typeof(StubClass);
            MethodInfo method = stubType.GetMethod("StubMethod");
            StubException stubException = new StubException("Message", "StackTrace");

            FailedResult result = new FailedResult(Reflector.Wrap(method), stubException, null);
            result.ExecutionTime = 1.2;

            Assert.Equal("StubMethod", result.MethodName);
            Assert.Equal(stubType.FullName, result.TypeName);
            Assert.Equal(typeof(StubException).FullName + " : Message", result.Message);
            Assert.Equal(1.2, result.ExecutionTime);
            Assert.Equal("StackTrace", result.StackTrace);
        }

        [Fact]
        public void InitializeFailedResultWithMultipleInnerExceptions()
        {
            Type stubType = typeof(StubClass);
            MethodInfo method = stubType.GetMethod("StubMethod");
            Exception thrownException = null;
            Exception innerException = null;

            try
            {
                try
                {
                    throw new InvalidOperationException();
                }
                catch (Exception ex)
                {
                    innerException = ex;
                    throw new Exception("message", ex);
                }
            }
            catch (Exception ex)
            {
                thrownException = ex;
            }

            FailedResult result = new FailedResult(Reflector.Wrap(method), thrownException, null);

            result.ExecutionTime = 1.2;

            string expectedMessage = string.Format("{0} : {1}{2}---- {3} : {4}",
                                                   thrownException.GetType().FullName,
                                                   thrownException.Message,
                                                   Environment.NewLine,
                                                   innerException.GetType().FullName,
                                                   innerException.Message);

            Assert.Equal(expectedMessage, result.Message);

            string expectedStackTrace =
                string.Format("{0}{1}----- Inner Stack Trace -----{1}{2}",
                              thrownException.StackTrace,
                              Environment.NewLine,
                              innerException.StackTrace);

            Assert.Equal(expectedStackTrace, result.StackTrace);
        }

        static void ThrowAnException()
        {
            throw new Exception("message!");
        }

        [Fact]
        public void ToXml()
        {
            Exception ex = new Exception();

            try
            {
                ThrowAnException();
            }
            catch (Exception e)
            {
                ex = e;
            }

            XmlDocument doc = new XmlDocument();
            doc.LoadXml("<foo/>");
            XmlNode parentNode = doc.ChildNodes[0];
            MethodInfo method = typeof(StubClass).GetMethod("StubMethod");
            FailedResult failedResult = new FailedResult(Reflector.Wrap(method), ex, null);
            failedResult.ExecutionTime = 1.2;

            XmlNode resultNode = failedResult.ToXml(parentNode);

            Assert.Equal("Fail", resultNode.Attributes["result"].Value);
            Assert.Equal("1.200", resultNode.Attributes["time"].Value);
            XmlNode failureXmlNode = resultNode.SelectSingleNode("failure");
            Assert.NotNull(failureXmlNode);
            Assert.Equal(ex.GetType().FullName, failureXmlNode.Attributes["exception-type"].Value);
            Assert.Equal(ex.GetType().FullName + " : " + ex.Message, failureXmlNode.SelectSingleNode("message").InnerText);
            Assert.Equal(ex.StackTrace, failureXmlNode.SelectSingleNode("stack-trace").InnerText);
            Assert.Null(resultNode.SelectSingleNode("reason"));
        }

        internal class StubClass
        {
            public void StubMethod() { }
        }

        class StubException : Exception
        {
            readonly string message;
            readonly string stackTrace;

            public StubException(string message,
                                 string stackTrace)
            {
                this.message = message;
                this.stackTrace = stackTrace;
            }

            public override string Message
            {
                get { return message; }
            }

            public override string StackTrace
            {
                get { return stackTrace; }
            }
        }
    }
}
