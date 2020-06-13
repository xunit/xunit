using System;
using Xunit;
using Xunit.Sdk;

namespace Xunit1
{
    public class SameTests
    {
        [Fact]
        public void BoxedTypesDontWork()
        {
            int index = 0;

            Assert.Throws<SameException>(() => Assert.Same(index, index));
        }

        [Fact]
        public void SameFailsWith()
        {
            string actual = "Abc";
            string expected = "a".ToUpperInvariant() + "bc";

            try
            {
                Assert.Same(expected, actual);
            }
            catch (Exception ex)
            {
                AssertException aex = Assert.IsAssignableFrom<AssertException>(ex);
                Assert.Equal("Assert.Same() Failure", aex.UserMessage);
                Assert.DoesNotContain("Position:", aex.Message);
            }
        }

        [Fact]
        public void ValuesAreNotTheSame()
        {
            Assert.Throws<SameException>(() => Assert.Same("bob", "jim"));
        }

        [Fact]
        public void ValuesAreTheSame()
        {
            string jim = "jim";

            Assert.Same(jim, jim);
        }
    }
}
