using System;
using System.Collections;
using System.Collections.Generic;
using NSubstitute;
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
        public static void EmptyCollection()
        {
            var list = new List<int>();

            Assert.Collection(list);
        }

        [Fact]
        public static void MismatchedElementCount()
        {
            var list = new List<int>();

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
        public static void NonEmptyCollection()
        {
            var list = new List<int> { 42, 2112 };

            Assert.Collection(list,
                item => Assert.Equal(42, item),
                item => Assert.Equal(2112, item)
            );
        }

        [Fact]
        public static void MismatchedElement()
        {
            var list = new List<int> { 42, 2112 };

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
        public static void GuardClause()
        {
            Assert.Throws<ArgumentNullException>("collection", () => Assert.Contains(14, (List<int>)null));
        }

        [Fact]
        public static void CanFindNullInContainer()
        {
            var list = new List<object> { 16, null, "Hi there" };

            Assert.Contains(null, list);
        }

        [Fact]
        public static void ItemInContainer()
        {
            var list = new List<int> { 42 };

            Assert.Contains(42, list);
        }

        [Fact]
        public static void ItemNotInContainer()
        {
            var list = new List<int> { 41, 43 };

            var ex = Assert.Throws<ContainsException>(() => Assert.Contains(42, list));

            Assert.Equal(
                "Assert.Contains() Failure" + Environment.NewLine +
                "Not found: 42" + Environment.NewLine +
                "In value:  List<Int32> [41, 43]", ex.Message);
        }

        [Fact]
        public static void NullsAllowedInContainer()
        {
            var list = new List<object> { null, 16, "Hi there" };

            Assert.Contains("Hi there", list);
        }
    }

    public class Contains_WithComparer
    {
        [Fact]
        public static void GuardClauses()
        {
            var comparer = Substitute.For<IEqualityComparer<int>>();

            Assert.Throws<ArgumentNullException>("collection", () => Assert.Contains(14, (List<int>)null, comparer));
            Assert.Throws<ArgumentNullException>("comparer", () => Assert.Contains(14, new int[0], null));
        }

        [Fact]
        public static void CanUseComparer()
        {
            var list = new List<int> { 42 };

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

    public class Contains_WithPredicate
    {
        [Fact]
        public static void GuardClauses()
        {
            Assert.Throws<ArgumentNullException>("collection", () => Assert.Contains((List<int>)null, item => true));
            Assert.Throws<ArgumentNullException>("filter", () => Assert.Contains(new int[0], (Predicate<int>)null));
        }

        [Fact]
        public static void ItemFound_DoesNotThrow()
        {
            var list = new[] { "Hello", "world" };

            Assert.Contains(list, item => item.StartsWith("w"));
        }

        [Fact]
        public static void ItemNotFound_Throws()
        {
            var list = new[] { "Hello", "world" };

            Assert.Throws<ContainsException>(() => Assert.Contains(list, item => item.StartsWith("q")));
        }
    }

    public class DoesNotContain
    {
        [Fact]
        public static void GuardClause()
        {
            Assert.Throws<ArgumentNullException>("collection", () => Assert.DoesNotContain(14, (List<int>)null));
        }

        [Fact]
        public static void CanSearchForNullInContainer()
        {
            var list = new List<object> { 16, "Hi there" };

            Assert.DoesNotContain(null, list);
        }

        [Fact]
        public static void ItemInContainer()
        {
            var list = new List<int> { 42 };

            DoesNotContainException ex =
                Assert.Throws<DoesNotContainException>(() => Assert.DoesNotContain(42, list));

            Assert.Equal("Assert.DoesNotContain() Failure" + Environment.NewLine +
                         "Found:    42" + Environment.NewLine +
                         "In value: List<Int32> [42]", ex.Message);
        }

        [Fact]
        public static void ItemNotInContainer()
        {
            var list = new List<int>();

            Assert.DoesNotContain(42, list);
        }

        [Fact]
        public static void NullsAllowedInContainer()
        {
            var list = new List<object> { null, 16, "Hi there" };

            Assert.DoesNotContain(42, list);
        }
    }

    public class DoesNotContain_WithComparer
    {
        [Fact]
        public static void GuardClauses()
        {
            var comparer = Substitute.For<IEqualityComparer<int>>();

            Assert.Throws<ArgumentNullException>("collection", () => Assert.DoesNotContain(14, (List<int>)null, comparer));
            Assert.Throws<ArgumentNullException>("comparer", () => Assert.DoesNotContain(14, new int[0], null));
        }

        [Fact]
        public static void CanUseComparer()
        {
            var list = new List<int>();
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

    public class DoesNotContain_WithPredicate
    {
        [Fact]
        public static void GuardClauses()
        {
            Assert.Throws<ArgumentNullException>("collection", () => Assert.DoesNotContain((List<int>)null, item => true));
            Assert.Throws<ArgumentNullException>("filter", () => Assert.DoesNotContain(new int[0], (Predicate<int>)null));
        }

        [Fact]
        public static void ItemFound_Throws()
        {
            var list = new[] { "Hello", "world" };

            Assert.Throws<DoesNotContainException>(() => Assert.DoesNotContain(list, item => item.StartsWith("w")));
        }

        [Fact]
        public static void ItemNotFound_DoesNotThrow()
        {
            var list = new[] { "Hello", "world" };

            Assert.DoesNotContain(list, item => item.StartsWith("q"));
        }
    }

    public class Empty
    {
        [Fact]
        public static void GuardClauses()
        {
            Assert.Throws<ArgumentNullException>(() => Assert.Empty(null));
        }

        [Fact]
        public static void EmptyContainer()
        {
            var list = new List<int>();

            Assert.Empty(list);
        }

        [Fact]
        public static void NonEmptyContainerThrows()
        {
            var list = new List<int>();
            list.Add(42);

            EmptyException ex = Assert.Throws<EmptyException>(() => Assert.Empty(list));

            Assert.Equal("Assert.Empty() Failure", ex.Message);
        }

        [Fact]
        public static void EmptyString()
        {
            Assert.Empty("");
        }

        [Fact]
        public static void NonEmptyStringThrows()
        {
            EmptyException ex = Assert.Throws<EmptyException>(() => Assert.Empty("Foo"));

            Assert.Equal("Assert.Empty() Failure", ex.Message);
        }
    }

    public class Equal
    {
        [Fact]
        public static void Array()
        {
            string[] expected = { "@", "a", "ab", "b" };
            string[] actual = { "@", "a", "ab", "b" };

            Assert.Equal(expected, actual);
            Assert.Throws<NotEqualException>(() => Assert.NotEqual(expected, actual));
        }

        [Fact]
        public static void ArrayInsideArray()
        {
            string[][] expected = { new[] { "@", "a" }, new[] { "ab", "b" } };
            string[][] actual = { new[] { "@", "a" }, new[] { "ab", "b" } };

            Assert.Equal(expected, actual);
            Assert.Throws<NotEqualException>(() => Assert.NotEqual(expected, actual));
        }

        [Fact]
        public static void ArraysOfDifferentLengthsAreNotEqual()
        {
            string[] expected = { "@", "a", "ab", "b", "c" };
            string[] actual = { "@", "a", "ab", "b" };

            Assert.Throws<EqualException>(() => Assert.Equal(expected, actual));
            Assert.NotEqual(expected, actual);
        }

        [Fact]
        public static void ArrayValuesAreDifferentNotEqual()
        {
            string[] expected = { "@", "d", "v", "d" };
            string[] actual = { "@", "a", "ab", "b" };

            Assert.Throws<EqualException>(() => Assert.Equal(expected, actual));
            Assert.NotEqual(expected, actual);
        }

        [Fact]
        public static void Equivalence()
        {
            int[] expected = new[] { 1, 2, 3, 4, 5 };
            List<int> actual = new List<int>(expected);

            Assert.Equal(expected, actual);
        }
    }

    public class EqualDictionary
    {
        [Fact]
        public static void InOrderDictionary()
        {
            var expected = new Dictionary<string, int> { { "a", 1 }, { "b", 2 }, { "c", 3 } };
            var actual = new Dictionary<string, int> { { "a", 1 }, { "b", 2 }, { "c", 3 } };

            Assert.Equal(expected, actual);
            Assert.Throws<NotEqualException>(() => Assert.NotEqual(expected, actual));
        }

        [Fact]
        public static void OutOfOrderDictionary()
        {
            var expected = new Dictionary<string, int> { { "a", 1 }, { "b", 2 }, { "c", 3 } };
            var actual = new Dictionary<string, int> { { "b", 2 }, { "c", 3 }, { "a", 1 } };

            Assert.Equal(expected, actual);
            Assert.Throws<NotEqualException>(() => Assert.NotEqual(expected, actual));
        }

        [Fact]
        public static void ExpectedLarger()
        {
            var expected = new Dictionary<string, int> { { "a", 1 }, { "b", 2 }, { "c", 3 } };
            var actual = new Dictionary<string, int> { { "a", 1 }, { "b", 2 } };

            Assert.NotEqual(expected, actual);
            Assert.Throws<EqualException>(() => Assert.Equal(expected, actual));
        }

        [Fact]
        public static void ActualLarger()
        {
            var expected = new Dictionary<string, int> { { "a", 1 }, { "b", 2 } };
            var actual = new Dictionary<string, int> { { "a", 1 }, { "b", 2 }, { "c", 3 } };

            Assert.NotEqual(expected, actual);
            Assert.Throws<EqualException>(() => Assert.Equal(expected, actual));
        }
    }

    public class Equal_WithComparer
    {
        [Fact]
        public static void EquivalenceWithComparer()
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

    public class NotEmpty
    {
        [Fact]
        public static void EmptyContainer()
        {
            var list = new List<int>();

            var ex = Assert.Throws<NotEmptyException>(() => Assert.NotEmpty(list));

            Assert.Equal("Assert.NotEmpty() Failure", ex.Message);
        }

        [Fact]
        public static void NonEmptyContainer()
        {
            var list = new List<int> { 42 };

            Assert.NotEmpty(list);
        }
    }

    public class NotEqual
    {
        [Fact]
        public static void EnumerableInequivalence()
        {
            int[] expected = new[] { 1, 2, 3, 4, 5 };
            List<int> actual = new List<int>(new[] { 1, 2, 3, 4, 6 });

            Assert.NotEqual(expected, actual);
        }

        [Fact]
        public static void EnumerableEquivalence()
        {
            int[] expected = new[] { 1, 2, 3, 4, 5 };
            List<int> actual = new List<int>(expected);

            Assert.Throws<NotEqualException>(() => Assert.NotEqual(expected, actual));
        }
    }

    public class NotEqual_WithComparer
    {
        [Fact]
        public static void EnumerableInequivalenceWithFailedComparer()
        {
            int[] expected = new[] { 1, 2, 3, 4, 5 };
            List<int> actual = new List<int>(new int[] { 1, 2, 3, 4, 5 });

            Assert.NotEqual(expected, actual, new IntComparer(false));
        }

        [Fact]
        public static void EnumerableEquivalenceWithSuccessfulComparer()
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
        public static void NullCollectionThrows()
        {
            Assert.Throws<ArgumentNullException>(() => Assert.Single(null));
        }

        [Fact]
        public static void EmptyCollectionThrows()
        {
            ArrayList collection = new ArrayList();

            Exception ex = Record.Exception(() => Assert.Single(collection));

            Assert.IsType<SingleException>(ex);
            Assert.Equal("The collection contained 0 matching element(s) instead of 1.", ex.Message);
        }

        [Fact]
        public static void MultiItemCollectionThrows()
        {
            ArrayList collection = new ArrayList { "Hello", "World" };

            Exception ex = Record.Exception(() => Assert.Single(collection));

            Assert.IsType<SingleException>(ex);
            Assert.Equal("The collection contained 2 matching element(s) instead of 1.", ex.Message);
        }

        [Fact]
        public static void SingleItemCollectionDoesNotThrow()
        {
            ArrayList collection = new ArrayList { "Hello" };

            Exception ex = Record.Exception(() => Assert.Single(collection));

            Assert.Null(ex);
        }

        [Fact]
        public static void SingleItemCollectionReturnsTheItem()
        {
            ArrayList collection = new ArrayList { "Hello" };

            object result = Assert.Single(collection);

            Assert.Equal("Hello", result);
        }
    }

    public class Single_NonGeneric_WithObject
    {
        [Fact]
        public static void NullCollectionThrows()
        {
            Assert.Throws<ArgumentNullException>(() => Assert.Single(null, null));
        }

        [Fact]
        public static void ObjectSingleMatch()
        {
            IEnumerable collection = new[] { "Hello", "World!" };

            Assert.Single(collection, "Hello");
        }

        [Fact]
        public static void NullSingleMatch()
        {
            IEnumerable collection = new[] { "Hello", "World!", null };

            Assert.Single(collection, null);
        }

        [Fact]
        public static void ObjectNoMatch()
        {
            IEnumerable collection = new[] { "Hello", "World!" };

            Exception ex = Record.Exception(() => Assert.Single(collection, "foo"));

            Assert.IsType<SingleException>(ex);
            Assert.Equal("The collection contained 0 matching element(s) instead of 1.", ex.Message);
        }

        [Fact]
        public static void PredicateTooManyMatches()
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
        public static void NullCollectionThrows()
        {
            Assert.Throws<ArgumentNullException>(() => Assert.Single<object>(null));
        }

        [Fact]
        public static void EmptyCollectionThrows()
        {
            object[] collection = new object[0];

            Exception ex = Record.Exception(() => Assert.Single(collection));

            Assert.IsType<SingleException>(ex);
            Assert.Equal("The collection contained 0 matching element(s) instead of 1.", ex.Message);
        }

        [Fact]
        public static void MultiItemCollectionThrows()
        {
            string[] collection = new[] { "Hello", "World!" };

            Exception ex = Record.Exception(() => Assert.Single(collection));

            Assert.IsType<SingleException>(ex);
            Assert.Equal("The collection contained 2 matching element(s) instead of 1.", ex.Message);
        }

        [Fact]
        public static void SingleItemCollectionDoesNotThrow()
        {
            string[] collection = new[] { "Hello" };

            Exception ex = Record.Exception(() => Assert.Single(collection));

            Assert.Null(ex);
        }

        [Fact]
        public static void SingleItemCollectionReturnsTheItem()
        {
            string[] collection = new[] { "Hello" };

            string result = Assert.Single(collection);

            Assert.Equal("Hello", result);
        }
    }

    public class Single_Generic_WithPredicate
    {
        [Fact]
        public static void NullCollectionThrows()
        {
            Assert.Throws<ArgumentNullException>(() => Assert.Single<object>(null, _ => true));
        }

        [Fact]
        public static void NullPredicateThrows()
        {
            Assert.Throws<ArgumentNullException>(() => Assert.Single<object>(new object[0], null));
        }

        [Fact]
        public static void PredicateSingleMatch()
        {
            string[] collection = new[] { "Hello", "World!" };

            string result = Assert.Single(collection, item => item.StartsWith("H"));

            Assert.Equal("Hello", result);
        }

        [Fact]
        public static void PredicateNoMatch()
        {
            string[] collection = new[] { "Hello", "World!" };

            Exception ex = Record.Exception(() => Assert.Single(collection, item => false));

            Assert.IsType<SingleException>(ex);
            Assert.Equal("The collection contained 0 matching element(s) instead of 1.", ex.Message);
        }

        [Fact]
        public static void PredicateTooManyMatches()
        {
            string[] collection = new[] { "Hello", "World!" };

            Exception ex = Record.Exception(() => Assert.Single(collection, item => true));

            Assert.IsType<SingleException>(ex);
            Assert.Equal("The collection contained 2 matching element(s) instead of 1.", ex.Message);
        }
    }
}
