#if NETFRAMEWORK

using System.Linq;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

public class AsyncAcceptanceTests : AcceptanceTestV2
{
    [Fact]
    public void AsyncTaskTestsRunCorrectly()
    {
        var results = Run<ITestResultMessage>(typeof(ClassWithAsyncTask));

        var result = Assert.Single(results);
        var failed = Assert.IsAssignableFrom<ITestFailed>(result);
        Assert.Equal("Xunit.Sdk.EqualException", failed.ExceptionTypes.Single());
    }

    [Fact]
    public void AsyncVoidTestsRunCorrectly()
    {
        var results = Run<ITestResultMessage>(typeof(ClassWithAsyncVoid));

        var result = Assert.Single(results);
        var failed = Assert.IsAssignableFrom<ITestFailed>(result);
        Assert.Equal("Xunit.Sdk.EqualException", failed.ExceptionTypes.Single());
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

#endif
