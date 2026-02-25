using System.Runtime.CompilerServices;
using Xunit;
using Xunit.Sdk;
using Xunit.v3;

partial class Xunit3AcceptanceTests
{
	// Custom Fact attributes require a custom source source generator
	public class CustomFacts : AcceptanceTestV3
	{
		[Fact]
		public async ValueTask CanUseCustomFactAttribute()
		{
			var results = await RunForResultsAsync(typeof(ClassWithCustomFact));

			var passed = Assert.Single(results.OfType<TestPassedWithMetadata>());
			Assert.Equal("Xunit3AcceptanceTests+CustomFacts+ClassWithCustomFact.Passing", passed.Test.TestDisplayName);
		}

		class MyCustomFact(
			[CallerFilePath] string? sourceFilePath = null,
			[CallerLineNumber] int sourceLineNumber = -1) :
				FactAttribute(sourceFilePath, sourceLineNumber)
		{ }

		class ClassWithCustomFact
		{
			[MyCustomFact]
			public void Passing() { }
		}

		[Fact]
		public async ValueTask CanUseCustomFactWithArrayParameters()
		{
			var results = await RunForResultsAsync(typeof(ClassWithCustomArrayFact));

			var passed = Assert.Single(results.OfType<TestPassedWithMetadata>());
			Assert.Equal("Xunit3AcceptanceTests+CustomFacts+ClassWithCustomArrayFact.Passing", passed.Test.TestDisplayName);
		}

#pragma warning disable xUnit3003 // Classes which extend FactAttribute (directly or indirectly) should provide a public constructor for source information

		class MyCustomArrayFact(params string[] values) :
			FactAttribute
		{ }

#pragma warning restore xUnit3003

		class ClassWithCustomArrayFact
		{
			[MyCustomArrayFact("1", "2", "3")]
			public void Passing() { }
		}

		// https://github.com/xunit/xunit/issues/2719
		[Fact]
		public async ValueTask ClassWithThrowingSkipGetterShouldReportThatAsFailure()
		{
			var msgs = await RunForResultsAsync(typeof(ClassWithThrowingSkip));

			var msg = Assert.Single(msgs);
			var fail = Assert.IsType<TestFailedWithMetadata>(msg, exactMatch: false);
			Assert.Equal("Xunit3AcceptanceTests+CustomFacts+ClassWithThrowingSkip.TestMethod", fail.Test.TestDisplayName);
			var message = Assert.Single(fail.Messages);
			Assert.StartsWith("Exception during discovery:" + Environment.NewLine + "System.DivideByZeroException: Attempted to divide by zero.", message);
		}

		class ClassWithThrowingSkip
		{
			[ThrowingSkipFact]
			public static void TestMethod()
			{
				Assert.True(false);
			}
		}

		[XunitTestCaseDiscoverer(typeof(FactDiscoverer))]
		[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
		public class ThrowingSkipFactAttribute : Attribute, IFactAttribute
		{
			public string? DisplayName => null;
			public bool Explicit => false;
			public string? Skip => throw new DivideByZeroException();
			public Type[]? SkipExceptions => null;
			public Type? SkipType => null;
			public string? SkipUnless => null;
			public string? SkipWhen => null;
			public string? SourceFilePath => null;
			public int? SourceLineNumber => null;
			public int Timeout => 0;
		}
	}

	// Native AOT reports these in the generator
	public class Guards : AcceptanceTestV3
	{
		[Fact]
		public async ValueTask AsyncVoidTestsAreFastFailed()
		{
			var results = await RunForResultsAsync(typeof(ClassWithAsyncVoidTest));

			var failed = Assert.Single(results.OfType<TestFailedWithMetadata>());
			Assert.Equal("Xunit3AcceptanceTests+Guards+ClassWithAsyncVoidTest.TestMethod", failed.Test.TestDisplayName);
			var message = Assert.Single(failed.Messages);
			Assert.Equal("Tests marked as 'async void' are no longer supported. Please convert to 'async Task' or 'async ValueTask'.", message);
		}

#pragma warning disable xUnit1049 // Do not use 'async void' for test methods as it is no longer supported

		class ClassWithAsyncVoidTest
		{
			[Fact]
			public async void TestMethod()
			{
				await Task.Yield();
			}
		}

#pragma warning restore xUnit1049 // Do not use 'async void' for test methods as it is no longer supported

		[Fact]
		public async ValueTask CannotMixMultipleFactDerivedAttributes()
		{
			var results = await RunForResultsAsync(typeof(ClassWithMultipleFacts));

			var failed = Assert.Single(results.OfType<TestFailedWithMetadata>());
			Assert.Equal(typeof(TestPipelineException).SafeName(), failed.ExceptionTypes.Single());
			Assert.Equal("Test method 'Xunit3AcceptanceTests+Guards+ClassWithMultipleFacts.Passing' has multiple [Fact]-derived attributes", failed.Messages.Single());
		}

#pragma warning disable xUnit1002 // Test methods cannot have multiple Fact or Theory attributes

		class ClassWithMultipleFacts
		{
			[Fact]
			[CulturedFact(["en-US"])]
			public void Passing() { }
		}

#pragma warning restore xUnit1002 // Test methods cannot have multiple Fact or Theory attributes
	}
}
