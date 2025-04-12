using System.Threading.Tasks;
using Xunit;
using Xunit.Sdk;

public class MultipleAssertsTests
{
	[Fact]
	public void NoActions_DoesNotThrow()
	{
		Assert.Multiple();
	}

	[Fact]
	public void SingleAssert_Success_DoesNotThrow()
	{
		Assert.Multiple(
			() => Assert.True(true)
		);
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
		Assert.Multiple(
			() => Assert.True(true),
			() => Assert.False(false)
		);
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
		Assert.Equal(
			"Assert.Multiple() Failure: Multiple failures were encountered",
			ex.Message
		);
		Assert.Collection(
			multiEx.InnerExceptions,
			innerEx => Assert.IsType<TrueException>(innerEx),
			innerEx => Assert.IsType<FalseException>(innerEx)
		);
	}

	[Fact]
	public async Task MultipleAsync_NoActions_DoesNotThrow()
	{
		await Assert.MultipleAsync();
	}

	[Fact]
	public async Task MultipleAsync_SingleAssert_Success_DoesNotThrow()
	{
		var task = (bool isTrue) => Task.FromResult(isTrue);

		await Assert.MultipleAsync(
			async () => Assert.True(await task(true))
		);
	}

	[Fact]
	public async Task MultipleAsync_Success_DoesNotThrow()
	{
		var task = (bool isTrue) => Task.FromResult(isTrue);

		await Assert.MultipleAsync(
			async () => Assert.True(await task(true)),
			async () => Assert.True(await task(true)),
			async () => Assert.True(await task(true))
		);
	}

	[Fact]
	public async Task MultipleAsync_SingleAssert_Fails_ThrowsFailingAssert()
	{
		var task = (bool isTrue) => Task.FromResult(isTrue);

		var ex = await Record.ExceptionAsync(async () =>
			await Assert.MultipleAsync(
				async () => Assert.False(await task(true))
			)
		);

		Assert.IsType<FalseException>(ex);
	}

	[Fact]
	public async Task MultipleAsync_SingleAssert_Multiple_ThrowsFailingAssert()
	{
		var task = (bool isTrue) => Task.FromResult(isTrue);

		var ex = await Record.ExceptionAsync(async () =>
			await Assert.MultipleAsync(
				async () => Assert.False(await task(true)),
				async () => Assert.False(await task(true))
			)
		);

		var multiEx = Assert.IsType<MultipleException>(ex);
		Assert.Collection(
			multiEx.InnerExceptions,
			innerEx => Assert.IsType<FalseException>(innerEx),
			innerEx => Assert.IsType<FalseException>(innerEx)
		);
	}
}
