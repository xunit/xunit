using System;
using System.Linq;
using TestUtility;
using Xunit;
using Xunit.Sdk;

public class FactMethodWithArgumentsTests : AcceptanceTest
{
    [Fact]
    public void FactMethodsCannotHaveArguments()
    {
        MethodResult result = RunClass(typeof(ClassUnderTest)).Single();

        FailedResult failedResult = Assert.IsType<FailedResult>(result);
        Assert.Equal(typeof(InvalidOperationException).FullName, failedResult.ExceptionType);
        Assert.Equal("System.InvalidOperationException : Fact method FactMethodWithArgumentsTests+ClassUnderTest.FactWithParameters cannot have parameters", failedResult.Message);
    }

    class ClassUnderTest
    {
        [Fact]
        public void FactWithParameters(int x) { }
    }
}
