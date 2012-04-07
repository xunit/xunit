using System;
using System.Reflection;
using Moq;
using Xunit;
using Xunit.Sdk;

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

    [Fact]
    public void TurnsParameterCountMismatchExceptionIntoInvalidOperationException()
    {
        Mock<IMethodInfo> method = new Mock<IMethodInfo>();
        method.SetupGet(m => m.Name)
              .Returns("StubbyName");
        method.SetupGet(m => m.TypeName)
              .Returns("StubbyType");
        method.Setup(m => m.Invoke(It.IsAny<object>(), It.IsAny<object[]>()))
              .Throws<ParameterCountMismatchException>();
        TestCommand command = new FactCommand(method.Object);

        Exception ex = Record.Exception(() => command.Execute(new TestMethodCommandClass()));

        Assert.IsType<InvalidOperationException>(ex);
        Assert.Equal("Fact method StubbyType.StubbyName cannot have parameters", ex.Message);
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