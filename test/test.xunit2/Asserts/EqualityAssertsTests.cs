using System;
using System.Collections.Generic;
using Xunit;
using Xunit.Sdk;

public class EqualityAssertsTests
{
    public class Equal
    {
        [Fact]
        public void Success()
        {
            Assert.Equal(42, 42);
        }

        [Fact]
        public void Failure()
        {
            var ex = Assert.Throws<EqualException>(() => Assert.Equal(42, 2112));
            Assert.Equal("42", ex.Expected);
            Assert.Equal("2112", ex.Actual);
        }

        [Fact]
        public void Comparable()
        {
            SpyComparable obj1 = new SpyComparable();
            SpyComparable obj2 = new SpyComparable();

            Assert.Equal(obj1, obj2);
            Assert.True(obj1.CompareCalled);
        }

        [Fact]
        public void Comparable_Generic()
        {
            SpyComparable_Generic obj1 = new SpyComparable_Generic();
            SpyComparable_Generic obj2 = new SpyComparable_Generic();

            Assert.Equal(obj1, obj2);
            Assert.True(obj1.CompareCalled);
        }

        [Fact]
        public void Equatable()
        {
            SpyEquatable obj1 = new SpyEquatable();
            SpyEquatable obj2 = new SpyEquatable();

            Assert.Equal(obj1, obj2);

            Assert.True(obj1.Equals__Called);
            Assert.Same(obj2, obj1.Equals_Other);
        }

        [Fact]
        public void NonComparable()
        {
            NonComparableObject nco1 = new NonComparableObject();
            NonComparableObject nco2 = new NonComparableObject();

            Assert.Equal(nco1, nco2);
        }

        [Fact]
        public void DepthExample()
        {
            var x = new List<object> { new List<object> { new List<object> { new List<object>() } } };
            var y = new List<object> { new List<object> { new List<object> { new List<object>() } } };

            Assert.Equal(x, y);
        }

        class SpyComparable : IComparable
        {
            public bool CompareCalled;

            public int CompareTo(object obj)
            {
                CompareCalled = true;
                return 0;
            }
        }

        class SpyComparable_Generic : IComparable<SpyComparable_Generic>
        {
            public bool CompareCalled;

            public int CompareTo(SpyComparable_Generic other)
            {
                CompareCalled = true;
                return 0;
            }
        }

        public class SpyEquatable : IEquatable<SpyEquatable>
        {
            public bool Equals__Called;
            public SpyEquatable Equals_Other;

            public bool Equals(SpyEquatable other)
            {
                Equals__Called = true;
                Equals_Other = other;

                return true;
            }
        }

        class NonComparableObject
        {
            public override bool Equals(object obj)
            {
                return true;
            }

            public override int GetHashCode()
            {
                return 42;
            }
        }
    }

    public class Equal_WithComparer
    {
        [Fact]
        public void Success()
        {
            Assert.Equal(42, 21, new Comparer<int>(true));
        }

        [Fact]
        public void Failure()
        {
            var ex = Assert.Throws<EqualException>(() => Assert.Equal(42, 42, new Comparer<int>(false)));
            Assert.Equal("42", ex.Expected);
            Assert.Equal("42", ex.Actual);
        }

        class Comparer<T> : IEqualityComparer<T>
        {
            bool result;

            public Comparer(bool result)
            {
                this.result = result;
            }

            public bool Equals(T x, T y)
            {
                return result;
            }

            public int GetHashCode(T obj)
            {
                throw new NotImplementedException();
            }
        }

    }

    public class Equal_Decimal
    {
        [Fact]
        public void Success()
        {
            Assert.Equal(0.11111M, 0.11444M, 2);
        }

        [Fact]
        public void Failure()
        {
            var ex = Assert.Throws<EqualException>(() => Assert.Equal(0.11111M, 0.11444M, 3));
            Assert.Equal("0.111 (rounded from 0.11111)", ex.Expected);
            Assert.Equal("0.114 (rounded from 0.11444)", ex.Actual);
        }
    }

    public class Equal_Double
    {
        [Fact]
        public void Success()
        {
            Assert.Equal(0.11111, 0.11444, 2);
        }

        [Fact]
        public void Failure()
        {
            var ex = Assert.Throws<EqualException>(() => Assert.Equal(0.11111, 0.11444, 3));
            Assert.Equal("0.111 (rounded from 0.11111)", ex.Expected);
            Assert.Equal("0.114 (rounded from 0.11444)", ex.Actual);
        }
    }

    public class NotEqual
    {
        [Fact]
        public void Success()
        {
            Assert.NotEqual("bob", "jim");
        }

        [Fact]
        public void Failure()
        {
            var ex = Record.Exception(() => Assert.NotEqual("actual", "actual"));

            Assert.IsType<NotEqualException>(ex);
            Assert.Equal("Assert.NotEqual() Failure", ex.Message);
        }
    }

    public class NotEqual_WithComparer
    {
        [Fact]
        public void Success()
        {
            Assert.NotEqual("TestString", "testString", StringComparer.CurrentCulture);
        }

        [Fact]
        public void NotEqualWithCustomComparer()
        {
            var ex = Record.Exception(
                () => Assert.NotEqual("TestString", "testString", StringComparer.CurrentCultureIgnoreCase));

            Assert.IsType<NotEqualException>(ex);
            Assert.Equal("Assert.NotEqual() Failure", ex.Message);
        }
    }
}
