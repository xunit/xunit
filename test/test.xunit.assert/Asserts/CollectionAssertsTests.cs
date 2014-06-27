using System;
using System.Collections;
using System.Collections.Generic;
using Xunit;
using Xunit.Sdk;

public class CollectionAssertsTests
{
    public class All
    {
        [Fact]
        public static void NullCollectionThrows()
        {
            Assert.Throws<ArgumentNullException>(() => Assert.All<object>(null, _ => { }));
        }

        [Fact]
        public static void NullActionThrows()
        {
            Assert.Throws<ArgumentNullException>(() => Assert.All<object>(new Object[0], null));
        }

        [Fact]
        public static void ActionWhereSomeFail()
        {
            var items = new[] { 1, 1, 2, 2, 1, 1 };

            var ex = Assert.Throws<AllException>(() => Assert.All(items, x => Assert.Equal(1, x)));

            Assert.Equal(2, ex.Failures.Count);
            Assert.All(ex.Failures, x => Assert.IsType<EqualException>(x));
        }

        [Fact]
        public static void ActionWhereNoneFail()
        {
            var items = new[] { 1, 1, 1, 1, 1, 1 };

            Assert.All(items, x => Assert.Equal(1, x));
        }

        [Fact]
        public static void ActionWhereAllFail()
        {
            var items = new[] { 1, 1, 2, 2, 1, 1 };

            var ex = Assert.Throws<AllException>(() => Assert.All(items, x => Assert.Equal(0, x)));

            Assert.Equal(6, ex.Failures.Count);
            Assert.All(ex.Failures, x => Assert.IsType<EqualException>(x));
        }
    }

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
        }
    }

    public class Contains
    {
        [Fact]
        public void GuardClauses()
        {
            Assert.Throws<ContainsException>(() => Assert.Contains(14, (List<int>)null));
        }

        [Fact]
        public void CanFindNullInContainer()
        {
            List<object> list = new List<object> { 16, null, "Hi there" };

            Assert.Contains(null, list);
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

            Assert.Equal("Assert.Contains() Failure: Not found: 42", ex.Message);
        }

        [Fact]
        public void NullsAllowedInContainer()
        {
            List<object> list = new List<object> { null, 16, "Hi there" };

            Assert.Contains("Hi there", list);
        }
    }

    public class Contains_WithComparer
    {
        [Fact]
        public void CanUseComparer()
        {
            List<int> list = new List<int> { 42 };

            Assert.Contains(43, list, new MyComparer());
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

    public class DoesNotContain
    {
        [Fact]
        public void CanSearchForNullInContainer()
        {
            List<object> list = new List<object> { 16, "Hi there" };

            Assert.DoesNotContain(null, list);
        }

        [Fact]
        public void ItemInContainer()
        {
            List<int> list = new List<int> { 42 };

            DoesNotContainException ex =
                Assert.Throws<DoesNotContainException>(() => Assert.DoesNotContain(42, list));

            Assert.Equal("Assert.DoesNotContain() Failure: Found: 42", ex.Message);
        }

        [Fact]
        public void ItemNotInContainer()
        {
            List<int> list = new List<int>();

            Assert.DoesNotContain(42, list);
        }

        [Fact]
        public void NullsAllowedInContainer()
        {
            List<object> list = new List<object> { null, 16, "Hi there" };

            Assert.DoesNotContain(42, list);
        }

        [Fact]
        public void NullContainerDoesNotThrow()
        {
            Assert.DoesNotThrow(() => Assert.DoesNotContain(14, (List<int>)null));
        }
    }

    public class DoesNotContain_WithComparer
    {
        [Fact]
        public void CanUseComparer()
        {
            List<int> list = new List<int>();
            list.Add(42);

            Assert.DoesNotContain(42, list, new MyComparer());
        }

        class MyComparer : IEqualityComparer<int>
        {
            public bool Equals(int x, int y)
            {
                return false;
            }

            public int GetHashCode(int obj)
            {
                throw new NotImplementedException();
            }
        }
    }

    public class Empty
    {
        [Fact]
        public void GuardClauses()
        {
            Assert.Throws<ArgumentNullException>(() => Assert.Empty(null));
        }

        [Fact]
        public void EmptyContainer()
        {
            List<int> list = new List<int>();

            Assert.Empty(list);
        }

        [Fact]
        public void NonEmptyContainerThrows()
        {
            List<int> list = new List<int>();
            list.Add(42);

            EmptyException ex = Assert.Throws<EmptyException>(() => Assert.Empty(list));

            Assert.Equal("Assert.Empty() Failure", ex.Message);
        }

        [Fact]
        public void EmptyString()
        {
            Assert.Empty("");
        }

        [Fact]
        public void NonEmptyStringThrows()
        {
            EmptyException ex = Assert.Throws<EmptyException>(() => Assert.Empty("Foo"));

            Assert.Equal("Assert.Empty() Failure", ex.Message);
        }
    }

    public class Equal
    {
        [Fact]
        public void Array()
        {
            string[] expected = { "@", "a", "ab", "b" };
            string[] actual = { "@", "a", "ab", "b" };

            Assert.Equal(expected, actual);
            Assert.Throws<NotEqualException>(() => Assert.NotEqual(expected, actual));
        }

        [Fact]
        public void ArrayInsideArray()
        {
            string[][] expected = { new[] { "@", "a" }, new[] { "ab", "b" } };
            string[][] actual = { new[] { "@", "a" }, new[] { "ab", "b" } };

            Assert.Equal(expected, actual);
            Assert.Throws<NotEqualException>(() => Assert.NotEqual(expected, actual));
        }

        [Fact]
        public void ArraysOfDifferentLengthsAreNotEqual()
        {
            string[] expected = { "@", "a", "ab", "b", "c" };
            string[] actual = { "@", "a", "ab", "b" };

            Assert.Throws<EqualException>(() => Assert.Equal(expected, actual));
            Assert.NotEqual(expected, actual);
        }

        [Fact]
        public void ArrayValuesAreDifferentNotEqual()
        {
            string[] expected = { "@", "d", "v", "d" };
            string[] actual = { "@", "a", "ab", "b" };

            Assert.Throws<EqualException>(() => Assert.Equal(expected, actual));
            Assert.NotEqual(expected, actual);
        }

        [Fact]
        public void Equivalence()
        {
            int[] expected = new[] { 1, 2, 3, 4, 5 };
            List<int> actual = new List<int>(expected);

            Assert.Equal(expected, actual);
        }
    }

    public class Equal_WithComparer
    {
        [Fact]
        public void EquivalenceWithComparer()
        {
            int[] expected = new[] { 1, 2, 3, 4, 5 };
            List<int> actual = new List<int>(new int[] { 0, 0, 0, 0, 0 });

            Assert.Equal(expected, actual, new IntComparer(true));
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

    public class None_NonGeneric_WithObject
    {
        [Fact]
        public void NullCollectionThrows()
        {
            Assert.Throws<ArgumentNullException>(() => Assert.None(null, null));
        }

        [Fact]
        public void ObjectNotPresent()
        {
            IEnumerable collection = new[] { "Hello", "World!" };

            Assert.None(collection, "there");
        }

        [Fact]
        public void NullNotPresent()
        {
            IEnumerable collection = new[] { "Hello", "World!" };

            Assert.None(collection, null);
        }

        [Fact]
        public void ObjectPresentThrows()
        {
            IEnumerable collection = new[] { "Hello", "World!" };

            Exception ex = Record.Exception(() => Assert.None(collection, "Hello"));

            Assert.IsType<NoneException>(ex);
            Assert.Equal("The collection contained 1 matching element(s) instead of 0.", ex.Message);
        }
    }

    public class None_Generic_WithPredicate
    {
        [Fact]
        public void NullCollectionThrows()
        {
            Assert.Throws<ArgumentNullException>(() => Assert.None<object>(null, _ => true));
        }

        [Fact]
        public void NullPredicateThrows()
        {
            Assert.Throws<ArgumentNullException>(() => Assert.None<object>(new object[0], null));
        }

        [Fact]
        public void PredicateWhereNoneMatch()
        {
            string[] collection = new[] { "Hello", "World!" };

            Assert.None(collection, item => false);
        }

        [Fact]
        public void PredicateWithMatch()
        {
            string[] collection = new[] { "Hello", "World!" };

            Exception ex = Record.Exception(() => Assert.None(collection, item => item.StartsWith("H")));

            Assert.IsType<NoneException>(ex);
            Assert.Equal("The collection contained 1 matching element(s) instead of 0.", ex.Message);
        }
    }

    public class NotEmpty
    {
        [Fact]
        public void EmptyContainer()
        {
            var list = new List<int>();

            var ex = Assert.Throws<NotEmptyException>(() => Assert.NotEmpty(list));

            Assert.Equal("Assert.NotEmpty() Failure", ex.Message);
        }

        [Fact]
        public void NonEmptyContainer()
        {
            var list = new List<int> { 42 };

            Assert.NotEmpty(list);
        }
    }

    public class NotEqual
    {
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
    }

    public class NotEqual_WithComparer
    {
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

    public class Single_NonGeneric
    {
        [Fact]
        public void NullCollectionThrows()
        {
            Assert.Throws<ArgumentNullException>(() => Assert.Single(null));
        }

        [Fact]
        public void EmptyCollectionThrows()
        {
            ArrayList collection = new ArrayList();

            Exception ex = Record.Exception(() => Assert.Single(collection));

            Assert.IsType<SingleException>(ex);
            Assert.Equal("The collection contained 0 matching element(s) instead of 1.", ex.Message);
        }

        [Fact]
        public void MultiItemCollectionThrows()
        {
            ArrayList collection = new ArrayList { "Hello", "World" };

            Exception ex = Record.Exception(() => Assert.Single(collection));

            Assert.IsType<SingleException>(ex);
            Assert.Equal("The collection contained 2 matching element(s) instead of 1.", ex.Message);
        }

        [Fact]
        public void SingleItemCollectionDoesNotThrow()
        {
            ArrayList collection = new ArrayList { "Hello" };

            Exception ex = Record.Exception(() => Assert.Single(collection));

            Assert.Null(ex);
        }

        [Fact]
        public void SingleItemCollectionReturnsTheItem()
        {
            ArrayList collection = new ArrayList { "Hello" };

            object result = Assert.Single(collection);

            Assert.Equal("Hello", result);
        }
    }

    public class Single_NonGeneric_WithObject
    {
        [Fact]
        public void NullCollectionThrows()
        {
            Assert.Throws<ArgumentNullException>(() => Assert.Single(null, null));
        }

        [Fact]
        public void ObjectSingleMatch()
        {
            IEnumerable collection = new[] { "Hello", "World!" };

            Assert.Single(collection, "Hello");
        }

        [Fact]
        public void NullSingleMatch()
        {
            IEnumerable collection = new[] { "Hello", "World!", null };

            Assert.Single(collection, null);
        }

        [Fact]
        public void ObjectNoMatch()
        {
            IEnumerable collection = new[] { "Hello", "World!" };

            Exception ex = Record.Exception(() => Assert.Single(collection, "foo"));

            Assert.IsType<SingleException>(ex);
            Assert.Equal("The collection contained 0 matching element(s) instead of 1.", ex.Message);
        }

        [Fact]
        public void PredicateTooManyMatches()
        {
            string[] collection = new[] { "Hello", "World!", "Hello" };

            Exception ex = Record.Exception(() => Assert.Single(collection, "Hello"));

            Assert.IsType<SingleException>(ex);
            Assert.Equal("The collection contained 2 matching element(s) instead of 1.", ex.Message);
        }
    }

    public class Single_Generic
    {
        [Fact]
        public void NullCollectionThrows()
        {
            Assert.Throws<ArgumentNullException>(() => Assert.Single<object>(null));
        }

        [Fact]
        public void EmptyCollectionThrows()
        {
            object[] collection = new object[0];

            Exception ex = Record.Exception(() => Assert.Single(collection));

            Assert.IsType<SingleException>(ex);
            Assert.Equal("The collection contained 0 matching element(s) instead of 1.", ex.Message);
        }

        [Fact]
        public void MultiItemCollectionThrows()
        {
            string[] collection = new[] { "Hello", "World!" };

            Exception ex = Record.Exception(() => Assert.Single(collection));

            Assert.IsType<SingleException>(ex);
            Assert.Equal("The collection contained 2 matching element(s) instead of 1.", ex.Message);
        }

        [Fact]
        public void SingleItemCollectionDoesNotThrow()
        {
            string[] collection = new[] { "Hello" };

            Exception ex = Record.Exception(() => Assert.Single(collection));

            Assert.Null(ex);
        }

        [Fact]
        public void SingleItemCollectionReturnsTheItem()
        {
            string[] collection = new[] { "Hello" };

            string result = Assert.Single(collection);

            Assert.Equal("Hello", result);
        }
    }

    public class Single_Generic_WithPredicate
    {
        [Fact]
        public void NullCollectionThrows()
        {
            Assert.Throws<ArgumentNullException>(() => Assert.Single<object>(null, _ => true));
        }

        [Fact]
        public void NullPredicateThrows()
        {
            Assert.Throws<ArgumentNullException>(() => Assert.Single<object>(new object[0], null));
        }

        [Fact]
        public void PredicateSingleMatch()
        {
            string[] collection = new[] { "Hello", "World!" };

            string result = Assert.Single(collection, item => item.StartsWith("H"));

            Assert.Equal("Hello", result);
        }

        [Fact]
        public void PredicateNoMatch()
        {
            string[] collection = new[] { "Hello", "World!" };

            Exception ex = Record.Exception(() => Assert.Single(collection, item => false));

            Assert.IsType<SingleException>(ex);
            Assert.Equal("The collection contained 0 matching element(s) instead of 1.", ex.Message);
        }

        [Fact]
        public void PredicateTooManyMatches()
        {
            string[] collection = new[] { "Hello", "World!" };

            Exception ex = Record.Exception(() => Assert.Single(collection, item => true));

            Assert.IsType<SingleException>(ex);
            Assert.Equal("The collection contained 2 matching element(s) instead of 1.", ex.Message);
        }
    }
}