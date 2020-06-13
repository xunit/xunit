using System.Linq;
using System.Threading;
using TestUtility;
using Xunit;
using Xunit.Sdk;

namespace Xunit1
{
    public class TestTimeoutFixture : AcceptanceTest
    {
        [Fact]
        public void TestHasTimeoutAndExceeds()
        {
            MethodResult result = RunClass(typeof(ClassUnderTest)).Single();

            FailedResult failedResult = Assert.IsType<FailedResult>(result);
            Assert.InRange(failedResult.ExecutionTime, 0.049, 0.051);
            Assert.Equal("Test execution time exceeded: 50ms", failedResult.Message);
        }

        class ClassUnderTest
        {
            [Fact(Timeout = 50)]
            public void TestShouldTimeout()
            {
                Thread.Sleep(120);
                Assert.Equal(2, 2);
            }
        }
    }
}
