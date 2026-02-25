using Xunit;

public partial class DynamicSkipAcceptanceTests
{
	static readonly string Ellipsis = new string((char)0x00B7, 3);

	public partial class Skip : AcceptanceTestV3
	{
		[Fact]
		public void GuardClause()
		{
			Assert.Throws<ArgumentNullException>("reason", () => Assert.Skip(null!));
		}

		[Fact]
		public async Task AcceptanceTest()
		{
#if XUNIT_AOT
			var results = await RunForResultsAsync("DynamicSkipAcceptanceTests+Skip+ClassUnderTest");
#else
			var results = await RunForResultsAsync(typeof(ClassUnderTest));
#endif

			var skipResult = Assert.Single(results.OfType<TestSkippedWithMetadata>());
			Assert.Equal("DynamicSkipAcceptanceTests+Skip+ClassUnderTest.Unconditional", skipResult.Test.TestDisplayName);
			Assert.Equal("This test was skipped", skipResult.Reason);
		}
	}

	public partial class SkipUnless : AcceptanceTestV3
	{
		[Fact]
		public void GuardClause()
		{
			Assert.Throws<ArgumentNullException>("reason", () => Assert.SkipUnless(true, null!));
		}

		[Fact]
		public async Task AcceptanceTest()
		{
#if XUNIT_AOT
			var results = await RunForResultsAsync("DynamicSkipAcceptanceTests+SkipUnless+ClassUnderTest");
#else
			var results = await RunForResultsAsync(typeof(ClassUnderTest));
#endif

			var skipResult = Assert.Single(results.OfType<TestSkippedWithMetadata>());
			Assert.Equal("DynamicSkipAcceptanceTests+SkipUnless+ClassUnderTest.Skipped", skipResult.Test.TestDisplayName);
			Assert.Equal("This test was skipped", skipResult.Reason);
			var passResult = Assert.Single(results.OfType<TestPassedWithMetadata>());
			Assert.Equal("DynamicSkipAcceptanceTests+SkipUnless+ClassUnderTest.Passed", passResult.Test.TestDisplayName);
		}
	}

	public partial class SkipWhen : AcceptanceTestV3
	{
		[Fact]
		public void GuardClause()
		{
			Assert.Throws<ArgumentNullException>("reason", () => Assert.SkipWhen(true, null!));
		}

		[Fact]
		public async Task AcceptanceTest()
		{
#if XUNIT_AOT
			var results = await RunForResultsAsync("DynamicSkipAcceptanceTests+SkipWhen+ClassUnderTest");
#else
			var results = await RunForResultsAsync(typeof(ClassUnderTest));
#endif

			var skipResult = Assert.Single(results.OfType<TestSkippedWithMetadata>());
			Assert.Equal("DynamicSkipAcceptanceTests+SkipWhen+ClassUnderTest.Skipped", skipResult.Test.TestDisplayName);
			Assert.Equal("This test was skipped", skipResult.Reason);
			var passResult = Assert.Single(results.OfType<TestPassedWithMetadata>());
			Assert.Equal("DynamicSkipAcceptanceTests+SkipWhen+ClassUnderTest.Passed", passResult.Test.TestDisplayName);
		}
	}

	public partial class SkipExceptions : AcceptanceTestV3
	{
		[Fact]
		public async Task AcceptanceTest()
		{
#if XUNIT_AOT
			var results = await RunForResultsAsync("DynamicSkipAcceptanceTests+SkipExceptions+ClassUnderTest");
#else
			var results = await RunForResultsAsync(typeof(ClassUnderTest));
#endif

			var failedResult = Assert.Single(results.OfType<TestFailedWithMetadata>());
			Assert.StartsWith("DynamicSkipAcceptanceTests+SkipExceptions+ClassUnderTest.TestMethod(ex: System.DivideByZeroException:", failedResult.Test.TestDisplayName);
			Assert.Empty(results.OfType<TestNotRunWithMetadata>());
			Assert.Empty(results.OfType<TestPassedWithMetadata>());
			Assert.Collection(
				results.OfType<TestSkippedWithMetadata>().OrderBy(t => t.Test.TestDisplayName),
				skipResult =>
				{
#if XUNIT_AOT  // Differences in behavior of ArgumentFormatter
					Assert.Equal($"DynamicSkipAcceptanceTests+SkipExceptions+ClassUnderTest.TestMethod(ex: MessagelessException {{ {Ellipsis} }})", skipResult.Test.TestDisplayName);
#else
					Assert.Equal("DynamicSkipAcceptanceTests+SkipExceptions+ClassUnderTest.TestMethod(ex: DynamicSkipAcceptanceTests+SkipExceptions+MessagelessException)", skipResult.Test.TestDisplayName);
#endif
					Assert.Equal("Exception of type 'DynamicSkipAcceptanceTests+SkipExceptions+MessagelessException' was thrown", skipResult.Reason);
				},
				skipResult =>
				{
#if XUNIT_AOT  // Differences in behavior of ArgumentFormatter
					Assert.Equal($"DynamicSkipAcceptanceTests+SkipExceptions+ClassUnderTest.TestMethod(ex: NullMessageException {{ {Ellipsis} }})", skipResult.Test.TestDisplayName);
#else
					Assert.Equal("DynamicSkipAcceptanceTests+SkipExceptions+ClassUnderTest.TestMethod(ex: DynamicSkipAcceptanceTests+SkipExceptions+NullMessageException)", skipResult.Test.TestDisplayName);
#endif
					Assert.Equal("Exception of type 'DynamicSkipAcceptanceTests+SkipExceptions+NullMessageException' was thrown", skipResult.Reason);
				},
				skipResult =>
				{
					Assert.Equal("DynamicSkipAcceptanceTests+SkipExceptions+ClassUnderTest.TestMethod(ex: System.NotImplementedException: The exception message)", skipResult.Test.TestDisplayName);
					Assert.Equal("The exception message", skipResult.Reason);
				}
			);
		}
	}
}
