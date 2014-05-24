using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xunit;
using Xunit.Sdk;

namespace test.xunit.pcltestlib
{
    public class PortableTests
    {
        [Fact]
        public void TestPortableTrue()
        {
            Assert.True(true);
        }
        
        [Fact]
        public void TestPortableExceptionRecorder()
        {
            var ex = Record.Exception(
                () => Assert.True(false));

            Assert.IsType<TrueException>(ex);
        }
    }
}
