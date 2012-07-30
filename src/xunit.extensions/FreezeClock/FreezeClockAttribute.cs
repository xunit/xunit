using System;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;

namespace Xunit.Extensions
{
    /// <summary>
    /// Apply this attribute to your test method to freeze the time represented by the
    /// <see cref="Clock"/> class.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
    [SuppressMessage("Microsoft.Design", "CA1019:DefineAccessorsForAttributeArguments", Justification = "The values are available indirectly on the base class.")]
    [SuppressMessage("Microsoft.Performance", "CA1813:AvoidUnsealedAttributes", Justification = "This attribute is designed as an extensibility point.")]
    public class FreezeClockAttribute : BeforeAfterTestAttribute
    {
        readonly DateTime frozenDateTime;

        /// <summary>
        /// Freeze the clock with the current date and time.
        /// </summary>
        public FreezeClockAttribute()
        {
            frozenDateTime = DateTime.MinValue;
        }

        /// <summary>
        /// Freeze the clock with the given date, considered to be local time.
        /// </summary>
        /// <param name="year">The frozen year</param>
        /// <param name="month">The frozen month</param>
        /// <param name="day">The frozen day</param>
        public FreezeClockAttribute(int year,
                                    int month,
                                    int day)
        {
            frozenDateTime = new DateTime(year, month, day, 0, 0, 0, DateTimeKind.Local);
        }

        /// <summary>
        /// Freeze the clock with the given date and time, considered to be in local time.
        /// </summary>
        /// <param name="year">The frozen year</param>
        /// <param name="month">The frozen month</param>
        /// <param name="day">The frozen day</param>
        /// <param name="hour">The frozen hour</param>
        /// <param name="minute">The frozen minute</param>
        /// <param name="second">The frozen second</param>
        public FreezeClockAttribute(int year,
                                    int month,
                                    int day,
                                    int hour,
                                    int minute,
                                    int second)
        {
            frozenDateTime = new DateTime(year, month, day, hour, minute, second, DateTimeKind.Local);
        }

        /// <summary>
        /// Freeze the clock with the given date and time, with the given kind of time.
        /// </summary>
        /// <param name="year">The frozen year</param>
        /// <param name="month">The frozen month</param>
        /// <param name="day">The frozen day</param>
        /// <param name="hour">The frozen hour</param>
        /// <param name="minute">The frozen minute</param>
        /// <param name="second">The frozen second</param>
        /// <param name="kind">The frozen time kind</param>
        public FreezeClockAttribute(int year,
                                    int month,
                                    int day,
                                    int hour,
                                    int minute,
                                    int second,
                                    DateTimeKind kind)
        {
            frozenDateTime = new DateTime(year, month, day, hour, minute, second, kind);
        }

        /// <summary>
        /// Thaws the clock.
        /// </summary>
        /// <param name="methodUnderTest">The method under test</param>
        public override void After(MethodInfo methodUnderTest)
        {
            Clock.Thaw();
        }

        /// <summary>
        /// Freezes the clock.
        /// </summary>
        /// <param name="methodUnderTest">The method under test</param>
        public override void Before(MethodInfo methodUnderTest)
        {
            if (frozenDateTime == DateTime.MinValue)
                Clock.Freeze();
            else
                Clock.FreezeUtc(frozenDateTime.ToUniversalTime());
        }
    }
}