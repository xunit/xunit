using System;
using Xunit;
using Xunit.Abstractions;
using Xunit.Sdk;

public class NullAssertsTests
{
    public class NotNull
    {
        [Fact]
        public void Success()
        {
            Assert.NotNull(new object());
        }

        [Fact]
        public void Failure()
        {
            var ex = Record.Exception(() => Assert.NotNull(null));

            Assert.IsType<NotNullException>(ex);
            Assert.Equal("Assert.NotNull() Failure", ex.Message);
        }
    }

    public class Null
    {
        private readonly ITestOutputHelper Output;
        public Null(ITestOutputHelper output)
        {
            Output = output;
        }

        [Fact]
        public void Success()
        {
            Assert.Null(null);
        }

        [Fact]
        public void Failure()
        {
            var ex = Record.Exception(
                () => Assert.Null(new object()));

            Assert.IsType<NullException>(ex);
            Assert.Equal("Assert.Null() Failure" + Environment.NewLine +
                         "Expected: (null)" + Environment.NewLine +
                         "Actual:   Object { }", ex.Message);
        }

        /// <summary>
        /// Test case specific to the issue #1932
        /// </summary>
        [Fact]
        public void PrintControlCharactersSuccess()
        {
            var ex = Record.Exception(
                () => Assert.Null("\0*.*\0"));
            
            Assert.IsType<NullException>(ex);
            Assert.Equal("Assert.Null() Failure" + Environment.NewLine +
                         "Expected: (null)" + Environment.NewLine +
                         "Actual:   \0*.*\0", ex.Message);

            Output.WriteLine(ex.Message);
        }
    }
}