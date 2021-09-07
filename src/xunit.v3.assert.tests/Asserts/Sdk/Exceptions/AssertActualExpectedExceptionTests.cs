using System;
using Xunit;
using Xunit.Sdk;

public class AssertActualExpectedExceptionTests
{
	[Fact]
	public void NullValuesInArraysCreateCorrectExceptionMessage()
	{
		var expectedValue = new string?[] { null, "hello" };
		var actualValue = new string?[] { null, "world" };

		var expectedMessage =
			"Message" + Environment.NewLine +
			"Expected: String[] [null, \"hello\"]" + Environment.NewLine +
			"Actual:   String[] [null, \"world\"]";

		var ex = new AssertActualExpectedException(expectedValue, actualValue, "Message");

		Assert.Equal(expectedMessage, ex.Message);
	}

	[Fact]
	public void ExpectedAndActualAreUsedInMessage()
	{
		var expectedMessage =
			"Message" + Environment.NewLine +
			"Expected: 2" + Environment.NewLine +
			"Actual:   1";

		var ex = new AssertActualExpectedException(2, 1, "Message");

		Assert.Equal(expectedMessage, ex.Message);
	}

	[Fact]
	public void MultiLineValuesAreIndented()
	{
		var expectedMessage =
			"Message" + Environment.NewLine +
			"Multi-Line" + Environment.NewLine +
			"Expected: Expected" + Environment.NewLine +
			"          Multi-Line" + Environment.NewLine +
			"Actual:   Actual" + Environment.NewLine +
			"          Multi-Line";

		var ex = new AssertActualExpectedException($"Expected{Environment.NewLine}Multi-Line", $"Actual{Environment.NewLine}Multi-Line", $"Message{Environment.NewLine}Multi-Line");

		Assert.Equal(expectedMessage, ex.Message);
	}

	[Fact]
	public void PreservesExpectedAndActual()
	{
		var ex = new AssertActualExpectedException(2, 1, null);

		Assert.Equal("1", ex.Actual);
		Assert.Equal("2", ex.Expected);
		Assert.Null(ex.UserMessage);
	}

	[Fact]
	public void SameVisibleValueDifferentTypes()
	{
		var expectedMessage =
			"Message" + Environment.NewLine +
			"Expected: 1 (System.String)" + Environment.NewLine +
			"Actual:   1 (System.Int32)";

		var ex = new AssertActualExpectedException("1", 1, "Message");

		Assert.Equal(expectedMessage, ex.Message);
	}

	[Fact]
	public void DifferentVisibleValueDifferentTypes()
	{
		var expectedMessage =
			"Message" + Environment.NewLine +
			"Expected: 2" + Environment.NewLine +
			"Actual:   1";

		var ex = new AssertActualExpectedException("2", 1, "Message");

		Assert.Equal(expectedMessage, ex.Message);
	}
}
