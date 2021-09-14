using System.Collections.Generic;
using System.Linq;
using TestUtility;
using Xunit;
using Xunit.Extensions;
using Xunit.Sdk;

namespace Xunit1.Extensions
{
    public class PropertyDataAcceptanceTests : AcceptanceTest
    {
        [Fact]
        public void PropertyDataTest()
        {
            MethodResult result = RunClass(typeof(PropertyDataTestClass)).Single();

            Assert.IsType<PassedResult>(result);
            Assert.Equal(@"Xunit1.Extensions.PropertyDataAcceptanceTests+PropertyDataTestClass.PassingTestData(foo: 1, bar: ""hello world"", baz: 2.3)", result.DisplayName);
        }

        class PropertyDataTestClass
        {
            public static IEnumerable<object[]> MyTestData
            {
                get { yield return new object[] { 1, "hello world", 2.3 }; }
            }

            [Theory, PropertyData("MyTestData")]
            public void PassingTestData(int foo, string bar, double baz)
            {
            }
        }
    }
}
