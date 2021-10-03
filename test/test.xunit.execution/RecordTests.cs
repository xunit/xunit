using System;
using System.Threading.Tasks;
using Xunit;

public class RecordTests
{
    public class MethodsWithoutReturnValues
    {
        [Fact]
        public void Exception()
        {
            Action testCode = () => { throw new InvalidOperationException(); };

            var ex = Record.Exception(testCode);

            Assert.NotNull(ex);
            Assert.IsType<InvalidOperationException>(ex);
        }

        [Fact]
        public void NoException()
        {
            Action testCode = () => { };

            var ex = Record.Exception(testCode);

            Assert.Null(ex);
        }
    }

    public class MethodsReturningTask
    {
        [Fact]
        public async void Exception()
        {
            Func<Task> testCode = () => Task.Run(() => { throw new InvalidOperationException(); });

            var ex = await Record.ExceptionAsync(testCode);

            Assert.NotNull(ex);
            Assert.IsType<InvalidOperationException>(ex);
        }

        [Fact]
        public async void NoException()
        {
            Func<Task> testCode = () => Task.Run(() => { });

            var ex = await Record.ExceptionAsync(testCode);

            Assert.Null(ex);
        }
    }

    public class MethodsWithReturnValues
    {
        [Fact]
        public void GuardClause()
        {
            Func<object> testCode = () => Task.Run(() => { });

            var ex = Record.Exception(() => Record.Exception(testCode));

            Assert.IsType<InvalidOperationException>(ex);
            Assert.Equal("You must call Assert.ThrowsAsync, Assert.DoesNotThrowAsync, or Record.ExceptionAsync when testing async code.", ex.Message);
        }

        [Fact]
        public void Exception()
        {
            var accessor = new StubAccessor();

            var ex = Record.Exception(() => accessor.FailingProperty);

            Assert.NotNull(ex);
            Assert.IsType<InvalidOperationException>(ex);
        }

        [Fact]
        public void NoException()
        {
            var accessor = new StubAccessor();

            var ex = Record.Exception(() => accessor.SuccessfulProperty);

            Assert.Null(ex);
        }

        class StubAccessor
        {
            public int SuccessfulProperty { get; set; }

            public int FailingProperty
            {
                get { throw new InvalidOperationException(); }
            }
        }
    }
}
