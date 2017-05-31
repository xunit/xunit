using Xunit;

namespace test.harness
{
    public class Tests
    {
        [Fact]
        public void ThisIsATest()
        {
            Assert.True(true);
        }

        [Fact]
        [Trait("TestCategory", "Slow")]
        public void TestWithTrait()
        {
            Assert.True(true);
        }
    }
}
