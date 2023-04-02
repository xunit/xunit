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
#if NET5_0_OR_GREATER
			Assert.Contains("FORTY-two", (IReadOnlySet<string>)set);
#endif
#if XUNIT_IMMUTABLE_COLLECTIONS
			Assert.Contains("FORTY-two", set.ToImmutableHashSet(StringComparer.OrdinalIgnoreCase));
#endif
		}

		[Fact]
		public static void ValueNotInSet()
		{
			var set = new HashSet<string>() { "eleventeen" };

			var ex = Record.Exception(() => Assert.Contains("FORTY-two", set));

			Assert.IsType<ContainsException>(ex);
			Assert.Equal(
				"Assert.Contains() Failure: Item not found in set" + Environment.NewLine +
				"Set:       [\"eleventeen\"]" + Environment.NewLine +
				"Not found: \"FORTY-two\"",
				ex.Message
			);

			Assert.Throws<ContainsException>(() => Assert.Contains("FORTY-two", (ISet<string>)set));
#if NET5_0_OR_GREATER
			Assert.Throws<ContainsException>(() => Assert.Contains("FORTY-two", (IReadOnlySet<string>)set));
#endif
#if XUNIT_IMMUTABLE_COLLECTIONS
			Assert.Throws<ContainsException>(() => Assert.Contains("FORTY-two", set.ToImmutableHashSet()));
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
#if NET5_0_OR_GREATER
			Assert.DoesNotContain("FORTY-two", (IReadOnlySet<string>)set);
#endif
#if XUNIT_IMMUTABLE_COLLECTIONS
			Assert.DoesNotContain("FORTY-two", set.ToImmutableHashSet(StringComparer.OrdinalIgnoreCase));
#endif
		}

		[Fact]
		public static void ValueInSet()
		{
			var set = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "forty-two" };

			var ex = Record.Exception(() => Assert.DoesNotContain("FORTY-two", set));

			Assert.IsType<DoesNotContainException>(ex);
			Assert.Equal(
				"Assert.DoesNotContain() Failure" + Environment.NewLine +
				"Found:    FORTY-two" + Environment.NewLine +
				"In value: HashSet<String> [\"forty-two\"]",
				ex.Message
			);

			Assert.Throws<DoesNotContainException>(() => Assert.DoesNotContain("FORTY-two", (ISet<string>)set));
#if NET5_0_OR_GREATER
			Assert.Throws<DoesNotContainException>(() => Assert.DoesNotContain("FORTY-two", (IReadOnlySet<string>)set));
#endif
#if XUNIT_IMMUTABLE_COLLECTIONS
			Assert.Throws<DoesNotContainException>(() => Assert.DoesNotContain("FORTY-two", set.ToImmutableHashSet(StringComparer.OrdinalIgnoreCase)));
#endif
		}
	}

	public class Equal
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

	public class Subset
	{
		[Fact]
		public static void GuardClauses()
		{
			Assert.Throws<ArgumentNullException>(() => Assert.Subset(null!, new HashSet<int>()));
			Assert.Throws<SubsetException>(() => Assert.Subset(new HashSet<int>(), null));
		}

		[Fact]
		public static void IsSubset()
		{
			var expectedSuperset = new HashSet<int> { 1, 2, 3 };
			var actual = new HashSet<int> { 1, 2, 3 };

			Assert.Subset(expectedSuperset, actual);
		}

		[Fact]
		public static void IsProperSubset()
		{
			var expectedSuperset = new HashSet<int> { 1, 2, 3, 4 };
			var actual = new HashSet<int> { 1, 2, 3 };

			Assert.Subset(expectedSuperset, actual);
		}

		[Fact]
		public static void IsNotSubset()
		{
			var expectedSuperset = new HashSet<int> { 1, 2, 3 };
			var actual = new HashSet<int> { 1, 2, 7 };

			var ex = Assert.Throws<SubsetException>(() => Assert.Subset(expectedSuperset, actual));

			Assert.Equal(
				@"Assert.Subset() Failure" + Environment.NewLine +
				@"Expected: HashSet<Int32> [1, 2, 3]" + Environment.NewLine +
				@"Actual:   HashSet<Int32> [1, 2, 7]",
				ex.Message
			);
		}
	}

	public class ProperSubset
	{
		[Fact]
		public static void GuardClauses()
		{
			Assert.Throws<ArgumentNullException>(() => Assert.ProperSubset(null!, new HashSet<int>()));
			Assert.Throws<ProperSubsetException>(() => Assert.ProperSubset(new HashSet<int>(), null));
		}

		[Fact]
		public static void IsSubsetButNotProperSubset()
		{
			var expectedSuperset = new HashSet<int> { 1, 2, 3 };
			var actual = new HashSet<int> { 1, 2, 3 };

			var ex = Assert.Throws<ProperSubsetException>(() => Assert.ProperSubset(expectedSuperset, actual));

			Assert.Equal(
				@"Assert.ProperSubset() Failure" + Environment.NewLine +
				@"Expected: HashSet<Int32> [1, 2, 3]" + Environment.NewLine +
				@"Actual:   HashSet<Int32> [1, 2, 3]",
				ex.Message
			);
		}

		[Fact]
		public static void IsProperSubset()
		{
			var expectedSuperset = new HashSet<int> { 1, 2, 3, 4 };
			var actual = new HashSet<int> { 1, 2, 3 };

			Assert.ProperSubset(expectedSuperset, actual);
		}

		[Fact]
		public static void IsNotSubset()
		{
			var expectedSuperset = new HashSet<int> { 1, 2, 3 };
			var actual = new HashSet<int> { 1, 2, 7 };

			Assert.Throws<ProperSubsetException>(() => Assert.ProperSubset(expectedSuperset, actual));
		}
	}

	public class Superset
	{
		[Fact]
		public static void GuardClauses()
		{
			Assert.Throws<ArgumentNullException>(() => Assert.Superset(null!, new HashSet<int>()));
			Assert.Throws<SupersetException>(() => Assert.Superset(new HashSet<int>(), null));
		}

		[Fact]
		public static void IsSuperset()
		{
			var expectedSubset = new HashSet<int> { 1, 2, 3 };
			var actual = new HashSet<int> { 1, 2, 3 };

			Assert.Superset(expectedSubset, actual);
		}

		[Fact]
		public static void IsProperSuperset()
		{
			var expectedSubset = new HashSet<int> { 1, 2, 3 };
			var actual = new HashSet<int> { 1, 2, 3, 4 };

			Assert.Superset(expectedSubset, actual);
		}

		[Fact]
		public static void IsNotSuperset()
		{
			var expectedSubset = new HashSet<int> { 1, 2, 3 };
			var actual = new HashSet<int> { 1, 2, 7 };

			var ex = Assert.Throws<SupersetException>(() => Assert.Superset(expectedSubset, actual));

			Assert.Equal(
				@"Assert.Superset() Failure" + Environment.NewLine +
				@"Expected: HashSet<Int32> [1, 2, 3]" + Environment.NewLine +
				@"Actual:   HashSet<Int32> [1, 2, 7]",
				ex.Message
			);
		}
	}

	public class ProperSuperset
	{
		[Fact]
		public static void GuardClauses()
		{
			Assert.Throws<ArgumentNullException>(() => Assert.ProperSuperset(null!, new HashSet<int>()));
			Assert.Throws<ProperSupersetException>(() => Assert.ProperSuperset(new HashSet<int>(), null));
		}

		[Fact]
		public static void IsSupersetButNotProperSuperset()
		{
			var expectedSubset = new HashSet<int> { 1, 2, 3 };
			var actual = new HashSet<int> { 1, 2, 3 };

			var ex = Assert.Throws<ProperSupersetException>(() => Assert.ProperSuperset(expectedSubset, actual));

			Assert.Equal(
				@"Assert.ProperSuperset() Failure" + Environment.NewLine +
				@"Expected: HashSet<Int32> [1, 2, 3]" + Environment.NewLine +
				@"Actual:   HashSet<Int32> [1, 2, 3]",
				ex.Message
			);
		}

		[Fact]
		public static void IsProperSuperset()
		{
			var expectedSubset = new HashSet<int> { 1, 2, 3 };
			var actual = new HashSet<int> { 1, 2, 3, 4 };

			Assert.ProperSuperset(expectedSubset, actual);
		}

		[Fact]
		public static void IsNotSuperset()
		{
			var expectedSubset = new HashSet<int> { 1, 2, 3 };
			var actual = new HashSet<int> { 1, 2, 7 };

			Assert.Throws<ProperSupersetException>(() => Assert.ProperSuperset(expectedSubset, actual));
		}
	}
}
