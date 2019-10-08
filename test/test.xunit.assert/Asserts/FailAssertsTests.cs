using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using Xunit.Sdk;

namespace test.xunit.assert.Asserts
{
    public class FailAssertsTests
    {
        public class Fail
        {
            [Fact]
            public void Failure()
            {
                var ex = Record.Exception(() => Assert.Fail());

                Assert.IsType<FailException>(ex);
                Assert.Equal("Assert.Fail() Failure", ex.Message);
            }

            [Fact]
            public void UserSuppliedMessage()
            {
                var ex = Record.Exception(() => Assert.Fail("Custom user message"));

                Assert.IsType<FailException>(ex);
                Assert.Equal("Custom user message", ex.Message);
            }
        }
    }
}
