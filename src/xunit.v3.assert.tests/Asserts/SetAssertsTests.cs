using System;
using System.Collections.Generic;
using Xunit;
using Xunit.Sdk;

#if XUNIT_IMMUTABLE_COLLECTIONS
using System.Collections.Immutable;
#endif

public class SetAssertsTests
{
	public class Contains
	{
		[Fact]
		public static void ValueInSet()
		{
			var set = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "forty-two" };

			Assert.Contains("FORTY-two", set);
			Assert.Contains("FORTY-two", (ISet<string>)set);
			Assert.Contains("FORTY-two", set.ToSortedSet(StringComparer.OrdinalIgnoreCase));
#if NET5_0_OR_GREATER
			Assert.Contains("FORTY-two", (IReadOnlySet<string>)set);
#endif
#if XUNIT_IMMUTABLE_COLLECTIONS
			Assert.Contains("FORTY-two", set.ToImmutableHashSet(StringComparer.OrdinalIgnoreCase));
			Assert.Contains("FORTY-two", set.ToImmutableSortedSet(StringComparer.OrdinalIgnoreCase));
#endif
		}

		[Fact]
		public static void ValueNotInSet()
		{
			var set = new HashSet<string>() { "eleventeen" };

			void assertFailure(Action action)
			{
				var ex = Record.Exception(action);

				Assert.IsType<ContainsException>(ex);
				Assert.Equal(
					"Assert.Contains() Failure: Item not found in set" + Environment.NewLine +
					"Set:       [\"eleventeen\"]" + Environment.NewLine +
					"Not found: \"FORTY-two\"",
					ex.Message
				);
			}

			assertFailure(() => Assert.Contains("FORTY-two", set));
			assertFailure(() => Assert.Contains("FORTY-two", (ISet<string>)set));
			assertFailure(() => Assert.Contains("FORTY-two", set.ToSortedSet()));
#if NET5_0_OR_GREATER
			assertFailure(() => Assert.Contains("FORTY-two", (IReadOnlySet<string>)set));
#endif
#if XUNIT_IMMUTABLE_COLLECTIONS
			assertFailure(() => Assert.Contains("FORTY-two", set.ToImmutableHashSet()));
			assertFailure(() => Assert.Contains("FORTY-two", set.ToImmutableSortedSet()));
#endif
		}
	}

	public class DoesNotContain
	{
		[Fact]
		public static void ValueNotInSet()
		{
			var set = new HashSet<string>() { "eleventeen" };

			Assert.DoesNotContain("FORTY-two", set);
			Assert.DoesNotContain("FORTY-two", (ISet<string>)set);
			Assert.DoesNotContain("FORTY-two", set.ToSortedSet());
#if NET5_0_OR_GREATER
			Assert.DoesNotContain("FORTY-two", (IReadOnlySet<string>)set);
#endif
#if XUNIT_IMMUTABLE_COLLECTIONS
			Assert.DoesNotContain("FORTY-two", set.ToImmutableHashSet());
			Assert.DoesNotContain("FORTY-two", set.ToImmutableSortedSet());
#endif
		}

		[Fact]
		public static void ValueInSet()
		{
			var set = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "forty-two" };

			void assertFailure(Action action)
			{
				var ex = Record.Exception(action);

				Assert.IsType<DoesNotContainException>(ex);
				Assert.Equal(
					"Assert.DoesNotContain() Failure: Item found in set" + Environment.NewLine +
					"Set:   [\"forty-two\"]" + Environment.NewLine +
					"Found: \"FORTY-two\"",
					ex.Message
				);
			}

			assertFailure(() => Assert.DoesNotContain("FORTY-two", set));
			assertFailure(() => Assert.DoesNotContain("FORTY-two", (ISet<string>)set));
			assertFailure(() => Assert.DoesNotContain("FORTY-two", set.ToSortedSet(StringComparer.OrdinalIgnoreCase)));
#if NET5_0_OR_GREATER
			assertFailure(() => Assert.DoesNotContain("FORTY-two", (IReadOnlySet<string>)set));
#endif
#if XUNIT_IMMUTABLE_COLLECTIONS
			assertFailure(() => Assert.DoesNotContain("FORTY-two", set.ToImmutableHashSet(StringComparer.OrdinalIgnoreCase)));
			assertFailure(() => Assert.DoesNotContain("FORTY-two", set.ToImmutableSortedSet(StringComparer.OrdinalIgnoreCase)));
#endif
		}
	}

	public class ProperSubset
	{
		[Fact]
		public static void GuardClause()
		{
			Assert.Throws<ArgumentNullException>("expectedSubset", () => Assert.ProperSubset(null!, new HashSet<int>()));
		}

		[Fact]
		public static void IsSubsetButNotProperSubset()
		{
			var expectedSubset = new HashSet<int> { 1, 2, 3 };
			var actual = new HashSet<int> { 1, 2, 3 };

			var ex = Record.Exception(() => Assert.ProperSubset(expectedSubset, actual));

			Assert.IsType<ProperSubsetException>(ex);
			Assert.Equal(
				"Assert.ProperSubset() Failure: Value is not a proper subset" + Environment.NewLine +
				"Expected: [1, 2, 3]" + Environment.NewLine +
				"Actual:   [1, 2, 3]",
				ex.Message
			);
		}

		[Fact]
		public static void IsProperSubset()
		{
			var expectedSubset = new HashSet<int> { 1, 2, 3, 4 };
			var actual = new HashSet<int> { 1, 2, 3 };

			Assert.ProperSubset(expectedSubset, actual);
		}

		[Fact]
		public static void IsNotSubset()
		{
			var expectedSubset = new HashSet<int> { 1, 2, 3 };
			var actual = new HashSet<int> { 1, 2, 7 };

			var ex = Record.Exception(() => Assert.ProperSubset(expectedSubset, actual));

			Assert.IsType<ProperSubsetException>(ex);
			Assert.Equal(
				"Assert.ProperSubset() Failure: Value is not a proper subset" + Environment.NewLine +
				"Expected: [1, 2, 3]" + Environment.NewLine +
				"Actual:   [1, 2, 7]",
				ex.Message
			);
		}

		[Fact]
		public static void NullActual()
		{
			var ex = Record.Exception(() => Assert.ProperSubset(new HashSet<int>(), null));

			Assert.IsType<ProperSubsetException>(ex);
			Assert.Equal(
				"Assert.ProperSubset() Failure: Value is not a proper subset" + Environment.NewLine +
				"Expected: []" + Environment.NewLine +
				"Actual:   null",
				ex.Message
			);
		}
	}

	public class ProperSuperset
	{
		[Fact]
		public static void GuardClause()
		{
			Assert.Throws<ArgumentNullException>("expectedSuperset", () => Assert.ProperSuperset(null!, new HashSet<int>()));
		}

		[Fact]
		public static void IsSupersetButNotProperSuperset()
		{
			var expectedSuperset = new HashSet<int> { 1, 2, 3 };
			var actual = new HashSet<int> { 1, 2, 3 };

			var ex = Record.Exception(() => Assert.ProperSuperset(expectedSuperset, actual));

			Assert.IsType<ProperSupersetException>(ex);
			Assert.Equal(
				"Assert.ProperSuperset() Failure: Value is not a proper superset" + Environment.NewLine +
				"Expected: [1, 2, 3]" + Environment.NewLine +
				"Actual:   [1, 2, 3]",
				ex.Message
			);
		}

		[Fact]
		public static void IsProperSuperset()
		{
			var expectedSuperset = new HashSet<int> { 1, 2, 3 };
			var actual = new HashSet<int> { 1, 2, 3, 4 };

			Assert.ProperSuperset(expectedSuperset, actual);
		}

		[Fact]
		public static void IsNotSuperset()
		{
			var expectedSuperset = new HashSet<int> { 1, 2, 3 };
			var actual = new HashSet<int> { 1, 2, 7 };

			var ex = Record.Exception(() => Assert.ProperSuperset(expectedSuperset, actual));

			Assert.IsType<ProperSupersetException>(ex);
			Assert.Equal(
				"Assert.ProperSuperset() Failure: Value is not a proper superset" + Environment.NewLine +
				"Expected: [1, 2, 3]" + Environment.NewLine +
				"Actual:   [1, 2, 7]",
				ex.Message
			);
		}

		[Fact]
		public void NullActual()
		{
			var ex = Record.Exception(() => Assert.ProperSuperset(new HashSet<int>(), null));

			Assert.IsType<ProperSupersetException>(ex);
			Assert.Equal(
				"Assert.ProperSuperset() Failure: Value is not a proper superset" + Environment.NewLine +
				"Expected: []" + Environment.NewLine +
				"Actual:   null",
				ex.Message
			);
		}
	}

	public class Subset
	{
		[Fact]
		public static void GuardClause()
		{
			Assert.Throws<ArgumentNullException>("expectedSubset", () => Assert.Subset(null!, new HashSet<int>()));
		}

		[Fact]
		public static void IsSubset()
		{
			var expectedSubset = new HashSet<int> { 1, 2, 3 };
			var actual = new HashSet<int> { 1, 2, 3 };

			Assert.Subset(expectedSubset, actual);
		}

		[Fact]
		public static void IsProperSubset()
		{
			var expectedSubset = new HashSet<int> { 1, 2, 3, 4 };
			var actual = new HashSet<int> { 1, 2, 3 };

			Assert.Subset(expectedSubset, actual);
		}

		[Fact]
		public static void IsNotSubset()
		{
			var expectedSubset = new HashSet<int> { 1, 2, 3 };
			var actual = new HashSet<int> { 1, 2, 7 };

			var ex = Record.Exception(() => Assert.Subset(expectedSubset, actual));

			Assert.IsType<SubsetException>(ex);
			Assert.Equal(
				"Assert.Subset() Failure: Value is not a subset" + Environment.NewLine +
				"Expected: [1, 2, 3]" + Environment.NewLine +
				"Actual:   [1, 2, 7]",
				ex.Message
			);
		}

		[Fact]
		public static void NullActual()
		{
			var ex = Record.Exception(() => Assert.Subset(new HashSet<int>(), null));

			Assert.IsType<SubsetException>(ex);
			Assert.Equal(
				"Assert.Subset() Failure: Value is not a subset" + Environment.NewLine +
				"Expected: []" + Environment.NewLine +
				"Actual:   null",
				ex.Message
			);
		}
	}

	public class Superset
	{
		[Fact]
		public static void GuardClause()
		{
			Assert.Throws<ArgumentNullException>("expectedSuperset", () => Assert.Superset(null!, new HashSet<int>()));
		}

		[Fact]
		public static void IsSuperset()
		{
			var expectedSuperset = new HashSet<int> { 1, 2, 3 };
			var actual = new HashSet<int> { 1, 2, 3 };

			Assert.Superset(expectedSuperset, actual);
		}

		[Fact]
		public static void IsProperSuperset()
		{
			var expectedSuperset = new HashSet<int> { 1, 2, 3 };
			var actual = new HashSet<int> { 1, 2, 3, 4 };

			Assert.Superset(expectedSuperset, actual);
		}

		[Fact]
		public static void IsNotSuperset()
		{
			var expectedSuperset = new HashSet<int> { 1, 2, 3 };
			var actual = new HashSet<int> { 1, 2, 7 };

			var ex = Assert.Throws<SupersetException>(() => Assert.Superset(expectedSuperset, actual));

			Assert.Equal(
				"Assert.Superset() Failure: Value is not a superset" + Environment.NewLine +
				"Expected: [1, 2, 3]" + Environment.NewLine +
				"Actual:   [1, 2, 7]",
				ex.Message
			);
		}

		[Fact]
		public void NullActual()
		{
			var ex = Record.Exception(() => Assert.Superset(new HashSet<int>(), null));

			Assert.IsType<SupersetException>(ex);
			Assert.Equal(
				"Assert.Superset() Failure: Value is not a superset" + Environment.NewLine +
				"Expected: []" + Environment.NewLine +
				"Actual:   null",
				ex.Message
			);
		}
	}
}
