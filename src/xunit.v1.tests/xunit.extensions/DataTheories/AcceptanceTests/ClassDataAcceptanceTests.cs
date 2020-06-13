using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TestUtility;
using Xunit;
using Xunit.Extensions;
using Xunit.Sdk;

namespace Xunit1.Extensions
{
    public class ClassDataAcceptanceTests : AcceptanceTest
    {
        [Fact]
        public void ClassDataTest()
        {
            MethodResult result = RunClass(typeof(ClassDataTestClass)).Single();

            Assert.IsType<PassedResult>(result);
            Assert.Equal(@"Xunit1.Extensions.ClassDataAcceptanceTests+ClassDataTestClass.PassingTestData(foo: 1, bar: ""hello world"", baz: 2.3)", result.DisplayName);
        }

        public class ClassDataTestData : IEnumerable<object[]>
        {
            public IEnumerator<object[]> GetEnumerator()
            {
                yield return new object[] { 1, "hello world", 2.3 };
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }
        }

        class ClassDataTestClass
        {
            [Theory, ClassData(typeof(ClassDataTestData))]
            public void PassingTestData(int foo, string bar, double baz)
            {
            }
        }
    }
}
