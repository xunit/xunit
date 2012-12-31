using System;
using System.Reflection;
using Moq;
using Xunit.Abstractions;

public class MockTestCase<TClassUnderTest> : Mock<IMethodTestCase>
{
    public MockTestCase(string methodName)
    {
        TypeUnderTest = typeof(TClassUnderTest);
        if (TypeUnderTest == null)
            throw new Exception("You gave me a bum type.");

        MethodInfo = TypeUnderTest.GetMethod(methodName);
        if (MethodInfo == null)
            throw new Exception("You gave me a bum method name.");

        var testMethod = new Mock<IReflectionMethodInfo>();
        testMethod.SetupGet(tm => tm.MethodInfo).Returns(MethodInfo);

        var testClass = new Mock<IReflectionTypeInfo>();
        testClass.SetupGet(tc => tc.Type).Returns(TypeUnderTest);

        this.SetupGet(tc => tc.Class).Returns(testClass.Object);
        this.SetupGet(tc => tc.Method).Returns(testMethod.Object);
    }

    public Assembly Assembly
    {
        get { return TypeUnderTest.Assembly; }
    }

    public MethodInfo MethodInfo { get; private set; }

    public Type TypeUnderTest { get; private set; }
}