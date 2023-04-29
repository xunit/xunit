using System;
using System.Collections.Generic;
using Xunit;
using Xunit.Sdk;

public class RangeAssertsTests
{
	public class InRange
	{
		[CulturedFact]
		public void DoubleNotWithinRange()
		{
			var ex = Record.Exception(() => Assert.InRange(1.50, .75, 1.25));

			Assert.IsType<InRangeException>(ex);
			Assert.Equal(
				"Assert.InRange() Failure: Value not in range" + Environment.NewLine +
				$"Range:  ({0.75:G17} - {1.25:G17})" + Environment.NewLine +
				$"Actual: {1.5:G17}",
				ex.Message
			);
		}

		[Fact]
		public void DoubleValueWithinRange()
		{
			Assert.InRange(1.0, .75, 1.25);
		}

		[Fact]
		public void IntNotWithinRangeWithZeroActual()
		{
			var ex = Record.Exception(() => Assert.InRange(0, 1, 2));

			Assert.IsType<InRangeException>(ex);
			Assert.Equal(
				"Assert.InRange() Failure: Value not in range" + Environment.NewLine +
				"Range:  (1 - 2)" + Environment.NewLine +
				"Actual: 0",
				ex.Message
			);
		}

		[Fact]
		public void IntNotWithinRangeWithZeroMinimum()
		{
			var ex = Record.Exception(() => Assert.InRange(2, 0, 1));

			Assert.IsType<InRangeException>(ex);
			Assert.Equal(
				"Assert.InRange() Failure: Value not in range" + Environment.NewLine +
				"Range:  (0 - 1)" + Environment.NewLine +
				"Actual: 2",
				ex.Message
			);
		}

		[Fact]
		public void IntValueWithinRange()
		{
			Assert.InRange(2, 1, 3);
		}

		[Fact]
		public void StringNotWithinRange()
		{
			var ex = Record.Exception(() => Assert.InRange("adam", "bob", "scott"));

			Assert.IsType<InRangeException>(ex);
			Assert.Equal(
				"Assert.InRange() Failure: Value not in range" + Environment.NewLine +
				"Range:  (\"bob\" - \"scott\")" + Environment.NewLine +
				"Actual: \"adam\"",
				ex.Message
			);
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

		[CulturedFact]
		public void DoubleValueNotWithinRange()
		{
			var ex = Record.Exception(() => Assert.InRange(1.0, .75, 1.25, new DoubleComparer(1)));

			Assert.IsType<InRangeException>(ex);
			Assert.Equal(
				"Assert.InRange() Failure: Value not in range" + Environment.NewLine +
				$"Range:  ({0.75:G17} - {1.25:G17})" + Environment.NewLine +
				$"Actual: {1:G17}",
				ex.Message
			);
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
			var ex = Record.Exception(() => Assert.NotInRange(1.0, .75, 1.25));

			Assert.IsType<NotInRangeException>(ex);
			Assert.Equal(
				"Assert.NotInRange() Failure: Value in range" + Environment.NewLine +
				"Range:  (0.75 - 1.25)" + Environment.NewLine +
				"Actual: 1",
				ex.Message
			);
		}

		[Fact]
		public void IntNotWithinRange()
		{
			Assert.NotInRange(1, 2, 3);
		}

		[Fact]
		public void IntWithinRange()
		{
			var ex = Record.Exception(() => Assert.NotInRange(2, 1, 3));

			Assert.IsType<NotInRangeException>(ex);
			Assert.Equal(
				"Assert.NotInRange() Failure: Value in range" + Environment.NewLine +
				"Range:  (1 - 3)" + Environment.NewLine +
				"Actual: 2",
				ex.Message
			);
		}

		[Fact]
		public void StringNotWithNotInRange()
		{
			Assert.NotInRange("adam", "bob", "scott");
		}

		[Fact]
		public void StringWithNotInRange()
		{
			var ex = Record.Exception(() => Assert.NotInRange("bob", "adam", "scott"));

			Assert.IsType<NotInRangeException>(ex);
			Assert.Equal(
				"Assert.NotInRange() Failure: Value in range" + Environment.NewLine +
				"Range:  (\"adam\" - \"scott\")" + Environment.NewLine +
				"Actual: \"bob\"",
				ex.Message
			);
		}
	}

	public class NotInRange_WithComparer
	{
		[Fact]
		public void DoubleValueWithinRange()
		{
			var ex = Record.Exception(() => Assert.NotInRange(400.0, .75, 1.25, new DoubleComparer(-1)));

			Assert.IsType<NotInRangeException>(ex);
			Assert.Equal(
				"Assert.NotInRange() Failure: Value in range" + Environment.NewLine +
				"Range:  (0.75 - 1.25)" + Environment.NewLine +
				"Actual: 400",
				ex.Message
			);
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
