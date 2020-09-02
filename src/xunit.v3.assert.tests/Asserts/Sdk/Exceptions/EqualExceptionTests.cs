using System;
using System.Collections.Generic;
using Xunit;

public class EqualExceptionTests
{
	public class StringTests
	{
		[Fact]
		public void OneStringAddsValueToEndOfTheOtherString()
		{
			var expectedMessage =
				"Assert.Equal() Failure" + Environment.NewLine +
				"                    ↓ (pos 10)" + Environment.NewLine +
				"Expected: first test 1" + Environment.NewLine +
				"Actual:   first test" + Environment.NewLine +
				"                    ↑ (pos 10)";

			var ex = Record.Exception(() => Assert.Equal("first test 1", "first test"));

			Assert.NotNull(ex);
			Assert.Equal(expectedMessage, ex.Message);
		}

		[Fact]
		public void OneStringOneNullDoesNotShowDifferencePoint()
		{
			var expectedMessage =
				"Assert.Equal() Failure" + Environment.NewLine +
				"Expected: first test 1" + Environment.NewLine +
				"Actual:   (null)";

			var ex = Record.Exception(() => Assert.Equal("first test 1", null));

			Assert.NotNull(ex);
			Assert.Equal(expectedMessage, ex.Message);
		}

		[Fact]
		public void StringsDifferInTheMiddle()
		{
			var expectedMessage =
				"Assert.Equal() Failure" + Environment.NewLine +
				"                ↓ (pos 6)" + Environment.NewLine +
				"Expected: first failure" + Environment.NewLine +
				"Actual:   first test" + Environment.NewLine +
				"                ↑ (pos 6)";

			var ex = Record.Exception(() => Assert.Equal("first failure", "first test"));

			Assert.NotNull(ex);
			Assert.Equal(expectedMessage, ex.Message);
		}
	}

	public class IEnumerableTests
	{
		[Fact]
		public void ExpectedNull()
		{
			var expectedMessage =
				"Assert.Equal() Failure" + Environment.NewLine +
				"Expected: (null)" + Environment.NewLine +
				"Actual:   Int32[] [1, 2, 3]";

			var ex = Record.Exception(
				() => Assert.Equal(
					null,
					new[] { 1, 2, 3 }
				)
			);

			Assert.NotNull(ex);
			Assert.Equal(expectedMessage, ex.Message);
		}

		[Fact]
		public void ActualNull()
		{
			var expectedMessage =
				"Assert.Equal() Failure" + Environment.NewLine +
				"Expected: Int32[] [1, 2, 3]" + Environment.NewLine +
				"Actual:   (null)";

			var ex = Record.Exception(
				() => Assert.Equal(
					new[] { 1, 2, 3 },
					null
				)
			);

			Assert.NotNull(ex);
			Assert.Equal(expectedMessage, ex.Message);
		}

		[Fact]
		public void SingleValue()
		{
			var expectedMessage =
				"Assert.Equal() Failure" + Environment.NewLine +
				"           ↓ (pos 0)" + Environment.NewLine +
				"Expected: [1]" + Environment.NewLine +
				"Actual:   [99]" + Environment.NewLine +
				"           ↑ (pos 0)";

			var ex = Record.Exception(
				() => Assert.Equal(
					new[] { 1 },
					new[] { 99 }
				)
			);

			Assert.NotNull(ex);
			Assert.Equal(expectedMessage, ex.Message);
		}

		[Fact]
		public void ExactArraySize_DifferenceAtStart()
		{
			var expectedMessage =
				"Assert.Equal() Failure" + Environment.NewLine +
				"           ↓ (pos 0)" + Environment.NewLine +
				"Expected: [1, 2, 3, 4, 5]" + Environment.NewLine +
				"Actual:   [99, 2, 3, 4, 5]" + Environment.NewLine +
				"           ↑ (pos 0)";

			var ex = Record.Exception(
				() => Assert.Equal(
					new[] { 1, 2, 3, 4, 5 },
					new[] { 99, 2, 3, 4, 5 }
				)
			);

			Assert.NotNull(ex);
			Assert.Equal(expectedMessage, ex.Message);
		}

		[Fact]
		public void ExactArraySize_DifferenceNearStart()
		{
			var expectedMessage =
				"Assert.Equal() Failure" + Environment.NewLine +
				"              ↓ (pos 1)" + Environment.NewLine +
				"Expected: [1, 2, 3, 4, 5]" + Environment.NewLine +
				"Actual:   [1, 99, 3, 4, 5]" + Environment.NewLine +
				"              ↑ (pos 1)";

			var ex = Record.Exception(
				() => Assert.Equal(
					new List<int> { 1, 2, 3, 4, 5 },
					new List<int> { 1, 99, 3, 4, 5 }
				)
			);

			Assert.NotNull(ex);
			Assert.Equal(expectedMessage, ex.Message);
		}

		[Fact]
		public void ExactArraySize_DifferenceNearEnd()
		{
			var expectedMessage =
				"Assert.Equal() Failure" + Environment.NewLine +
				"                    ↓ (pos 3)" + Environment.NewLine +
				"Expected: [1, 2, 3, 4, 5]" + Environment.NewLine +
				"Actual:   [1, 2, 3, 99, 5]" + Environment.NewLine +
				"                    ↑ (pos 3)";

			var ex = Record.Exception(
				() => Assert.Equal(
					new[] { 1, 2, 3, 4, 5 },
					new[] { 1, 2, 3, 99, 5 }
				)
			);

			Assert.NotNull(ex);
			Assert.Equal(expectedMessage, ex.Message);
		}

		[Fact]
		public void ExactArraySize_DifferenceAtEnd()
		{
			var expectedMessage =
				"Assert.Equal() Failure" + Environment.NewLine +
				"                       ↓ (pos 4)" + Environment.NewLine +
				"Expected: [1, 2, 3, 4, 5]" + Environment.NewLine +
				"Actual:   [1, 2, 3, 4, 99]" + Environment.NewLine +
				"                       ↑ (pos 4)";

			var ex = Record.Exception(
				() => Assert.Equal(
					new[] { 1, 2, 3, 4, 5 },
					new[] { 1, 2, 3, 4, 99 }
				)
			);

			Assert.NotNull(ex);
			Assert.Equal(expectedMessage, ex.Message);
		}

		[Fact]
		public void ExpectedShorter()
		{
			var expectedMessage =
				"Assert.Equal() Failure" + Environment.NewLine +
				"Expected: [1, 2, 3]" + Environment.NewLine +
				"Actual:   [1, 2, 3, 4]" + Environment.NewLine +
				"                    ↑ (pos 3)";

			var ex = Record.Exception(
				() => Assert.Equal(
					new[] { 1, 2, 3 },
					new[] { 1, 2, 3, 4 }
				)
			);

			Assert.NotNull(ex);
			Assert.Equal(expectedMessage, ex.Message);
		}

		[Fact]
		public void ExpectedLonger()
		{
			var expectedMessage =
				"Assert.Equal() Failure" + Environment.NewLine +
				"                    ↓ (pos 3)" + Environment.NewLine +
				"Expected: [1, 2, 3, 4]" + Environment.NewLine +
				"Actual:   [1, 2, 3]";

			var ex = Record.Exception(
				() => Assert.Equal(
					new[] { 1, 2, 3, 4 },
					new[] { 1, 2, 3 }
				)
			);

			Assert.NotNull(ex);
			Assert.Equal(expectedMessage, ex.Message);
		}

		[Fact]
		public void LongArray_DifferenceAtStart()
		{
			var expectedMessage =
				"Assert.Equal() Failure" + Environment.NewLine +
				"           ↓ (pos 0)" + Environment.NewLine +
				"Expected: [1, 2, 3, 4, 5, ...]".Replace("'", "\"") + Environment.NewLine +
				"Actual:   [99, 2, 3, 4, 5, ...]".Replace("'", "\"") + Environment.NewLine +
				"           ↑ (pos 0)";

			var ex = Record.Exception(
				() => Assert.Equal(
					new[] { 1, 2, 3, 4, 5, 6, 7 },
					new[] { 99, 2, 3, 4, 5, 6, 7 }
				)
			);

			Assert.NotNull(ex);
			Assert.Equal(expectedMessage, ex.Message);
		}

		[Fact]
		public void LongArray_DifferenceInMiddle()
		{
			var expectedMessage =
				"Assert.Equal() Failure" + Environment.NewLine +
				"                      ↓ (pos 3)" + Environment.NewLine +
				"Expected: [..., 2, 3, 4, 5, 6, ...]".Replace("'", "\"") + Environment.NewLine +
				"Actual:   [..., 2, 3, 99, 5, 6, ...]".Replace("'", "\"") + Environment.NewLine +
				"                      ↑ (pos 3)";

			var ex = Record.Exception(
				() => Assert.Equal(
					new[] { 1, 2, 3, 4, 5, 6, 7 },
					new[] { 1, 2, 3, 99, 5, 6, 7 }
				)
			);

			Assert.NotNull(ex);
			Assert.Equal(expectedMessage, ex.Message);
		}

		[Fact]
		public void LongArray_DifferenceAtEnd()
		{
			var expectedMessage =
				"Assert.Equal() Failure" + Environment.NewLine +
				"                            ↓ (pos 6)" + Environment.NewLine +
				"Expected: [..., 3, 4, 5, 6, 7]".Replace("'", "\"") + Environment.NewLine +
				"Actual:   [..., 3, 4, 5, 6, 99]".Replace("'", "\"") + Environment.NewLine +
				"                            ↑ (pos 6)";

			var ex = Record.Exception(
				() => Assert.Equal(
					new[] { 1, 2, 3, 4, 5, 6, 7 },
					new[] { 1, 2, 3, 4, 5, 6, 99 }
				)
			);

			Assert.NotNull(ex);
			Assert.Equal(expectedMessage, ex.Message);
		}
	}
}
