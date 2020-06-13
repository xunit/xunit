using System.Threading;
using Moq;
using Xunit;
using Xunit.Sdk;

namespace Xunit1
{
    public class TimeoutCommandTests
    {
        [Fact]
        public void TestFinshedOnTimePassedResult()
        {
            Mock<ITestCommand> testCommand = new Mock<ITestCommand>();
            testCommand.Setup(tc => tc.Execute(null))
                       .Returns(new PassedResult(GetMethodInfo(), null));
            TimeoutCommand command = new TimeoutCommand(testCommand.Object, 10000, GetMethodInfo());

            MethodResult result = command.Execute(null);

            Assert.IsType<PassedResult>(result);
        }

        [Fact]
        public void TestTookTooLongFailedResult()
        {
            Mock<ITestCommand> testCommand = new Mock<ITestCommand>();
            testCommand.Setup(tc => tc.Execute(null))
                       .Callback<object>(_ => Thread.Sleep(500));
            TimeoutCommand command = new TimeoutCommand(testCommand.Object, 20, GetMethodInfo());

            MethodResult result = command.Execute(null);

            FailedResult failedResult = Assert.IsType<FailedResult>(result);
            Assert.Equal("Test execution time exceeded: 20ms", failedResult.Message);
        }

        IMethodInfo GetMethodInfo()
        {
            return Reflector.Wrap(GetType().GetMethod("TestFinshedOnTimePassedResult"));
        }
    }
}
