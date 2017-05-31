using Xunit;

namespace test.testcasefilter
{
    public class Tests
    {
        [Fact]
        [Trait("FilterCategory", "Exclude")]
        public void TestWithTraitToFilterOn()
        {
            Assert.True(true);
        }
    }
}
