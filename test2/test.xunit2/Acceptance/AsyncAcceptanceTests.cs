using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

public class AsyncAcceptanceTests : AcceptanceTest
{
    [Fact]
    public void AsyncTestsRunCorrectly()
    {
        var results = this.Run<ITestResultMessage>(typeof(ClassUnderTest));

        var result = Assert.Single(results);
        var failed = Assert.IsAssignableFrom<ITestFailed>(result);
        Assert.Equal("Xunit.Sdk.EqualException", failed.ExceptionType);
    }

    class ClassUnderTest
    {
        [Fact]
        public async Task AsyncTest()
        {
            var result = await AsyncMethod();

            Assert.Equal(42, result);
        }

        public Task<int> AsyncMethod()
        {
            return Task.FromResult(21);
        }
    }
}