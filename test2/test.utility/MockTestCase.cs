using System;
using System.Reflection;
using Moq;
using Xunit.Abstractions;

public class MockTestCase<TClassUnderTest> : Mock<ITestCase>
{
    public MockTestCase(string methodName)
    {
        var typeUnderTest = typeof(TClassUnderTest);
        if (typeUnderTest == null)
            throw new Exception("You gave me a bum type.");

        Assembly = typeUnderTest.Assembly;

        var methodInfo = typeUnderTest.GetMethod(methodName);
        if (methodInfo == null)
            throw new Exception("You gave me a bum method name.");

        var testMethod = new Mock<IReflectionMethodInfo>();
        testMethod.SetupGet(tm => tm.MethodInfo).Returns(methodInfo);

        var testClass = new Mock<IReflectionTypeInfo>();
        testClass.SetupGet(tc => tc.Type).Returns(typeUnderTest);

        this.SetupGet(tc => tc.Class).Returns(typeUnderTest);
        this.SetupGet(tc => tc.Method).Returns(methodInfo);
    }

    public Assembly Assembly { get; private set; }
}