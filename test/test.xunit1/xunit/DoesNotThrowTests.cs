using System;
using Xunit;
using Xunit.Sdk;

namespace Xunit1
{
    public class DoesNotThrowTests
    {
        [Fact]
        public void CorrectExceptionType()
        {
            DoesNotThrowException ex =
                Assert.Throws<DoesNotThrowException>(
                    () => Assert.DoesNotThrow(
                        () => { throw new NotImplementedException("Exception Message"); }));

            Assert.Equal("Assert.DoesNotThrow() failure", ex.UserMessage);
            Assert.Equal("(No exception)", ex.Expected);
            Assert.Equal("System.NotImplementedException: Exception Message", ex.Actual);
        }

        [Fact]
        public void PassingTest()
        {
            Assert.DoesNotThrow(() => { });
        }
    }
}
