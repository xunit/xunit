using System.Collections.Generic;
using Xunit;
using Xunit.Sdk;

public class RangeAssertsTests
{
	public class InRange
	{
		[Fact]
		public void DoubleNotWithinRange()
		{
			Assert.Throws<InRangeException>(() => Assert.InRange(1.50, .75, 1.25));
		}

		[Fact]
		public void DoubleValueWithinRange()
		{
			Assert.InRange(1.0, .75, 1.25);
		}

		[Fact]
		public void IntNotWithinRangeWithZeroActual()
		{
			Assert.Throws<InRangeException>(() => Assert.InRange(0, 1, 2));
		}

		[Fact]
		public void IntNotWithinRangeWithZeroMinimum()
		{
			Assert.Throws<InRangeException>(() => Assert.InRange(2, 0, 1));
		}

		[Fact]
		public void IntValueWithinRange()
		{
			Assert.InRange(2, 1, 3);
		}

		[Fact]
		public void StringNotWithinRange()
		{
			Assert.Throws<InRangeException>(() => Assert.InRange("adam", "bob", "scott"));
		}

		[Fact]
		public void StringValueWithinRange()
		{
			Assert.InRange("bob", "adam", "scott");
		}
	}

	public class InRange_WithComparer
	{
		[Fact]
		public void DoubleValueWithinRange()
		{
			Assert.InRange(400.0, .75, 1.25, new DoubleComparer(-1));
		}

		[Fact]
		public void DoubleValueNotWithinRange()
		{
			Assert.Throws<InRangeException>(() => Assert.InRange(1.0, .75, 1.25, new DoubleComparer(1)));
		}
	}

	public class NotInRange
	{
		[Fact]
		public void DoubleNotWithinRange()
		{
			Assert.NotInRange(1.50, .75, 1.25);
		}

		[Fact]
		public void DoubleWithinRange()
		{
			Assert.Throws<NotInRangeException>(() => Assert.NotInRange(1.0, .75, 1.25));
		}

		[Fact]
		public void IntNotWithinRange()
		{
			Assert.NotInRange(1, 2, 3);
		}

		[Fact]
		public void IntWithinRange()
		{
			Assert.Throws<NotInRangeException>(() => Assert.NotInRange(2, 1, 3));
		}

		[Fact]
		public void StringNotWithNotInRange()
		{
			Assert.NotInRange("adam", "bob", "scott");
		}

		[Fact]
		public void StringWithNotInRange()
		{
			Assert.Throws<NotInRangeException>(() => Assert.NotInRange("bob", "adam", "scott"));
		}
	}

	public class NotInRange_WithComparer
	{
		[Fact]
		public void DoubleValueWithinRange()
		{
			Assert.Throws<NotInRangeException>(() => Assert.NotInRange(400.0, .75, 1.25, new DoubleComparer(-1)));
		}

		[Fact]
		public void DoubleValueNotWithinRange()
		{
			Assert.NotInRange(1.0, .75, 1.25, new DoubleComparer(1));
		}
	}

	class DoubleComparer : IComparer<double>
	{
		readonly int returnValue;

		public DoubleComparer(int returnValue)
		{
			this.returnValue = returnValue;
		}

		public int Compare(double x, double y) => returnValue;
	}
}
