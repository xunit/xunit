using System;
using Xunit;

namespace test.xunit.runner.visualstudio.wpa81
{
    public class UnitTest1
    {
        [Fact]
        public void TestFact()
        {
            Assert.True(true);
        }

        [Theory]
        [InlineData(1)]
        [InlineData(2)]
        [InlineData(3)]
        public void TestTheory(int i)
        {
            Assert.True(i % 2 == 0);
        }
    }
}
