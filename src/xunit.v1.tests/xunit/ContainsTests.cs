using System;
using System.Collections.Generic;
using Xunit;
using Xunit.Sdk;

namespace Xunit1
{
    public class ContainsTests
    {
        public class CollectionTests
        {
            [Fact]
            public void CanFindNullInContainer()
            {
                List<object> list = new List<object> { 16, null, "Hi there" };

                Assert.Contains(null, list);
            }

            [Fact]
            public void CanUseComparer()
            {
                List<int> list = new List<int> { 42 };

                Assert.Contains(43, list, new MyComparer());
            }

            [Fact]
            public void ItemInContainer()
            {
                List<int> list = new List<int> { 42 };

                Assert.Contains(42, list);
            }

            [Fact]
            public void ItemNotInContainer()
            {
                List<int> list = new List<int>();

                ContainsException ex = Assert.Throws<ContainsException>(() => Assert.Contains(42, list));

                Assert.Equal("Assert.Contains() failure: Not found: 42", ex.Message);
            }

            [Fact]
            public void NullsAllowedInContainer()
            {
                List<object> list = new List<object> { null, 16, "Hi there" };

                Assert.Contains("Hi there", list);
            }

            [Fact]
            public void NullContainerThrows()
            {
                Assert.Throws<ContainsException>(() => Assert.Contains(14, (List<int>)null));
            }

            class MyComparer : IEqualityComparer<int>
            {
                public bool Equals(int x, int y)
                {
                    return true;
                }

                public int GetHashCode(int obj)
                {
                    throw new NotImplementedException();
                }
            }
        }

        public class SubstringTests
        {
            [Fact]
            public void CanSearchForSubstrings()
            {
                Assert.Contains("wor", "Hello, world!");
            }

            [Fact]
            public void CanSearchForSubstringsCaseInsensitive()
            {
                Assert.Contains("WORLD", "Hello, world!", StringComparison.InvariantCultureIgnoreCase);
            }

            [Fact]
            public void SubstringContainsIsCaseSensitiveByDefault()
            {
                var ex = Record.Exception(() => Assert.Contains("WORLD", "Hello, world!"));

                Assert.IsType<ContainsException>(ex);
                Assert.Equal("Assert.Contains() failure:" + Environment.NewLine +
                             "Not found: WORLD" + Environment.NewLine +
                             "In value:  Hello, world!", ex.Message);
            }

            [Fact]
            public void SubstringNotFound()
            {
                Assert.Throws<ContainsException>(() => Assert.Contains("hey", "Hello, world!"));
            }

            [Fact]
            public void NullActualStringThrows()
            {
                Assert.Throws<ContainsException>(() => Assert.Contains("foo", (string)null));
            }
        }
    }
}
