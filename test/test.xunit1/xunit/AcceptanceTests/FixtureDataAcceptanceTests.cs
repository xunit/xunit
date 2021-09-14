using System.Linq;
using TestUtility;
using Xunit;
using Xunit.Sdk;

namespace Xunit1
{
    public class FixtureDataAcceptanceTests : AcceptanceTest
    {
        [Fact]
        public void ClassWithFixtureAndSkippedFactDoesNotSetFixtureData()
        {
            MethodResult result = RunClass(typeof(FixtureWithAllSkips)).Single();

            // If it ran the fixture, then we would get a class failure
            Assert.IsType<SkipResult>(result);
        }

        class FixtureWithAllSkips : IUseFixture<object>
        {
            public void SetFixture(object data)
            {
                Assert.True(false);
            }

            [Fact(Skip = "Skip Me!")]
            public void SkippedTest() { }
        }

        [Fact]
        public void ClassWithFixtureAndStaticFactDoesNotSetFixtureData()
        {
            MethodResult result = RunClass(typeof(FixtureWithAllStatics)).Single();

            // If it ran the fixture, then we would get a class failure
            Assert.IsType<PassedResult>(result);
        }

        class FixtureWithAllStatics : IUseFixture<object>
        {
            public void SetFixture(object data)
            {
                Assert.True(false);
            }

            [Fact]
            public static void StaticPassingTest() { }
        }
    }
}
