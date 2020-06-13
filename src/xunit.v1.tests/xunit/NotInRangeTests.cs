using Xunit;
using Xunit.Sdk;

namespace Xunit1
{
    public class NotInRangeTests
    {
        public class RangeForDoubles
        {
            [Fact]
            public void DoubleNotWithinRange()
            {
                Assert.NotInRange(1.50, .75, 1.25);
            }

            [Fact]
            public void DoubleWithinRange()
            {
                Assert.Throws<NotInRangeException>(() => Assert.NotInRange(1.0, .75, 1.25));
            }
        }

        public class RangeForInt
        {
            [Fact]
            public void IntNotWithinRange()
            {
                Assert.NotInRange(1, 2, 3);
            }

            [Fact]
            public void IntWithinRange()
            {
                Assert.Throws<NotInRangeException>(() => Assert.NotInRange(2, 1, 3));
            }
        }

        public class RangeForStrings
        {
            [Fact]
            public void StringNotWithNotInRange()
            {
                Assert.NotInRange("adam", "bob", "scott");
            }

            [Fact]
            public void StringWithNotInRange()
            {
                Assert.Throws<NotInRangeException>(() => Assert.NotInRange("bob", "adam", "scott"));
            }
        }
    }
}
