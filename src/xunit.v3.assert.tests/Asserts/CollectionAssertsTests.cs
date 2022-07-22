using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using NSubstitute;
using Xunit;
using Xunit.Sdk;

#if XUNIT_VALUETASK
using System.Threading.Tasks;
#endif

public class CollectionAssertsTests
{
	public class All
	{
		[Fact]
		public static void NullCollectionThrows()
		{
			Assert.Throws<ArgumentNullException>(() => Assert.All<object>(null!, _ => { }));
		}

		[Fact]
		public static void NullActionThrows()
		{
			Assert.Throws<ArgumentNullException>(() => Assert.All(new object[0], (Action<object>)null!));
			Assert.Throws<ArgumentNullException>(() => Assert.All(new object[0], (Action<object, int>)null!));
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

			var ex = Record.Exception(() => Assert.All(items, x => Assert.Equal(1, x)));

			Assert.Null(ex);
		}

		[Fact]
		public static void ActionWhereAllFail()
		{
			var items = new[] { 1, 1, 2, 2, 1, 1 };

			var ex = Assert.Throws<AllException>(() => Assert.All(items, x => Assert.Equal(0, x)));

			Assert.Equal(6, ex.Failures.Count);
			Assert.All(ex.Failures, x => Assert.IsType<EqualException>(x));
		}

		[Fact]
		public static void ActionCanReceiveIndex()
		{
			var items = new[] { 1, 1, 2, 2, 1, 1 };
			var indices = new List<int>();

			Assert.All(items, (x, idx) => indices.Add(idx));

			Assert.Equal(new[] { 0, 1, 2, 3, 4, 5 }, indices);
		}

		[Fact]
		public static void CollectionWithNullThrowsAllException()
		{
			var collection = new object?[]
			{
				new object(),
				null
			};

			var ex = Assert.Throws<AllException>(() => Assert.All(collection, Assert.NotNull));
			Assert.Contains("[1]: Item: ", ex.Message);
		}
	}

#if XUNIT_VALUETASK
	public class AllAsync
	{
		[Fact]
		public static async ValueTask NullCollectionThrows()
		{
			await Assert.ThrowsAsync<ArgumentNullException>(() => Assert.AllAsync<object>(null!, async _ => await Task.Yield()));
		}

		[Fact]
		public static async ValueTask NullActionThrows()
		{
			await Assert.ThrowsAsync<ArgumentNullException>(() => Assert.AllAsync(new object[0], (Func<object, ValueTask>)null!));
			await Assert.ThrowsAsync<ArgumentNullException>(() => Assert.AllAsync(new object[0], (Func<object, int, ValueTask>)null!));
		}

		[Fact]
		public static async ValueTask ActionWhereSomeFail()
		{
			var items = new[] { 1, 1, 2, 2, 1, 1 };

			var ex = await Assert.ThrowsAsync<AllException>(() => Assert.AllAsync(items, async x =>
			{
				await Task.Yield();
				Assert.Equal(1, x);
			}));

			Assert.Equal(2, ex.Failures.Count);
			Assert.All(ex.Failures, x => Assert.IsType<EqualException>(x));
		}

		[Fact]
		public static async ValueTask ActionWhereNoneFail()
		{
			var items = new[] { 1, 1, 1, 1, 1, 1 };

			var ex = await Record.ExceptionAsync(() => Assert.AllAsync(items, async x =>
			{
				await Task.Yield();
				Assert.Equal(1, x);
			}));

			Assert.Null(ex);
		}

		[Fact]
		public static async ValueTask ActionWhereAllFail()
		{
			var items = new[] { 1, 1, 2, 2, 1, 1 };

			var ex = await Assert.ThrowsAsync<AllException>(() => Assert.AllAsync(items, async x =>
			{
				await Task.Yield();
				Assert.Equal(0, x);
			}));

			Assert.Equal(6, ex.Failures.Count);
			Assert.All(ex.Failures, x => Assert.IsType<EqualException>(x));
		}

		[Fact]
		public static async ValueTask ActionCanReceiveIndex()
		{
			var items = new[] { 1, 1, 2, 2, 1, 1 };
			var indices = new List<int>();

			await Assert.AllAsync(items, async (x, idx) =>
			{
				await Task.Yield();
				indices.Add(idx);
			});

			Assert.Equal(new[] { 0, 1, 2, 3, 4, 5 }, indices);
		}

		[Fact]
		public static async ValueTask CollectionWithNullThrowsAllException()
		{
			var collection = new object?[]
			{
				new object(),
				null
			};

			var ex = await Assert.ThrowsAsync<AllException>(() => Assert.AllAsync(collection, async x =>
			{
				await Task.Yield();
				Assert.NotNull(x);
			}));
			Assert.Contains("[1]: Item: ", ex.Message);
		}
	}
#endif

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
			Assert.Equal(
				"Assert.Collection() Failure" + Environment.NewLine +
				"Collection: []" + Environment.NewLine +
				"Expected item count: 1" + Environment.NewLine +
				"Actual item count:   0",
				collEx.Message
			);
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
			Assert.Equal(
				"Assert.Collection() Failure" + Environment.NewLine +
				"Collection: [42, 2112]" + Environment.NewLine +
				"Error during comparison of item at index 1" + Environment.NewLine +
				"Inner exception: Assert.Equal() Failure" + Environment.NewLine +
				"        Expected: 2113" + Environment.NewLine +
				"        Actual:   2112",
				ex.Message
			);
		}
	}

#if XUNIT_VALUETASK
	public class CollectionAsync
	{
		[Fact]
		public static async ValueTask EmptyCollection()
		{
			var list = new List<int>();

			await Assert.CollectionAsync(list);
		}

		[Fact]
		public static async ValueTask MismatchedElementCountAsync()
		{
			var list = new List<int>();

			var ex = await Record.ExceptionAsync(
				() => Assert.CollectionAsync(list,
					async item => await Task.Yield()
				)
			);

			var collEx = Assert.IsType<CollectionException>(ex);
			Assert.Equal(1, collEx.ExpectedCount);
			Assert.Equal(0, collEx.ActualCount);
			Assert.Equal(
				"Assert.Collection() Failure" + Environment.NewLine +
				"Collection: []" + Environment.NewLine +
				"Expected item count: 1" + Environment.NewLine +
				"Actual item count:   0",
				collEx.Message
			);
			Assert.Null(collEx.InnerException);
		}

		[Fact]
		public static async ValueTask NonEmptyCollectionAsync()
		{
			var list = new List<int> { 42, 2112 };

			await Assert.CollectionAsync(list,
				async item =>
				{
					await Task.Yield();
					Assert.Equal(42, item);
				},
				async item =>
				{
					await Task.Yield();
					Assert.Equal(2112, item);
				}
			);
		}

		[Fact]
		public static async ValueTask MismatchedElementAsync()
		{
			var list = new List<int> { 42, 2112 };

			var ex = await Record.ExceptionAsync(() =>
				 Assert.CollectionAsync(list,
				 async item =>
				 {
					 await Task.Yield();
					 Assert.Equal(42, item);
				 },
				 async item =>
				 {
					 await Task.Yield();
					 Assert.Equal(2113, item);
				 }
				 )
			);

			var collEx = Assert.IsType<CollectionException>(ex);
			Assert.Equal(1, collEx.IndexFailurePoint);
			Assert.Equal(
				"Assert.Collection() Failure" + Environment.NewLine +
				"Collection: [42, 2112]" + Environment.NewLine +
				"Error during comparison of item at index 1" + Environment.NewLine +
				"Inner exception: Assert.Equal() Failure" + Environment.NewLine +
				"        Expected: 2113" + Environment.NewLine +
				"        Actual:   2112",
				ex.Message
			);
		}
	}
#endif

	public class Contains
	{
		[Fact]
		public static void GuardClause()
		{
			Assert.Throws<ArgumentNullException>("collection", () => Assert.Contains(14, null!));
		}

		[Fact]
		public static void CanFindNullInContainer()
		{
			var list = new List<object?> { 16, null, "Hi there" };

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
				"In value:  List<Int32> [41, 43]",
				ex.Message
			);
		}

		[Fact]
		public static void NullsAllowedInContainer()
		{
			var list = new List<object?> { null, 16, "Hi there" };

			Assert.Contains("Hi there", list);
		}

		[Fact]
		public static void ICollectionContainsIsTrueButContainsWithDefaultComparerIsFalse()
		{
			var set = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "Hi there" };

			// ICollection<T>.Contains is called if the container implements ICollection<T>.
			// If either ICollection<T>.Contains or the default equality comparer report that
			// the collection has the item, the assert should pass.
			Assert.Contains("HI THERE", set);
		}

		[Fact]
		public static void ICollectionContainsIsFalseButContainsWithDefaultComparerIsTrue()
		{
			var collections = new[]
			{
				new[] { 1, 2, 3, 4 }
			};

			// ICollection<T>.Contains is called if the container implements ICollection<T>.
			// If either ICollection<T>.Contains or the default equality comparer report that
			// the collection has the item, the assert should pass.
			Assert.Contains(new[] { 1, 2, 3, 4 }, collections);
		}

		[Fact]
		public static void KeyInDictionary()
		{
			IDictionary<string, int> dictionary = new Dictionary<string, int>
			{
				["forty-two"] = 42
			};

			var actual = Assert.Contains("forty-two", dictionary);
			Assert.Equal(42, actual);
		}

		[Fact]
		public static void KeyNotInDictionary()
		{
			IDictionary<string, int> dictionary = new Dictionary<string, int>
			{
				["eleventeen"] = 110
			};

			var actual = Record.Exception(() => Assert.Contains("forty-two", dictionary));

			var ex = Assert.IsType<ContainsException>(actual);
			Assert.Equal(
				"Assert.Contains() Failure" + Environment.NewLine +
				"Not found: forty-two" + Environment.NewLine +
				@"In value:  KeyCollection<String, Int32> [""eleventeen""]",
				ex.Message
			);
		}

		[Fact]
		public static void KeyInReadOnlyDictionary()
		{
			IReadOnlyDictionary<string, int> dictionary = new Dictionary<string, int>
			{
				["forty-two"] = 42
			};

			var actual = Assert.Contains("forty-two", dictionary);
			Assert.Equal(42, actual);
		}

		[Fact]
		public static void KeyNotInReadOnlyDictionary()
		{
			IReadOnlyDictionary<string, int> dictionary = new Dictionary<string, int>
			{
				["eleventeen"] = 110
			};

			var actual = Record.Exception(() => Assert.Contains("forty-two", dictionary));

			var ex = Assert.IsType<ContainsException>(actual);
			Assert.Equal(
				"Assert.Contains() Failure" + Environment.NewLine +
				"Not found: forty-two" + Environment.NewLine +
				@"In value:  KeyCollection<String, Int32> [""eleventeen""]",
				ex.Message
			);
		}
	}

	public class Contains_WithComparer
	{
		[Fact]
		public static void GuardClauses()
		{
			var comparer = Substitute.For<IEqualityComparer<int>>();

			Assert.Throws<ArgumentNullException>("collection", () => Assert.Contains(14, null!, comparer));
			Assert.Throws<ArgumentNullException>("comparer", () => Assert.Contains(14, new int[0], null!));
		}

		[Fact]
		public static void CanUseComparer()
		{
			var list = new List<int> { 42 };

			Assert.Contains(43, list, new MyComparer());
		}

		[Fact]
		public static void DoesNotTryToCallICollectionContains()
		{
			var set = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "Hi there" };

			// ICollection<T>.Contains would return true, but we're passing in a custom comparer to Assert.Contains
			// (and ICollection<T>.Contains does not accept a comparer) so we should not attempt to use that result.
			var ex = Assert.Throws<ContainsException>(() => Assert.Contains("HI THERE", set, StringComparer.Ordinal));

			Assert.Equal(
				"Assert.Contains() Failure" + Environment.NewLine +
				"Not found: HI THERE" + Environment.NewLine +
				@"In value:  HashSet<String> [""Hi there""]",
				ex.Message
			);
		}

		class MyComparer : IEqualityComparer<int>
		{
			public bool Equals(int x, int y) => true;

			public int GetHashCode(int obj) => throw new NotImplementedException();
		}
	}

	public class Contains_WithPredicate
	{
		[Fact]
		public static void GuardClauses()
		{
			Assert.Throws<ArgumentNullException>("collection", () => Assert.Contains<int>(null!, item => true));
			Assert.Throws<ArgumentNullException>("filter", () => Assert.Contains(new int[0], (Predicate<int>)null!));
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

	public class Distinct
	{
		[Fact]
		public static void GuardClauses()
		{
			Assert.Throws<ArgumentNullException>("collection", () => Assert.Distinct<int>(null!));
			Assert.Throws<ArgumentNullException>("comparer", () => Assert.Distinct(new object[0], null!));
		}

		[Fact]
		public static void WithNull()
		{
			var list = new List<object?> { 16, "Hi there", null };

			Assert.Distinct(list);
		}

		[Fact]
		public static void TwoItems()
		{
			var list = new List<int> { 42, 42 };

			var ex = Assert.Throws<ContainsDuplicateException>(() => Assert.Distinct(list));

			Assert.Equal(
				"Assert.Distinct() Failure: The item 42 occurs multiple times in [42, 42].",
				ex.Message
			);
		}

		[Fact]
		public static void TwoNulls()
		{
			var list = new List<string?> { "a", null, "b", null };

			var ex = Assert.Throws<ContainsDuplicateException>(() => Assert.Distinct(list));

			Assert.Equal(
				"Assert.Distinct() Failure: The item null occurs multiple times in [\"a\", null, \"b\", null].",
				ex.Message
			);
		}

		[Fact]
		public static void CaseSensitiveStrings()
		{
			var list = new string[] { "a", "b", "A" };

			Assert.Distinct(list);
			Assert.Distinct(list, StringComparer.Ordinal);
		}

		[Fact]
		public static void CaseInsensitiveStrings()
		{
			var list = new string[] { "a", "b", "A" };

			var ex = Assert.Throws<ContainsDuplicateException>(() => Assert.Distinct(list, StringComparer.OrdinalIgnoreCase));

			Assert.Equal(
				"Assert.Distinct() Failure: The item \"A\" occurs multiple times in [\"a\", \"b\", \"A\"].",
				ex.Message
			);
		}
	}

	public class DoesNotContain
	{
		[Fact]
		public static void GuardClause()
		{
			Assert.Throws<ArgumentNullException>("collection", () => Assert.DoesNotContain(14, null!));
		}

		[Fact]
		public static void CanSearchForNullInContainer()
		{
			var list = new List<object?> { 16, "Hi there" };

			Assert.DoesNotContain(null, list);
		}

		[Fact]
		public static void ItemInContainer()
		{
			var list = new List<int> { 42 };

			DoesNotContainException ex =
				Assert.Throws<DoesNotContainException>(() => Assert.DoesNotContain(42, list));

			Assert.Equal(
				"Assert.DoesNotContain() Failure" + Environment.NewLine +
				"Found:    42" + Environment.NewLine +
				"In value: List<Int32> [42]",
				ex.Message
			);
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
			var list = new List<object?> { null, 16, "Hi there" };

			Assert.DoesNotContain(42, list);
		}

		[Fact]
		public static void ICollectionContainsIsTrueButContainsWithDefaultComparerIsFalse()
		{
			var set = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "Hi there" };

			// ICollection<T>.Contains is called if the container implements ICollection<T>.
			// If either ICollection<T>.Contains or the default equality comparer report that
			// the collection has the item, the assert should fail.
			var ex = Assert.Throws<DoesNotContainException>(() => Assert.DoesNotContain("HI THERE", set));

			Assert.Equal(
				"Assert.DoesNotContain() Failure" + Environment.NewLine +
				"Found:    HI THERE" + Environment.NewLine +
				@"In value: HashSet<String> [""Hi there""]",
				ex.Message
			);
		}

		[Fact]
		public static void ICollectionContainsIsFalseButContainsWithDefaultComparerIsTrue()
		{
			var collections = new[]
			{
				new[] { 1, 2, 3, 4 }
			};

			// ICollection<T>.Contains is called if the container implements ICollection<T>.
			// If either ICollection<T>.Contains or the default equality comparer report that
			// the collection has the item, the assert should fail.
			var ex = Assert.Throws<DoesNotContainException>(() => Assert.DoesNotContain(new[] { 1, 2, 3, 4 }, collections));

			Assert.Equal(
				"Assert.DoesNotContain() Failure" + Environment.NewLine +
				"Found:    Int32[] [1, 2, 3, 4]" + Environment.NewLine +
				"In value: Int32[][] [[1, 2, 3, 4]]",
				ex.Message
			);
		}

		[Fact]
		public static void KeyNotInDictionary()
		{
			var dictionary = (IDictionary<string, int>)new Dictionary<string, int>
			{
				["eleventeen"] = 110
			};

			var actual = Record.Exception(() => Assert.DoesNotContain("forty-two", dictionary));

			Assert.Null(actual);
		}

		[Fact]
		public static void KeyInDictionary()
		{
			var dictionary = (IDictionary<string, int>)new Dictionary<string, int>
			{
				["forty-two"] = 42
			};

			var actual = Record.Exception(() => Assert.DoesNotContain("forty-two", dictionary));

			var ex = Assert.IsType<DoesNotContainException>(actual);
			Assert.Equal(
				"Assert.DoesNotContain() Failure" + Environment.NewLine +
				"Found:    forty-two" + Environment.NewLine +
				@"In value: KeyCollection<String, Int32> [""forty-two""]",
				ex.Message
			);
		}

		[Fact]
		public static void KeyNotInReadOnlyDictionary()
		{
			var dictionary = (IReadOnlyDictionary<string, int>)new Dictionary<string, int>
			{
				["eleventeen"] = 110
			};

			var actual = Record.Exception(() => Assert.DoesNotContain("forty-two", dictionary));

			Assert.Null(actual);
		}

		[Fact]
		public static void KeyInReadOnlyDictionary()
		{
			var dictionary = (IReadOnlyDictionary<string, int>)new Dictionary<string, int>
			{
				["forty-two"] = 42
			};

			var actual = Record.Exception(() => Assert.DoesNotContain("forty-two", dictionary));

			var ex = Assert.IsType<DoesNotContainException>(actual);
			Assert.Equal(
				"Assert.DoesNotContain() Failure" + Environment.NewLine +
				"Found:    forty-two" + Environment.NewLine +
				@"In value: KeyCollection<String, Int32> [""forty-two""]",
				ex.Message
			);
		}
	}

	public class DoesNotContain_WithComparer
	{
		[Fact]
		public static void GuardClauses()
		{
			var comparer = Substitute.For<IEqualityComparer<int>>();

			Assert.Throws<ArgumentNullException>("collection", () => Assert.DoesNotContain(14, null!, comparer));
			Assert.Throws<ArgumentNullException>("comparer", () => Assert.DoesNotContain(14, new int[0], null!));
		}

		[Fact]
		public static void CanUseComparer()
		{
			var list = new List<int> { 42 };

			Assert.DoesNotContain(42, list, new MyComparer());
		}

		[Fact]
		public static void DoesNotTryToCallICollectionContains()
		{
			var set = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "Hi there" };

			// ICollection<T>.Contains would return true, but we're passing in a custom comparer to Assert.DoesNotContain
			// (and ICollection<T>.Contains does not accept a comparer) so we should not attempt to use that result.
			Assert.DoesNotContain("HI THERE", set, StringComparer.Ordinal);
		}

		class MyComparer : IEqualityComparer<int>
		{
			public bool Equals(int x, int y) => false;

			public int GetHashCode(int obj) => throw new NotImplementedException();
		}
	}

	public class DoesNotContain_WithPredicate
	{
		[Fact]
		public static void GuardClauses()
		{
			Assert.Throws<ArgumentNullException>("collection", () => Assert.DoesNotContain((List<int>)null!, item => true));
			Assert.Throws<ArgumentNullException>("filter", () => Assert.DoesNotContain(new int[0], (Predicate<int>)null!));
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
			Assert.Throws<ArgumentNullException>(() => Assert.Empty(null!));
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
			var list = new List<int> { 42 };

			EmptyException ex = Assert.Throws<EmptyException>(() => Assert.Empty(list));

			Assert.Equal($"Assert.Empty() Failure{Environment.NewLine}Expected: <empty>{Environment.NewLine}Actual:   [42]", ex.Message);
		}

		[Fact]
		public static void EnumeratorDisposed()
		{
			var enumerator = new SpyEnumerator<int>(Enumerable.Empty<int>());

			Assert.Empty(enumerator);

			Assert.True(enumerator.IsDisposed);
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

			Assert.Equal($"Assert.Empty() Failure{Environment.NewLine}Expected: <empty>{Environment.NewLine}Actual:   \"Foo\"", ex.Message);
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
			var expected = new[] { 1, 2, 3, 4, 5 };
			var actual = new List<int>(expected);

			Assert.Equal(expected, actual);
		}

		[Fact]
		public static void EnumeratesOnlyOnce()
		{
			var expected = new[] { 1, 2, 3, 4, 5 };
			var actual = new RunOnceEnumerable<int>(expected);
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

		[Fact]
		public static void SomeKeysDiffer()
		{
			var expected = new Dictionary<string, int>
			{
				["a"] = 1,
				["be"] = 2,
				["c"] = 3,
				["d"] = 4,
				["e"] = 5,
				["f"] = 6,
			};
			var actual = new Dictionary<string, int>
			{
				["a"] = 1,
				["ba"] = 2,
				["c"] = 3,
				["d"] = 4,
				["e"] = 5,
				["f"] = 6,
			};

			Assert.NotEqual(expected, actual);
			var ex = Assert.Throws<EqualException>(() => Assert.Equal(expected, actual));
			Assert.Equal(
				"Assert.Equal() Failure" + Environment.NewLine +
				"Expected: Dictionary<String, Int32> [[\"a\"] = 1, [\"be\"] = 2, [\"c\"] = 3, [\"d\"] = 4, [\"e\"] = 5, ...]" + Environment.NewLine +
				"Actual:   Dictionary<String, Int32> [[\"a\"] = 1, [\"ba\"] = 2, [\"c\"] = 3, [\"d\"] = 4, [\"e\"] = 5, ...]",
				ex.Message
			);
		}
	}

	public class EqualSet
	{
		[Fact]
		public static void InOrderSet()
		{
			var expected = new HashSet<int> { 1, 2, 3 };
			var actual = new HashSet<int> { 1, 2, 3 };

			Assert.Equal(expected, actual);
			Assert.Throws<NotEqualException>(() => Assert.NotEqual(expected, actual));
		}

		[Fact]
		public static void OutOfOrderSet()
		{
			var expected = new HashSet<int> { 1, 2, 3 };
			var actual = new HashSet<int> { 2, 3, 1 };

			Assert.Equal(expected, actual);
			Assert.Throws<NotEqualException>(() => Assert.NotEqual(expected, actual));
		}

		[Fact]
		public static void ExpectedLarger()
		{
			var expected = new HashSet<int> { 1, 2, 3 };
			var actual = new HashSet<int> { 1, 2 };

			Assert.NotEqual(expected, actual);
			Assert.Throws<EqualException>(() => Assert.Equal(expected, actual));
		}

		[Fact]
		public static void ActualLarger()
		{
			var expected = new HashSet<int> { 1, 2 };
			var actual = new HashSet<int> { 1, 2, 3 };

			Assert.NotEqual(expected, actual);
			Assert.Throws<EqualException>(() => Assert.Equal(expected, actual));
		}
	}

	public class Equal_WithComparer
	{
		[Fact]
		public static void EquivalenceWithComparer()
		{
			var expected = new[] { 1, 2, 3, 4, 5 };
			var actual = new List<int>(new int[] { 0, 0, 0, 0, 0 });

			Assert.Equal(expected, actual, new IntComparer(true));
		}

		class IntComparer : IEqualityComparer<int>
		{
			readonly bool answer;

			public IntComparer(bool answer)
			{
				this.answer = answer;
			}

			public bool Equals(int x, int y) => answer;

			public int GetHashCode(int obj) => throw new NotImplementedException();
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

		[Fact]
		public static void EnumeratorDisposed()
		{
			var enumerator = new SpyEnumerator<int>(Enumerable.Range(0, 1));

			Assert.NotEmpty(enumerator);

			Assert.True(enumerator.IsDisposed);
		}
	}

	public class NotEqual
	{
		[Fact]
		public static void EnumerableInequivalence()
		{
			var expected = new[] { 1, 2, 3, 4, 5 };
			var actual = new List<int>(new[] { 1, 2, 3, 4, 6 });

			Assert.NotEqual(expected, actual);
		}

		[Fact]
		public static void EnumerableEquivalence()
		{
			var expected = new[] { 1, 2, 3, 4, 5 };
			var actual = new List<int>(expected);

			Assert.Throws<NotEqualException>(() => Assert.NotEqual(expected, actual));
		}
	}

	public class NotEqual_WithComparer
	{
		[Fact]
		public static void EnumerableInequivalenceWithFailedComparer()
		{
			var expected = new[] { 1, 2, 3, 4, 5 };
			var actual = new List<int>(new int[] { 1, 2, 3, 4, 5 });

			Assert.NotEqual(expected, actual, new IntComparer(false));
		}

		[Fact]
		public static void EnumerableEquivalenceWithSuccessfulComparer()
		{
			var expected = new[] { 1, 2, 3, 4, 5 };
			var actual = new List<int>(new int[] { 0, 0, 0, 0, 0 });

			Assert.Throws<NotEqualException>(() => Assert.NotEqual(expected, actual, new IntComparer(true)));
		}

		class IntComparer : IEqualityComparer<int>
		{
			readonly bool answer;

			public IntComparer(bool answer)
			{
				this.answer = answer;
			}

			public bool Equals(int x, int y) => answer;

			public int GetHashCode(int obj) => throw new NotImplementedException();
		}
	}

	public class Single_NonGeneric
	{
		[Fact]
		public static void NullCollectionThrows()
		{
			Assert.Throws<ArgumentNullException>(() => Assert.Single((IEnumerable)null!));
			Assert.Throws<ArgumentNullException>(() => Assert.Single((IEnumerable<object>)null!));
		}

		[Fact]
		public static void EmptyCollectionThrows()
		{
			var collection = new ArrayList();

			var ex = Record.Exception(() => Assert.Single(collection));

			Assert.IsType<SingleException>(ex);
			Assert.Equal("The collection was expected to contain a single element, but it was empty.", ex.Message);
		}

		[Fact]
		public static void MultiItemCollectionThrows()
		{
			var collection = new ArrayList { "Hello", "World" };

			var ex = Record.Exception(() => Assert.Single(collection));

			Assert.IsType<SingleException>(ex);
			Assert.Equal("The collection was expected to contain a single element, but it contained 2 elements.", ex.Message);
		}

		[Fact]
		public static void SingleItemCollectionDoesNotThrow()
		{
			var collection = new ArrayList { "Hello" };

			var ex = Record.Exception(() => Assert.Single(collection));

			Assert.Null(ex);
		}

		[Fact]
		public static void SingleItemCollectionReturnsTheItem()
		{
			var collection = new ArrayList { "Hello" };

			var result = Assert.Single(collection);

			Assert.Equal("Hello", result);
		}
	}

	public class Single_NonGeneric_WithObject
	{
		[Fact]
		public static void NullCollectionThrows()
		{
			Assert.Throws<ArgumentNullException>(() => Assert.Single(null!, null));
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

			var ex = Record.Exception(() => Assert.Single(collection, "foo"));

			Assert.IsType<SingleException>(ex);
			Assert.Equal("The collection was expected to contain a single element matching \"foo\", but it contained no matching elements.", ex.Message);
		}

		[Fact]
		public static void PredicateTooManyMatches()
		{
			var collection = new[] { "Hello", "World!", "Hello" };

			var ex = Record.Exception(() => Assert.Single(collection, "Hello"));

			Assert.IsType<SingleException>(ex);
			Assert.Equal("The collection was expected to contain a single element matching \"Hello\", but it contained 2 matching elements.", ex.Message);
		}
	}

	public class Single_Generic
	{
		[Fact]
		public static void NullCollectionThrows()
		{
			Assert.Throws<ArgumentNullException>(() => Assert.Single<object>(null!));
		}

		[Fact]
		public static void EmptyCollectionThrows()
		{
			var collection = new object[0];

			var ex = Record.Exception(() => Assert.Single(collection));

			Assert.IsType<SingleException>(ex);
			Assert.Equal("The collection was expected to contain a single element, but it was empty.", ex.Message);
		}

		[Fact]
		public static void MultiItemCollectionThrows()
		{
			var collection = new[] { "Hello", "World!" };

			var ex = Record.Exception(() => Assert.Single(collection));

			Assert.IsType<SingleException>(ex);
			Assert.Equal("The collection was expected to contain a single element, but it contained 2 elements.", ex.Message);
		}

		[Fact]
		public static void SingleItemCollectionDoesNotThrow()
		{
			var collection = new[] { "Hello" };

			var ex = Record.Exception(() => Assert.Single(collection));

			Assert.Null(ex);
		}

		[Fact]
		public static void SingleItemCollectionReturnsTheItem()
		{
			var collection = new[] { "Hello" };

			var result = Assert.Single(collection);

			Assert.Equal("Hello", result);
		}
	}

	public class Single_Generic_WithPredicate
	{
		[Fact]
		public static void GuardClauses()
		{
			Assert.Throws<ArgumentNullException>(() => Assert.Single<object>(null!, _ => true));
			Assert.Throws<ArgumentNullException>(() => Assert.Single<object>(new object[0], null!));
		}

		[Fact]
		public static void PredicateSingleMatch()
		{
			var collection = new[] { "Hello", "World!" };

			var result = Assert.Single(collection, item => item.StartsWith("H"));

			Assert.Equal("Hello", result);
		}

		[Fact]
		public static void PredicateNoMatch()
		{
			var collection = new[] { "Hello", "World!" };

			var ex = Record.Exception(() => Assert.Single(collection, item => false));

			Assert.IsType<SingleException>(ex);
			Assert.Equal("The collection was expected to contain a single element matching (filter expression), but it contained no matching elements.", ex.Message);
		}

		[Fact]
		public static void PredicateTooManyMatches()
		{
			var collection = new[] { "Hello", "World!" };

			var ex = Record.Exception(() => Assert.Single(collection, item => true));

			Assert.IsType<SingleException>(ex);
			Assert.Equal("The collection was expected to contain a single element matching (filter expression), but it contained 2 matching elements.", ex.Message);
		}
	}

	sealed class RunOnceEnumerable<T> : IEnumerable<T>
	{
		private readonly IEnumerable<T> _source;
		private bool _called;

		public RunOnceEnumerable(IEnumerable<T> source)
		{
			_source = source;
		}

		public IEnumerator<T> GetEnumerator()
		{
			Assert.False(_called, "GetEnumerator() was called more than once");
			_called = true;
			return _source.GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
	}

	sealed class SpyEnumerator<T> : IEnumerable<T>, IEnumerator<T>
	{
		IEnumerator<T>? innerEnumerator;

		public SpyEnumerator(IEnumerable<T> enumerable)
		{
			innerEnumerator = enumerable.GetEnumerator();
		}

		public T Current =>
			GuardNotNull("Tried to get Current on a disposed enumerator", innerEnumerator).Current;

		object? IEnumerator.Current =>
			GuardNotNull("Tried to get Current on a disposed enumerator", innerEnumerator).Current;

		public bool IsDisposed => innerEnumerator == null;

		public IEnumerator<T> GetEnumerator() => this;

		IEnumerator IEnumerable.GetEnumerator() => this;

		public bool MoveNext() =>
			GuardNotNull("Tried to call MoveNext() on a disposed enumerator", innerEnumerator).MoveNext();

		public void Reset() => throw new NotImplementedException();

		public void Dispose()
		{
			innerEnumerator?.Dispose();
			innerEnumerator = null;
		}

		/// <summary/>
		static T2 GuardNotNull<T2>(
			string message,
			[NotNull] T2? value)
				where T2 : class
		{
			if (value == null)
				throw new InvalidOperationException(message);

			return value;
		}
	}
}
