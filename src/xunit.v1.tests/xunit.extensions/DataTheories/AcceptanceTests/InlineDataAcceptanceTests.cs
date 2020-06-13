using System.Linq;
using TestUtility;
using Xunit;
using Xunit.Extensions;
using Xunit.Sdk;

namespace Xunit1.Extensions
{
    public class InlineDataAcceptanceTests : AcceptanceTest
    {
        [Fact]
        public void MultipleDataSets()
        {
            MethodResult[] results = RunClass(typeof(MultipleDataSetsClass)).ToArray();

            Assert.Equal(2, results.Length);
            PassedResult passedResult = results.OfType<PassedResult>().Single();
            Assert.Equal(@"Xunit1.Extensions.InlineDataAcceptanceTests+MultipleDataSetsClass.PassingTestData(foo: 1, bar: ""hello"", baz: 2.3)", passedResult.DisplayName);
            FailedResult failedResult = results.OfType<FailedResult>().Single();
            Assert.Equal(@"Xunit1.Extensions.InlineDataAcceptanceTests+MultipleDataSetsClass.PassingTestData(foo: 42, bar: ""world"", baz: 21.12)", failedResult.DisplayName);
        }

        class MultipleDataSetsClass
        {
            [Theory]
            [InlineData(1, "hello", 2.3)]
            [InlineData(42, "world", 21.12)]
            public void PassingTestData(int foo, string bar, double baz)
            {
                Assert.Equal(1, foo);
            }
        }

        [Fact]
        public void NullValue()
        {
            MethodResult result = RunClass(typeof(NullValueClass)).Single();

            Assert.IsType<PassedResult>(result);
            Assert.Equal(@"Xunit1.Extensions.InlineDataAcceptanceTests+NullValueClass.PassingTestData(foo: null)", result.DisplayName);
        }

        public class NullValueClass
        {
            [Theory]
            [InlineData(null)]
            public void PassingTestData(string foo)
            {
                Assert.Null(foo);
            }
        }
    }
}
