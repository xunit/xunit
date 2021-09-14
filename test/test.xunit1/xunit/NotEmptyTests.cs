using System.Collections.Generic;
using Xunit;
using Xunit.Sdk;

namespace Xunit1
{
    public class NotEmptyTests
    {
        [Fact]
        public void ContainerIsEmpty()
        {
            List<int> list = new List<int>();

            NotEmptyException ex =
                Assert.Throws<NotEmptyException>(() => Assert.NotEmpty(list));

            Assert.Equal("Assert.NotEmpty() failure", ex.Message);
        }

        [Fact]
        public void ContainerIsNotEmpty()
        {
            List<int> list = new List<int>();
            list.Add(42);

            Assert.NotEmpty(list);
        }
    }
}
