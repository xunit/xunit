using System;
using System.Collections.Generic;
using Xunit;
using Xunit.Sdk;

public class SetAssertsTests
{
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
