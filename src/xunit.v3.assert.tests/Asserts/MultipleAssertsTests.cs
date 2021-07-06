using Xunit;
using Xunit.Sdk;

public class MultipleAssertsTests
{
	[Fact]
	public void NoActions_DoesNotThrow()
	{
		var ex = Record.Exception(() => Assert.Multiple());

		Assert.Null(ex);
	}

	[Fact]
	public void SingleAssert_Success_DoesNotThrow()
	{
		var ex = Record.Exception(() =>
			Assert.Multiple(
				() => Assert.True(true)
			)
		);

		Assert.Null(ex);
	}

	[Fact]
	public void SingleAssert_Fails_ThrowsFailingAssert()
	{
		var ex = Record.Exception(() =>
			Assert.Multiple(
				() => Assert.True(false)
			)
		);

		Assert.IsType<TrueException>(ex);
	}

	[Fact]
	public void MultipleAssert_Success_DoesNotThrow()
	{
		var ex = Record.Exception(() =>
			Assert.Multiple(
				() => Assert.True(true),
				() => Assert.False(false)
			)
		);

		Assert.Null(ex);
	}

	[Fact]
	public void MultipleAssert_SingleFailure_ThrowsFailingAssert()
	{
		var ex = Record.Exception(() =>
			Assert.Multiple(
				() => Assert.True(true),
				() => Assert.False(true)
			)
		);

		Assert.IsType<FalseException>(ex);
	}

	[Fact]
	public void MultipleAssert_MultipleFailures_ThrowsMultipleException()
	{
		var ex = Record.Exception(() =>
			Assert.Multiple(
				() => Assert.True(false),
				() => Assert.False(true)
			)
		);

		var multiEx = Assert.IsType<MultipleException>(ex);
		Assert.Collection(
			multiEx.InnerExceptions,
			innerEx => Assert.IsType<TrueException>(innerEx),
			innerEx => Assert.IsType<FalseException>(innerEx)
		);
	}
}
