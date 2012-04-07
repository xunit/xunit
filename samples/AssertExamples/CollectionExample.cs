using System;
using System.Collections.Generic;
using Xunit;

namespace AssertExtensibility
{
    // Collection equivalence means the collections have the exact same values
    // in any order.

    public class CollectionExamples
    {
        [Fact]
        public void CollectionEquality()
        {
            List<int> left = new List<int>(new int[] { 4, 12, 16, 27 });
            List<int> right = new List<int>(new int[] { 4, 12, 16, 27 });

            Assert.Equal(left, right, new CollectionEquivalenceComparer<int>());
        }

        [Fact]
        public void LeftCollectionSmallerThanRight()
        {
            List<int> left = new List<int>(new int[] { 4, 12, 16 });
            List<int> right = new List<int>(new int[] { 4, 12, 16, 27 });

            Assert.NotEqual(left, right, new CollectionEquivalenceComparer<int>());
        }

        [Fact]
        public void LeftCollectionLargerThanRight()
        {
            List<int> left = new List<int>(new int[] { 4, 12, 16, 27, 42 });
            List<int> right = new List<int>(new int[] { 4, 12, 16, 27 });

            Assert.NotEqual(left, right, new CollectionEquivalenceComparer<int>());
        }

        [Fact]
        public void SameValuesOutOfOrder()
        {
            List<int> left = new List<int>(new int[] { 4, 16, 12, 27 });
            List<int> right = new List<int>(new int[] { 4, 12, 16, 27 });

            Assert.Equal(left, right, new CollectionEquivalenceComparer<int>());
        }

        [Fact]
        public void DuplicatedItemInOneListOnly()
        {
            List<int> left = new List<int>(new int[] { 4, 16, 12, 27, 4 });
            List<int> right = new List<int>(new int[] { 4, 12, 16, 27 });

            Assert.NotEqual(left, right, new CollectionEquivalenceComparer<int>());
        }

        [Fact]
        public void DuplicatedItemInBothLists()
        {
            List<int> left = new List<int>(new int[] { 4, 16, 12, 27, 4 });
            List<int> right = new List<int>(new int[] { 4, 12, 16, 4, 27 });

            Assert.Equal(left, right, new CollectionEquivalenceComparer<int>());
        }
    }

    class CollectionEquivalenceComparer<T> : IEqualityComparer<IEnumerable<T>>
       where T : IEquatable<T>
    {
        public bool Equals(IEnumerable<T> x, IEnumerable<T> y)
        {
            List<T> leftList = new List<T>(x);
            List<T> rightList = new List<T>(y);
            leftList.Sort();
            rightList.Sort();

            IEnumerator<T> enumeratorX = leftList.GetEnumerator();
            IEnumerator<T> enumeratorY = rightList.GetEnumerator();

            while (true)
            {
                bool hasNextX = enumeratorX.MoveNext();
                bool hasNextY = enumeratorY.MoveNext();

                if (!hasNextX || !hasNextY)
                    return (hasNextX == hasNextY);

                if (!enumeratorX.Current.Equals(enumeratorY.Current))
                    return false;
            }
        }

        public int GetHashCode(IEnumerable<T> obj)
        {
            throw new NotImplementedException();
        }
    }
}