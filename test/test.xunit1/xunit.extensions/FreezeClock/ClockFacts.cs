using System;
using Xunit;
using Xunit.Extensions;

namespace Xunit1.Extensions
{
    public class ClockFacts
    {
        public class Freeze
        {
            [Fact]
            public void DoubleFreezeThrows()
            {
                Clock.Freeze();

                try
                {
                    Assert.Throws<InvalidOperationException>(() => Clock.Freeze());
                }
                finally
                {
                    Clock.Thaw();
                }
            }
        }

        public class Now
        {
            [Fact]
            public void ConvertsFrozenUtcTimeIntoLocalTime()
            {
                DateTime frozenTime = DateTime.SpecifyKind(DateTime.Now.AddDays(-1), DateTimeKind.Utc);
                Clock.FreezeUtc(frozenTime);

                try
                {
                    DateTime result = Clock.Now;

                    if (TimeZone.CurrentTimeZone.GetUtcOffset(frozenTime) != TimeSpan.Zero)
                        Assert.NotEqual(frozenTime, result);

                    Assert.Equal(frozenTime.ToLocalTime(), result);
                }
                finally
                {
                    Clock.Thaw();
                }
            }

            [Fact]
            public void ReturnsToCurrentTimeAfterThaw()
            {
                DateTime frozenTime = DateTime.SpecifyKind(DateTime.Now.AddDays(-1), DateTimeKind.Local);
                Clock.FreezeLocal(frozenTime);
                Clock.Thaw();

                DateTime clockNow = Clock.Now;

                Assert.NotEqual(frozenTime, clockNow);
            }

            [Fact]
            public void UsesCurrentTimeByDefault()
            {
                DateTime now = DateTime.Now;

                DateTime result = Clock.Now;

                Assert.True((result - now).TotalMilliseconds < 1000);
            }

            [Fact]
            public void UsesFrozenLocalTime()
            {
                DateTime frozenTime = DateTime.SpecifyKind(DateTime.Now.AddDays(-1), DateTimeKind.Local);
                Clock.FreezeLocal(frozenTime);

                try
                {
                    DateTime result = Clock.Now;

                    Assert.Equal(frozenTime, result);
                }
                finally
                {
                    Clock.Thaw();
                }
            }
        }

        public class Thaw
        {
            [Fact]
            public void ThawWithoutFreezeThrows()
            {
                Assert.Throws<InvalidOperationException>(() => Clock.Thaw());
            }
        }

        public class UtcNow
        {
            [Fact]
            public void ConvertsFrozenLocalTimeIntoUtcTime()
            {
                DateTime frozenTime = DateTime.SpecifyKind(DateTime.Now.AddDays(-1), DateTimeKind.Local);
                Clock.FreezeLocal(frozenTime);

                try
                {
                    DateTime result = Clock.UtcNow;

                    if (TimeZone.CurrentTimeZone.GetUtcOffset(frozenTime) != TimeSpan.Zero)
                        Assert.NotEqual(frozenTime, result);

                    Assert.Equal(frozenTime.ToUniversalTime(), result);
                }
                finally
                {
                    Clock.Thaw();
                }
            }

            [Fact]
            public void ReturnsToCurrentTimeAfterThaw()
            {
                DateTime frozenTime = DateTime.SpecifyKind(DateTime.Now.AddDays(-1), DateTimeKind.Utc);
                Clock.FreezeUtc(frozenTime);
                Clock.Thaw();

                DateTime clockNow = Clock.UtcNow;

                Assert.NotEqual(frozenTime, clockNow);
            }

            [Fact]
            public void UsesCurrentTimeByDefault()
            {
                DateTime now = DateTime.UtcNow;

                DateTime result = Clock.UtcNow;

                Assert.True((result - now).TotalMilliseconds < 1000);
            }

            [Fact]
            public void UsesFrozenUtcTime()
            {
                DateTime frozenTime = DateTime.SpecifyKind(DateTime.Now.AddDays(-1), DateTimeKind.Utc);
                Clock.FreezeUtc(frozenTime);

                try
                {
                    DateTime result = Clock.UtcNow;

                    Assert.Equal(frozenTime, result);
                }
                finally
                {
                    Clock.Thaw();
                }
            }
        }
    }
}
