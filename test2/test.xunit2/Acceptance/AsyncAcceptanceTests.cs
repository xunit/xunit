using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

public class AsyncAcceptanceTests : AcceptanceTest
{
    [Fact]
    public void AsyncTaskTestsRunCorrectly()
    {
        var results = Run<ITestResultMessage>(typeof(ClassWithAsyncTask));

        var result = Assert.Single(results);
        var failed = Assert.IsAssignableFrom<ITestFailed>(result);
        Assert.Equal("Xunit.Sdk.EqualException", failed.ExceptionType);
    }

    [Fact]
    public void AsyncVoidTestsRunCorrectly()
    {
        var results = Run<ITestResultMessage>(typeof(ClassWithAsyncVoid));

        var result = Assert.Single(results);
        var failed = Assert.IsAssignableFrom<ITestFailed>(result);
        Assert.Equal("Xunit.Sdk.EqualException", failed.ExceptionType);
    }

    class ClassWithAsyncTask
    {
        [Fact]
        public async Task AsyncTest()
        {
            var result = await Task.FromResult(21);

            Assert.Equal(42, result);
        }
    }

    class ClassWithAsyncVoid
    {
        [Fact]
        public async void AsyncTest_Void()
        {
            var result = await Task.FromResult(21);

            Assert.Equal(42, result);
        }
    }
}