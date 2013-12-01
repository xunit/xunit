using System.Collections.Generic;
using System.Threading.Tasks;
using TestUtility;
using Xunit;
using Xunit.Sdk;

public class AsyncAcceptanceTests : AcceptanceTest
{
    [Fact]
    public void AsyncTaskTestsRunCorrectly()
    {
        var results = RunClass(typeof(ClassWithAsyncTask));

        var result = Assert.Single(results);
        var failedResult = Assert.IsType<FailedResult>(result);
        Assert.Equal(typeof(EqualException).FullName, failedResult.ExceptionType);
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

    [Fact]
    public void AsyncVoidTestsRunCorrectly()
    {
        var results = RunClass(typeof(ClassWithAsyncVoid));

        var result = Assert.Single(results);
        var failedResult = Assert.IsType<FailedResult>(result);
        Assert.Equal(typeof(EqualException).FullName, failedResult.ExceptionType);
    }

    class ClassWithAsyncVoid
    {
        [Fact]
        public async void AsyncTest()
        {
            var result = await Task.FromResult(21);

            Assert.Equal(42, result);
        }
    }
}
