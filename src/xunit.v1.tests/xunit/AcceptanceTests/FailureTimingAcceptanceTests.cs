using System.Linq;
using System.Threading;
using TestUtility;
using Xunit;
using Xunit.Sdk;

namespace Xunit1
{
    public class FailureTimingAcceptanceTests : AcceptanceTest
    {
        [Fact]
        public void TimingForFailedTestShouldReflectActualRunTime()
        {
            MethodResult result = RunClass(typeof(ClassUnderTest)).Single();

            Assert.IsType<FailedResult>(result);
            Assert.NotEqual(0.0, result.ExecutionTime);
        }

        class ClassUnderTest
        {
            [Fact]
            public void TwoNumbersAreNotEqual()
            {
                Thread.Sleep(100);
                Assert.Equal(2, 3);
            }
        }
    }
}
