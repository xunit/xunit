using System;
using System.Threading;
using Xunit;
using Xunit.Extensions;

namespace Xunit1.Extensions
{
    public class FreezeClockAttributeAcceptanceTests
    {
        [Fact, FreezeClock]
        public void FrozenWithCurrentTime()
        {
            DateTime reference = DateTime.Now;

            DateTime result1 = Clock.Now;
            Thread.Sleep(100);
            DateTime result2 = Clock.Now;

            Assert.Equal(result1, result2);
            Assert.True((reference - result1).TotalMilliseconds < 1000);
        }

        [Fact, FreezeClock(2006, 12, 31)]
        public void FrozenWithSpecificDate()
        {
            DateTime result = Clock.Now;

            Assert.Equal(new DateTime(2006, 12, 31), result);
        }

        [Fact, FreezeClock(2006, 12, 31, 4, 5, 6)]
        public void FrozenWithSpecificLocalDateTime()
        {
            DateTime result = Clock.Now;

            Assert.Equal(new DateTime(2006, 12, 31, 4, 5, 6), result);
        }

        [Fact, FreezeClock(2006, 12, 31, 4, 5, 6, DateTimeKind.Utc)]
        public void FrozenWithSpecificUTCDateTime()
        {
            DateTime result = Clock.Now;

            Assert.Equal(new DateTime(2006, 12, 31, 4, 5, 6, DateTimeKind.Utc).ToLocalTime(), result);
        }

        [Fact]
        public void NotFrozen()
        {
            DateTime result1 = Clock.Now;
            Thread.Sleep(100);
            DateTime result2 = Clock.Now;

            Assert.NotEqual(result1, result2);
        }
    }
}
