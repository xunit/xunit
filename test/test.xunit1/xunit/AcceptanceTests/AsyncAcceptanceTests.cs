using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using TestUtility;
using Xunit;
using Xunit.Sdk;

namespace Xunit1
{
    public class AsyncAcceptanceTests : AcceptanceTest
    {
        [Fact]
        public void Async40AcceptanceTest()
        {
            IEnumerable<MethodResult> results = RunClass(typeof(Async40AcceptanceTestClass));

            MethodResult result = Assert.Single(results);
            FailedResult failedResult = Assert.IsType<FailedResult>(result);
            Assert.Equal(typeof(TrueException).FullName, failedResult.ExceptionType);
        }

        class Async40AcceptanceTestClass
        {
            [Fact]
            public Task MethodUnderTest()
            {
                return Task.Factory.StartNew(() =>
                {
                    Thread.Sleep(1);
                })
                .ContinueWith(_ =>
                {
                    Assert.True(false);
                });
            }
        }
    }
}
