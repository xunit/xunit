using System;
using System.Threading;
using Xunit;
using Xunit.Extensions;

namespace Xunit1.Extensions
{
    public class FreezeClockAttributeFacts
    {
        [Fact]
        public void DefaultConstructor()
        {
            FreezeClockAttribute attr = new FreezeClockAttribute();

            attr.Before(null);

            try
            {
                DateTime reference = DateTime.Now;
                DateTime result1 = Clock.Now;
                Thread.Sleep(100);
                DateTime result2 = Clock.Now;

                Assert.Equal(result1, result2);
                Assert.True((result1 - reference).TotalMilliseconds < 1000);
            }
            finally
            {
                attr.After(null);
            }
        }

        [Fact]
        public void DateSpecificConstructor()
        {
            FreezeClockAttribute attr = new FreezeClockAttribute(2006, 12, 31);

            attr.Before(null);

            try
            {
                DateTime reference = DateTime.Now;
                DateTime result = Clock.Now;

                Assert.NotEqual(reference, result);
                Assert.Equal(new DateTime(2006, 12, 31), result);
            }
            finally
            {
                attr.After(null);
            }
        }

        [Fact]
        public void LocalDateTimeConstructor()
        {
            FreezeClockAttribute attr = new FreezeClockAttribute(2006, 12, 31, 4, 5, 6);

            attr.Before(null);

            try
            {
                DateTime reference = DateTime.Now;
                DateTime result = Clock.Now;

                Assert.NotEqual(reference, result);
                Assert.Equal(new DateTime(2006, 12, 31, 4, 5, 6), result);
            }
            finally
            {
                attr.After(null);
            }
        }

        [Fact]
        public void KindSpecificDateTimeConstructor()
        {
            FreezeClockAttribute attr = new FreezeClockAttribute(2006, 12, 31, 4, 5, 6, DateTimeKind.Utc);

            attr.Before(null);

            try
            {
                DateTime reference = DateTime.Now;
                DateTime result = Clock.Now;

                Assert.NotEqual(reference, result);
                Assert.Equal(new DateTime(2006, 12, 31, 4, 5, 6, DateTimeKind.Utc).ToLocalTime(), result);
            }
            finally
            {
                attr.After(null);
            }
        }
    }
}
