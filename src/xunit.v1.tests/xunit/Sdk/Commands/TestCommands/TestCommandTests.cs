using System;
using System.Reflection;
using System.Xml;
using Moq;
using TestUtility;
using Xunit;
using Xunit.Sdk;

namespace Xunit1
{
    public class TestCommandTests
    {
        [Fact]
        public void TestCommandReturnsStartXml_WithoutDisplayName()
        {
            MethodInfo method = typeof(TestMethodCommandClass).GetMethod("TestMethod");
            Mock<TestCommand> command = new Mock<TestCommand>(Reflector.Wrap(method), null, 0);
            command.CallBase = true;

            XmlNode result = command.Object.ToStartXml();

            Assert.Equal("start", result.Name);
            ResultXmlUtility.AssertAttribute(result, "name", typeof(TestMethodCommandClass).FullName + ".TestMethod");
            ResultXmlUtility.AssertAttribute(result, "type", typeof(TestMethodCommandClass).FullName);
            ResultXmlUtility.AssertAttribute(result, "method", "TestMethod");
        }

        [Fact]
        public void TestCommandReturnsStartXml_WithDisplayName()
        {
            MethodInfo method = typeof(TestMethodCommandClass).GetMethod("TestMethod");
            Mock<TestCommand> command = new Mock<TestCommand>(Reflector.Wrap(method), "Display Name!", 0);
            command.CallBase = true;

            XmlNode result = command.Object.ToStartXml();

            Assert.Equal("start", result.Name);
            ResultXmlUtility.AssertAttribute(result, "name", "Display Name!");
            ResultXmlUtility.AssertAttribute(result, "type", typeof(TestMethodCommandClass).FullName);
            ResultXmlUtility.AssertAttribute(result, "method", "TestMethod");
        }

        internal class TestMethodCommandClass
        {
            public static int testCounter;

            public void TestMethod()
            {
                ++testCounter;
            }

            public void ThrowsException()
            {
                throw new InvalidOperationException();
            }

            public void ThrowsTargetInvocationException()
            {
                throw new TargetInvocationException(null);
            }
        }
    }
}
