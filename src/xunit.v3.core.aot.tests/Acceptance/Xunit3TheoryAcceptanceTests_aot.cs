using Xunit;

partial class Xunit3TheoryAcceptanceTests
{
	static readonly string Ellipsis = new((char)0x00B7, 3);

	partial class TheoryTests
	{
		// Implicit/explicit conversion operators are not supported in Native AOT
		[Fact]
		public async ValueTask ImplicitExplicitConversions()
		{
			var results = await RunForResultsAsync("Xunit3TheoryAcceptanceTests+TheoryTests+ClassWithOperatorConversions");

			Assert.Collection(
				results.OfType<TestPassedWithMetadata>().Select(passed => passed.Test.TestDisplayName).OrderBy(x => x),
				displayName => Assert.Equal("Xunit3TheoryAcceptanceTests+TheoryTests+ClassWithOperatorConversions.DecimalToInt(value: 43)", displayName),
				displayName => Assert.Equal("Xunit3TheoryAcceptanceTests+TheoryTests+ClassWithOperatorConversions.IntToDecimal(value: 43)", displayName),
				displayName => Assert.Equal("Xunit3TheoryAcceptanceTests+TheoryTests+ClassWithOperatorConversions.IntToLong(i: 1)", displayName),
				displayName => Assert.Equal("Xunit3TheoryAcceptanceTests+TheoryTests+ClassWithOperatorConversions.UIntToULong(i: 1)", displayName)
			);
			Assert.Collection(
				results.OfType<TestFailedWithMetadata>().Select(failed => failed.Test.TestDisplayName).OrderBy(x => x),
				displayName => Assert.Equal($"Xunit3TheoryAcceptanceTests+TheoryTests+ClassWithOperatorConversions.ArgumentDeclaredExplicitConversion(value: Explicit {{ {Ellipsis} }})", displayName),
				displayName => Assert.Equal($"Xunit3TheoryAcceptanceTests+TheoryTests+ClassWithOperatorConversions.ArgumentDeclaredImplicitConversion(value: Implicit {{ {Ellipsis} }})", displayName),
				displayName => Assert.Equal(@"Xunit3TheoryAcceptanceTests+TheoryTests+ClassWithOperatorConversions.ParameterDeclaredExplicitConversion(e: ""abc"")", displayName),
				displayName => Assert.Equal(@"Xunit3TheoryAcceptanceTests+TheoryTests+ClassWithOperatorConversions.ParameterDeclaredImplicitConversion(i: ""abc"")", displayName)
			);
			Assert.Empty(results.OfType<TestSkippedWithMetadata>());
		}
	}
}
