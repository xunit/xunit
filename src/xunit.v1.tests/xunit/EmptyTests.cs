using System;
using System.Collections.Generic;
using Xunit;
using Xunit.Sdk;

namespace Xunit1
{
    public class EmptyTests
    {
        public class Containers
        {
            [Fact]
            public void IsEmpty()
            {
                List<int> list = new List<int>();

                Assert.Empty(list);
            }

            [Fact]
            public void IsNotEmpty()
            {
                List<int> list = new List<int>();
                list.Add(42);

                EmptyException ex = Assert.Throws<EmptyException>(() => Assert.Empty(list));

                Assert.Equal("Assert.Empty() failure", ex.Message);
            }

            [Fact]
            public void NullIsNotEmpty()
            {
                Assert.Throws<ArgumentNullException>(() => Assert.Empty(null));
            }
        }

        public class Strings
        {
            [Fact]
            public void IsEmpty()
            {
                Assert.Empty("");
            }

            [Fact]
            public void IsNotEmpty()
            {
                EmptyException ex = Assert.Throws<EmptyException>(() => Assert.Empty("Foo"));

                Assert.Equal("Assert.Empty() failure", ex.Message);
            }
        }
    }
}
