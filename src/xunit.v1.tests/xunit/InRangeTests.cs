using Xunit;
using Xunit.Sdk;

namespace Xunit1
{
    public class InRangeTests
    {
        public class RangeForDoubles
        {
            [Fact]
            public void DoubleNotWithinRange()
            {
                Assert.Throws<InRangeException>(() => Assert.InRange(1.50, .75, 1.25));
            }

            [Fact]
            public void DoubleValueWithinRange()
            {
                Assert.InRange(1.0, .75, 1.25);
            }
        }

        public class RangeForInts
        {
            [Fact]
            public void IntNotWithinRangeWithZeroActual()
            {
                Assert.Throws<InRangeException>(() => Assert.InRange(0, 1, 2));
            }

            [Fact]
            public void IntNotWithinRangeWithZeroMinimum()
            {
                Assert.Throws<InRangeException>(() => Assert.InRange(2, 0, 1));
            }

            [Fact]
            public void IntValueWithinRange()
            {
                Assert.InRange(2, 1, 3);
            }
        }

        public class RangeForStrings
        {
            [Fact]
            public void StringNotWithinRange()
            {
                Assert.Throws<InRangeException>(() => Assert.InRange("adam", "bob", "scott"));
            }

            [Fact]
            public void StringValueWithinRange()
            {
                Assert.InRange("bob", "adam", "scott");
            }
        }
    }
}
