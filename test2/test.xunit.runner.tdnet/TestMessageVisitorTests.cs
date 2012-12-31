using System;
using System.Reflection;
using Moq;
using Moq.Protected;
using Xunit;
using Xunit.Abstractions;
using Xunit.Runner.TdNet;

public class TestMessageVisitorTests
{
    [Theory]
    [InlineData(typeof(ITestAssemblyFinished))]
    [InlineData(typeof(ITestFailed))]
    [InlineData(typeof(ITestPassed))]
    [InlineData(typeof(ITestSkipped))]
    public void ProcessesVisitorTypes(Type type)
    {
        // Make a mock of the interface in question
        Type mockType = typeof(Mock<>).MakeGenericType(type);
        ConstructorInfo ctor = mockType.GetConstructor(new Type[0]);
        Mock mock = (Mock)ctor.Invoke(new object[0]);
        ITestMessage mockImplementation = (ITestMessage)mock.Object;

        // Make the expression for the method we expect to be called
        MethodInfo isAny = typeof(ItExpr).GetMethod("IsAny", BindingFlags.Static | BindingFlags.Public)
                                         .MakeGenericMethod(new[] { type });
        object expression = isAny.Invoke(null, new object[0]);

        // Make a mock of the visitor so we can verify the right method was called
        Mock<TestMessageVisitor> mockVisitor = new Mock<TestMessageVisitor>();
        mockVisitor.CallBase = true;
        mockVisitor.Protected().Setup("Visit", expression).Verifiable();

        // Call the visitor with the mock
        mockVisitor.Object.Visit(mockImplementation);

        // Assert
        mockVisitor.Verify();
    }
}