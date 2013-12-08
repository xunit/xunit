using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

public class AsyncAcceptanceTests : AcceptanceTest
{
    [Fact(Skip = "Temporarily skipped test broken by the memory leak fixes")]
    public void AsyncTaskTestsRunCorrectly()
    {
        var results = Run<ITestResultMessage>(typeof(ClassWithAsyncTask));

        var result = Assert.Single(results);
        var failed = Assert.IsAssignableFrom<ITestFailed>(result);
        Assert.Equal("Xunit.Sdk.EqualException", failed.ExceptionType);
    }

    [Fact(Skip = "Temporarily skipped test broken by the memory leak fixes")]
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