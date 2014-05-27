using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xunit;

namespace xunit.runner.visualstudio.Win8
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
            Assert.True(i%2 == 0);
        }
    }
}
