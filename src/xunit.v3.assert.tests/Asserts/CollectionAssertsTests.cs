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
		public static void GuardClauses()
		{
			Assert.Throws<ArgumentNullException>("collection", () => Assert.All<object>(null!, _ => { }));
			Assert.Throws<ArgumentNullException>("action", () => Assert.All(new object[0], (Action<object>)null!));
			Assert.Throws<ArgumentNullException>("action", () => Assert.All(new object[0], (Action<object, int>)null!));
		}

		[Fact]
		public static void Success()
		{
			var items = new[] { 1, 1, 1, 1, 1, 1 };

			Assert.All(items, x => Assert.Equal(1, x));
		}

		[Fact]
		public static void Failure()
		{
			var items = new[] { 1, 1, 42, 2112, 1, 1 };

			var ex = Record.Exception(() => Assert.All(items, item => Assert.Equal(1, item)));

			Assert.IsType<AllException>(ex);
			Assert.Equal(
				"Assert.All() Failure: 2 out of 6 items in the collection did not pass." + Environment.NewLine +
				"[2]: Item:  42" + Environment.NewLine +
				"     Error: Assert.Equal() Failure: Values differ" + Environment.NewLine +
				"            Expected: 1" + Environment.NewLine +
				"            Actual:   42" + Environment.NewLine +
				"[3]: Item:  2112" + Environment.NewLine +
				"     Error: Assert.Equal() Failure: Values differ" + Environment.NewLine +
				"            Expected: 1" + Environment.NewLine +
				"            Actual:   2112",
				ex.Message
			);
		}

		[Fact]
		public static void ActionCanReceiveIndex()
		{
			var items = new[] { 1, 1, 2, 2, 1, 1 };
			var indices = new List<int>();

			Assert.All(items, (_, idx) => indices.Add(idx));

			Assert.Equal(new[] { 0, 1, 2, 3, 4, 5 }, indices);
		}
	}

#if XUNIT_VALUETASK
	public class AllAsync
	{
		[Fact]
		public static async ValueTask GuardClauses()
		{
			await Assert.ThrowsAsync<ArgumentNullException>("collection", () => Assert.AllAsync<object>(null!, async _ => await Task.Yield()));
			await Assert.ThrowsAsync<ArgumentNullException>("action", () => Assert.AllAsync(Array.Empty<object>(), (Func<object, ValueTask>)null!));
			await Assert.ThrowsAsync<ArgumentNullException>("action", () => Assert.AllAsync(Array.Empty<object>(), (Func<object, int, ValueTask>)null!));
		}

		[Fact]
		public static async ValueTask Success()
		{
			var items = new[] { 1, 1, 1, 1, 1, 1 };

			await Assert.AllAsync(items, async item => { await Task.Yield(); Assert.Equal(1, item); });
		}

		[Fact]
		public static void Failure()
		{
			var items = new[] { 1, 1, 42, 2112, 1, 1 };

			var ex = Record.Exception(() => Assert.All(items, x => Assert.Equal(1, x)));

			Assert.IsType<AllException>(ex);
			Assert.Equal(
				"Assert.All() Failure: 2 out of 6 items in the collection did not pass." + Environment.NewLine +
				"[2]: Item:  42" + Environment.NewLine +
				"     Error: Assert.Equal() Failure: Values differ" + Environment.NewLine +
				"            Expected: 1" + Environment.NewLine +
				"            Actual:   42" + Environment.NewLine +
				"[3]: Item:  2112" + Environment.NewLine +
				"     Error: Assert.Equal() Failure: Values differ" + Environment.NewLine +
				"            Expected: 1" + Environment.NewLine +
				"            Actual:   2112",
				ex.Message
			);
		}

		[Fact]
		public static async ValueTask ActionCanReceiveIndex()
		{
			var items = new[] { 1, 1, 2, 2, 1, 1 };
			var indices = new List<int>();

			await Assert.AllAsync(items, async (_, idx) => { await Task.Yield(); indices.Add(idx); });

			Assert.Equal(new[] { 0, 1, 2, 3, 4, 5 }, indices);
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
			Assert.Equal(
				"Assert.Collection() Failure: Mismatched item count" + Environment.NewLine +
				"Collection:     []" + Environment.NewLine +
				"Expected count: 1" + Environment.NewLine +
				"Actual count:   0",
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
			Assert.Equal(
				"Assert.Collection() Failure: Item comparison failure" + Environment.NewLine +
				"                 ↓ (pos 1)" + Environment.NewLine +
				"Collection: [42, 2112]" + Environment.NewLine +
				"Error:      Assert.Equal() Failure: Values differ" + Environment.NewLine +
				"            Expected: 2113" + Environment.NewLine +
				"            Actual:   2112",
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
			Assert.Equal(
				"Assert.Collection() Failure: Mismatched item count" + Environment.NewLine +
				"Collection:     []" + Environment.NewLine +
				"Expected count: 1" + Environment.NewLine +
				"Actual count:   0",
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
			Assert.Equal(
				"Assert.Collection() Failure: Item comparison failure" + Environment.NewLine +
				"                 ↓ (pos 1)" + Environment.NewLine +
				"Collection: [42, 2112]" + Environment.NewLine +
				"Error:      Assert.Equal() Failure: Values differ" + Environment.NewLine +
				"            Expected: 2113" + Environment.NewLine +
				"            Actual:   2112",
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
			Assert.Throws<ArgumentNullException>("collection", () => Assert.Contains(14, default(IEnumerable<int>)!));
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

			var ex = Record.Exception(() => Assert.Contains(42, list));

			Assert.IsType<ContainsException>(ex);
			Assert.Equal(
				"Assert.Contains() Failure: Item not found in collection" + Environment.NewLine +
				"Collection: [41, 43]" + Environment.NewLine +
				"Not found:  42",
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
		public static void HashSetIsTreatedSpecially()
		{
			// HashSet.Contains() is a custom implementation since the comparer is passed
			// to the constructor. If this comes in via the IEnumerable<T> overload, we want
			// to make sure it still gets treated like a HashSet.
			IEnumerable<string> set = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "Hi there" };

			Assert.Contains("HI THERE", set);
		}
	}

	public class Contains_Comparer
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
		public static void HashSetConstructorComparerIsIgnored()
		{
			IEnumerable<string> set = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "Hi there" };

			var ex = Record.Exception(() => Assert.Contains("HI THERE", set, StringComparer.Ordinal));

			Assert.IsType<ContainsException>(ex);
			Assert.Equal(
				"Assert.Contains() Failure: Item not found in collection" + Environment.NewLine +
				"Collection: [\"Hi there\"]" + Environment.NewLine +
				"Not found:  \"HI THERE\"",
				ex.Message
			);
		}

		class MyComparer : IEqualityComparer<int>
		{
			public bool Equals(int x, int y) => true;

			public int GetHashCode(int obj) => throw new NotImplementedException();
		}
	}

	public class Contains_Predicate
	{
		[Fact]
		public static void GuardClauses()
		{
			Assert.Throws<ArgumentNullException>("collection", () => Assert.Contains<int>(null!, item => true));
			Assert.Throws<ArgumentNullException>("filter", () => Assert.Contains(new int[0], (Predicate<int>)null!));
		}

		[Fact]
		public static void ItemFound()
		{
			var list = new[] { "Hello", "world" };

			Assert.Contains(list, item => item.StartsWith("w"));
		}

		[Fact]
		public static void ItemNotFound()
		{
			var list = new[] { "Hello", "world" };

			var ex = Record.Exception(() => Assert.Contains(list, item => item.StartsWith("q")));

			Assert.IsType<ContainsException>(ex);
			Assert.Equal(
				"Assert.Contains() Failure: Filter not matched in collection" + Environment.NewLine +
				"Collection: [\"Hello\", \"world\"]",
				ex.Message
			);
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

			var ex = Record.Exception(() => Assert.Distinct(list));

			Assert.IsType<DistinctException>(ex);
			Assert.Equal(
				"Assert.Distinct() Failure: Duplicate item found" + Environment.NewLine +
				"Collection: [42, 42]" + Environment.NewLine +
				"Item:       42",
				ex.Message
			);
		}

		[Fact]
		public static void TwoNulls()
		{
			var list = new List<string?> { "a", null, "b", null, "c", "d" };

			var ex = Record.Exception(() => Assert.Distinct(list));

			Assert.IsType<DistinctException>(ex);
			Assert.Equal(
				"Assert.Distinct() Failure: Duplicate item found" + Environment.NewLine +
				$"Collection: [\"a\", null, \"b\", null, \"c\", {ArgumentFormatter.Ellipsis}]" + Environment.NewLine +
				"Item:       null",
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

			var ex = Record.Exception(() => Assert.Distinct(list, StringComparer.OrdinalIgnoreCase));

			Assert.IsType<DistinctException>(ex);
			Assert.Equal(
				"Assert.Distinct() Failure: Duplicate item found" + Environment.NewLine +
				"Collection: [\"a\", \"b\", \"A\"]" + Environment.NewLine +
				"Item:       \"A\"",
				ex.Message
			);
		}
	}

	public class DoesNotContain
	{
		[Fact]
		public static void GuardClause()
		{
			Assert.Throws<ArgumentNullException>("collection", () => Assert.DoesNotContain(14, default(IEnumerable<int>)!));
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

			var ex = Record.Exception(() => Assert.DoesNotContain(42, list));

			Assert.IsType<DoesNotContainException>(ex);
			Assert.Equal(
				"Assert.DoesNotContain() Failure: Item found in collection" + Environment.NewLine +
				"             ↓ (pos 0)" + Environment.NewLine +
				"Collection: [42]" + Environment.NewLine +
				"Found:      42",
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
		public static void HashSetIsTreatedSpecially()
		{
			// HashSet.Contains() is a custom implementation since the comparer is passed
			// to the constructor. If this comes in via the IEnumerable<T> overload, we want
			// to make sure it still gets treated like a HashSet.
			IEnumerable<string> set = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "Hi there" };

			var ex = Record.Exception(() => Assert.DoesNotContain("HI THERE", set));

			Assert.IsType<DoesNotContainException>(ex);
			// Note: There is no pointer for sets, unlike other collections
			Assert.Equal(
				"Assert.DoesNotContain() Failure: Item found in set" + Environment.NewLine +
				"Set:   [\"Hi there\"]" + Environment.NewLine +
				"Found: \"HI THERE\"",
				ex.Message
			);
		}
	}

	public class DoesNotContain_Comparer
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
		public static void HashSetConstructorComparerIsIgnored()
		{
			IEnumerable<string> set = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "Hi there" };

			Assert.DoesNotContain("HI THERE", set, StringComparer.Ordinal);
		}

		class MyComparer : IEqualityComparer<int>
		{
			public bool Equals(int x, int y) => false;

			public int GetHashCode(int obj) => throw new NotImplementedException();
		}
	}

	public class DoesNotContain_Predicate
	{
		[Fact]
		public static void GuardClauses()
		{
			Assert.Throws<ArgumentNullException>("collection", () => Assert.DoesNotContain((List<int>)null!, item => true));
			Assert.Throws<ArgumentNullException>("filter", () => Assert.DoesNotContain(new int[0], (Predicate<int>)null!));
		}

		[Fact]
		public static void ItemFound()
		{
			var list = new[] { "Hello", "world" };

			var ex = Record.Exception(() => Assert.DoesNotContain(list, item => item.StartsWith("w")));

			Assert.IsType<DoesNotContainException>(ex);
			Assert.Equal(
				"Assert.DoesNotContain() Failure: Filter matched in collection" + Environment.NewLine +
				"                      ↓ (pos 1)" + Environment.NewLine +
				"Collection: [\"Hello\", \"world\"]",
				ex.Message
			);
		}

		[Fact]
		public static void ItemNotFound()
		{
			var list = new[] { "Hello", "world" };

			Assert.DoesNotContain(list, item => item.StartsWith("q"));
		}
	}

	public class Empty
	{
		[Fact]
		public static void GuardClause()
		{
			Assert.Throws<ArgumentNullException>("collection", () => Assert.Empty(default(IEnumerable)!));
		}

		[Fact]
		public static void EmptyCollection()
		{
			var list = new List<int>();

			Assert.Empty(list);
		}

		[Fact]
		public static void NonEmptyCollection()
		{
			var list = new List<int> { 42 };

			var ex = Record.Exception(() => Assert.Empty(list));

			Assert.IsType<EmptyException>(ex);
			Assert.Equal(
				"Assert.Empty() Failure: Collection was not empty" + Environment.NewLine +
				"Collection: [42]",
				ex.Message
			);
		}

		[Fact]
		public static void CollectionEnumeratorDisposed()
		{
			var enumerator = new SpyEnumerator<int>(Enumerable.Empty<int>());

			Assert.Empty(enumerator);

			Assert.True(enumerator.IsDisposed);
		}
	}

	public class Equal
	{
		public class Null
		{
			[Fact]
			public static void BothNull()
			{
				var expected = default(IEnumerable<int>);
				var actual = default(IEnumerable<int>);

				Assert.Equal(expected, actual);
			}

			[Fact]
			public static void EmptyExpectedNullActual()
			{
				var expected = new int[0];
				var actual = default(IEnumerable<int>);

				var ex = Record.Exception(() => Assert.Equal(expected, actual));

				Assert.IsType<EqualException>(ex);
				Assert.Equal(
					"Assert.Equal() Failure: Collections differ" + Environment.NewLine +
					"Expected: int[] []" + Environment.NewLine +
					"Actual:         null",
					ex.Message
				);
			}

			[Fact]
			public static void NullExpectedEmptyActual()
			{
				var expected = default(IEnumerable<int>);
				var actual = new int[0];

				var ex = Record.Exception(() => Assert.Equal(expected, actual));

				Assert.IsType<EqualException>(ex);
				Assert.Equal(
					"Assert.Equal() Failure: Collections differ" + Environment.NewLine +
					"Expected:       null" + Environment.NewLine +
					"Actual:   int[] []",
					ex.Message
				);
			}
		}

		public class Arrays
		{
			[Fact]
			public static void Equal()
			{
				string[] expected = { "@", "a", "ab", "b" };
				string[] actual = { "@", "a", "ab", "b" };

				Assert.Equal(expected, actual);
			}

			[Fact]
			public static void EmbeddedArrays_Equal()
			{
				string[][] expected = { new[] { "@", "a" }, new[] { "ab", "b" } };
				string[][] actual = { new[] { "@", "a" }, new[] { "ab", "b" } };

				Assert.Equal(expected, actual);
			}

			[Theory]
			// Nulls
			[InlineData(null, new[] { 1, 2, 3 }, null, null)]
			[InlineData(new[] { 1, 2, 3 }, null, null, null)]
			// Difference at start
			[InlineData(new[] { 0, 2, 3, 4 }, new[] { 1, 2, 3, 4 }, "↓ (pos 0)", "↑ (pos 0)")]
			// Inline difference
			[InlineData(new[] { 1, 0, 3, 4 }, new[] { 1, 2, 3, 4 }, "   ↓ (pos 1)", "   ↑ (pos 1)")]
			// Difference at end
			[InlineData(new[] { 1, 2, 3, 0 }, new[] { 1, 2, 3, 4 }, "         ↓ (pos 3)", "         ↑ (pos 3)")]
			// Overruns
			[InlineData(new[] { 1, 2, 3, 4 }, new[] { 1, 2, 3, 4, 5 }, null, "            ↑ (pos 4)")]
			[InlineData(new[] { 1, 2, 3, 4, 5 }, new[] { 1, 2, 3, 4 }, "            ↓ (pos 4)", null)]
			[InlineData(new[] { 1 }, new int[0], "↓ (pos 0)", null)]
			[InlineData(new int[0], new[] { 1 }, null, "↑ (pos 0)")]
			public void NotEqual(
				int[]? expected,
				int[]? actual,
				string? expectedPointer,
				string? actualPointer)
			{
				string message = "Assert.Equal() Failure: Collections differ";

				if (expectedPointer != null)
					message += Environment.NewLine + "           " + expectedPointer;

				var (expectedType, actualType) = (expected, actual) switch
				{
					(null, _) => ("      ", "int[] "),
					(_, null) => ("int[] ", "      "),
					(_, _) => ("", ""),
				};

				message +=
					Environment.NewLine + "Expected: " + expectedType + ArgumentFormatter.Format(expected) +
					Environment.NewLine + "Actual:   " + actualType + ArgumentFormatter.Format(actual);

				if (actualPointer != null)
					message += Environment.NewLine + "           " + actualPointer;

				var ex = Record.Exception(() => Assert.Equal(expected, actual));

				Assert.IsType<EqualException>(ex);
				Assert.Equal(message, ex.Message);
			}

			[Theory]
			// Nulls
			[InlineData(
				null, null, "      null",
				new[] { 1, 2, 3, 4, 5, 6, 7 }, "int[] [1, 2, 3, 4, 5, $$ELLIPSIS$$]", null
			)]
			[InlineData(
				new[] { 1, 2, 3, 4, 5, 6, 7 }, null, "int[] [1, 2, 3, 4, 5, $$ELLIPSIS$$]",
				null, "      null", null
			)]
			// Start of array
			[InlineData(
				new[] { 1, 2, 3, 4, 5, 6, 7 }, "↓ (pos 0)", "[1, 2, 3, 4, 5, $$ELLIPSIS$$]",
				new[] { 99, 2, 3, 4, 5, 6, 7 }, "[99, 2, 3, 4, 5, $$ELLIPSIS$$]", "↑ (pos 0)"
			)]
			// Middle of array
			[InlineData(
				new[] { 1, 2, 3, 4, 5, 6, 7 }, "           ↓ (pos 3)", "[$$ELLIPSIS$$, 2, 3, 4, 5, 6, $$ELLIPSIS$$]",
				new[] { 1, 2, 3, 99, 5, 6, 7 }, "[$$ELLIPSIS$$, 2, 3, 99, 5, 6, $$ELLIPSIS$$]", "           ↑ (pos 3)"
			)]
			// End of array
			[InlineData(
				new[] { 1, 2, 3, 4, 5, 6, 7 }, "                 ↓ (pos 6)", "[$$ELLIPSIS$$, 3, 4, 5, 6, 7]",
				new[] { 1, 2, 3, 4, 5, 6, 99 }, "[$$ELLIPSIS$$, 3, 4, 5, 6, 99]", "                 ↑ (pos 6)"
			)]
			// Overruns
			[InlineData(
				new[] { 1, 2, 3, 4, 5, 6, 7 }, null, "[$$ELLIPSIS$$, 4, 5, 6, 7]",
				new[] { 1, 2, 3, 4, 5, 6, 7, 8 }, "[$$ELLIPSIS$$, 4, 5, 6, 7, 8]", "                 ↑ (pos 7)"
			)]
			[InlineData(
				new[] { 1, 2, 3, 4, 5, 6, 7 }, null, "[$$ELLIPSIS$$, 6, 7]",
				new[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12 }, "[$$ELLIPSIS$$, 6, 7, 8, 9, 10, $$ELLIPSIS$$]", "           ↑ (pos 7)"
			)]
			[InlineData(
				new[] { 1, 2, 3, 4, 5, 6, 7, 8 }, "                 ↓ (pos 7)", "[$$ELLIPSIS$$, 4, 5, 6, 7, 8]",
				new[] { 1, 2, 3, 4, 5, 6, 7 }, "[$$ELLIPSIS$$, 4, 5, 6, 7]", null
			)]
			[InlineData(
				new[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12 }, "           ↓ (pos 7)", "[$$ELLIPSIS$$, 6, 7, 8, 9, 10, $$ELLIPSIS$$]",
				new[] { 1, 2, 3, 4, 5, 6, 7 }, "[$$ELLIPSIS$$, 6, 7]", null
			)]
			public void Truncation(
				int[]? expected,
				string? expectedPointer,
				string expectedDisplay,
				int[]? actual,
				string actualDisplay,
				string? actualPointer)
			{
				var message = "Assert.Equal() Failure: Collections differ";

				if (expectedPointer != null)
					message += Environment.NewLine + "           " + expectedPointer;

				message +=
					Environment.NewLine + "Expected: " + expectedDisplay +
					Environment.NewLine + "Actual:   " + actualDisplay;

				if (actualPointer != null)
					message += Environment.NewLine + "           " + actualPointer;

				var ex = Record.Exception(() => Assert.Equal(expected, actual));

				Assert.IsType<EqualException>(ex);
				Assert.Equal(message.Replace("$$ELLIPSIS$$", ArgumentFormatter.Ellipsis), ex.Message);
			}

			[Fact]
			public void SameValueDifferentType()
			{
				var ex = Record.Exception(() => Assert.Equal(new object[] { 1, 2, 3 }, new object[] { 1, 2, 3L }));

				Assert.IsType<EqualException>(ex);
				Assert.Equal(
					"Assert.Equal() Failure: Collections differ" + Environment.NewLine +
					"                 ↓ (pos 2, type System.Int32)" + Environment.NewLine +
					"Expected: [1, 2, 3]" + Environment.NewLine +
					"Actual:   [1, 2, 3]" + Environment.NewLine +
					"                 ↑ (pos 2, type System.Int64)",
					ex.Message
				);
			}
		}

		public class Collections
		{
			[Fact]
			public static void Equal()
			{
				var expected = new[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 };
				var actual = new List<int>(expected);

				Assert.Equal(expected, actual);
			}

			[Theory]
			// Nulls
			[InlineData(
				null, null, "          null",
				new[] { 1, 2, 3, 4, 5, 6, 7 }, "List<int> [1, 2, 3, 4, 5, $$ELLIPSIS$$]", null
			)]
			[InlineData(
				new[] { 1, 2, 3, 4, 5, 6, 7 }, null, "int[] [1, 2, 3, 4, 5, $$ELLIPSIS$$]",
				null, "      null", null
			)]
			// Start of array
			[InlineData(
				new[] { 1, 2, 3, 4, 5, 6, 7 }, "           ↓ (pos 0)", "int[]     [1, 2, 3, 4, 5, $$ELLIPSIS$$]",
				new[] { 99, 2, 3, 4, 5, 6, 7 }, "List<int> [99, 2, 3, 4, 5, $$ELLIPSIS$$]", "           ↑ (pos 0)"
			)]
			// Middle of array
			[InlineData(
				new[] { 1, 2, 3, 4, 5, 6, 7 }, "                      ↓ (pos 3)", "int[]     [$$ELLIPSIS$$, 2, 3, 4, 5, 6, $$ELLIPSIS$$]",
				new[] { 1, 2, 3, 99, 5, 6, 7 }, "List<int> [$$ELLIPSIS$$, 2, 3, 99, 5, 6, $$ELLIPSIS$$]", "                      ↑ (pos 3)"
			)]
			// End of array
			[InlineData(
				new[] { 1, 2, 3, 4, 5, 6, 7 }, "                            ↓ (pos 6)", "int[]     [$$ELLIPSIS$$, 3, 4, 5, 6, 7]",
				new[] { 1, 2, 3, 4, 5, 6, 99 }, "List<int> [$$ELLIPSIS$$, 3, 4, 5, 6, 99]", "                            ↑ (pos 6)"
			)]
			// Overruns
			[InlineData(
				new[] { 1, 2, 3, 4, 5, 6, 7 }, null, "int[]     [$$ELLIPSIS$$, 4, 5, 6, 7]",
				new[] { 1, 2, 3, 4, 5, 6, 7, 8 }, "List<int> [$$ELLIPSIS$$, 4, 5, 6, 7, 8]", "                            ↑ (pos 7)"
			)]
			[InlineData(
				new[] { 1, 2, 3, 4, 5, 6, 7 }, null, "int[]     [$$ELLIPSIS$$, 6, 7]",
				new[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12 }, "List<int> [$$ELLIPSIS$$, 6, 7, 8, 9, 10, $$ELLIPSIS$$]", "                      ↑ (pos 7)"
			)]
			[InlineData(
				new[] { 1, 2, 3, 4, 5, 6, 7, 8 }, "                            ↓ (pos 7)", "int[]     [$$ELLIPSIS$$, 4, 5, 6, 7, 8]",
				new[] { 1, 2, 3, 4, 5, 6, 7 }, "List<int> [$$ELLIPSIS$$, 4, 5, 6, 7]", null
			)]
			[InlineData(
				new[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12 }, "                      ↓ (pos 7)", "int[]     [$$ELLIPSIS$$, 6, 7, 8, 9, 10, $$ELLIPSIS$$]",
				new[] { 1, 2, 3, 4, 5, 6, 7 }, "List<int> [$$ELLIPSIS$$, 6, 7]", null
			)]
			public void NotEqual(
				int[]? expected,
				string? expectedPointer,
				string expectedDisplay,
				int[]? actualArray,
				string actualDisplay,
				string? actualPointer)
			{
				var actual = actualArray == null ? null : new List<int>(actualArray);
				var message = "Assert.Equal() Failure: Collections differ";

				if (expectedPointer != null)
					message += Environment.NewLine + "          " + expectedPointer;

				message +=
					Environment.NewLine + "Expected: " + expectedDisplay +
					Environment.NewLine + "Actual:   " + actualDisplay;

				if (actualPointer != null)
					message += Environment.NewLine + "          " + actualPointer;

				var ex = Record.Exception(() => Assert.Equal(expected, actual));

				Assert.IsType<EqualException>(ex);
				Assert.Equal(message.Replace("$$ELLIPSIS$$", ArgumentFormatter.Ellipsis), ex.Message);
			}
		}

		public class CollectionsWithComparer
		{
			[Fact]
			public static void AlwaysFalse()
			{
				var expected = new[] { 1, 2, 3, 4, 5 };
				var actual = new List<int>(new int[] { 1, 2, 3, 4, 5 });

				var ex = Record.Exception(() => Assert.Equal(expected, actual, new IntComparer(false)));

				Assert.IsType<EqualException>(ex);
				Assert.Equal(
					"Assert.Equal() Failure: Collections differ" + Environment.NewLine +
					"                     ↓ (pos 0)" + Environment.NewLine +
					"Expected: int[]     [1, 2, 3, 4, 5]" + Environment.NewLine +
					"Actual:   List<int> [1, 2, 3, 4, 5]" + Environment.NewLine +
					"                     ↑ (pos 0)",
					ex.Message
				);
			}

			[Fact]
			public static void AlwaysTrue()
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

		public class CollectionsWithFunc
		{
			[Fact]
			public static void AlwaysFalse()
			{
				var expected = new[] { 1, 2, 3, 4, 5 };
				var actual = new List<int>(new int[] { 1, 2, 3, 4, 5 });

				var ex = Record.Exception(() => Assert.Equal(expected, actual, (x, y) => false));

				Assert.IsType<EqualException>(ex);
				Assert.Equal(
					"Assert.Equal() Failure: Collections differ" + Environment.NewLine +
					"                     ↓ (pos 0)" + Environment.NewLine +
					"Expected: int[]     [1, 2, 3, 4, 5]" + Environment.NewLine +
					"Actual:   List<int> [1, 2, 3, 4, 5]" + Environment.NewLine +
					"                     ↑ (pos 0)",
					ex.Message
				);
			}

			[Fact]
			public static void AlwaysTrue()
			{
				var expected = new[] { 1, 2, 3, 4, 5 };
				var actual = new List<int>(new int[] { 0, 0, 0, 0, 0 });

				Assert.Equal(expected, actual, (x, y) => true);
			}
		}

		public class Dictionaries
		{
			[Fact]
			public static void InOrderDictionary()
			{
				var expected = new Dictionary<string, int> { { "a", 1 }, { "b", 2 }, { "c", 3 } };
				var actual = new Dictionary<string, int> { { "a", 1 }, { "b", 2 }, { "c", 3 } };

				Assert.Equal(expected, actual);
			}

			[Fact]
			public static void OutOfOrderDictionary()
			{
				var expected = new Dictionary<string, int> { { "a", 1 }, { "b", 2 }, { "c", 3 } };
				var actual = new Dictionary<string, int> { { "b", 2 }, { "c", 3 }, { "a", 1 } };

				Assert.Equal(expected, actual);
			}

			[Fact]
			public static void ExpectedLarger()
			{
				var expected = new Dictionary<string, int> { { "a", 1 }, { "b", 2 }, { "c", 3 } };
				var actual = new Dictionary<string, int> { { "a", 1 }, { "b", 2 } };

				var ex = Record.Exception(() => Assert.Equal(expected, actual));

				Assert.IsType<EqualException>(ex);
				Assert.Equal(
					"Assert.Equal() Failure: Dictionaries differ" + Environment.NewLine +
					"Expected: [[\"a\"] = 1, [\"b\"] = 2, [\"c\"] = 3]" + Environment.NewLine +
					"Actual:   [[\"a\"] = 1, [\"b\"] = 2]",
					ex.Message
				);
			}

			[Fact]
			public static void ActualLarger()
			{
				var expected = new Dictionary<string, int> { { "a", 1 }, { "b", 2 } };
				var actual = new Dictionary<string, int> { { "a", 1 }, { "b", 2 }, { "c", 3 } };

				var ex = Record.Exception(() => Assert.Equal(expected, actual));

				Assert.IsType<EqualException>(ex);
				Assert.Equal(
					"Assert.Equal() Failure: Dictionaries differ" + Environment.NewLine +
					"Expected: [[\"a\"] = 1, [\"b\"] = 2]" + Environment.NewLine +
					"Actual:   [[\"a\"] = 1, [\"b\"] = 2, [\"c\"] = 3]",
					ex.Message
				);
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

				var ex = Record.Exception(() => Assert.Equal(expected, actual));

				Assert.IsType<EqualException>(ex);
				Assert.Equal(
					"Assert.Equal() Failure: Dictionaries differ" + Environment.NewLine +
					$"Expected: [[\"a\"] = 1, [\"be\"] = 2, [\"c\"] = 3, [\"d\"] = 4, [\"e\"] = 5, {ArgumentFormatter.Ellipsis}]" + Environment.NewLine +
					$"Actual:   [[\"a\"] = 1, [\"ba\"] = 2, [\"c\"] = 3, [\"d\"] = 4, [\"e\"] = 5, {ArgumentFormatter.Ellipsis}]",
					ex.Message
				);
			}
		}
	}

	public class NotEmpty
	{
		[Fact]
		public static void EmptyContainer()
		{
			var list = new List<int>();

			var ex = Record.Exception(() => Assert.NotEmpty(list));

			Assert.IsType<NotEmptyException>(ex);
			Assert.Equal(
				"Assert.NotEmpty() Failure: Collection was empty",
				ex.Message
			);
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
		public class Null
		{
			[Fact]
			public static void BothNull()
			{
				var expected = default(IEnumerable<int>);
				var actual = default(IEnumerable<int>);

				var ex = Record.Exception(() => Assert.NotEqual(expected, actual));
				Assert.IsType<NotEqualException>(ex);
				Assert.Equal(
					"Assert.NotEqual() Failure: Values are equal" + Environment.NewLine +
					"Expected: Not null" + Environment.NewLine +
					"Actual:       null",
					ex.Message
				);
			}

			[Fact]
			public static void EmptyExpectedNullActual()
			{
				var expected = new int[0];
				var actual = default(IEnumerable<int>);

				Assert.NotEqual(expected, actual);
			}

			[Fact]
			public static void NullExpectedEmptyActual()
			{
				var expected = default(IEnumerable<int>);
				var actual = new int[0];

				Assert.NotEqual(expected, actual);
			}
		}

		public class Arrays
		{
			[Fact]
			public static void Equal()
			{
				string[] expected = { "@", "a", "ab", "b" };
				string[] actual = { "@", "a", "ab", "b" };

				var ex = Record.Exception(() => Assert.NotEqual(expected, actual));
				Assert.IsType<NotEqualException>(ex);
				Assert.Equal(
					"Assert.NotEqual() Failure: Collections are equal" + Environment.NewLine +
					"Expected: Not [\"@\", \"a\", \"ab\", \"b\"]" + Environment.NewLine +
					"Actual:       [\"@\", \"a\", \"ab\", \"b\"]",
					ex.Message
				);
			}

			[Fact]
			public static void EmbeddedArrays_Equal()
			{
				string[][] expected = { new[] { "@", "a" }, new[] { "ab", "b" } };
				string[][] actual = { new[] { "@", "a" }, new[] { "ab", "b" } };

				var ex = Record.Exception(() => Assert.NotEqual(expected, actual));
				Assert.IsType<NotEqualException>(ex);
				Assert.Equal(
					"Assert.NotEqual() Failure: Collections are equal" + Environment.NewLine +
					"Expected: Not [[\"@\", \"a\"], [\"ab\", \"b\"]]" + Environment.NewLine +
					"Actual:       [[\"@\", \"a\"], [\"ab\", \"b\"]]",
					ex.Message
				);
			}

			[Fact]
			public static void NotEqual()
			{
				IEnumerable<int> expected = new[] { 1, 2, 3 };
				IEnumerable<int> actual = new[] { 1, 2, 4 };

				Assert.NotEqual(expected, actual);
			}

			[Fact]
			public static void SameValueDifferentType()
			{
				Assert.NotEqual(new object[] { 1, 2, 3 }, new object[] { 1, 2, 3L });
			}
		}

		public class Collections
		{
			[Fact]
			public static void Equal()
			{
				var expected = new[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 };
				var actual = new List<int>(expected);

				var ex = Record.Exception(() => Assert.NotEqual(expected, actual));

				Assert.IsType<NotEqualException>(ex);
				Assert.Equal(
					"Assert.NotEqual() Failure: Collections are equal" + Environment.NewLine +
					$"Expected: Not int[]     [1, 2, 3, 4, 5, {ArgumentFormatter.Ellipsis}]" + Environment.NewLine +
					$"Actual:       List<int> [1, 2, 3, 4, 5, {ArgumentFormatter.Ellipsis}]",
					ex.Message
				);
			}

			[Fact]
			public static void NotEqual()
			{
				var expected = new[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 };
				var actual = new List<int>(new[] { 1, 2, 3, 4, 0, 6, 7, 8, 9, 10 });

				Assert.NotEqual(expected, actual);
			}
		}

		public class CollectionsWithComparer
		{
			[Fact]
			public static void AlwaysFalse()
			{
				var expected = new[] { 1, 2, 3, 4, 5 };
				var actual = new List<int>(new int[] { 1, 2, 3, 4, 5 });

				Assert.NotEqual(expected, actual, new IntComparer(false));
			}

			[Fact]
			public static void AlwaysTrue()
			{
				var expected = new[] { 1, 2, 3, 4, 5 };
				var actual = new List<int>(new int[] { 0, 0, 0, 0, 0 });

				var ex = Record.Exception(() => Assert.NotEqual(expected, actual, new IntComparer(true)));

				Assert.IsType<NotEqualException>(ex);
				Assert.Equal(
					"Assert.NotEqual() Failure: Collections are equal" + Environment.NewLine +
					"Expected: Not int[]     [1, 2, 3, 4, 5]" + Environment.NewLine +
					"Actual:       List<int> [0, 0, 0, 0, 0]",
					ex.Message
				);
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

		public class CollectionsWithFunc
		{
			[Fact]
			public static void AlwaysFalse()
			{
				var expected = new[] { 1, 2, 3, 4, 5 };
				var actual = new List<int>(new int[] { 1, 2, 3, 4, 5 });

				Assert.NotEqual(expected, actual, (x, y) => false);
			}

			[Fact]
			public static void AlwaysTrue()
			{
				var expected = new[] { 1, 2, 3, 4, 5 };
				var actual = new List<int>(new int[] { 0, 0, 0, 0, 0 });

				var ex = Record.Exception(() => Assert.NotEqual(expected, actual, (x, y) => true));

				Assert.IsType<NotEqualException>(ex);
				Assert.Equal(
					"Assert.NotEqual() Failure: Collections are equal" + Environment.NewLine +
					"Expected: Not int[]     [1, 2, 3, 4, 5]" + Environment.NewLine +
					"Actual:       List<int> [0, 0, 0, 0, 0]",
					ex.Message
				);
			}
		}

		public class Dictionaries
		{
			[Fact]
			public static void InOrderDictionary()
			{
				var expected = new Dictionary<string, int> { { "a", 1 }, { "b", 2 }, { "c", 3 } };
				var actual = new Dictionary<string, int> { { "a", 1 }, { "b", 2 }, { "c", 3 } };

				var ex = Record.Exception(() => Assert.NotEqual(expected, actual));

				Assert.IsType<NotEqualException>(ex);
				Assert.Equal(
					"Assert.NotEqual() Failure: Dictionaries are equal" + Environment.NewLine +
					"Expected: Not [[\"a\"] = 1, [\"b\"] = 2, [\"c\"] = 3]" + Environment.NewLine +
					"Actual:       [[\"a\"] = 1, [\"b\"] = 2, [\"c\"] = 3]",
					ex.Message
				);
			}

			[Fact]
			public static void OutOfOrderDictionary()
			{
				var expected = new Dictionary<string, int> { { "a", 1 }, { "b", 2 }, { "c", 3 } };
				var actual = new Dictionary<string, int> { { "b", 2 }, { "c", 3 }, { "a", 1 } };

				var ex = Record.Exception(() => Assert.NotEqual(expected, actual));

				Assert.IsType<NotEqualException>(ex);
				Assert.Equal(
					"Assert.NotEqual() Failure: Dictionaries are equal" + Environment.NewLine +
					"Expected: Not [[\"a\"] = 1, [\"b\"] = 2, [\"c\"] = 3]" + Environment.NewLine +
					"Actual:       [[\"b\"] = 2, [\"c\"] = 3, [\"a\"] = 1]",
					ex.Message
				);
			}

			[Fact]
			public static void ExpectedLarger()
			{
				var expected = new Dictionary<string, int> { { "a", 1 }, { "b", 2 }, { "c", 3 } };
				var actual = new Dictionary<string, int> { { "a", 1 }, { "b", 2 } };

				Assert.NotEqual(expected, actual);
			}

			[Fact]
			public static void ActualLarger()
			{
				var expected = new Dictionary<string, int> { { "a", 1 }, { "b", 2 } };
				var actual = new Dictionary<string, int> { { "a", 1 }, { "b", 2 }, { "c", 3 } };

				Assert.NotEqual(expected, actual);
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
			}
		}
	}

	public class Single_NonGeneric
	{
		[Fact]
		public static void GuardClause()
		{
			Assert.Throws<ArgumentNullException>("collection", () => Assert.Single(null!));
		}

		[Fact]
		public static void EmptyCollection()
		{
			var collection = new ArrayList();

			var ex = Record.Exception(() => Assert.Single(collection));

			Assert.IsType<SingleException>(ex);
			Assert.Equal("Assert.Single() Failure: The collection was empty", ex.Message);
		}

		[Fact]
		public static void SingleItemCollection()
		{
			var collection = new ArrayList { "Hello" };

			var item = Assert.Single(collection);

			Assert.Equal("Hello", item);
		}

		[Fact]
		public static void MultiItemCollection()
		{
			var collection = new ArrayList { "Hello", "World" };

			var ex = Record.Exception(() => Assert.Single(collection));

			Assert.IsType<SingleException>(ex);
			Assert.Equal(
				"Assert.Single() Failure: The collection contained 2 items" + Environment.NewLine +
				"Collection: [\"Hello\", \"World\"]",
				ex.Message
			);
		}

		[Fact]
		public static void Truncation()
		{
			var collection = new ArrayList { 1, 2, 3, 4, 5, 6, 7 };

			var ex = Record.Exception(() => Assert.Single(collection));

			Assert.IsType<SingleException>(ex);
			Assert.Equal(
				"Assert.Single() Failure: The collection contained 7 items" + Environment.NewLine +
				$"Collection: [1, 2, 3, 4, 5, {ArgumentFormatter.Ellipsis}]",
				ex.Message
			);
		}
	}

	public class Single_NonGeneric_WithObject
	{
		[Fact]
		public static void GuardClause()
		{
			Assert.Throws<ArgumentNullException>("collection", () => Assert.Single(null!, null));
		}

		[Fact]
		public static void SingleMatch()
		{
			IEnumerable collection = new ArrayList { "Hello", "World" };

			Assert.Single(collection, "Hello");
		}

		[Fact]
		public static void SingleMatch_Null()
		{
			IEnumerable collection = new ArrayList { "Hello", "World!", null };

			Assert.Single(collection, null);
		}

		[Fact]
		public static void NoMatches()
		{
			IEnumerable collection = new ArrayList { "Hello", "World" };

			var ex = Record.Exception(() => Assert.Single(collection, "foo"));

			Assert.IsType<SingleException>(ex);
			Assert.Equal(
				"Assert.Single() Failure: The collection did not contain any matching items" + Environment.NewLine +
				"Expected:   \"foo\"" + Environment.NewLine +
				"Collection: [\"Hello\", \"World\"]",
				ex.Message
			);
		}

		[Fact]
		public static void TooManyMatches()
		{
			var collection = new ArrayList { "Hello", "World", "Hello" };

			var ex = Record.Exception(() => Assert.Single(collection, "Hello"));

			Assert.IsType<SingleException>(ex);
			Assert.Equal(
				"Assert.Single() Failure: The collection contained 2 matching items" + Environment.NewLine +
				"Expected:      \"Hello\"" + Environment.NewLine +
				"Collection:    [\"Hello\", \"World\", \"Hello\"]" + Environment.NewLine +
				"Match indices: 0, 2",
				ex.Message
			);
		}

		[Fact]
		public static void Truncation()
		{
			var collection = new ArrayList { 1, 2, 3, 4, 5, 6, 7, 8, 4 };

			var ex = Record.Exception(() => Assert.Single(collection, 4));

			Assert.IsType<SingleException>(ex);
			Assert.Equal(
				"Assert.Single() Failure: The collection contained 2 matching items" + Environment.NewLine +
				"Expected:      4" + Environment.NewLine +
				$"Collection:    [1, 2, 3, 4, 5, {ArgumentFormatter.Ellipsis}]" + Environment.NewLine +
				"Match indices: 3, 8",
				ex.Message
			);
		}
	}

	public class Single_Generic
	{
		[Fact]
		public static void GuardClause()
		{
			Assert.Throws<ArgumentNullException>("collection", () => Assert.Single<object>(null!));
		}

		[Fact]
		public static void EmptyCollection()
		{
			var collection = new object[0];

			var ex = Record.Exception(() => Assert.Single(collection));

			Assert.IsType<SingleException>(ex);
			Assert.Equal("Assert.Single() Failure: The collection was empty", ex.Message);
		}

		[Fact]
		public static void SingleItemCollection()
		{
			var collection = new[] { "Hello" };

			var item = Assert.Single(collection);

			Assert.Equal("Hello", item);
		}

		[Fact]
		public static void MultiItemCollection()
		{
			var collection = new[] { "Hello", "World" };

			var ex = Record.Exception(() => Assert.Single(collection));

			Assert.IsType<SingleException>(ex);
			Assert.Equal(
				"Assert.Single() Failure: The collection contained 2 items" + Environment.NewLine +
				"Collection: [\"Hello\", \"World\"]",
				ex.Message
			);
		}

		[Fact]
		public static void Truncation()
		{
			var collection = new[] { 1, 2, 3, 4, 5, 6, 7 };

			var ex = Record.Exception(() => Assert.Single(collection));

			Assert.IsType<SingleException>(ex);
			Assert.Equal(
				"Assert.Single() Failure: The collection contained 7 items" + Environment.NewLine +
				$"Collection: [1, 2, 3, 4, 5, {ArgumentFormatter.Ellipsis}]",
				ex.Message
			);
		}
	}

	public class Single_Generic_WithPredicate
	{
		[Fact]
		public static void GuardClauses()
		{
			Assert.Throws<ArgumentNullException>("collection", () => Assert.Single<object>(null!, _ => true));
			Assert.Throws<ArgumentNullException>("predicate", () => Assert.Single(new object[0], null!));
		}

		[Fact]
		public static void SingleMatch()
		{
			var collection = new[] { "Hello", "World" };

			var result = Assert.Single(collection, item => item.StartsWith("H"));

			Assert.Equal("Hello", result);
		}

		[Fact]
		public static void NoMatches()
		{
			var collection = new[] { "Hello", "World" };

			var ex = Record.Exception(() => Assert.Single(collection, item => false));

			Assert.IsType<SingleException>(ex);
			Assert.Equal(
				"Assert.Single() Failure: The collection did not contain any matching items" + Environment.NewLine +
				"Expected:   (predicate expression)" + Environment.NewLine +
				"Collection: [\"Hello\", \"World\"]",
				ex.Message
			);
		}

		[Fact]
		public static void TooManyMatches()
		{
			var collection = new[] { "Hello", "World" };

			var ex = Record.Exception(() => Assert.Single(collection, item => true));

			Assert.IsType<SingleException>(ex);
			Assert.Equal(
				"Assert.Single() Failure: The collection contained 2 matching items" + Environment.NewLine +
				"Expected:      (predicate expression)" + Environment.NewLine +
				"Collection:    [\"Hello\", \"World\"]" + Environment.NewLine +
				"Match indices: 0, 1",
				ex.Message
			);
		}

		[Fact]
		public static void Truncation()
		{
			var collection = new[] { 1, 2, 3, 4, 5, 6, 7, 8, 4 };

			var ex = Record.Exception(() => Assert.Single(collection, item => item == 4));

			Assert.IsType<SingleException>(ex);
			Assert.Equal(
				"Assert.Single() Failure: The collection contained 2 matching items" + Environment.NewLine +
				"Expected:      (predicate expression)" + Environment.NewLine +
				$"Collection:    [1, 2, 3, 4, 5, {ArgumentFormatter.Ellipsis}]" + Environment.NewLine +
				"Match indices: 3, 8",
				ex.Message
			);
		}
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
