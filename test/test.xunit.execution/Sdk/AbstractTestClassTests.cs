using System.Collections.Generic;
using System.Linq;
using Xunit;
using Xunit.Abstractions;

public class AbstractTestClassTests
{
    [Fact]
    public void AbstractTestClassesCanDependOnDataFromDerivedClasses()
    {
        var messages = InMemoryAcceptanceTestV2.Run(typeof(MyRealTest));

        var passed = Assert.Single(messages.OfType<ITestPassed>());
        Assert.Equal("AbstractTestClassTests+MyRealTest.Test(value: 1)", passed.Test.DisplayName);
        var failed = Assert.Single(messages.OfType<ITestFailed>());
        Assert.Equal("AbstractTestClassTests+MyRealTest.Test(value: 5)", failed.Test.DisplayName);
    }

    abstract class MyTestBase
    {
        [Theory]
        [MemberData("MyData")]
        public void Test(int value)
        {
            Assert.Equal(1, value);
        }
    }

    sealed class MyRealTest : MyTestBase
    {
        public static IEnumerable<object[]> MyData()
        {
            yield return new object[] { 1 };
            yield return new object[] { 5 };
        }
    }
}
