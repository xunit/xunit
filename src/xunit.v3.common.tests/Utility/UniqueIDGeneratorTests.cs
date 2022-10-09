using System;
using Xunit;
using Xunit.Sdk;

public class UniqueIDGeneratorTests
{
	public class Compute
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

	public class ForAssembly
	{
		[Fact]
		public void GuardClause()
		{
			var ex = Record.Exception(() => UniqueIDGenerator.ForAssembly(null!, "asm-path", "config-path"));

			var argnEx = Assert.IsType<ArgumentNullException>(ex);
			Assert.Equal("assemblyName", argnEx.ParamName);
		}

		[Theory]
		[InlineData("asm-name", null, null, "bce3e8164ccc32eecdfef49e78069aa95aea6f76d67815b0ac2ee836bc478ea6")]
		[InlineData("asm-name", "asm-path", null, "705edea4327cfdf358252ca273366183df25159b9168a00a1f4c157229f8ba02")]
		[InlineData("asm-name", null, "config-path", "9b917782fbbf6985d53762753aff3b5af44a18d329489ef67fdc125ac26f8733")]
		[InlineData("asm-name", "asm-path", "config-path", "e24f7c871899e85c8fc304b6015bff534dae9947cb1cad8ae91e8d79d79f0a23")]
		public void SuccessCases(
			string assemblyName,
			string? assemblyPath,
			string? configFilePath,
			string expected)
		{
			var actual = UniqueIDGenerator.ForAssembly(assemblyName, assemblyPath, configFilePath);

			Assert.Equal(expected, actual);
		}

		[Fact]
		public void UniqueIDUsesOnlyShortAssemblyNameForDiscoveryVsExecutionConsistency()
		{
			var longName = "mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089";
			var shortName = "mscorlib";

			var longID = UniqueIDGenerator.ForAssembly(longName, null, null);
			var shortID = UniqueIDGenerator.ForAssembly(shortName, null, null);

			Assert.NotEmpty(longID);
			Assert.Equal(shortID, longID);
		}
	}
}
