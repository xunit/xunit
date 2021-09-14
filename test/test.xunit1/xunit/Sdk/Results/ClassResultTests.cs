using System;
using System.Xml;
using Xunit;
using Xunit.Sdk;

namespace Xunit1
{
    public class ClassResultTests
    {
        [Fact]
        public void ConstructorGuardClauses()
        {
            Assert.Throws<ArgumentNullException>(() => new ClassResult(null));
        }

        [Fact]
        public void ConstructWithStrings()
        {
            ClassResult result = new ClassResult("name", "fullname", "namespace");

            Assert.Equal("fullname", result.FullyQualifiedName);
            Assert.Equal("name", result.Name);
            Assert.Equal("namespace", result.Namespace);
        }

        [Fact]
        public void ConstructWithType()
        {
            ClassResult result = new ClassResult(typeof(object));

            Assert.Equal(typeof(object).FullName, result.FullyQualifiedName);
            Assert.Equal(typeof(object).Name, result.Name);
            Assert.Equal(typeof(object).Namespace, result.Namespace);
        }

        [Fact]
        public void SetAssertException()
        {
            ClassResult result = new ClassResult(typeof(object));
            Exception thrownException;

            try
            {
                throw new EqualException(2, 3);
            }
            catch (Exception ex)
            {
                thrownException = ex;
                result.SetException(ex);
            }

            Assert.Equal(thrownException.GetType().FullName, result.ExceptionType);
            Assert.Equal(thrownException.Message, result.Message);
            Assert.Equal(thrownException.StackTrace, result.StackTrace);
        }

        [Fact]
        public void SetExceptionNull()
        {
            ClassResult result = new ClassResult(typeof(object));

            result.SetException(null);

            Assert.Null(result.ExceptionType);
            Assert.Null(result.Message);
            Assert.Null(result.StackTrace);
        }

        [Fact]
        public void SetNonAssertException()
        {
            ClassResult result = new ClassResult(typeof(object));
            Exception thrownException;

            try
            {
                throw new Exception("message");
            }
            catch (Exception ex)
            {
                thrownException = ex;
                result.SetException(ex);
            }

            Assert.Equal(thrownException.GetType().FullName, result.ExceptionType);
            Assert.Equal(result.ExceptionType + " : " + thrownException.Message, result.Message);
            Assert.Equal(thrownException.StackTrace, result.StackTrace);
        }

        [Fact]
        public void SetNonAssertExceptionWithInnerException()
        {
            ClassResult result = new ClassResult(typeof(object));
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
                result.SetException(ex);
            }

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

        [Fact]
        public void ToXml()
        {
            XmlDocument doc = new XmlDocument();
            doc.LoadXml("<foo/>");
            XmlNode parentNode = doc.ChildNodes[0];
            ClassResult classResult = new ClassResult(typeof(object));

            XmlNode resultNode = classResult.ToXml(parentNode);

            Assert.Equal("class", resultNode.Name);
            Assert.Equal(classResult.FullyQualifiedName, resultNode.Attributes["name"].Value);
            Assert.Equal("0.000", resultNode.Attributes["time"].Value);
            Assert.Equal("0", resultNode.Attributes["total"].Value);
            Assert.Equal("0", resultNode.Attributes["passed"].Value);
            Assert.Equal("0", resultNode.Attributes["failed"].Value);
            Assert.Equal("0", resultNode.Attributes["skipped"].Value);
        }

        [Fact]
        public void ToXml_WithChildren()
        {
            XmlDocument doc = new XmlDocument();
            doc.LoadXml("<foo/>");
            XmlNode parentNode = doc.ChildNodes[0];
            ClassResult classResult = new ClassResult(typeof(object));
            PassedResult passedResult = new PassedResult("foo", "bar", null, null);
            passedResult.ExecutionTime = 1.1;
            FailedResult failedResult = new FailedResult("foo", "bar", null, null, "extype", "message", "stack");
            failedResult.ExecutionTime = 2.2;
            SkipResult skipResult = new SkipResult("foo", "bar", null, null, "reason");
            classResult.Add(passedResult);
            classResult.Add(failedResult);
            classResult.Add(skipResult);

            XmlNode resultNode = classResult.ToXml(parentNode);

            Assert.Equal("3.300", resultNode.Attributes["time"].Value);
            Assert.Equal("3", resultNode.Attributes["total"].Value);
            Assert.Equal("1", resultNode.Attributes["passed"].Value);
            Assert.Equal("1", resultNode.Attributes["failed"].Value);
            Assert.Equal("1", resultNode.Attributes["skipped"].Value);
            Assert.Equal(3, resultNode.SelectNodes("test").Count);
        }

        [Fact]
        public void ToXml_WithClassFailure()
        {
            Exception ex;

            try
            {
                throw new InvalidOperationException("message");
            }
            catch (Exception e)
            {
                ex = e;
            }

            XmlDocument doc = new XmlDocument();
            doc.LoadXml("<foo/>");
            XmlNode parentNode = doc.ChildNodes[0];
            ClassResult classResult = new ClassResult(typeof(object));
            classResult.SetException(ex);

            XmlNode resultNode = classResult.ToXml(parentNode);

            Assert.Equal("class", resultNode.Name);
            Assert.Equal(classResult.FullyQualifiedName, resultNode.Attributes["name"].Value);
            Assert.Equal("0.000", resultNode.Attributes["time"].Value);
            Assert.Equal("1", resultNode.Attributes["total"].Value);
            Assert.Equal("0", resultNode.Attributes["passed"].Value);
            Assert.Equal("1", resultNode.Attributes["failed"].Value);
            Assert.Equal("0", resultNode.Attributes["skipped"].Value);
            XmlNode failureNode = resultNode.SelectSingleNode("failure");
            Assert.Equal(ex.GetType().FullName, failureNode.Attributes["exception-type"].Value);
            Assert.Equal(ExceptionUtility.GetMessage(ex), failureNode.SelectSingleNode("message").InnerText);
            Assert.Equal(ExceptionUtility.GetStackTrace(ex), failureNode.SelectSingleNode("stack-trace").InnerText);
        }

        [Fact]
        public void ToXmlTwiceDoesNotDoubleCounts()
        {
            XmlDocument doc = new XmlDocument();
            doc.LoadXml("<foo/>");
            XmlNode parentNode = doc.ChildNodes[0];
            ClassResult classResult = new ClassResult(typeof(object));
            PassedResult passedResult = new PassedResult("foo", "bar", null, null);
            passedResult.ExecutionTime = 1.1;
            FailedResult failedResult = new FailedResult("foo", "bar", null, null, "extype", "message", "stack");
            failedResult.ExecutionTime = 2.2;
            SkipResult skipResult = new SkipResult("foo", "bar", null, null, "reason");
            classResult.Add(passedResult);
            classResult.Add(failedResult);
            classResult.Add(skipResult);

            XmlNode resultNode1 = classResult.ToXml(parentNode);
            XmlNode resultNode2 = classResult.ToXml(parentNode);

            Assert.Equal(resultNode1.Attributes["time"].Value, resultNode2.Attributes["time"].Value);
            Assert.Equal(resultNode1.Attributes["total"].Value, resultNode2.Attributes["total"].Value);
            Assert.Equal(resultNode1.Attributes["passed"].Value, resultNode2.Attributes["passed"].Value);
            Assert.Equal(resultNode1.Attributes["failed"].Value, resultNode2.Attributes["failed"].Value);
            Assert.Equal(resultNode1.Attributes["skipped"].Value, resultNode2.Attributes["skipped"].Value);
            Assert.Equal(resultNode1.SelectNodes("test").Count, resultNode2.SelectNodes("test").Count);
        }
    }
}
