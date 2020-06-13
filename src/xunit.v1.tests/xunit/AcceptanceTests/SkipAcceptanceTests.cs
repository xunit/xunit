using System;
using System.Linq;
using TestUtility;
using Xunit;
using Xunit.Sdk;

namespace Xunit1
{
    public class SkipAcceptanceTests : AcceptanceTest
    {
        [Fact]
        public void TestClassIsNotInstantiatedForSkippedTests()
        {
            MethodResult result = RunClass(typeof(ClassUnderTest)).Single();

            // If we ran the constructor, we would get a failure instead of a skip.
            Assert.IsType<SkipResult>(result);
        }

        class ClassUnderTest
        {
            public ClassUnderTest()
            {
                throw new Exception("Should not reach me!");
            }

            [Fact(Skip = "the reason")]
            public void TestThatShouldBeSkipped()
            {
            }
        }
    }
}
