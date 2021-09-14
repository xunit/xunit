using System;
using System.Reflection;
using Xunit;
using Xunit.Sdk;

namespace Xunit1
{
    public class FactCommandTests
    {
        [Fact]
        public void ExecuteRunsTest()
        {
            MethodInfo method = typeof(TestMethodCommandClass).GetMethod("TestMethod");
            TestCommand command = new FactCommand(Reflector.Wrap(method));
            TestMethodCommandClass.testCounter = 0;

            command.Execute(new TestMethodCommandClass());

            Assert.Equal(1, TestMethodCommandClass.testCounter);
        }

        [Fact]
        public void TestMethodReturnPassedResult()
        {
            MethodInfo method = typeof(TestMethodCommandClass).GetMethod("TestMethod");
            TestCommand command = new FactCommand(Reflector.Wrap(method));

            MethodResult result = command.Execute(new TestMethodCommandClass());

            Assert.IsType<PassedResult>(result);
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
