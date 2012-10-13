using Xunit;
using Xunit.Sdk;

public class XunitTestCaseTests
{
    [Fact]
    public void DefaultFactAttribute()
    {
        var fact = new MockFactAttribute();
        var method = new MockMethodInfo(attributes: new[] { fact.Object });
        var type = new MockTypeInfo(methods: new[] { method.Object });
        var assmInfo = new MockAssemblyInfo(types: new[] { type.Object });

        var testCase = new XunitTestCase(assmInfo.Object, type.Object, method.Object, fact.Object);

        Assert.Equal("MockType.MockMethod", testCase.DisplayName);
        Assert.Null(testCase.SkipReason);
    }

    [Fact]
    public void DisplayName()
    {
        var fact = new MockFactAttribute(displayName: "Custom Display Name");
        var method = new MockMethodInfo(attributes: new[] { fact.Object });
        var type = new MockTypeInfo(methods: new[] { method.Object });
        var assmInfo = new MockAssemblyInfo(types: new[] { type.Object });

        var testCase = new XunitTestCase(assmInfo.Object, type.Object, method.Object, fact.Object);

        Assert.Equal("Custom Display Name", testCase.DisplayName);
    }

    [Fact]
    public void Skip()
    {
        var fact = new MockFactAttribute(skip: "Skip Reason");
        var method = new MockMethodInfo(attributes: new[] { fact.Object });
        var type = new MockTypeInfo(methods: new[] { method.Object });
        var assmInfo = new MockAssemblyInfo(types: new[] { type.Object });

        var testCase = new XunitTestCase(assmInfo.Object, type.Object, method.Object, fact.Object);

        Assert.Equal("Skip Reason", testCase.SkipReason);
    }

    [Fact]
    public void CorrectNumberOfTestArguments()
    {
        var fact = new MockFactAttribute();
        var param1 = new MockParameterInfo("p1");
        var param2 = new MockParameterInfo("p2");
        var param3 = new MockParameterInfo("p3");
        var method = new MockMethodInfo(attributes: new[] { fact.Object }, parameters: new[] { param1.Object, param2.Object, param3.Object });
        var type = new MockTypeInfo(methods: new[] { method.Object });
        var assmInfo = new MockAssemblyInfo(types: new[] { type.Object });
        var arguments = new object[] { 42, "Hello, world!", 'A' };

        var testCase = new XunitTestCase(assmInfo.Object, type.Object, method.Object, fact.Object, arguments);

        Assert.Equal("MockType.MockMethod(p1: 42, p2: \"Hello, world!\", p3: 'A')", testCase.DisplayName);
    }

    [Fact]
    public void NotEnoughTestArguments()
    {
        var fact = new MockFactAttribute();
        var param = new MockParameterInfo("p1");
        var method = new MockMethodInfo(attributes: new[] { fact.Object }, parameters: new[] { param.Object });
        var type = new MockTypeInfo(methods: new[] { method.Object });
        var assmInfo = new MockAssemblyInfo(types: new[] { type.Object });

        var testCase = new XunitTestCase(assmInfo.Object, type.Object, method.Object, fact.Object, arguments: new object[0]);

        Assert.Equal("MockType.MockMethod(p1: ???)", testCase.DisplayName);
    }

    [Fact]
    public void TooManyTestArguments()
    {
        var fact = new MockFactAttribute();
        var param = new MockParameterInfo("p1");
        var method = new MockMethodInfo(attributes: new[] { fact.Object }, parameters: new[] { param.Object });
        var type = new MockTypeInfo(methods: new[] { method.Object });
        var assmInfo = new MockAssemblyInfo(types: new[] { type.Object });
        var arguments = new object[] { 42, 21.12 };

        var testCase = new XunitTestCase(assmInfo.Object, type.Object, method.Object, fact.Object, arguments);

        Assert.Equal("MockType.MockMethod(p1: 42, ???: 21.12)", testCase.DisplayName);
    }
}
