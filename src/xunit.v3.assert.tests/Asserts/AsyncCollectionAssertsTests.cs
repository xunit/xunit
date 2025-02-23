#if NET8_0_OR_GREATER

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using NSubstitute;
using Xunit;
using Xunit.Sdk;

public class AsyncCollectionAssertsTests
{
	public class All
	{
		[Fact]
		public static void GuardClauses()
		{
			Assert.Throws<ArgumentNullException>("collection", () => Assert.All(default(IAsyncEnumerable<object>)!, _ => { }));
			Assert.Throws<ArgumentNullException>("action", () => Assert.All(new object[0].ToAsyncEnumerable(), (Action<object>)null!));
			Assert.Throws<ArgumentNullException>("action", () => Assert.All(new object[0].ToAsyncEnumerable(), (Action<object, int>)null!));
		}

		[Fact]
		public static void Success()
		{
			var items = new[] { 1, 1, 1, 1, 1, 1 }.ToAsyncEnumerable();

			Assert.All(items, x => Assert.Equal(1, x));
		}

		[Fact]
		public static void Failure()
		{
			var items = new[] { 1, 1, 42, 2112, 1, 1 }.ToAsyncEnumerable();

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

	public class AllAsync
	{
		[Fact]
		public static async Task GuardClauses()
		{
			await Assert.ThrowsAsync<ArgumentNullException>("collection", () => Assert.AllAsync(default(IAsyncEnumerable<object>)!, async _ => await Task.Yield()));
			await Assert.ThrowsAsync<ArgumentNullException>("action", () => Assert.AllAsync(new object[0].ToAsyncEnumerable(), (Func<object, Task>)null!));
			await Assert.ThrowsAsync<ArgumentNullException>("action", () => Assert.AllAsync(new object[0].ToAsyncEnumerable(), (Func<object, int, Task>)null!));
		}

		[Fact]
		public static async Task Success()
		{
			var items = new[] { 1, 1, 1, 1, 1, 1 }.ToAsyncEnumerable();

			await Assert.AllAsync(items, async item => { await Task.Yield(); Assert.Equal(1, item); });
		}

		[Fact]
		public static void Failure()
		{
			var items = new[] { 1, 1, 42, 2112, 1, 1 }.ToAsyncEnumerable();

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
		public static async Task ActionCanReceiveIndex()
		{
			var items = new[] { 1, 1, 2, 2, 1, 1 }.ToAsyncEnumerable();
			var indices = new List<int>();

			await Assert.AllAsync(items, async (_, idx) => { await Task.Yield(); indices.Add(idx); });

			Assert.Equal(new[] { 0, 1, 2, 3, 4, 5 }, indices);
		}
	}

	public class Collection
	{
		[Fact]
		public static void EmptyCollection()
		{
			var list = new List<int>().ToAsyncEnumerable();

#pragma warning disable xUnit2011 // Do not use empty collection check
			Assert.Collection(list);
#pragma warning restore xUnit2011 // Do not use empty collection check
		}

		[Fact]
		public static void MismatchedElementCount()
		{
			var list = new List<int>().ToAsyncEnumerable();

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
			var list = new List<int> { 42, 2112 }.ToAsyncEnumerable();

			Assert.Collection(list,
				item => Assert.Equal(42, item),
				item => Assert.Equal(2112, item)
			);
		}

		[Fact]
		public static void MismatchedElement()
		{
			var list = new List<int> { 42, 2112 }.ToAsyncEnumerable();

			var ex = Record.Exception(() =>
				Assert.Collection(list,
					item => Assert.Equal(42, item),
					item => Assert.Equal(2113, item)
				)
			);

			var collEx = Assert.IsType<CollectionException>(ex);
			Assert.StartsWith(
				"Assert.Collection() Failure: Item comparison failure" + Environment.NewLine +
				"                 ↓ (pos 1)" + Environment.NewLine +
				"Collection: [42, 2112]" + Environment.NewLine +
				"Error:      Assert.Equal() Failure: Values differ" + Environment.NewLine +
				"            Expected: 2113" + Environment.NewLine +
				"            Actual:   2112" + Environment.NewLine +
				"            Stack Trace:",
				ex.Message
			);
		}
	}

	public class CollectionAsync
	{
		[Fact]
		public static async Task EmptyCollection()
		{
			var list = new List<int>().ToAsyncEnumerable();

#pragma warning disable xUnit2011 // Do not use empty collection check
			await Assert.CollectionAsync(list);
#pragma warning restore xUnit2011 // Do not use empty collection check
		}

		[Fact]
		public static async Task MismatchedElementCountAsync()
		{
			var list = new List<int>().ToAsyncEnumerable();

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
		public static async Task NonEmptyCollectionAsync()
		{
			var list = new List<int> { 42, 2112 }.ToAsyncEnumerable();

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
		public static async Task MismatchedElementAsync()
		{
			var list = new List<int> { 42, 2112 }.ToAsyncEnumerable();

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
			Assert.StartsWith(
				"Assert.Collection() Failure: Item comparison failure" + Environment.NewLine +
				"                 ↓ (pos 1)" + Environment.NewLine +
				"Collection: [42, 2112]" + Environment.NewLine +
				"Error:      Assert.Equal() Failure: Values differ" + Environment.NewLine +
				"            Expected: 2113" + Environment.NewLine +
				"            Actual:   2112" + Environment.NewLine +
				"            Stack Trace:",
				ex.Message
			);
		}
	}

	public class Contains
	{
		[Fact]
		public static void GuardClause()
		{
			Assert.Throws<ArgumentNullException>("collection", () => Assert.Contains(14, default(IAsyncEnumerable<int>)!));
		}

		[Fact]
		public static void CanFindNullInContainer()
		{
			var list = new List<object?> { 16, null, "Hi there" }.ToAsyncEnumerable();

			Assert.Contains(null, list);
		}

		[Fact]
		public static void ItemInContainer()
		{
			var list = new List<int> { 42 }.ToAsyncEnumerable();

			Assert.Contains(42, list);
		}

		[Fact]
		public static void ItemNotInContainer()
		{
			var list = new List<int> { 41, 43 }.ToAsyncEnumerable();

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
			var list = new List<object?> { null, 16, "Hi there" }.ToAsyncEnumerable();

			Assert.Contains("Hi there", list);
		}
	}

	public class Contains_Comparer
	{
		[Fact]
		public static void GuardClauses()
		{
			var comparer = Substitute.For<IEqualityComparer<int>>();

			Assert.Throws<ArgumentNullException>("collection", () => Assert.Contains(14, default(IAsyncEnumerable<int>)!, comparer));
			Assert.Throws<ArgumentNullException>("comparer", () => Assert.Contains(14, new int[0].ToAsyncEnumerable(), null!));
		}

		[Fact]
		public static void CanUseComparer()
		{
			var list = new List<int> { 42 }.ToAsyncEnumerable();

			Assert.Contains(43, list, new MyComparer());
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
			Assert.Throws<ArgumentNullException>("collection", () => Assert.Contains(default(IAsyncEnumerable<int>)!, item => true));
			Assert.Throws<ArgumentNullException>("filter", () => Assert.Contains(new int[0].ToAsyncEnumerable(), (Predicate<int>)null!));
		}

		[Fact]
		public static void ItemFound()
		{
			var list = new[] { "Hello", "world" }.ToAsyncEnumerable();

			Assert.Contains(list, item => item.StartsWith("w"));
		}

		[Fact]
		public static void ItemNotFound()
		{
			var list = new[] { "Hello", "world" }.ToAsyncEnumerable();

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
			Assert.Throws<ArgumentNullException>("collection", () => Assert.Distinct(default(IAsyncEnumerable<int>)!));
			Assert.Throws<ArgumentNullException>("comparer", () => Assert.Distinct(new object[0].ToAsyncEnumerable(), null!));
		}

		[Fact]
		public static void WithNull()
		{
			var list = new List<object?> { 16, "Hi there", null }.ToAsyncEnumerable();

			Assert.Distinct(list);
		}

		[Fact]
		public static void TwoItems()
		{
			var list = new List<int> { 42, 42 }.ToAsyncEnumerable();

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
			var list = new List<string?> { "a", null, "b", null, "c", "d" }.ToAsyncEnumerable();

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
			var list = new string[] { "a", "b", "A" }.ToAsyncEnumerable();

			Assert.Distinct(list);
			Assert.Distinct(list, StringComparer.Ordinal);
		}

		[Fact]
		public static void CaseInsensitiveStrings()
		{
			var list = new string[] { "a", "b", "A" }.ToAsyncEnumerable();

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
			Assert.Throws<ArgumentNullException>("collection", () => Assert.DoesNotContain(14, default(IAsyncEnumerable<int>)!));
		}

		[Fact]
		public static void CanSearchForNullInContainer()
		{
			var list = new List<object?> { 16, "Hi there" }.ToAsyncEnumerable();

			Assert.DoesNotContain(null, list);
		}

		[Fact]
		public static void ItemInContainer()
		{
			var list = new List<int> { 42 }.ToAsyncEnumerable();

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
			var list = new List<int>().ToAsyncEnumerable();

			Assert.DoesNotContain(42, list);
		}

		[Fact]
		public static void NullsAllowedInContainer()
		{
			var list = new List<object?> { null, 16, "Hi there" }.ToAsyncEnumerable();

			Assert.DoesNotContain(42, list);
		}
	}

	public class DoesNotContain_Comparer
	{
		[Fact]
		public static void GuardClauses()
		{
			var comparer = Substitute.For<IEqualityComparer<int>>();

			Assert.Throws<ArgumentNullException>("collection", () => Assert.DoesNotContain(14, default(IAsyncEnumerable<int>)!, comparer));
			Assert.Throws<ArgumentNullException>("comparer", () => Assert.DoesNotContain(14, new int[0].ToAsyncEnumerable(), null!));
		}

		[Fact]
		public static void CanUseComparer()
		{
			var list = new List<int> { 42 }.ToAsyncEnumerable();

			Assert.DoesNotContain(42, list, new MyComparer());
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
			Assert.Throws<ArgumentNullException>("collection", () => Assert.DoesNotContain(default(IAsyncEnumerable<int>)!, item => true));
			Assert.Throws<ArgumentNullException>("filter", () => Assert.DoesNotContain(new int[0].ToAsyncEnumerable(), (Predicate<int>)null!));
		}

		[Fact]
		public static void ItemFound()
		{
			var list = new[] { "Hello", "world" }.ToAsyncEnumerable();

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
			var list = new[] { "Hello", "world" }.ToAsyncEnumerable();

			Assert.DoesNotContain(list, item => item.StartsWith("q"));
		}
	}

	public class Empty
	{
		[Fact]
		public static void GuardClause()
		{
			Assert.Throws<ArgumentNullException>("collection", () => Assert.Empty(default(IAsyncEnumerable<int>)!));
		}

		[Fact]
		public static void EmptyCollection()
		{
			var list = new List<int>().ToAsyncEnumerable();

			Assert.Empty(list);
		}

		[Fact]
		public static void NonEmptyCollection()
		{
			var list = new List<int> { 42 }.ToAsyncEnumerable();

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
			var enumerator = new SpyEnumerator<int>(Enumerable.Empty<int>().ToAsyncEnumerable());

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
				var nullEnumerable = default(IEnumerable<int>);
				var nullAsyncEnumerable = default(IAsyncEnumerable<int>);

				Assert.Equal(nullEnumerable, nullAsyncEnumerable);
				Assert.Equal(nullAsyncEnumerable, nullAsyncEnumerable);
			}

			[Fact]
			public static void EmptyExpectedNullActual()
			{
				var expected = new int[0];
				var actual = default(IAsyncEnumerable<int>);

				void validateError(
					Action action,
					string expectedType)
				{
					var ex = Record.Exception(action);

					Assert.IsType<EqualException>(ex);
					Assert.Equal(
						"Assert.Equal() Failure: Collections differ" + Environment.NewLine +
						"Expected: " + expectedType + " []" + Environment.NewLine +
						"Actual:   " + new string(' ', expectedType.Length) + " null",
						ex.Message
					);
				}

				validateError(() => Assert.Equal(expected, actual), "int[]");
				validateError(() => Assert.Equal(expected.ToAsyncEnumerable(), actual), "<generated>");
			}

			[Fact]
			public static void NullExpectedEmptyActual()
			{
				var actual = new int[0].ToAsyncEnumerable();

				void validateError(Action action)
				{
					var ex = Record.Exception(action);

					Assert.IsType<EqualException>(ex);
					Assert.Equal(
						"Assert.Equal() Failure: Collections differ" + Environment.NewLine +
						"Expected:             null" + Environment.NewLine +
						"Actual:   <generated> []",
						ex.Message
					);
				}

				validateError(() => Assert.Equal(default(IEnumerable<int>), actual));
				validateError(() => Assert.Equal(default(IAsyncEnumerable<int>), actual));
			}
		}

		public class Collections
		{
			[Fact]
			public static void Equal()
			{
				var expected = new[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 };
				var actual = expected.ToAsyncEnumerable();

				Assert.Equal(expected, actual);
				Assert.Equal(expected.ToAsyncEnumerable(), actual);
			}

			[Theory]
			// Nulls
			[InlineData(
				null, null, "null",
				new[] { 1, 2, 3, 4, 5, 6, 7 }, "[1, 2, 3, 4, 5, $$ELLIPSIS$$]", null
			)]
			[InlineData(
				new[] { 1, 2, 3, 4, 5, 6, 7 }, null, "[1, 2, 3, 4, 5, $$ELLIPSIS$$]",
				null, "null", null
			)]
			// Start of array
			[InlineData(
				new[] { 1, 2, 3, 4, 5, 6, 7 }, " ↓ (pos 0)", "[1, 2, 3, 4, 5, $$ELLIPSIS$$]",
				new[] { 99, 2, 3, 4, 5, 6, 7 }, "[99, 2, 3, 4, 5, $$ELLIPSIS$$]", " ↑ (pos 0)"
			)]
			// Middle of array
			[InlineData(
				new[] { 1, 2, 3, 4, 5, 6, 7 }, "            ↓ (pos 3)", "[$$ELLIPSIS$$, 2, 3, 4, 5, 6, $$ELLIPSIS$$]",
				new[] { 1, 2, 3, 99, 5, 6, 7 }, "[$$ELLIPSIS$$, 2, 3, 99, 5, 6, $$ELLIPSIS$$]", "            ↑ (pos 3)"
			)]
			// End of array
			[InlineData(
				new[] { 1, 2, 3, 4, 5, 6, 7 }, "                  ↓ (pos 6)", "[$$ELLIPSIS$$, 3, 4, 5, 6, 7]",
				new[] { 1, 2, 3, 4, 5, 6, 99 }, "[$$ELLIPSIS$$, 3, 4, 5, 6, 99]", "                  ↑ (pos 6)"
			)]
			//// Overruns
			[InlineData(
				new[] { 1, 2, 3, 4, 5, 6, 7 }, null, "[$$ELLIPSIS$$, 4, 5, 6, 7]",
				new[] { 1, 2, 3, 4, 5, 6, 7, 8 }, "[$$ELLIPSIS$$, 4, 5, 6, 7, 8]", "                  ↑ (pos 7)"
			)]
			[InlineData(
				new[] { 1, 2, 3, 4, 5, 6, 7 }, null, "[$$ELLIPSIS$$, 6, 7]",
				new[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12 }, "[$$ELLIPSIS$$, 6, 7, 8, 9, 10, $$ELLIPSIS$$]", "            ↑ (pos 7)"
			)]
			[InlineData(
				new[] { 1, 2, 3, 4, 5, 6, 7, 8 }, "                  ↓ (pos 7)", "[$$ELLIPSIS$$, 4, 5, 6, 7, 8]",
				new[] { 1, 2, 3, 4, 5, 6, 7 }, "[$$ELLIPSIS$$, 4, 5, 6, 7]", null
			)]
			[InlineData(
				new[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12 }, "            ↓ (pos 7)", "[$$ELLIPSIS$$, 6, 7, 8, 9, 10, $$ELLIPSIS$$]",
				new[] { 1, 2, 3, 4, 5, 6, 7 }, "[$$ELLIPSIS$$, 6, 7]", null
			)]
			public void NotEqual(
				int[]? expected,
				string? expectedPointer,
				string expectedDisplay,
				int[]? actualArray,
				string actualDisplay,
				string? actualPointer)
			{
				var actual = actualArray is null ? null : new List<int>(actualArray).ToAsyncEnumerable();

				void validateError(
					Action action,
					string expectedType)
				{
					var message = "Assert.Equal() Failure: Collections differ";
					var actualType = actualArray is null ? "" : "<generated> ";

					if (actualType == expectedType)
					{
						actualType = "";
						expectedType = "";
					}

					var padding = Math.Max(expectedType.Length, actualType.Length);
					var paddingBlanks = new string(' ', padding);

					if (expectedPointer is not null)
						message += Environment.NewLine + "          " + paddingBlanks + expectedPointer;

					message +=
						Environment.NewLine + "Expected: " + expectedType.PadRight(padding) + expectedDisplay +
						Environment.NewLine + "Actual:   " + actualType.PadRight(padding) + actualDisplay;

					if (actualPointer is not null)
						message += Environment.NewLine + "          " + paddingBlanks + actualPointer;

					var ex = Record.Exception(action);

					Assert.IsType<EqualException>(ex);
					Assert.Equal(message.Replace("$$ELLIPSIS$$", ArgumentFormatter.Ellipsis), ex.Message);
				}

				validateError(() => Assert.Equal(expected, actual), expected is null ? "" : "int[] ");
				validateError(() => Assert.Equal(expected?.ToAsyncEnumerable(), actual), expected is null ? "" : "<generated> ");
			}
		}

		public class CollectionsWithComparer
		{
			[Fact]
			public static void AlwaysFalse()
			{
				var expected = new[] { 1, 2, 3, 4, 5 };
				var actual = expected.ToAsyncEnumerable();

				void validateError(
					Action action,
					string expectedType,
					string actualType)
				{
					var ex = Record.Exception(action);
					var padding = Math.Max(expectedType.Length, actualType.Length);

					Assert.IsType<EqualException>(ex);
					Assert.Equal(
						"Assert.Equal() Failure: Collections differ" + Environment.NewLine +
						"          " + new string(' ', padding) + " ↓ (pos 0)" + Environment.NewLine +
						"Expected: " + expectedType.PadRight(padding) + "[1, 2, 3, 4, 5]" + Environment.NewLine +
						"Actual:   " + actualType.PadRight(padding) + "[1, 2, 3, 4, 5]" + Environment.NewLine +
						"          " + new string(' ', padding) + " ↑ (pos 0)",
						ex.Message
					);
				}

				validateError(() => Assert.Equal(expected, actual, new IntComparer(false)), "int[] ", "<generated> ");
				validateError(() => Assert.Equal(expected.ToAsyncEnumerable(), actual, new IntComparer(false)), "", "");
			}

			[Fact]
			public static void AlwaysTrue()
			{
				var expected = new[] { 1, 2, 3, 4, 5 };
				var actual = new int[] { 0, 0, 0, 0, 0 }.ToAsyncEnumerable();

				Assert.Equal(expected, actual, new IntComparer(true));
				Assert.Equal(expected.ToAsyncEnumerable(), actual, new IntComparer(true));
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

			// https://github.com/xunit/xunit/issues/2795
			[Fact]
			public void CollectionItemIsEnumerable()
			{
				List<EnumerableItem> actual = new List<EnumerableItem> { new(0), new(2) };
				List<EnumerableItem> expected = new List<EnumerableItem> { new(1), new(3) };

				Assert.Equal(expected, actual.ToAsyncEnumerable(), new EnumerableItemComparer());
				Assert.Equal(expected.ToAsyncEnumerable(), actual.ToAsyncEnumerable(), new EnumerableItemComparer());
			}

			public class EnumerableItemComparer : IEqualityComparer<EnumerableItem>
			{
				public bool Equals(EnumerableItem? x, EnumerableItem? y) =>
					x?.Value / 2 == y?.Value / 2;

				public int GetHashCode(EnumerableItem obj) =>
					throw new NotImplementedException();
			}

			public sealed class EnumerableItem : IEnumerable<string>
			{
				public int Value { get; }

				public EnumerableItem(int value) => Value = value;

				public IEnumerator<string> GetEnumerator() => Enumerable.Repeat("", Value).GetEnumerator();

				IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
			}

			[Fact]
			public void WithThrow_PrintsPointerWhereThrowOccurs_RecordsInnerException()
			{
				void validateError(
					Action action,
					string expectedType,
					string actualType)
				{
					var ex = Record.Exception(action);
					var padding = Math.Max(expectedType.Length, actualType.Length);

					Assert.IsType<EqualException>(ex);
					Assert.Equal(
						"Assert.Equal() Failure: Exception thrown during comparison" + Environment.NewLine +
						"          " + new string(' ', padding) + " ↓ (pos 0)" + Environment.NewLine +
						"Expected: " + expectedType.PadRight(padding) + "[1, 2]" + Environment.NewLine +
						"Actual:   " + actualType.PadRight(padding) + "[1, 3]" + Environment.NewLine +
						"          " + new string(' ', padding) + " ↑ (pos 0)",
						ex.Message
					);
					Assert.IsType<DivideByZeroException>(ex.InnerException);
				}

				validateError(() => Assert.Equal(new[] { 1, 2 }, new[] { 1, 3 }.ToAsyncEnumerable(), new ThrowingComparer()), "int[] ", "<generated> ");
				validateError(() => Assert.Equal(new[] { 1, 2 }.ToAsyncEnumerable(), new[] { 1, 3 }.ToAsyncEnumerable(), new ThrowingComparer()), "", "");
			}

			public class ThrowingComparer : IEqualityComparer<int>
			{
				public bool Equals(int x, int y) =>
					throw new DivideByZeroException();

				public int GetHashCode(int obj) =>
					throw new NotImplementedException();
			}
		}

		public class CollectionsWithEquatable
		{
			[Fact]
			public void Equal()
			{
				var expected = new[] { new EquatableObject { Char = 'a' } };
				var actual = new[] { new EquatableObject { Char = 'a' } }.ToAsyncEnumerable();

				Assert.Equal(expected, actual);
				Assert.Equal(expected.ToAsyncEnumerable(), actual);
			}

			[Fact]
			public void NotEqual()
			{
				var expected = new[] { new EquatableObject { Char = 'a' } };
				var actual = new[] { new EquatableObject { Char = 'b' } }.ToAsyncEnumerable();

				void validateError(
					Action action,
					string expectedType,
					string actualType)
				{
					var ex = Record.Exception(action);
					var padding = Math.Max(expectedType.Length, actualType.Length);

					Assert.IsType<EqualException>(ex);
					Assert.Equal(
						"Assert.Equal() Failure: Collections differ" + Environment.NewLine +
						"          " + new string(' ', padding) + " ↓ (pos 0)" + Environment.NewLine +
						"Expected: " + expectedType.PadRight(padding) + "[EquatableObject { Char = 'a' }]" + Environment.NewLine +
						"Actual:   " + actualType.PadRight(padding) + "[EquatableObject { Char = 'b' }]" + Environment.NewLine +
						"          " + new string(' ', padding) + " ↑ (pos 0)",
						ex.Message
					);
				}

				validateError(() => Assert.Equal(expected, actual), "EquatableObject[] ", "<generated> ");
				validateError(() => Assert.Equal(expected.ToAsyncEnumerable(), actual), "", "");
			}

			public class EquatableObject : IEquatable<EquatableObject>
			{
				public char Char { get; set; }

				public bool Equals(EquatableObject? other) =>
					other != null && other.Char == Char;
			}
		}

		public class CollectionsWithFunc
		{
			[Fact]
			public static void AlwaysFalse()
			{
				var expected = new[] { 1, 2, 3, 4, 5 };
				var actual = new List<int>(new int[] { 1, 2, 3, 4, 5 }).ToAsyncEnumerable();

				void validateError(
					Action action,
					string expectedType,
					string actualType)
				{
					var ex = Record.Exception(action);
					var padding = Math.Max(expectedType.Length, actualType.Length);

					Assert.IsType<EqualException>(ex);
					Assert.Equal(
						"Assert.Equal() Failure: Collections differ" + Environment.NewLine +
						"          " + new string(' ', padding) + " ↓ (pos 0)" + Environment.NewLine +
						"Expected: " + expectedType.PadRight(padding) + "[1, 2, 3, 4, 5]" + Environment.NewLine +
						"Actual:   " + actualType.PadRight(padding) + "[1, 2, 3, 4, 5]" + Environment.NewLine +
						"          " + new string(' ', padding) + " ↑ (pos 0)",
						ex.Message
					);
				}

				validateError(() => Assert.Equal(expected, actual, (x, y) => false), "int[] ", "<generated> ");
				validateError(() => Assert.Equal(expected.ToAsyncEnumerable(), actual, (int x, int y) => false), "", "");
			}

			[Fact]
			public static void AlwaysTrue()
			{
				var expected = new[] { 1, 2, 3, 4, 5 };
				var actual = new List<int>(new int[] { 0, 0, 0, 0, 0 }).ToAsyncEnumerable();

				Assert.Equal(expected, actual, (x, y) => true);
				Assert.Equal(expected.ToAsyncEnumerable(), actual, (int x, int y) => true);
			}

			// https://github.com/xunit/xunit/issues/2795
			[Fact]
			public void CollectionItemIsEnumerable()
			{
				var expected = new List<EnumerableItem> { new(1), new(3) };
				var actual = new List<EnumerableItem> { new(0), new(2) }.ToAsyncEnumerable();

				Assert.Equal(expected, actual, (x, y) => x.Value / 2 == y.Value / 2);
				Assert.Equal(expected.ToAsyncEnumerable(), actual, (x, y) => x.Value / 2 == y.Value / 2);
			}

			public sealed class EnumerableItem : IEnumerable<string>
			{
				public int Value { get; }

				public EnumerableItem(int value) => Value = value;

				public IEnumerator<string> GetEnumerator() => Enumerable.Repeat("", Value).GetEnumerator();

				IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
			}

			[Fact]
			public void WithThrow_PrintsPointerWhereThrowOccurs_RecordsInnerException()
			{
				var expected = new[] { 1, 2 };
				var actual = new[] { 1, 3 }.ToAsyncEnumerable();

				void validateError(
					Action action,
					string expectedType,
					string actualType)
				{
					var ex = Record.Exception(action);
					var padding = Math.Max(expectedType.Length, actualType.Length);

					Assert.IsType<EqualException>(ex);
					Assert.Equal(
						"Assert.Equal() Failure: Exception thrown during comparison" + Environment.NewLine +
						"          " + new string(' ', padding) + " ↓ (pos 0)" + Environment.NewLine +
						"Expected: " + expectedType.PadRight(padding) + "[1, 2]" + Environment.NewLine +
						"Actual:   " + actualType.PadRight(padding) + "[1, 3]" + Environment.NewLine +
						"          " + new string(' ', padding) + " ↑ (pos 0)",
						ex.Message
					);
					Assert.IsType<DivideByZeroException>(ex.InnerException);
				}

				validateError(() => Assert.Equal(expected, actual, (int e, int a) => throw new DivideByZeroException()), "int[] ", "<generated> ");
				validateError(() => Assert.Equal(expected.ToAsyncEnumerable(), actual, (int e, int a) => throw new DivideByZeroException()), "", "");
			}
		}
	}

	public class NotEmpty
	{
		[Fact]
		public static void EmptyContainer()
		{
			var list = new List<int>().ToAsyncEnumerable();

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
			var list = new List<int> { 42 }.ToAsyncEnumerable();

			Assert.NotEmpty(list);
		}

		[Fact]
		public static void EnumeratorDisposed()
		{
			var enumerator = new SpyEnumerator<int>(Enumerable.Range(0, 1).ToAsyncEnumerable());

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
				var nullEnumerable = default(IEnumerable<int>);
				var nullAsyncEnumerable = default(IAsyncEnumerable<int>);

				void validateError(Action action)
				{
					var ex = Record.Exception(action);
					Assert.IsType<NotEqualException>(ex);
					Assert.Equal(
						"Assert.NotEqual() Failure: Values are equal" + Environment.NewLine +
						"Expected: Not null" + Environment.NewLine +
						"Actual:       null",
						ex.Message
					);
				}

				validateError(() => Assert.NotEqual(nullEnumerable, nullAsyncEnumerable));
				validateError(() => Assert.NotEqual(nullAsyncEnumerable, nullAsyncEnumerable));
			}

			[Fact]
			public static void EmptyExpectedNullActual()
			{
				var expected = new int[0];
				var actual = default(IAsyncEnumerable<int>);

				Assert.NotEqual(expected, actual);
				Assert.NotEqual(expected.ToAsyncEnumerable(), actual);
			}

			[Fact]
			public static void NullExpectedEmptyActual()
			{
				var actual = new int[0].ToAsyncEnumerable();

				Assert.NotEqual(default(IEnumerable<int>), actual);
				Assert.NotEqual(default(IAsyncEnumerable<int>), actual);
			}
		}

		public class Collections
		{
			[Fact]
			public static void Equal()
			{
				var expected = new[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 };
				var actual = new List<int>(expected).ToAsyncEnumerable();

				void validateError(
					Action action,
					string expectedType,
					string actualType)
				{
					var ex = Record.Exception(action);
					var padding = Math.Max(expectedType.Length, actualType.Length);

					Assert.IsType<NotEqualException>(ex);
					Assert.Equal(
						"Assert.NotEqual() Failure: Collections are equal" + Environment.NewLine +
						$"Expected: Not {expectedType.PadRight(padding)}[1, 2, 3, 4, 5, {ArgumentFormatter.Ellipsis}]" + Environment.NewLine +
						$"Actual:       {actualType.PadRight(padding)}[1, 2, 3, 4, 5, {ArgumentFormatter.Ellipsis}]",
						ex.Message
					);
				}

				validateError(() => Assert.NotEqual(expected, actual), "int[] ", "<generated> ");
				validateError(() => Assert.NotEqual(expected.ToAsyncEnumerable(), actual), "", "");
			}

			[Fact]
			public static void NotEqual()
			{
				var expected = new[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 };
				var actual = new List<int>(new[] { 1, 2, 3, 4, 0, 6, 7, 8, 9, 10 }).ToAsyncEnumerable();

				Assert.NotEqual(expected, actual);
				Assert.NotEqual(expected.ToAsyncEnumerable(), actual);
			}
		}

		public class CollectionsWithComparer
		{
			[Fact]
			public static void AlwaysFalse()
			{
				var expected = new[] { 1, 2, 3, 4, 5 };
				var actual = new List<int>(new int[] { 1, 2, 3, 4, 5 }).ToAsyncEnumerable();

				Assert.NotEqual(expected, actual, new IntComparer(false));
				Assert.NotEqual(expected.ToAsyncEnumerable(), actual, new IntComparer(false));
			}

			[Fact]
			public static void AlwaysTrue()
			{
				var expected = new[] { 1, 2, 3, 4, 5 };
				var actual = new List<int>(new int[] { 0, 0, 0, 0, 0 }).ToAsyncEnumerable();

				void validateError(
					Action action,
					string expectedType,
					string actualType)
				{
					var ex = Record.Exception(action);
					var padding = Math.Max(expectedType.Length, actualType.Length);

					Assert.IsType<NotEqualException>(ex);
					Assert.Equal(
						"Assert.NotEqual() Failure: Collections are equal" + Environment.NewLine +
						"Expected: Not " + expectedType.PadRight(padding) + "[1, 2, 3, 4, 5]" + Environment.NewLine +
						"Actual:       " + actualType.PadRight(padding) + "[0, 0, 0, 0, 0]",
						ex.Message
					);
				}

				validateError(() => Assert.NotEqual(expected, actual, new IntComparer(true)), "int[] ", "<generated> ");
				validateError(() => Assert.NotEqual(expected.ToAsyncEnumerable(), actual, new IntComparer(true)), "", "");
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

			[Fact]
			public void WithThrow_PrintsPointerWhereThrowOccurs_RecordsInnerException()
			{
				var expected = new[] { 1, 2 };
				var actual = new[] { 1, 2 }.ToAsyncEnumerable();

				void validateError(
					Action action,
					string expectedType,
					string actualType)
				{
					var ex = Record.Exception(action);
					var padding = Math.Max(expectedType.Length, actualType.Length);

					Assert.IsType<NotEqualException>(ex);
					Assert.Equal(
						"Assert.NotEqual() Failure: Exception thrown during comparison" + Environment.NewLine +
						"              " + new string(' ', padding) + " ↓ (pos 0)" + Environment.NewLine +
						"Expected: Not " + expectedType.PadRight(padding) + "[1, 2]" + Environment.NewLine +
						"Actual:       " + actualType.PadRight(padding) + "[1, 2]" + Environment.NewLine +
						"              " + new string(' ', padding) + " ↑ (pos 0)",
						ex.Message
					);
					Assert.IsType<DivideByZeroException>(ex.InnerException);
				}

				validateError(() => Assert.NotEqual(expected, actual, new ThrowingComparer()), "int[] ", "<generated> ");
				validateError(() => Assert.NotEqual(expected.ToAsyncEnumerable(), actual, new ThrowingComparer()), "", "");
			}

			public class ThrowingComparer : IEqualityComparer<int>
			{
				public bool Equals(int x, int y) =>
					throw new DivideByZeroException();

				public int GetHashCode(int obj) =>
					throw new NotImplementedException();
			}
		}

		public class CollectionsWithEquatable
		{
			[Fact]
			public void Equal()
			{
				var expected = new[] { new EquatableObject { Char = 'a' } };
				var actual = new[] { new EquatableObject { Char = 'a' } }.ToAsyncEnumerable();

				void validateError(
					Action action,
					string expectedType,
					string actualType)
				{
					var ex = Record.Exception(action);
					var padding = Math.Max(expectedType.Length, actualType.Length);

					Assert.IsType<NotEqualException>(ex);
					Assert.Equal(
						"Assert.NotEqual() Failure: Collections are equal" + Environment.NewLine +
						"Expected: Not " + expectedType.PadRight(padding) + "[EquatableObject { Char = 'a' }]" + Environment.NewLine +
						"Actual:       " + actualType.PadRight(padding) + "[EquatableObject { Char = 'a' }]",
						ex.Message
					);
				}

				validateError(() => Assert.NotEqual(expected, actual), "EquatableObject[] ", "<generated> ");
				validateError(() => Assert.NotEqual(expected.ToAsyncEnumerable(), actual), "", "");
			}

			[Fact]
			public void NotEqual()
			{
				var expected = new[] { new EquatableObject { Char = 'a' } };
				var actual = new[] { new EquatableObject { Char = 'b' } }.ToAsyncEnumerable();

				Assert.NotEqual(expected, actual);
				Assert.NotEqual(expected.ToAsyncEnumerable(), actual);
			}

			public class EquatableObject : IEquatable<EquatableObject>
			{
				public char Char { get; set; }

				public bool Equals(EquatableObject? other) =>
					other != null && other.Char == Char;
			}
		}

		public class CollectionsWithFunc
		{
			[Fact]
			public static void AlwaysFalse()
			{
				var expected = new[] { 1, 2, 3, 4, 5 };
				var actual = new List<int>(new int[] { 1, 2, 3, 4, 5 }).ToAsyncEnumerable();

				Assert.NotEqual(expected, actual, (x, y) => false);
				Assert.NotEqual(expected.ToAsyncEnumerable(), actual, (int x, int y) => false);
			}

			[Fact]
			public static void AlwaysTrue()
			{
				var expected = new[] { 1, 2, 3, 4, 5 };
				var actual = new List<int>(new int[] { 0, 0, 0, 0, 0 }).ToAsyncEnumerable();

				void validateError(
					Action action,
					string expectedType,
					string actualType)
				{
					var ex = Record.Exception(action);
					var padding = Math.Max(expectedType.Length, actualType.Length);

					Assert.IsType<NotEqualException>(ex);
					Assert.Equal(
						"Assert.NotEqual() Failure: Collections are equal" + Environment.NewLine +
						"Expected: Not " + expectedType.PadRight(padding) + "[1, 2, 3, 4, 5]" + Environment.NewLine +
						"Actual:       " + actualType.PadRight(padding) + "[0, 0, 0, 0, 0]",
						ex.Message
					);
				}

				validateError(() => Assert.NotEqual(expected, actual, (x, y) => true), "int[] ", "<generated> ");
				validateError(() => Assert.NotEqual(expected.ToAsyncEnumerable(), actual, (int x, int y) => true), "", "");
			}

			[Fact]
			public void WithThrow_PrintsPointerWhereThrowOccurs_RecordsInnerException()
			{
				var expected = new[] { 1, 2 };
				var actual = new[] { 1, 2 }.ToAsyncEnumerable();

				void validateError(
					Action action,
					string expectedType,
					string actualType)
				{
					var ex = Record.Exception(action);
					var padding = Math.Max(expectedType.Length, actualType.Length);

					Assert.IsType<NotEqualException>(ex);
					Assert.Equal(
						"Assert.NotEqual() Failure: Exception thrown during comparison" + Environment.NewLine +
						"              " + new string(' ', padding) + " ↓ (pos 0)" + Environment.NewLine +
						"Expected: Not " + expectedType.PadRight(padding) + "[1, 2]" + Environment.NewLine +
						"Actual:       " + actualType.PadRight(padding) + "[1, 2]" + Environment.NewLine +
						"              " + new string(' ', padding) + " ↑ (pos 0)",
						ex.Message
					);
					Assert.IsType<DivideByZeroException>(ex.InnerException);
				}

				validateError(() => Assert.NotEqual(expected, actual, (int e, int a) => throw new DivideByZeroException()), "int[] ", "<generated> ");
				validateError(() => Assert.NotEqual(expected.ToAsyncEnumerable(), actual, (int e, int a) => throw new DivideByZeroException()), "", "");
			}
		}
	}

	public class Single
	{
		[Fact]
		public static void GuardClause()
		{
			Assert.Throws<ArgumentNullException>("collection", () => Assert.Single(default(IAsyncEnumerable<object>)!));
		}

		[Fact]
		public static void EmptyCollection()
		{
			var collection = new object[0].ToAsyncEnumerable();

			var ex = Record.Exception(() => Assert.Single(collection));

			Assert.IsType<SingleException>(ex);
			Assert.Equal("Assert.Single() Failure: The collection was empty", ex.Message);
		}

		[Fact]
		public static void SingleItemCollection()
		{
			var collection = new[] { "Hello" }.ToAsyncEnumerable();

			var item = Assert.Single(collection);

			Assert.Equal("Hello", item);
		}

		[Fact]
		public static void MultiItemCollection()
		{
			var collection = new[] { "Hello", "World" }.ToAsyncEnumerable();

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
			var collection = new[] { 1, 2, 3, 4, 5, 6, 7 }.ToAsyncEnumerable();

			var ex = Record.Exception(() => Assert.Single(collection));

			Assert.IsType<SingleException>(ex);
			Assert.Equal(
				"Assert.Single() Failure: The collection contained 7 items" + Environment.NewLine +
				$"Collection: [1, 2, 3, 4, 5, {ArgumentFormatter.Ellipsis}]",
				ex.Message
			);
		}

		[Fact]
		public static void StringAsCollection_Match()
		{
			var collection = "H".ToAsyncEnumerable();

			var value = Assert.Single(collection);

			Assert.Equal('H', value);
		}

		[Fact]
		public static void StringAsCollection_NoMatch()
		{
			var collection = "Hello".ToAsyncEnumerable();

			var ex = Record.Exception(() => Assert.Single(collection));

			Assert.IsType<SingleException>(ex);
			Assert.Equal(
				"Assert.Single() Failure: The collection contained 5 items" + Environment.NewLine +
				"Collection: ['H', 'e', 'l', 'l', 'o']",
				ex.Message
			);
		}
	}

	public class Single_WithPredicate
	{
		[Fact]
		public static void GuardClauses()
		{
			Assert.Throws<ArgumentNullException>("collection", () => Assert.Single(default(IAsyncEnumerable<object>)!, _ => true));
			Assert.Throws<ArgumentNullException>("predicate", () => Assert.Single(new object[0].ToAsyncEnumerable(), null!));
		}

		[Fact]
		public static void SingleMatch()
		{
			var collection = new[] { "Hello", "World" }.ToAsyncEnumerable();

			var result = Assert.Single(collection, item => item.StartsWith("H"));

			Assert.Equal("Hello", result);
		}

		[Fact]
		public static void NoMatches()
		{
			var collection = new[] { "Hello", "World" }.ToAsyncEnumerable();

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
			var collection = new[] { "Hello", "World" }.ToAsyncEnumerable();

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
			var collection = new[] { 1, 2, 3, 4, 5, 6, 7, 8, 4 }.ToAsyncEnumerable();

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

		[Fact]
		public static void StringAsCollection_Match()
		{
			var collection = "H".ToAsyncEnumerable();

			var value = Assert.Single(collection, c => c != 'Q');

			Assert.Equal('H', value);
		}

		[Fact]
		public static void StringAsCollection_NoMatch()
		{
			var collection = "H".ToAsyncEnumerable();

			var ex = Record.Exception(() => Assert.Single(collection, c => c == 'Q'));

			Assert.IsType<SingleException>(ex);
			Assert.Equal(
				"Assert.Single() Failure: The collection did not contain any matching items" + Environment.NewLine +
				"Expected:   (predicate expression)" + Environment.NewLine +
				"Collection: ['H']",
				ex.Message
			);
		}
	}

	sealed class SpyEnumerator<T> : IAsyncEnumerable<T>, IAsyncEnumerator<T>
	{
		IAsyncEnumerator<T>? innerEnumerator;

		public SpyEnumerator(IAsyncEnumerable<T> enumerable)
		{
			innerEnumerator = enumerable.GetAsyncEnumerator();
		}

		public T Current =>
			GuardNotNull("Tried to get Current on a disposed enumerator", innerEnumerator).Current;

		public bool IsDisposed => innerEnumerator is null;

		public IAsyncEnumerator<T> GetAsyncEnumerator(CancellationToken cancellationToken = default) => this;

		public ValueTask<bool> MoveNextAsync() =>
			GuardNotNull("Tried to call MoveNext() on a disposed enumerator", innerEnumerator).MoveNextAsync();

		public async ValueTask DisposeAsync()
		{
			if (innerEnumerator is not null)
				await innerEnumerator.DisposeAsync();

			innerEnumerator = null;
		}

		/// <summary/>
		static T2 GuardNotNull<T2>(
			string message,
			[NotNull] T2? value)
				where T2 : class
		{
			if (value is null)
				throw new InvalidOperationException(message);

			return value;
		}
	}
}

#endif
