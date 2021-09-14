using System;
using System.Collections.Generic;
using Xunit;
using Xunit.Sdk;

namespace Xunit1
{
    public class NotEqualTests
    {
        [Fact]
        public void NotEqualFailsString()
        {
            try
            {
                Assert.NotEqual("actual", "actual");
            }
            catch (NotEqualException exception)
            {
                Assert.Equal("Assert.NotEqual() Failure", exception.UserMessage);
            }
        }

        [Fact]
        public void NotEqualWithCustomComparer()
        {
            string expected = "TestString";
            string actual = "testString";

            Assert.False(actual == expected);
            Assert.Equal(expected, actual, StringComparer.CurrentCultureIgnoreCase);
            Assert.NotEqual(expected, actual, StringComparer.CurrentCulture);
        }

        [Fact]
        public void ValuesNotEqual()
        {
            Assert.NotEqual("bob", "jim");
        }

        [Fact]
        public void EnumerableInequivalence()
        {
            int[] expected = new[] { 1, 2, 3, 4, 5 };
            List<int> actual = new List<int>(new[] { 1, 2, 3, 4, 6 });

            Assert.NotEqual(expected, actual);
        }

        [Fact]
        public void EnumerableEquivalence()
        {
            int[] expected = new[] { 1, 2, 3, 4, 5 };
            List<int> actual = new List<int>(expected);

            Assert.Throws<NotEqualException>(() => Assert.NotEqual(expected, actual));
        }

        [Fact]
        public void EnumerableInequivalenceWithFailedComparer()
        {
            int[] expected = new[] { 1, 2, 3, 4, 5 };
            List<int> actual = new List<int>(new int[] { 1, 2, 3, 4, 5 });

            Assert.NotEqual(expected, actual, new IntComparer(false));
        }

        [Fact]
        public void EnumerableEquivalenceWithSuccessfulComparer()
        {
            int[] expected = new[] { 1, 2, 3, 4, 5 };
            List<int> actual = new List<int>(new int[] { 0, 0, 0, 0, 0 });

            Assert.Throws<NotEqualException>(() => Assert.NotEqual(expected, actual, new IntComparer(true)));
        }

        class IntComparer : IEqualityComparer<int>
        {
            bool answer;

            public IntComparer(bool answer)
            {
                this.answer = answer;
            }

            public bool Equals(int x, int y)
            {
                return answer;
            }

            public int GetHashCode(int obj)
            {
                throw new NotImplementedException();
            }
        }
    }
}
