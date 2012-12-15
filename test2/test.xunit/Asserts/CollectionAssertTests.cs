using System;
using System.Collections.Generic;
using Xunit;
using Xunit.Sdk;

public class CollectionAssertTests
{
    public class Collection
    {
        [Fact]
        public void EmptyCollection()
        {
            List<int> list = new List<int>();

            Assert.Collection(list);
        }

        [Fact]
        public void MismatchedElementCount()
        {
            List<int> list = new List<int>();

            var ex = Record.Exception(
                () => Assert.Collection(list,
                    item => Assert.True(false)
                )
            );

            var collEx = Assert.IsType<CollectionException>(ex);
            Assert.Equal(1, collEx.ExpectedCount);
            Assert.Equal(0, collEx.ActualCount);
            Assert.Equal("Assert.Collection() Failure" + Environment.NewLine +
                         "Expected item count: 1" + Environment.NewLine +
                         "Actual item count:   0", collEx.Message);
            Assert.Null(collEx.InnerException);
        }

        [Fact]
        public void NonEmptyCollection()
        {
            List<int> list = new List<int> { 42, 2112 };

            Assert.Collection(list,
                item => Assert.Equal(42, item),
                item => Assert.Equal(2112, item)
            );
        }

        [Fact]
        public void MismatchedElement()
        {
            List<int> list = new List<int> { 42, 2112 };

            var ex = Record.Exception(() =>
                Assert.Collection(list,
                    item => Assert.Equal(42, item),
                    item => Assert.Equal(2113, item)
                )
            );

            var collEx = Assert.IsType<CollectionException>(ex);
            Assert.Equal(1, collEx.IndexFailurePoint);
            Assert.Equal("Assert.Collection() Failure" + Environment.NewLine +
                         "Error during comparison of item at index 1" + Environment.NewLine +
                         "Inner exception: Assert.Equal() Failure" + Environment.NewLine +
                         "        Expected: 2113" + Environment.NewLine +
                         "        Actual:   2112", ex.Message);
            Assert.IsType<EqualException>(ex.InnerException);
        }
    }
}
