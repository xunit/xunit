using System;
using Xunit;

public class UniqueIDGeneratorTests
{
	[Fact]
	public void GuardClause()
	{
		using var generator = new UniqueIDGenerator();

		var ex = Record.Exception(() => generator.Add(null!));

		var anex = Assert.IsType<ArgumentNullException>(ex);
		Assert.Equal("value", anex.ParamName);
	}

	[Fact]
	public void NoData()
	{
		using var generator = new UniqueIDGenerator();

		var result = generator.Compute();

		Assert.Equal("e3b0c44298fc1c149afbf4c8996fb92427ae41e4649b934ca495991b7852b855", result);
	}

	[Fact]
	public void SingleString()
	{
		using var generator = new UniqueIDGenerator();
		generator.Add("Hello, world!");

		var result = generator.Compute();

		Assert.Equal("5450bb49d375ba935c1fe9c4dc48775d7d343fb97f22f07f8950f34a75a2708f", result);
	}

	[Fact]
	public void CannotAddAfterCompute()
	{
		using var generator = new UniqueIDGenerator();
		generator.Compute();

		var ex = Record.Exception(() => generator.Add("Hello, world!"));

		Assert.IsType<ObjectDisposedException>(ex);
		Assert.StartsWith("Cannot use UniqueIDGenerator after you have called Compute or Dispose", ex.Message);
	}

	[Fact]
	public void CannotComputeTwice()
	{
		using var generator = new UniqueIDGenerator();
		generator.Compute();

		var ex = Record.Exception(() => generator.Compute());

		Assert.IsType<ObjectDisposedException>(ex);
		Assert.StartsWith("Cannot use UniqueIDGenerator after you have called Compute or Dispose", ex.Message);
	}

	[Fact]
	public void CannotAddAfterDipose()
	{
		using var generator = new UniqueIDGenerator();
		generator.Dispose();

		var ex = Record.Exception(() => generator.Add("Hello, world!"));

		Assert.IsType<ObjectDisposedException>(ex);
		Assert.StartsWith("Cannot use UniqueIDGenerator after you have called Compute or Dispose", ex.Message);
	}

	[Fact]
	public void CannotComputeAfterDispose()
	{
		using var generator = new UniqueIDGenerator();
		generator.Dispose();

		var ex = Record.Exception(() => generator.Compute());

		Assert.IsType<ObjectDisposedException>(ex);
		Assert.StartsWith("Cannot use UniqueIDGenerator after you have called Compute or Dispose", ex.Message);
	}
}
