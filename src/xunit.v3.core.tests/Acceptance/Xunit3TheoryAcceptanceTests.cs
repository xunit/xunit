using System.Diagnostics;
using Xunit;
using Xunit.Sdk;

public static partial class Xunit3TheoryAcceptanceTests
{
	public partial class DataAttributeTests : AcceptanceTestV3
	{
#if XUNIT_AOT
		[Fact]
		public static async ValueTask ExplicitAcceptanceTest_ExplicitOff()
#else
		[Theory]
		[InlineData(true)]
		[InlineData(false)]
		public static async ValueTask ExplicitAcceptanceTest_ExplicitOff(bool preEnumerateTheories)
#endif
		{
#if XUNIT_AOT
			var testMessages = await RunForResultsAsync("Xunit3TheoryAcceptanceTests+DataAttributeTests+ClassUnderTest_ExplicitAcceptanceTests", ExplicitOption.Off);
#else
			var testMessages = await RunForResultsAsync(typeof(ClassUnderTest_ExplicitAcceptanceTests), preEnumerateTheories, ExplicitOption.Off);
#endif

			var passedTests = testMessages.OfType<TestPassedWithMetadata>().OrderBy(x => x.Test.TestDisplayName).ToArray();
			Assert.Collection(
				passedTests,
				passed => Assert.Equal("Xunit3TheoryAcceptanceTests+DataAttributeTests+ClassUnderTest_ExplicitAcceptanceTests.TestWithTheoryExplicitFalse(x: 2112, _: \"Inline forced false\")", passed.Test.TestDisplayName),
				passed => Assert.Equal("Xunit3TheoryAcceptanceTests+DataAttributeTests+ClassUnderTest_ExplicitAcceptanceTests.TestWithTheoryExplicitFalse(x: 2113, _: \"Member forced false\")", passed.Test.TestDisplayName),
				passed => Assert.Equal("Xunit3TheoryAcceptanceTests+DataAttributeTests+ClassUnderTest_ExplicitAcceptanceTests.TestWithTheoryExplicitFalse(x: 42, _: \"Inline inherited\")", passed.Test.TestDisplayName),
				passed => Assert.Equal("Xunit3TheoryAcceptanceTests+DataAttributeTests+ClassUnderTest_ExplicitAcceptanceTests.TestWithTheoryExplicitFalse(x: 43, _: \"Member inherited\")", passed.Test.TestDisplayName),
				passed => Assert.Equal("Xunit3TheoryAcceptanceTests+DataAttributeTests+ClassUnderTest_ExplicitAcceptanceTests.TestWithTheoryExplicitTrue(x: 2112, _: \"Inline forced false\")", passed.Test.TestDisplayName),
				passed => Assert.Equal("Xunit3TheoryAcceptanceTests+DataAttributeTests+ClassUnderTest_ExplicitAcceptanceTests.TestWithTheoryExplicitTrue(x: 2113, _: \"Member forced false\")", passed.Test.TestDisplayName)
			);
			Assert.Empty(testMessages.OfType<TestFailedWithMetadata>());
			Assert.Empty(testMessages.OfType<TestSkippedWithMetadata>());
			var testsNotRun = testMessages.OfType<TestNotRunWithMetadata>().OrderBy(x => x.Test.TestDisplayName).ToArray();
			Assert.Collection(
				testsNotRun,
				notRun => Assert.Equal("Xunit3TheoryAcceptanceTests+DataAttributeTests+ClassUnderTest_ExplicitAcceptanceTests.TestWithTheoryExplicitFalse(x: 0, _: \"Inline forced true\")", notRun.Test.TestDisplayName),
				notRun => Assert.Equal("Xunit3TheoryAcceptanceTests+DataAttributeTests+ClassUnderTest_ExplicitAcceptanceTests.TestWithTheoryExplicitFalse(x: 0, _: \"Member forced true\")", notRun.Test.TestDisplayName),
				notRun => Assert.Equal("Xunit3TheoryAcceptanceTests+DataAttributeTests+ClassUnderTest_ExplicitAcceptanceTests.TestWithTheoryExplicitTrue(x: 0, _: \"Inline forced true\")", notRun.Test.TestDisplayName),
				notRun => Assert.Equal("Xunit3TheoryAcceptanceTests+DataAttributeTests+ClassUnderTest_ExplicitAcceptanceTests.TestWithTheoryExplicitTrue(x: 0, _: \"Member forced true\")", notRun.Test.TestDisplayName),
				notRun => Assert.Equal("Xunit3TheoryAcceptanceTests+DataAttributeTests+ClassUnderTest_ExplicitAcceptanceTests.TestWithTheoryExplicitTrue(x: 42, _: \"Inline inherited\")", notRun.Test.TestDisplayName),
				notRun => Assert.Equal("Xunit3TheoryAcceptanceTests+DataAttributeTests+ClassUnderTest_ExplicitAcceptanceTests.TestWithTheoryExplicitTrue(x: 43, _: \"Member inherited\")", notRun.Test.TestDisplayName)
			);
		}

#if XUNIT_AOT
		[Fact]
		public static async ValueTask ExplicitAcceptanceTest_ExplicitOn()
#else
		[Theory]
		[InlineData(true)]
		[InlineData(false)]
		public static async ValueTask ExplicitAcceptanceTest_ExplicitOn(bool preEnumerateTheories)
#endif
		{
#if XUNIT_AOT
			var testMessages = await RunForResultsAsync("Xunit3TheoryAcceptanceTests+DataAttributeTests+ClassUnderTest_ExplicitAcceptanceTests", ExplicitOption.On);
#else
			var testMessages = await RunForResultsAsync(typeof(ClassUnderTest_ExplicitAcceptanceTests), preEnumerateTheories, ExplicitOption.On);
#endif

			Assert.Collection(
				testMessages.OfType<TestPassedWithMetadata>().OrderBy(x => x.Test.TestDisplayName),
				passed => Assert.Equal("Xunit3TheoryAcceptanceTests+DataAttributeTests+ClassUnderTest_ExplicitAcceptanceTests.TestWithTheoryExplicitFalse(x: 2112, _: \"Inline forced false\")", passed.Test.TestDisplayName),
				passed => Assert.Equal("Xunit3TheoryAcceptanceTests+DataAttributeTests+ClassUnderTest_ExplicitAcceptanceTests.TestWithTheoryExplicitFalse(x: 2113, _: \"Member forced false\")", passed.Test.TestDisplayName),
				passed => Assert.Equal("Xunit3TheoryAcceptanceTests+DataAttributeTests+ClassUnderTest_ExplicitAcceptanceTests.TestWithTheoryExplicitFalse(x: 42, _: \"Inline inherited\")", passed.Test.TestDisplayName),
				passed => Assert.Equal("Xunit3TheoryAcceptanceTests+DataAttributeTests+ClassUnderTest_ExplicitAcceptanceTests.TestWithTheoryExplicitFalse(x: 43, _: \"Member inherited\")", passed.Test.TestDisplayName),
				passed => Assert.Equal("Xunit3TheoryAcceptanceTests+DataAttributeTests+ClassUnderTest_ExplicitAcceptanceTests.TestWithTheoryExplicitTrue(x: 2112, _: \"Inline forced false\")", passed.Test.TestDisplayName),
				passed => Assert.Equal("Xunit3TheoryAcceptanceTests+DataAttributeTests+ClassUnderTest_ExplicitAcceptanceTests.TestWithTheoryExplicitTrue(x: 2113, _: \"Member forced false\")", passed.Test.TestDisplayName),
				passed => Assert.Equal("Xunit3TheoryAcceptanceTests+DataAttributeTests+ClassUnderTest_ExplicitAcceptanceTests.TestWithTheoryExplicitTrue(x: 42, _: \"Inline inherited\")", passed.Test.TestDisplayName),
				passed => Assert.Equal("Xunit3TheoryAcceptanceTests+DataAttributeTests+ClassUnderTest_ExplicitAcceptanceTests.TestWithTheoryExplicitTrue(x: 43, _: \"Member inherited\")", passed.Test.TestDisplayName)
			);
			Assert.Collection(
				testMessages.OfType<TestFailedWithMetadata>().OrderBy(x => x.Test.TestDisplayName),
				failed => Assert.Equal("Xunit3TheoryAcceptanceTests+DataAttributeTests+ClassUnderTest_ExplicitAcceptanceTests.TestWithTheoryExplicitFalse(x: 0, _: \"Inline forced true\")", failed.Test.TestDisplayName),
				failed => Assert.Equal("Xunit3TheoryAcceptanceTests+DataAttributeTests+ClassUnderTest_ExplicitAcceptanceTests.TestWithTheoryExplicitFalse(x: 0, _: \"Member forced true\")", failed.Test.TestDisplayName),
				failed => Assert.Equal("Xunit3TheoryAcceptanceTests+DataAttributeTests+ClassUnderTest_ExplicitAcceptanceTests.TestWithTheoryExplicitTrue(x: 0, _: \"Inline forced true\")", failed.Test.TestDisplayName),
				failed => Assert.Equal("Xunit3TheoryAcceptanceTests+DataAttributeTests+ClassUnderTest_ExplicitAcceptanceTests.TestWithTheoryExplicitTrue(x: 0, _: \"Member forced true\")", failed.Test.TestDisplayName)
			);
			Assert.Empty(testMessages.OfType<TestSkippedWithMetadata>());
			Assert.Empty(testMessages.OfType<TestNotRunWithMetadata>());
		}

#if XUNIT_AOT
		[Fact]
		public static async ValueTask ExplicitAcceptanceTest_ExplicitOnly()
#else
		[Theory]
		[InlineData(true)]
		[InlineData(false)]
		public static async ValueTask ExplicitAcceptanceTest_ExplicitOnly(bool preEnumerateTheories)
#endif
		{
#if XUNIT_AOT
			var testMessages = await RunForResultsAsync("Xunit3TheoryAcceptanceTests+DataAttributeTests+ClassUnderTest_ExplicitAcceptanceTests", ExplicitOption.Only);
#else
			var testMessages = await RunForResultsAsync(typeof(ClassUnderTest_ExplicitAcceptanceTests), preEnumerateTheories, ExplicitOption.Only);
#endif

			Assert.Collection(
				testMessages.OfType<TestPassedWithMetadata>().OrderBy(x => x.Test.TestDisplayName),
				passed => Assert.Equal("Xunit3TheoryAcceptanceTests+DataAttributeTests+ClassUnderTest_ExplicitAcceptanceTests.TestWithTheoryExplicitTrue(x: 42, _: \"Inline inherited\")", passed.Test.TestDisplayName),
				passed => Assert.Equal("Xunit3TheoryAcceptanceTests+DataAttributeTests+ClassUnderTest_ExplicitAcceptanceTests.TestWithTheoryExplicitTrue(x: 43, _: \"Member inherited\")", passed.Test.TestDisplayName)
			);
			Assert.Collection(
				testMessages.OfType<TestFailedWithMetadata>().OrderBy(x => x.Test.TestDisplayName),
				failed => Assert.Equal("Xunit3TheoryAcceptanceTests+DataAttributeTests+ClassUnderTest_ExplicitAcceptanceTests.TestWithTheoryExplicitFalse(x: 0, _: \"Inline forced true\")", failed.Test.TestDisplayName),
				failed => Assert.Equal("Xunit3TheoryAcceptanceTests+DataAttributeTests+ClassUnderTest_ExplicitAcceptanceTests.TestWithTheoryExplicitFalse(x: 0, _: \"Member forced true\")", failed.Test.TestDisplayName),
				failed => Assert.Equal("Xunit3TheoryAcceptanceTests+DataAttributeTests+ClassUnderTest_ExplicitAcceptanceTests.TestWithTheoryExplicitTrue(x: 0, _: \"Inline forced true\")", failed.Test.TestDisplayName),
				failed => Assert.Equal("Xunit3TheoryAcceptanceTests+DataAttributeTests+ClassUnderTest_ExplicitAcceptanceTests.TestWithTheoryExplicitTrue(x: 0, _: \"Member forced true\")", failed.Test.TestDisplayName)
			);
			Assert.Empty(testMessages.OfType<TestSkippedWithMetadata>());
			Assert.Collection(
				testMessages.OfType<TestNotRunWithMetadata>().OrderBy(x => x.Test.TestDisplayName),
				notRun => Assert.Equal("Xunit3TheoryAcceptanceTests+DataAttributeTests+ClassUnderTest_ExplicitAcceptanceTests.TestWithTheoryExplicitFalse(x: 2112, _: \"Inline forced false\")", notRun.Test.TestDisplayName),
				notRun => Assert.Equal("Xunit3TheoryAcceptanceTests+DataAttributeTests+ClassUnderTest_ExplicitAcceptanceTests.TestWithTheoryExplicitFalse(x: 2113, _: \"Member forced false\")", notRun.Test.TestDisplayName),
				notRun => Assert.Equal("Xunit3TheoryAcceptanceTests+DataAttributeTests+ClassUnderTest_ExplicitAcceptanceTests.TestWithTheoryExplicitFalse(x: 42, _: \"Inline inherited\")", notRun.Test.TestDisplayName),
				notRun => Assert.Equal("Xunit3TheoryAcceptanceTests+DataAttributeTests+ClassUnderTest_ExplicitAcceptanceTests.TestWithTheoryExplicitFalse(x: 43, _: \"Member inherited\")", notRun.Test.TestDisplayName),
				notRun => Assert.Equal("Xunit3TheoryAcceptanceTests+DataAttributeTests+ClassUnderTest_ExplicitAcceptanceTests.TestWithTheoryExplicitTrue(x: 2112, _: \"Inline forced false\")", notRun.Test.TestDisplayName),
				notRun => Assert.Equal("Xunit3TheoryAcceptanceTests+DataAttributeTests+ClassUnderTest_ExplicitAcceptanceTests.TestWithTheoryExplicitTrue(x: 2113, _: \"Member forced false\")", notRun.Test.TestDisplayName)
			);
		}

#if XUNIT_AOT
		[Fact]
		public static async ValueTask LabelAcceptanceTests()
#else
		[Theory]
		[InlineData(true)]
		[InlineData(false)]
		public static async ValueTask LabelAcceptanceTests(bool preEnumerateTheories)
#endif
		{
#if XUNIT_AOT
			var testMessages = await RunForResultsAsync("Xunit3TheoryAcceptanceTests+DataAttributeTests+ClassUnderTest_LabelAcceptanceTests");
#else
			var testMessages = await RunForResultsAsync(typeof(ClassUnderTest_LabelAcceptanceTests), preEnumerateTheories);
#endif

			Assert.Collection(
				testMessages.OfType<TestPassedWithMetadata>().OrderBy(x => x.Test.TestDisplayName),
				passed => Assert.Equal("Xunit3TheoryAcceptanceTests+DataAttributeTests+ClassUnderTest_LabelAcceptanceTests.TestMethod_Inline [Custom inline]", passed.Test.TestDisplayName),
				passed => Assert.Equal("Xunit3TheoryAcceptanceTests+DataAttributeTests+ClassUnderTest_LabelAcceptanceTests.TestMethod_Inline(x: 42, _: \"Inline unset\")", passed.Test.TestDisplayName),
				passed => Assert.Equal("Xunit3TheoryAcceptanceTests+DataAttributeTests+ClassUnderTest_LabelAcceptanceTests.TestMethod_Member [Custom member]", passed.Test.TestDisplayName),
				passed => Assert.Equal("Xunit3TheoryAcceptanceTests+DataAttributeTests+ClassUnderTest_LabelAcceptanceTests.TestMethod_Member(x: 42, _: \"Member unset\")", passed.Test.TestDisplayName),
				passed => Assert.Equal("Xunit3TheoryAcceptanceTests+DataAttributeTests+ClassUnderTest_LabelAcceptanceTests.TestMethod_MemberWithBaseLabel [Base label]", passed.Test.TestDisplayName),
				passed => Assert.Equal("Xunit3TheoryAcceptanceTests+DataAttributeTests+ClassUnderTest_LabelAcceptanceTests.TestMethod_MemberWithBaseLabel [Custom member]", passed.Test.TestDisplayName)
			);
			Assert.Collection(
				testMessages.OfType<TestFailedWithMetadata>().OrderBy(x => x.Test.TestDisplayName),
				failed => Assert.Equal("Xunit3TheoryAcceptanceTests+DataAttributeTests+ClassUnderTest_LabelAcceptanceTests.TestMethod_Inline", failed.Test.TestDisplayName),
				failed => Assert.Equal("Xunit3TheoryAcceptanceTests+DataAttributeTests+ClassUnderTest_LabelAcceptanceTests.TestMethod_Member", failed.Test.TestDisplayName),
				failed => Assert.Equal("Xunit3TheoryAcceptanceTests+DataAttributeTests+ClassUnderTest_LabelAcceptanceTests.TestMethod_MemberWithBaseLabel", failed.Test.TestDisplayName)
			);
			Assert.Empty(testMessages.OfType<TestSkippedWithMetadata>());
			Assert.Empty(testMessages.OfType<TestNotRunWithMetadata>());
		}

#if XUNIT_AOT
		[Fact]
		public static async ValueTask SkipAcceptanceTest()
#else
		[Theory]
		[InlineData(true)]
		[InlineData(false)]
		public static async ValueTask SkipAcceptanceTest(bool preEnumerateTheories)
#endif
		{
#if XUNIT_AOT
			var testMessages = await RunForResultsAsync("Xunit3TheoryAcceptanceTests+DataAttributeTests+ClassUnderTest_SkipTests");
#else
			var testMessages = await RunForResultsAsync(typeof(ClassUnderTest_SkipTests), preEnumerateTheories);
#endif

			Assert.Collection(
				testMessages.OfType<TestPassedWithMetadata>().OrderBy(x => x.Test.TestDisplayName),
				// Two passing for TestWithDynamicSkipOnTheory
				passed => Assert.Equal("Xunit3TheoryAcceptanceTests+DataAttributeTests+ClassUnderTest_SkipTests.TestWithDynamicSkipOnTheory(_: 1)", passed.Test.TestDisplayName),
				passed => Assert.Equal("Xunit3TheoryAcceptanceTests+DataAttributeTests+ClassUnderTest_SkipTests.TestWithDynamicSkipOnTheory(_: 2)", passed.Test.TestDisplayName),
				// Single passing for TestWithNoSkipOnTheory
				passed => Assert.Equal("Xunit3TheoryAcceptanceTests+DataAttributeTests+ClassUnderTest_SkipTests.TestWithNoSkipOnTheory(_: 42)", passed.Test.TestDisplayName)
			);
			Assert.Collection(
				testMessages.OfType<TestSkippedWithMetadata>().OrderBy(x => x.Test.TestDisplayName),
				// Skipped tests for TestWithDynamicSkipOnTheory
				skipped =>
				{
					Assert.Equal("Xunit3TheoryAcceptanceTests+DataAttributeTests+ClassUnderTest_SkipTests.TestWithDynamicSkipOnTheory(_: 3)", skipped.Test.TestDisplayName);
					Assert.Equal("Always skipped", skipped.Reason);
				},
				skipped =>
				{
					Assert.Equal("Xunit3TheoryAcceptanceTests+DataAttributeTests+ClassUnderTest_SkipTests.TestWithDynamicSkipOnTheory(_: 4)", skipped.Test.TestDisplayName);
					Assert.Equal("Skip dynamically flipped", skipped.Reason);
				},
				// Skip per data row for TestWithNoSkipOnTheory
				skipped =>
				{
					Assert.Equal("Xunit3TheoryAcceptanceTests+DataAttributeTests+ClassUnderTest_SkipTests.TestWithNoSkipOnTheory(_: 2112)", skipped.Test.TestDisplayName);
					Assert.Equal("Skip from InlineData", skipped.Reason);
				},
				skipped =>
				{
					Assert.Equal("Xunit3TheoryAcceptanceTests+DataAttributeTests+ClassUnderTest_SkipTests.TestWithNoSkipOnTheory(_: 2113)", skipped.Test.TestDisplayName);
					Assert.Equal("Skip from theory data row", skipped.Reason);
				},
				skipped =>
				{
					Assert.Equal("Xunit3TheoryAcceptanceTests+DataAttributeTests+ClassUnderTest_SkipTests.TestWithNoSkipOnTheory(_: 43)", skipped.Test.TestDisplayName);
					Assert.Equal("Skip from MemberData", skipped.Reason);
				},
				// Single skipped theory, not one per data row, for TestWithSkipOnTheory
				skipped =>
				{
					Assert.Equal("Xunit3TheoryAcceptanceTests+DataAttributeTests+ClassUnderTest_SkipTests.TestWithSkipOnTheory", skipped.Test.TestDisplayName);
					Assert.Equal("Skip from theory", skipped.Reason);
				}
			);
		}

#if XUNIT_AOT
		[Fact]
		public static async ValueTask TestDisplayNameAcceptanceTest()
#else
		[Theory]
		[InlineData(true)]
		[InlineData(false)]
		public static async ValueTask TestDisplayNameAcceptanceTest(bool preEnumerateTheories)
#endif
		{
#if XUNIT_AOT
			var testMessages = await RunForResultsAsync("Xunit3TheoryAcceptanceTests+DataAttributeTests+ClassUnderTest_TestDisplayNameTests");
#else
			var testMessages = await RunForResultsAsync(typeof(ClassUnderTest_TestDisplayNameTests), preEnumerateTheories);
#endif

			Assert.Collection(
				testMessages.OfType<TestPassedWithMetadata>().OrderBy(x => x.Test.TestDisplayName),
				passed => Assert.Equal("Default Member Test(_: 43)", passed.Test.TestDisplayName),
				passed => Assert.Equal("One Test Default (Member)(_: 1)", passed.Test.TestDisplayName),
				passed => Assert.Equal("Override Member Test(_: 45)", passed.Test.TestDisplayName),
				passed => Assert.Equal("Theory Display Name(_: 44)", passed.Test.TestDisplayName),
				passed => Assert.Equal("Three Test Override (Member)(_: 3)", passed.Test.TestDisplayName),
				passed => Assert.Equal("Two Test Override (Inline)(_: 2)", passed.Test.TestDisplayName),
				passed => Assert.Equal("Xunit3TheoryAcceptanceTests+DataAttributeTests+ClassUnderTest_TestDisplayNameTests.TestWithDefaultName(_: 42)", passed.Test.TestDisplayName),
				passed => Assert.Equal("Zero Test Default (Inline)(_: 0)", passed.Test.TestDisplayName)
			);
		}

#if XUNIT_AOT
		[Fact]
		public static async ValueTask TraitsAcceptanceTest()
#else
		[Theory]
		[InlineData(true)]
		[InlineData(false)]
		public static async ValueTask TraitsAcceptanceTest(bool preEnumerateTheories)
#endif
		{
#if XUNIT_AOT
			var testMessages = await RunForResultsAsync("Xunit3TheoryAcceptanceTests+DataAttributeTests+ClassUnderTests_TraitsTests");
#else
			var testMessages = await RunForResultsAsync(typeof(ClassUnderTests_TraitsTests), preEnumerateTheories);
#endif

			Assert.Collection(
				testMessages.OrderBy(x => x.Test.TestDisplayName),
				starting =>
				{
					Assert.Equal("Xunit3TheoryAcceptanceTests+DataAttributeTests+ClassUnderTests_TraitsTests.TestMethod(_: 0)", starting.Test.TestDisplayName);
#if XUNIT_AOT
					Assert.Equal(["Assembly", "Class", "Collection", "InlineData", "Method"], starting.Test.Traits["Location"].OrderBy(x => x).ToArray());
#else
					Assert.Equal(["Class", "Collection", "InlineData", "Method"], starting.Test.Traits["Location"].OrderBy(x => x).ToArray());
#endif
					Assert.False(starting.Test.Traits.ContainsKey("Discarded"));
				},
				starting =>
				{
					Assert.Equal("Xunit3TheoryAcceptanceTests+DataAttributeTests+ClassUnderTests_TraitsTests.TestMethod(_: 2112)", starting.Test.TestDisplayName);
#if XUNIT_AOT
					Assert.Equal(["Assembly", "Class", "Collection", "MemberData", "Method"], starting.Test.Traits["Location"].OrderBy(x => x).ToArray());
#else
					Assert.Equal(["Class", "Collection", "MemberData", "Method"], starting.Test.Traits["Location"].OrderBy(x => x).ToArray());
#endif
					Assert.False(starting.Test.Traits.ContainsKey("Discarded"));
				},
				starting =>
				{
					Assert.Equal("Xunit3TheoryAcceptanceTests+DataAttributeTests+ClassUnderTests_TraitsTests.TestMethod(_: 42)", starting.Test.TestDisplayName);
#if XUNIT_AOT
					Assert.Equal(["Assembly", "Class", "Collection", "MemberData", "Method", "TheoryDataRow"], starting.Test.Traits["Location"].OrderBy(x => x).ToArray());
#else
					Assert.Equal(["Class", "Collection", "MemberData", "Method", "TheoryDataRow"], starting.Test.Traits["Location"].OrderBy(x => x).ToArray());
#endif
					Assert.False(starting.Test.Traits.ContainsKey("Discarded"));
				}
			);
		}
	}

	[CollectionDefinition("Timeout Tests", DisableParallelization = true)]
	public static class TimeoutTestsCollection { }

	[Collection("Timeout Tests")]
	public partial class DataAttributeTimeoutTests : AcceptanceTestV3
	{
#if XUNIT_AOT
		[Fact(Skip = "Cannot run under a debugger", SkipWhen = nameof(Debugger.IsAttached), SkipType = typeof(Debugger))]
		public static async ValueTask TimeoutAcceptanceTest()
#else
		[Theory(Skip = "Cannot run under a debugger", SkipWhen = nameof(Debugger.IsAttached), SkipType = typeof(Debugger))]
		[InlineData(true)]
		[InlineData(false)]
		public static async ValueTask TimeoutAcceptanceTest(bool preEnumerateTheories)
#endif
		{
			var stopwatch = Stopwatch.StartNew();
#if XUNIT_AOT
			var results = await RunForResultsAsync("Xunit3TheoryAcceptanceTests+DataAttributeTimeoutTests+ClassUnderTest");
#else
			var results = await RunForResultsAsync(typeof(ClassUnderTest), preEnumerateTheories);
#endif
			stopwatch.Stop();

			Assert.Collection(
				results.OfType<TestPassedWithMetadata>().OrderBy(x => x.Test.TestDisplayName),
				passed => Assert.Equal("Xunit3TheoryAcceptanceTests+DataAttributeTimeoutTests+ClassUnderTest.LongRunningTask(delay: 10)", passed.Test.TestDisplayName),
				passed => Assert.Equal("Xunit3TheoryAcceptanceTests+DataAttributeTimeoutTests+ClassUnderTest.LongRunningTask(delay: 100)", passed.Test.TestDisplayName)
			);
			Assert.Collection(
				results.OfType<TestFailedWithMetadata>().OrderBy(f => f.Test.TestDisplayName),
				failed =>
				{
					Assert.Equal("Xunit3TheoryAcceptanceTests+DataAttributeTimeoutTests+ClassUnderTest.LongRunningTask(delay: 10000)", failed.Test.TestDisplayName);
					Assert.Equal("Test execution timed out after 42 milliseconds", failed.Messages.Single());
				},
				failed =>
				{
					Assert.Equal("Xunit3TheoryAcceptanceTests+DataAttributeTimeoutTests+ClassUnderTest.LongRunningTask(delay: 11000)", failed.Test.TestDisplayName);
					Assert.Equal("Test execution timed out after 10 milliseconds", failed.Messages.Single());
				}
			);

			Assert.True(stopwatch.ElapsedMilliseconds < 10000, "Elapsed time should be less than 10 seconds");
		}
	}

	public partial class DataConversionTests : AcceptanceTestV3
	{
		[Fact]
		public static async ValueTask IncompatibleDataThrows()
		{
#if XUNIT_AOT
			var testMessages = await RunForResultsAsync("Xunit3TheoryAcceptanceTests+DataConversionTests+ClassWithIncompatibleData");
#else
			var testMessages = await RunForResultsAsync(typeof(ClassWithIncompatibleData));
#endif

			var failed = Assert.Single(testMessages.OfType<TestFailedWithMetadata>());
			Assert.Equal(@"Xunit3TheoryAcceptanceTests+DataConversionTests+ClassWithIncompatibleData.TestViaIncompatibleData(_: ""Foo"")", failed.Test.TestDisplayName);
#if XUNIT_AOT
			Assert.Equal("Xunit.Sdk.TestPipelineException", failed.ExceptionTypes.Single());
			Assert.Equal("Test method had one or more invalid theory data arguments: int _ (Foo)", failed.Messages.Single());
#else
			Assert.Equal("System.ArgumentException", failed.ExceptionTypes.Single());
			Assert.Equal("Object of type 'System.String' cannot be converted to type 'System.Int32'.", failed.Messages.Single());
#endif
		}

		[Fact]
		public static async ValueTask ImplicitlyConvertibleDataPasses()
		{
#if XUNIT_AOT
			var testMessages = await RunForResultsAsync("Xunit3TheoryAcceptanceTests+DataConversionTests+ClassWithImplicitlyConvertibleData");
#else
			var testMessages = await RunForResultsAsync(typeof(ClassWithImplicitlyConvertibleData));
#endif

			var passed = Assert.Single(testMessages.OfType<TestPassedWithMetadata>());
			Assert.Equal(@"Xunit3TheoryAcceptanceTests+DataConversionTests+ClassWithImplicitlyConvertibleData.TestViaImplicitData(_: 42)", passed.Test.TestDisplayName);
		}

		[Fact]
		public static async ValueTask IConvertibleDataPasses()
		{
#if XUNIT_AOT
			var testMessages = await RunForResultsAsync("Xunit3TheoryAcceptanceTests+DataConversionTests+ClassWithIConvertibleData");
#else
			var testMessages = await RunForResultsAsync(typeof(ClassWithIConvertibleData));
#endif

			var passed = Assert.Single(testMessages.OfType<TestPassedWithMetadata>());
#if XUNIT_AOT
			Assert.Equal($"Xunit3TheoryAcceptanceTests+DataConversionTests+ClassWithIConvertibleData.TestViaIConvertible(_: MyConvertible {{ {Ellipsis} }})", passed.Test.TestDisplayName);
#else
			Assert.Equal("Xunit3TheoryAcceptanceTests+DataConversionTests+ClassWithIConvertibleData.TestViaIConvertible(_: 42)", passed.Test.TestDisplayName);
#endif
		}
	}

	public partial class ErrorAggregation : AcceptanceTestV3
	{
		[Fact]
		public static async ValueTask EachTheoryHasIndividualExceptionMessage()
		{
#if XUNIT_AOT
			var testMessages = await RunForResultsAsync("Xunit3TheoryAcceptanceTests+ErrorAggregation+ClassUnderTest");
#else
			var testMessages = await RunForResultsAsync(typeof(ClassUnderTest));
#endif

			Assert.Collection(
				testMessages.OfType<TestFailedWithMetadata>().OrderBy(t => t.Test.TestDisplayName),
				failure =>
				{
					Assert.Equal("Xunit3TheoryAcceptanceTests+ErrorAggregation+ClassUnderTest.TestMethod(x: 0, _: 0, z: null)", failure.Test.TestDisplayName);
					Assert.Contains("Assert.NotNull() Failure", failure.Messages.Single());
				},
				failure =>
				{
#if XUNIT_AOT
					Assert.Equal($"Xunit3TheoryAcceptanceTests+ErrorAggregation+ClassUnderTest.TestMethod(x: 42, _: 21.120000000000001, z: ClassUnderTest {{ {Ellipsis} }})", failure.Test.TestDisplayName);
#else
					Assert.Equal("Xunit3TheoryAcceptanceTests+ErrorAggregation+ClassUnderTest.TestMethod(x: 42, _: 21.120000000000001, z: ClassUnderTest { })", failure.Test.TestDisplayName);
#endif
					Assert.Contains("Assert.Equal() Failure", failure.Messages.Single());
				}
			);
		}
	}

	public partial class InlineDataTests : AcceptanceTestV3
	{
		[Fact]
		public static async ValueTask RunsForEachDataElement()
		{
#if XUNIT_AOT
			var testMessages = await RunForResultsAsync("Xunit3TheoryAcceptanceTests+InlineDataTests+ClassUnderTest");
#else
			var testMessages = await RunForResultsAsync(typeof(ClassUnderTest));
#endif
			var passed = Assert.Single(testMessages.OfType<TestPassedWithMetadata>());
			Assert.Equal($"Xunit3TheoryAcceptanceTests+InlineDataTests+ClassUnderTest.TestMethod(_1: 42, _2: {21.12:G17}, z: \"Hello, world!\")", passed.Test.TestDisplayName);
			var failed = Assert.Single(testMessages.OfType<TestFailedWithMetadata>());
			Assert.Equal("Xunit3TheoryAcceptanceTests+InlineDataTests+ClassUnderTest.TestMethod(_1: 0, _2: 0, z: null)", failed.Test.TestDisplayName);
			Assert.Empty(testMessages.OfType<TestSkippedWithMetadata>());
		}

		[Fact]
		public static async ValueTask SingleNullValuesWork()
		{
#if XUNIT_AOT
			var testMessages = await RunForResultsAsync("Xunit3TheoryAcceptanceTests+InlineDataTests+ClassUnderTestForNullValues");
#else
			var testMessages = await RunForResultsAsync(typeof(ClassUnderTestForNullValues));
#endif

			var passed = Assert.Single(testMessages.OfType<TestPassedWithMetadata>());
			Assert.Equal("Xunit3TheoryAcceptanceTests+InlineDataTests+ClassUnderTestForNullValues.TestMethod(_: null)", passed.Test.TestDisplayName);
		}

		[Fact]
		public static async ValueTask ArraysWork()
		{
#if XUNIT_AOT
			var testMessages = await RunForResultsAsync("Xunit3TheoryAcceptanceTests+InlineDataTests+ClassUnderTestForArrays");
#else
			var testMessages = await RunForResultsAsync(typeof(ClassUnderTestForArrays));
#endif

			var passed = Assert.Single(testMessages.OfType<TestPassedWithMetadata>());
			Assert.Contains("Xunit3TheoryAcceptanceTests+InlineDataTests+ClassUnderTestForArrays.TestMethod", passed.Test.TestDisplayName);
		}

		[Fact]
		public static async ValueTask ValueArraysWithObjectParameterInjectCorrectType()
		{
#if XUNIT_AOT
			var testMessages = await RunForResultsAsync("Xunit3TheoryAcceptanceTests+InlineDataTests+ClassUnderTestForValueArraysWithObjectParameter");
#else
			var testMessages = await RunForResultsAsync(typeof(ClassUnderTestForValueArraysWithObjectParameter));
#endif

			var passed = Assert.Single(testMessages.OfType<TestPassedWithMetadata>());
			Assert.Contains("Xunit3TheoryAcceptanceTests+InlineDataTests+ClassUnderTestForValueArraysWithObjectParameter.TestMethod", passed.Test.TestDisplayName);
		}

		[Fact]
		public static async ValueTask AsyncTaskMethod_MultipleInlineDataAttributes()
		{
#if XUNIT_AOT
			var testMessages = await RunForResultsAsync("Xunit3TheoryAcceptanceTests+InlineDataTests+ClassWithAsyncTaskMethod");
#else
			var testMessages = await RunForResultsAsync(typeof(ClassWithAsyncTaskMethod));
#endif

			Assert.Collection(
				testMessages.OfType<TestPassedWithMetadata>().Select(passed => passed.Test.TestDisplayName).OrderBy(x => x),
				displayName => Assert.Equal("Xunit3TheoryAcceptanceTests+InlineDataTests+ClassWithAsyncTaskMethod.TestMethod(_: A)", displayName),
				displayName => Assert.Equal("Xunit3TheoryAcceptanceTests+InlineDataTests+ClassWithAsyncTaskMethod.TestMethod(_: B)", displayName)
			);
		}

		// https://github.com/xunit/xunit/issues/3524
		[Fact]
		public static async ValueTask SpecialFloatingPointValuesAreSupported()
		{
#if XUNIT_AOT
			var testMessages = await RunForResultsAsync("Xunit3TheoryAcceptanceTests+InlineDataTests+ClassWithSpecialFloatingPointValues");
#else
			var testMessages = await RunForResultsAsync(typeof(ClassWithSpecialFloatingPointValues));
#endif

			Assert.Collection(
				testMessages.OfType<TestPassedWithMetadata>().Select(passed => passed.Test.TestDisplayName).OrderBy(x => x),
#if NETFRAMEWORK  // Unclear why these sort differently in .NET vs. NET Framework
				displayName => Assert.Equal("Xunit3TheoryAcceptanceTests+InlineDataTests+ClassWithSpecialFloatingPointValues.TestMethod(_1: Infinity, _2: Infinity)", displayName),
				displayName => Assert.Equal("Xunit3TheoryAcceptanceTests+InlineDataTests+ClassWithSpecialFloatingPointValues.TestMethod(_1: -Infinity, _2: -Infinity)", displayName),
#else
				displayName => Assert.Equal("Xunit3TheoryAcceptanceTests+InlineDataTests+ClassWithSpecialFloatingPointValues.TestMethod(_1: -Infinity, _2: -Infinity)", displayName),
				displayName => Assert.Equal("Xunit3TheoryAcceptanceTests+InlineDataTests+ClassWithSpecialFloatingPointValues.TestMethod(_1: Infinity, _2: Infinity)", displayName),
#endif
				displayName => Assert.Equal("Xunit3TheoryAcceptanceTests+InlineDataTests+ClassWithSpecialFloatingPointValues.TestMethod(_1: NaN, _2: NaN)", displayName)
			);
		}
	}

	public static class LabelTests
	{
		[Theory(DisableDiscoveryEnumeration = false)]
		[InlineData(null)]
		[InlineData("", Label = "")]
		[InlineData("abc123", Label = "abc123")]
		public static void LabelAvailable_SerializableData_WithPreEnumeration(string? expectedLabel) =>
			Assert.Equal(expectedLabel, TestContext.Current.Test!.TestLabel);

		[Theory(DisableDiscoveryEnumeration = true)]
		[InlineData(null)]
		[InlineData("", Label = "")]
		[InlineData("abc123", Label = "abc123")]
		public static void LabelAvailable_SerializableData_NoPreEnumeration(string? expectedLabel) =>
			Assert.Equal(expectedLabel, TestContext.Current.Test!.TestLabel);

		public static IEnumerable<TheoryDataRow<object, string?>> NonSerializableData =>
		[
			new(new(), null),
			new(new(), "") { Label = "" },
			new(new(), "abc123") { Label = "abc123" },
		];

		[Theory(DisableDiscoveryEnumeration = true)]
		[MemberData(nameof(NonSerializableData))]
		public static void LabelAvailable_NonSerializableData(object _, string? expectedLabel) =>
			Assert.Equal(expectedLabel, TestContext.Current.Test!.TestLabel);
	}

	public partial class MemberDataTests : AcceptanceTestV3
	{
		[Theory]
#if XUNIT_AOT
		[InlineData("Xunit3TheoryAcceptanceTests+MemberDataTests+ClassUnderTest_IAsyncEnumerable")]
		[InlineData("Xunit3TheoryAcceptanceTests+MemberDataTests+ClassUnderTest_IEnumerable")]
		[InlineData("Xunit3TheoryAcceptanceTests+MemberDataTests+ClassUnderTest_TaskOfIAsyncEnumerable")]
		[InlineData("Xunit3TheoryAcceptanceTests+MemberDataTests+ClassUnderTest_TaskOfIEnumerable")]
		[InlineData("Xunit3TheoryAcceptanceTests+MemberDataTests+ClassUnderTest_ValueTaskOfIAsyncEnumerable")]
		[InlineData("Xunit3TheoryAcceptanceTests+MemberDataTests+ClassUnderTest_ValueTaskOfIEnumerable")]
		public static async ValueTask AcceptanceTest(string className)
#else
		[InlineData(typeof(ClassUnderTest_IAsyncEnumerable))]
		[InlineData(typeof(ClassUnderTest_IEnumerable))]
		[InlineData(typeof(ClassUnderTest_TaskOfIAsyncEnumerable))]
		[InlineData(typeof(ClassUnderTest_TaskOfIEnumerable))]
		[InlineData(typeof(ClassUnderTest_ValueTaskOfIAsyncEnumerable))]
		[InlineData(typeof(ClassUnderTest_ValueTaskOfIEnumerable))]
		public static async ValueTask AcceptanceTest(Type classUnderTest)
#endif
		{
#if XUNIT_AOT
			var testMessages = await RunForResultsAsync(className);
#else
			var testMessages = await RunForResultsAsync(classUnderTest);
			var className = classUnderTest.SafeName();
#endif

			Assert.Collection(
				testMessages.OfType<TestPassedWithMetadata>().Select(passed => passed.Test.TestDisplayName).OrderBy(x => x),
				displayName => Assert.Equal($"{className}.FieldTestMethod(z: \"Hello from base\")", displayName),
				displayName => Assert.Equal($"{className}.FieldTestMethod(z: \"Hello from other source\")", displayName),
				displayName => Assert.Equal($"{className}.FieldTestMethod(z: \"Hello, world!\")", displayName),
				displayName => Assert.Equal($"{className}.MethodTestMethod(z: \"Hello from base\")", displayName),
				displayName => Assert.Equal($"{className}.MethodTestMethod(z: \"Hello from other source\")", displayName),
				displayName => Assert.Equal($"{className}.MethodTestMethod(z: \"Hello, world!\")", displayName),
				displayName => Assert.Equal($"{className}.PropertyTestMethod(z: \"Hello from base\")", displayName),
				displayName => Assert.Equal($"{className}.PropertyTestMethod(z: \"Hello from other source\")", displayName),
				displayName => Assert.Equal($"{className}.PropertyTestMethod(z: \"Hello, world!\")", displayName)
			);
			Assert.Collection(
				testMessages.OfType<TestFailedWithMetadata>().Select(failed => failed.Test.TestDisplayName).OrderBy(x => x),
				displayName => Assert.Equal($"{className}.FieldTestMethod(z: \"Base will fail\")", displayName),
				displayName => Assert.Equal($"{className}.FieldTestMethod(z: \"I will fail\")", displayName),
				displayName => Assert.Equal($"{className}.FieldTestMethod(z: \"Other source will fail\")", displayName),
				displayName => Assert.Equal($"{className}.MethodTestMethod(z: \"Base will fail\")", displayName),
				displayName => Assert.Equal($"{className}.MethodTestMethod(z: \"I will fail\")", displayName),
				displayName => Assert.Equal($"{className}.MethodTestMethod(z: \"Other source will fail\")", displayName),
				displayName => Assert.Equal($"{className}.PropertyTestMethod(z: \"Base will fail\")", displayName),
				displayName => Assert.Equal($"{className}.PropertyTestMethod(z: \"I will fail\")", displayName),
				displayName => Assert.Equal($"{className}.PropertyTestMethod(z: \"Other source will fail\")", displayName)
			);
			Assert.Collection(
				testMessages.OfType<TestSkippedWithMetadata>().Select(skipped => skipped.Test.TestDisplayName).OrderBy(x => x),
				displayName => Assert.Equal($"{className}.FieldTestMethod(z: \"Base would fail if I ran\")", displayName),
				displayName => Assert.Equal($"{className}.FieldTestMethod(z: \"I would fail if I ran\")", displayName),
				displayName => Assert.Equal($"{className}.FieldTestMethod(z: \"Other source would fail if I ran\")", displayName),
				displayName => Assert.Equal($"{className}.MethodTestMethod(z: \"Base would fail if I ran\")", displayName),
				displayName => Assert.Equal($"{className}.MethodTestMethod(z: \"I would fail if I ran\")", displayName),
				displayName => Assert.Equal($"{className}.MethodTestMethod(z: \"Other source would fail if I ran\")", displayName),
				displayName => Assert.Equal($"{className}.PropertyTestMethod(z: \"Base would fail if I ran\")", displayName),
				displayName => Assert.Equal($"{className}.PropertyTestMethod(z: \"I would fail if I ran\")", displayName),
				displayName => Assert.Equal($"{className}.PropertyTestMethod(z: \"Other source would fail if I ran\")", displayName)
			);
		}
	}

	public partial class MethodDataTests : AcceptanceTestV3
	{
		[Fact]
		public static async ValueTask NonMatchingMethodInputDataThrows()
		{
#if XUNIT_AOT
			var testMessages = await RunForResultsAsync("Xunit3TheoryAcceptanceTests+MethodDataTests+ClassWithMismatchedMethodData");
#else
			var testMessages = await RunForResultsAsync(typeof(ClassWithMismatchedMethodData));
#endif

			var failed = Assert.Single(testMessages.OfType<TestFailedWithMetadata>());
			Assert.Equal("Xunit3TheoryAcceptanceTests+MethodDataTests+ClassWithMismatchedMethodData.TestViaMethodData", failed.Test.TestDisplayName);
#if XUNIT_AOT
			Assert.Equal("Xunit.Sdk.TestPipelineException", failed.ExceptionTypes.Single());
			Assert.Equal(@"Member data method 'Xunit3TheoryAcceptanceTests.MethodDataTests.ClassWithMismatchedMethodData.DataSource' had one or more invalid theory data arguments: int _ (""Hello world"")", failed.Messages.Single());
#else
			Assert.Equal("System.ArgumentException", failed.ExceptionTypes.Single());
			Assert.Equal("Could not find public static member (property, field, or method) named 'DataSource' on 'Xunit3TheoryAcceptanceTests+MethodDataTests+ClassWithMismatchedMethodData' with parameter types: System.String", failed.Messages.Single());
#endif
		}

		[Fact]
		public static async ValueTask SubTypeInheritsTestsFromBaseType()
		{
#if XUNIT_AOT
			var testMessages = await RunForResultsAsync("Xunit3TheoryAcceptanceTests+MethodDataTests+SubClassWithNoTests");
#else
			var testMessages = await RunForResultsAsync(typeof(SubClassWithNoTests));
#endif

			var passed = Assert.Single(testMessages.OfType<TestPassedWithMetadata>());
			Assert.Equal("Xunit3TheoryAcceptanceTests+MethodDataTests+SubClassWithNoTests.Test(x: 42)", passed.Test.TestDisplayName);
		}

		[Fact]
		public static async ValueTask CanPassParametersToDataMethod()
		{
#if XUNIT_AOT
			var testMessages = await RunForResultsAsync("Xunit3TheoryAcceptanceTests+MethodDataTests+ClassWithParameterizedMethodData");
#else
			var testMessages = await RunForResultsAsync(typeof(ClassWithParameterizedMethodData));
#endif

			var passed = Assert.Single(testMessages.OfType<TestPassedWithMetadata>());
			Assert.Equal($"Xunit3TheoryAcceptanceTests+MethodDataTests+ClassWithParameterizedMethodData.TestViaMethodData(_1: 42, _2: {21.12:G17}, z: \"Hello, world!\")", passed.Test.TestDisplayName);
			var failed = Assert.Single(testMessages.OfType<TestFailedWithMetadata>());
			Assert.Equal("Xunit3TheoryAcceptanceTests+MethodDataTests+ClassWithParameterizedMethodData.TestViaMethodData(_1: 0, _2: 0, z: null)", failed.Test.TestDisplayName);
			Assert.Empty(testMessages.OfType<TestSkippedWithMetadata>());
		}

		[Fact]
		public static async ValueTask CanDowncastMethodData()
		{
#if XUNIT_AOT
			var testMessages = await RunAsync("Xunit3TheoryAcceptanceTests+MethodDataTests+ClassWithDowncastedMethodData");
#else
			var testMessages = await RunAsync(typeof(ClassWithDowncastedMethodData));
#endif

			Assert.Equal(2, testMessages.OfType<ITestPassed>().Count());
			Assert.Empty(testMessages.OfType<ITestFailed>());
			Assert.Empty(testMessages.OfType<ITestSkipped>());
		}

		[Fact]
		public static async Task OptionalParametersSupported()
		{
#if XUNIT_AOT
			var testMessages = await RunForResultsAsync("Xunit3TheoryAcceptanceTests+MethodDataTests+ClassWithDataMethodsWithOptionalParameters");
#else
			var testMessages = await RunForResultsAsync(typeof(ClassWithDataMethodsWithOptionalParameters));
#endif

			Assert.NotEmpty(testMessages.OfType<TestPassedWithMetadata>());
			Assert.Empty(testMessages.OfType<TestFailedWithMetadata>());
			Assert.Empty(testMessages.OfType<TestSkippedWithMetadata>());
		}

		[Fact]
		public static async Task CanProvideAsyncData()
		{
#if XUNIT_AOT
			var testMessages = await RunForResultsAsync("Xunit3TheoryAcceptanceTests+MethodDataTests+ClassWithAsyncDataSources");
#else
			var testMessages = await RunForResultsAsync(typeof(ClassWithAsyncDataSources));
#endif

			Assert.Empty(testMessages.OfType<TestFailedWithMetadata>());
			Assert.Collection(
				testMessages.OfType<TestPassedWithMetadata>().OrderBy(t => t.Test.TestDisplayName),
				passed => Assert.Equal("Xunit3TheoryAcceptanceTests+MethodDataTests+ClassWithAsyncDataSources.TestMethod(_1: 0, _2: 0, _3: null)", passed.Test.TestDisplayName),
				passed => Assert.Equal("Xunit3TheoryAcceptanceTests+MethodDataTests+ClassWithAsyncDataSources.TestMethod(_1: 42, _2: 21.12, _3: \"Hello world\")", passed.Test.TestDisplayName)
			);
			var skipped = Assert.Single(testMessages.OfType<TestSkippedWithMetadata>());
			Assert.Equal("Xunit3TheoryAcceptanceTests+MethodDataTests+ClassWithAsyncDataSources.TestMethod(_1: 1, _2: 2.3, _3: \"No\")", skipped.Test.TestDisplayName);
			Assert.Equal("This row is skipped", skipped.Reason);
		}
	}

	public partial class TheoryTests : AcceptanceTestV3
	{
		[Fact]
		public static async ValueTask OptionalParameters_Valid()
		{
#if XUNIT_AOT
			var results = await RunForResultsAsync("Xunit3TheoryAcceptanceTests+TheoryTests+ClassWithOptionalParameters");
#else
			var results = await RunForResultsAsync(typeof(ClassWithOptionalParameters));
#endif

			Assert.Collection(
				results.OfType<TestPassedWithMetadata>().Select(passed => passed.Test.TestDisplayName).OrderBy(x => x, StringComparer.OrdinalIgnoreCase),
				displayName => Assert.Equal(@"Xunit3TheoryAcceptanceTests+TheoryTests+ClassWithOptionalParameters.OneOptional_OneNullParameter_NonePassed(s: null)", displayName),
				displayName => Assert.Equal(@"Xunit3TheoryAcceptanceTests+TheoryTests+ClassWithOptionalParameters.OneOptional_OneNullParameter_OneNonNullPassed(s: ""abc"")", displayName),
				displayName => Assert.Equal(@"Xunit3TheoryAcceptanceTests+TheoryTests+ClassWithOptionalParameters.OneOptional_OneNullParameter_OneNullPassed(s: null)", displayName),
				displayName => Assert.Equal(@"Xunit3TheoryAcceptanceTests+TheoryTests+ClassWithOptionalParameters.OneOptional_OneParameter_NonePassed(s: ""abc"")", displayName),
				displayName => Assert.Equal(@"Xunit3TheoryAcceptanceTests+TheoryTests+ClassWithOptionalParameters.OneOptional_OneParameter_OnePassed(s: ""def"")", displayName),
				displayName => Assert.Equal(@"Xunit3TheoryAcceptanceTests+TheoryTests+ClassWithOptionalParameters.OneOptional_TwoParameters_OnePassed(s: ""abc"", i: 5)", displayName),
				displayName => Assert.Equal(@"Xunit3TheoryAcceptanceTests+TheoryTests+ClassWithOptionalParameters.OneOptional_TwoParameters_TwoPassed(s: ""abc"", i: 6)", displayName),
#if XUNIT_AOT
				displayName => Assert.Equal(@"Xunit3TheoryAcceptanceTests+TheoryTests+ClassWithOptionalParameters.TwoOptionalAttributes_NonePassed(x: null, y: default(int))", displayName),
#else
				displayName => Assert.Equal(@"Xunit3TheoryAcceptanceTests+TheoryTests+ClassWithOptionalParameters.TwoOptionalAttributes_NonePassed(x: null, y: 0)", displayName),
#endif
				displayName => Assert.Equal(@"Xunit3TheoryAcceptanceTests+TheoryTests+ClassWithOptionalParameters.TwoOptional_TwoParameters_FirstOnePassed(s: ""def"", i: 5)", displayName),
				displayName => Assert.Equal(@"Xunit3TheoryAcceptanceTests+TheoryTests+ClassWithOptionalParameters.TwoOptional_TwoParameters_NonePassed(s: ""abc"", i: 5)", displayName),
				displayName => Assert.Equal(@"Xunit3TheoryAcceptanceTests+TheoryTests+ClassWithOptionalParameters.TwoOptional_TwoParameters_TwoPassedInOrder(s: ""def"", i: 6)", displayName)
			);
		}

		[Fact]
		public static async ValueTask Skipped()
		{
#if XUNIT_AOT
			var testMessages = await RunForResultsAsync("Xunit3TheoryAcceptanceTests+TheoryTests+ClassWithSkips");
#else
			var testMessages = await RunForResultsAsync(typeof(ClassWithSkips));
#endif

			Assert.Collection(
				testMessages.OfType<TestSkippedWithMetadata>().OrderBy(x => x.Test.TestDisplayName),
				skipped =>
				{
					Assert.Equal("Xunit3TheoryAcceptanceTests+TheoryTests+ClassWithSkips.SkippedDataRow(_: 0, y: null)", skipped.Test.TestDisplayName);
					Assert.Equal("Don't run this!", skipped.Reason);
				},
				skipped =>
				{
					Assert.Equal("Xunit3TheoryAcceptanceTests+TheoryTests+ClassWithSkips.SkippedInlineData(_: 0, y: null)", skipped.Test.TestDisplayName);
					Assert.Equal("Don't run this!", skipped.Reason);
				},
				skipped =>
				{
					Assert.Equal("Xunit3TheoryAcceptanceTests+TheoryTests+ClassWithSkips.SkippedMemberData(_: 0, y: null)", skipped.Test.TestDisplayName);
					Assert.Equal("Don't run this!", skipped.Reason);
				},
				skipped =>
				{
					// TODO: For AOT, the generator should not enumerate data for statically skipped theories
					Assert.Equal("Xunit3TheoryAcceptanceTests+TheoryTests+ClassWithSkips.SkippedTheory", skipped.Test.TestDisplayName);
					Assert.Equal("Don't run this!", skipped.Reason);
				}
			);
			Assert.Collection(
				testMessages.OfType<TestPassedWithMetadata>().OrderBy(x => x.Test.TestDisplayName),
				passed => Assert.Equal("Xunit3TheoryAcceptanceTests+TheoryTests+ClassWithSkips.SkippedDataRow(_: 42, y: \"Hello, world!\")", passed.Test.TestDisplayName),
				passed => Assert.Equal("Xunit3TheoryAcceptanceTests+TheoryTests+ClassWithSkips.SkippedInlineData(_: 42, y: \"Hello, world!\")", passed.Test.TestDisplayName),
				passed => Assert.Equal("Xunit3TheoryAcceptanceTests+TheoryTests+ClassWithSkips.SkippedMemberData(_: 42, y: \"Hello, world!\")", passed.Test.TestDisplayName)
			);
		}

		[Fact]
		public async ValueTask IncludeTestCaseIndex_SetToFalse_YieldsTestCasesWithoutIndex()
		{
#if XUNIT_AOT
			var testMessages = await RunForResultsAsync("Xunit3TheoryAcceptanceTests+TheoryTests+IndexedTheoryClassWithoutIndex");
#else
			var testMessages = await RunForResultsAsync(typeof(IndexedTheoryClassWithoutIndex));
#endif

			Assert.Collection(
				testMessages.OfType<TestPassedWithMetadata>().OrderBy(x => x.Test.TestDisplayName),
				passed => Assert.Equal("Xunit3TheoryAcceptanceTests+TheoryTests+IndexedTheoryClassWithoutIndex.DisabledTestCaseIndex(_: 1)", passed.Test.TestDisplayName),
				passed => Assert.Equal("Xunit3TheoryAcceptanceTests+TheoryTests+IndexedTheoryClassWithoutIndex.DisabledTestCaseIndex(_: 2)", passed.Test.TestDisplayName),
				passed => Assert.Equal("Xunit3TheoryAcceptanceTests+TheoryTests+IndexedTheoryClassWithoutIndex.DisabledTestCaseIndex(_: 3)", passed.Test.TestDisplayName));
		}

		[Fact]
		public async ValueTask IncludeTestCaseIndex_SetToTrue_YieldsTestCasesWithZeroPaddedIndex()
		{
#if XUNIT_AOT
			var testMessages = await RunForResultsAsync("Xunit3TheoryAcceptanceTests+TheoryTests+IndexedTheoryClassWithIndex");
#else
			var testMessages = await RunForResultsAsync(typeof(IndexedTheoryClassWithIndex));
#endif

			Assert.Collection(
				testMessages.OfType<TestPassedWithMetadata>().OrderBy(x => x.Test.TestDisplayName),
				passed => Assert.Equal("Xunit3TheoryAcceptanceTests+TheoryTests+IndexedTheoryClassWithIndex.EnabledTestCaseIndex_001(_: 1)", passed.Test.TestDisplayName),
				passed => Assert.Equal("Xunit3TheoryAcceptanceTests+TheoryTests+IndexedTheoryClassWithIndex.EnabledTestCaseIndex_002(_: 2)", passed.Test.TestDisplayName),
				passed => Assert.Equal("Xunit3TheoryAcceptanceTests+TheoryTests+IndexedTheoryClassWithIndex.EnabledTestCaseIndex_003(_: 3)", passed.Test.TestDisplayName));
		}

		[Fact]
		public async ValueTask IncludeTestCaseIndex_SetToTrue_IndexPaddingScalesWithCount()
		{
#if XUNIT_AOT
			var testMessages = await RunForResultsAsync("Xunit3TheoryAcceptanceTests+TheoryTests+IndexedTheoryClassWithLargeDataSet");
#else
			var testMessages = await RunForResultsAsync(typeof(IndexedTheoryClassWithLargeDataSet));
#endif

			testMessages = testMessages.OrderBy(x => x.Test.TestDisplayName).ToList();
			Assert.Equal("Xunit3TheoryAcceptanceTests+TheoryTests+IndexedTheoryClassWithLargeDataSet.IndexedTestCases_001(_: 1)", testMessages.First().Test.TestDisplayName);
			Assert.Equal("Xunit3TheoryAcceptanceTests+TheoryTests+IndexedTheoryClassWithLargeDataSet.IndexedTestCases_100(_: 100)", testMessages.Last().Test.TestDisplayName);
		}

		[Fact]
		public async ValueTask IncludeTestCaseIndex_SetToTrueWithLabel_YieldsIndexedDisplayNameWithLabel()
		{
#if XUNIT_AOT
			var testMessages = await RunForResultsAsync("Xunit3TheoryAcceptanceTests+TheoryTests+IndexedTheoryClassWithLabel");
#else
			var testMessages = await RunForResultsAsync(typeof(IndexedTheoryClassWithLabel));
#endif

			Assert.Collection(
				testMessages.OfType<TestPassedWithMetadata>().OrderBy(x => x.Test.TestDisplayName),
				passed => Assert.Equal("Xunit3TheoryAcceptanceTests+TheoryTests+IndexedTheoryClassWithLabel.IndexedMemberDataWithLabel_001 [smoke]", passed.Test.TestDisplayName),
				passed => Assert.Equal("Xunit3TheoryAcceptanceTests+TheoryTests+IndexedTheoryClassWithLabel.IndexedMemberDataWithLabel_002 [smoke]", passed.Test.TestDisplayName));
		}

		[Fact]
		public async ValueTask IncludeTestCaseIndex_SetToTrueWithTestDisplayName_YieldsIndexedCustomDisplayName()
		{
#if XUNIT_AOT
			var testMessages = await RunForResultsAsync("Xunit3TheoryAcceptanceTests+TheoryTests+IndexedTheoryClassWithTestDisplayName");
#else
			var testMessages = await RunForResultsAsync(typeof(IndexedTheoryClassWithTestDisplayName));
#endif

			Assert.Collection(
				testMessages.OrderBy(x => x.Test.TestDisplayName),
				testCase => Assert.Equal("first case_001(_: 1)", testCase.Test.TestDisplayName),
				testCase => Assert.Equal("second case_002(_: 2)", testCase.Test.TestDisplayName));
		}

		[Fact]
		public async ValueTask IncludeTestCaseIndex_SetToTrueWithLabelAndTestDisplayName_TestDisplayNameTakesPrecedence()
		{
#if XUNIT_AOT
			var testMessages = await RunForResultsAsync("Xunit3TheoryAcceptanceTests+TheoryTests+IndexedTheoryClassWithLabelAndTestDisplayName");
#else
			var testMessages = await RunForResultsAsync(typeof(IndexedTheoryClassWithLabelAndTestDisplayName));
#endif

			Assert.Collection(
				testMessages.OfType<TestPassedWithMetadata>().OrderBy(x => x.Test.TestDisplayName),
				testCase => Assert.Equal("my custom name_001 [smoke]", testCase.Test.TestDisplayName),
				testCase => Assert.Equal("my custom name_002 [smoke]", testCase.Test.TestDisplayName));
		}
	}
}
