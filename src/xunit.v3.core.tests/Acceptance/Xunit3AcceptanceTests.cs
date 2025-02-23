using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using Xunit.Sdk;
using Xunit.v3;

public class Xunit3AcceptanceTests
{
	public class EndToEndMessageInspection : AcceptanceTestV3
	{
		[Fact]
		public async ValueTask NoTests()
		{
			var results = await RunAsync(typeof(NoTestsClass));

			Assert.Collection(
				results,
				message => Assert.IsAssignableFrom<ITestAssemblyStarting>(message),
				message =>
				{
					var finished = Assert.IsAssignableFrom<ITestAssemblyFinished>(message);
					Assert.Equal(0, finished.TestsFailed);
					Assert.Equal(0, finished.TestsNotRun);
					Assert.Equal(0, finished.TestsSkipped);
					Assert.Equal(0, finished.TestsTotal);
				}
			);
		}

		class NoTestsClass { }

		[Fact]
		public async ValueTask SinglePassingTest()
		{
			string? observedAssemblyID = default;
			string? observedCollectionID = default;
			string? observedClassID = default;
			string? observedMethodID = default;
			string? observedTestCaseID = default;
			string? observedTestID = default;

			var results = await RunAsync(typeof(SinglePassingTestClass));

			Assert.Collection(
				results,
				message =>
				{
					var assemblyStarting = Assert.IsAssignableFrom<ITestAssemblyStarting>(message);
					observedAssemblyID = assemblyStarting.AssemblyUniqueID;
				},
				message =>
				{
					var collectionStarting = Assert.IsAssignableFrom<ITestCollectionStarting>(message);
					Assert.Null(collectionStarting.TestCollectionClassName);
#if BUILD_X86  // Assembly name changes for x86 testing, so that changes the ID
					Assert.Equal($"Test collection for {typeof(SinglePassingTestClass).SafeName()} (id: 54db054978cb9e2ff055289822aa7a3be7890e656e6369c93c5ac9437858b903)", collectionStarting.TestCollectionDisplayName);
#else
					Assert.Equal($"Test collection for {typeof(SinglePassingTestClass).SafeName()} (id: 685938d165702e64e5ba3c42e052172b50dc14d4e497683a69d48324c1935208)", collectionStarting.TestCollectionDisplayName);
#endif
					Assert.NotEmpty(collectionStarting.TestCollectionUniqueID);
					Assert.Equal(observedAssemblyID, collectionStarting.AssemblyUniqueID);
					observedCollectionID = collectionStarting.TestCollectionUniqueID;
				},
				message =>
				{
					var classStarting = Assert.IsAssignableFrom<ITestClassStarting>(message);
					Assert.Equal(typeof(SinglePassingTestClass).SafeName(), classStarting.TestClassName);
					Assert.Equal(observedAssemblyID, classStarting.AssemblyUniqueID);
					Assert.Equal(observedCollectionID, classStarting.TestCollectionUniqueID);
					observedClassID = classStarting.TestClassUniqueID;
				},
				message =>
				{
					var testMethodStarting = Assert.IsAssignableFrom<ITestMethodStarting>(message);
					Assert.Equal(nameof(SinglePassingTestClass.TestMethod), testMethodStarting.MethodName);
					Assert.Equal(observedAssemblyID, testMethodStarting.AssemblyUniqueID);
					Assert.Equal(observedCollectionID, testMethodStarting.TestCollectionUniqueID);
					Assert.Equal(observedClassID, testMethodStarting.TestClassUniqueID);
					observedMethodID = testMethodStarting.TestMethodUniqueID;
				},
				message =>
				{
					var testCaseStarting = Assert.IsAssignableFrom<ITestCaseStarting>(message);
					Assert.Equal(observedAssemblyID, testCaseStarting.AssemblyUniqueID);
					Assert.Equal($"{typeof(SinglePassingTestClass).SafeName()}.{nameof(SinglePassingTestClass.TestMethod)}", testCaseStarting.TestCaseDisplayName);
					Assert.Equal(observedCollectionID, testCaseStarting.TestCollectionUniqueID);
					Assert.Equal(observedClassID, testCaseStarting.TestClassUniqueID);
					Assert.Equal(observedMethodID, testCaseStarting.TestMethodUniqueID);
					observedTestCaseID = testCaseStarting.TestCaseUniqueID;
				},
				message =>
				{
					var testStarting = Assert.IsAssignableFrom<ITestStarting>(message);
					Assert.Equal(observedAssemblyID, testStarting.AssemblyUniqueID);
					// Test display name == test case display name for Facts
					Assert.Equal($"{typeof(SinglePassingTestClass).SafeName()}.{nameof(SinglePassingTestClass.TestMethod)}", testStarting.TestDisplayName);
					Assert.Equal(observedTestCaseID, testStarting.TestCaseUniqueID);
					Assert.Equal(observedCollectionID, testStarting.TestCollectionUniqueID);
					Assert.Equal(observedClassID, testStarting.TestClassUniqueID);
					Assert.Equal(observedMethodID, testStarting.TestMethodUniqueID);
					observedTestID = testStarting.TestUniqueID;
				},
				message =>
				{
					var classConstructionStarting = Assert.IsAssignableFrom<ITestClassConstructionStarting>(message);
					Assert.Equal(observedAssemblyID, classConstructionStarting.AssemblyUniqueID);
					Assert.Equal(observedTestCaseID, classConstructionStarting.TestCaseUniqueID);
					Assert.Equal(observedCollectionID, classConstructionStarting.TestCollectionUniqueID);
					Assert.Equal(observedClassID, classConstructionStarting.TestClassUniqueID);
					Assert.Equal(observedMethodID, classConstructionStarting.TestMethodUniqueID);
					Assert.Equal(observedTestID, classConstructionStarting.TestUniqueID);
				},
				message =>
				{
					var classConstructionFinished = Assert.IsAssignableFrom<ITestClassConstructionFinished>(message);
					Assert.Equal(observedAssemblyID, classConstructionFinished.AssemblyUniqueID);
					Assert.Equal(observedTestCaseID, classConstructionFinished.TestCaseUniqueID);
					Assert.Equal(observedCollectionID, classConstructionFinished.TestCollectionUniqueID);
					Assert.Equal(observedClassID, classConstructionFinished.TestClassUniqueID);
					Assert.Equal(observedMethodID, classConstructionFinished.TestMethodUniqueID);
					Assert.Equal(observedTestID, classConstructionFinished.TestUniqueID);
				},
				message =>
				{
					var testPassed = Assert.IsAssignableFrom<ITestPassed>(message);
					Assert.Equal(observedAssemblyID, testPassed.AssemblyUniqueID);
					Assert.NotEqual(0M, testPassed.ExecutionTime);
					Assert.Empty(testPassed.Output);
					Assert.Equal(observedTestCaseID, testPassed.TestCaseUniqueID);
					Assert.Equal(observedClassID, testPassed.TestClassUniqueID);
					Assert.Equal(observedCollectionID, testPassed.TestCollectionUniqueID);
					Assert.Equal(observedMethodID, testPassed.TestMethodUniqueID);
					Assert.Equal(observedTestID, testPassed.TestUniqueID);
					Assert.Null(testPassed.Warnings);
				},
				message =>
				{
					var testFinished = Assert.IsAssignableFrom<ITestFinished>(message);
					Assert.Equal(observedAssemblyID, testFinished.AssemblyUniqueID);
					Assert.Equal(observedTestCaseID, testFinished.TestCaseUniqueID);
					Assert.Equal(observedClassID, testFinished.TestClassUniqueID);
					Assert.Equal(observedCollectionID, testFinished.TestCollectionUniqueID);
					Assert.Equal(observedMethodID, testFinished.TestMethodUniqueID);
					Assert.Equal(observedTestID, testFinished.TestUniqueID);
				},
				message =>
				{
					var testCaseFinished = Assert.IsAssignableFrom<ITestCaseFinished>(message);
					Assert.Equal(observedAssemblyID, testCaseFinished.AssemblyUniqueID);
					Assert.NotEqual(0M, testCaseFinished.ExecutionTime);
					Assert.Equal(observedTestCaseID, testCaseFinished.TestCaseUniqueID);
					Assert.Equal(observedClassID, testCaseFinished.TestClassUniqueID);
					Assert.Equal(observedCollectionID, testCaseFinished.TestCollectionUniqueID);
					Assert.Equal(observedMethodID, testCaseFinished.TestMethodUniqueID);
					Assert.Equal(0, testCaseFinished.TestsFailed);
					Assert.Equal(0, testCaseFinished.TestsNotRun);
					Assert.Equal(0, testCaseFinished.TestsSkipped);
					Assert.Equal(1, testCaseFinished.TestsTotal);
				},
				message =>
				{
					var testMethodFinished = Assert.IsAssignableFrom<ITestMethodFinished>(message);
					Assert.Equal(observedAssemblyID, testMethodFinished.AssemblyUniqueID);
					Assert.NotEqual(0M, testMethodFinished.ExecutionTime);
					Assert.Equal(observedClassID, testMethodFinished.TestClassUniqueID);
					Assert.Equal(observedCollectionID, testMethodFinished.TestCollectionUniqueID);
					Assert.Equal(observedMethodID, testMethodFinished.TestMethodUniqueID);
					Assert.Equal(0, testMethodFinished.TestsFailed);
					Assert.Equal(0, testMethodFinished.TestsNotRun);
					Assert.Equal(0, testMethodFinished.TestsSkipped);
					Assert.Equal(1, testMethodFinished.TestsTotal);
				},
				message =>
				{
					var classFinished = Assert.IsAssignableFrom<ITestClassFinished>(message);
					Assert.Equal(observedAssemblyID, classFinished.AssemblyUniqueID);
					Assert.NotEqual(0M, classFinished.ExecutionTime);
					Assert.Equal(observedClassID, classFinished.TestClassUniqueID);
					Assert.Equal(observedCollectionID, classFinished.TestCollectionUniqueID);
					Assert.Equal(0, classFinished.TestsFailed);
					Assert.Equal(0, classFinished.TestsNotRun);
					Assert.Equal(0, classFinished.TestsSkipped);
					Assert.Equal(1, classFinished.TestsTotal);
				},
				message =>
				{
					var collectionFinished = Assert.IsAssignableFrom<ITestCollectionFinished>(message);
					Assert.Equal(observedAssemblyID, collectionFinished.AssemblyUniqueID);
					Assert.NotEqual(0M, collectionFinished.ExecutionTime);
					Assert.Equal(observedCollectionID, collectionFinished.TestCollectionUniqueID);
					Assert.Equal(0, collectionFinished.TestsFailed);
					Assert.Equal(0, collectionFinished.TestsNotRun);
					Assert.Equal(0, collectionFinished.TestsSkipped);
					Assert.Equal(1, collectionFinished.TestsTotal);
				},
				message =>
				{
					var assemblyFinished = Assert.IsAssignableFrom<ITestAssemblyFinished>(message);
					Assert.Equal(observedAssemblyID, assemblyFinished.AssemblyUniqueID);
					Assert.NotEqual(0M, assemblyFinished.ExecutionTime);
					Assert.Equal(0, assemblyFinished.TestsFailed);
					Assert.Equal(0, assemblyFinished.TestsNotRun);
					Assert.Equal(0, assemblyFinished.TestsSkipped);
					Assert.Equal(1, assemblyFinished.TestsTotal);
				}
			);
		}

		class SinglePassingTestClass
		{
			[Fact]
			public void TestMethod() { }
		}
	}

	public class SkippedTests : AcceptanceTestV3
	{
		[Fact]
		public async ValueTask SingleSkippedTest()
		{
			var results = await RunAsync(typeof(SingleSkippedTestClass));

			var skippedMessage = Assert.Single(results.OfType<ITestSkipped>());
			var skippedStarting = Assert.Single(results.OfType<ITestStarting>(), s => s.TestUniqueID == skippedMessage.TestUniqueID);
			Assert.Equal($"{typeof(SingleSkippedTestClass).SafeName()}.{nameof(SingleSkippedTestClass.TestMethod)}", skippedStarting.TestDisplayName);
			Assert.Equal("This is a skipped test", skippedMessage.Reason);

			var classFinishedMessage = Assert.Single(results.OfType<ITestClassFinished>());
			Assert.Equal(1, classFinishedMessage.TestsTotal);
			Assert.Equal(1, classFinishedMessage.TestsSkipped);

			var collectionFinishedMessage = Assert.Single(results.OfType<ITestCollectionFinished>());
			Assert.Equal(1, collectionFinishedMessage.TestsTotal);
			Assert.Equal(1, collectionFinishedMessage.TestsSkipped);
		}

		class SingleSkippedTestClass
		{
			[Fact(Skip = "This is a skipped test")]
			public void TestMethod() => Assert.True(false);
		}

		[Theory]
		[InlineData(true)]
		[InlineData(false)]
		public async ValueTask ConditionallySkippedTests(bool preEnumerateTheories)
		{
			var results = await RunAsync(typeof(ConditionallySkippedTestClass), preEnumerateTheories: preEnumerateTheories);

			var skippedMessage = Assert.Single(results.OfType<ITestSkipped>());
			var skippedStarting = Assert.Single(results.OfType<ITestStarting>(), s => s.TestUniqueID == skippedMessage.TestUniqueID);
			Assert.Equal($"{typeof(ConditionallySkippedTestClass).SafeName()}.{nameof(ConditionallySkippedTestClass.ConditionallyAlwaysSkipped)}(value: False)", skippedStarting.TestDisplayName);
			Assert.Equal("I am always skipped, conditionally", skippedMessage.Reason);

			var passedMessage = Assert.Single(results.OfType<ITestPassed>());
			var passedStarting = Assert.Single(results.OfType<ITestStarting>(), s => s.TestUniqueID == passedMessage.TestUniqueID);
			Assert.Equal($"{typeof(ConditionallySkippedTestClass).SafeName()}.{nameof(ConditionallySkippedTestClass.ConditionallyNeverSkipped)}(value: True)", passedStarting.TestDisplayName);

			var failedMessage = Assert.Single(results.OfType<ITestFailed>());
			var failedStarting = Assert.Single(results.OfType<ITestStarting>(), s => s.TestUniqueID == failedMessage.TestUniqueID);
			Assert.Equal($"{typeof(ConditionallySkippedTestClass).SafeName()}.{nameof(ConditionallySkippedTestClass.ConditionallyNeverSkipped)}(value: False)", failedStarting.TestDisplayName);
			Assert.Equal("Xunit.Sdk.TrueException", failedMessage.ExceptionTypes.Single());

			var classFinishedMessage = Assert.Single(results.OfType<ITestClassFinished>());
			Assert.Equal(3, classFinishedMessage.TestsTotal);
			Assert.Equal(1, classFinishedMessage.TestsSkipped);
			Assert.Equal(1, classFinishedMessage.TestsFailed);

			var collectionFinishedMessage = Assert.Single(results.OfType<ITestCollectionFinished>());
			Assert.Equal(3, collectionFinishedMessage.TestsTotal);
			Assert.Equal(1, collectionFinishedMessage.TestsSkipped);
			Assert.Equal(1, collectionFinishedMessage.TestsFailed);
		}

		// Use theories for both to ensure that any "skip" logic in TheoryDiscoverer doesn't kick in for conditional skips
		class ConditionallySkippedTestClass
		{
			public static bool Always => true;

			[Theory(Skip = "I am always skipped, conditionally", SkipWhen = nameof(Always))]
			[InlineData(false)]
			public void ConditionallyAlwaysSkipped(bool value) => Assert.True(value);

			[Theory(Skip = "I am never skipped, conditionally", SkipUnless = nameof(Always))]
			[InlineData(false)]
			[InlineData(true)]
			public void ConditionallyNeverSkipped(bool value) => Assert.True(value);
		}

		[Fact]
		public async ValueTask ConditionallySkippedTests_UsingSkipType()
		{
			var results = await RunAsync(typeof(ConditionallySkippedTestsClass_UsingSkipType));

			var skippedMessage = Assert.Single(results.OfType<ITestSkipped>());
			var skippedStarting = Assert.Single(results.OfType<ITestStarting>(), s => s.TestUniqueID == skippedMessage.TestUniqueID);
			Assert.Equal($"{typeof(ConditionallySkippedTestsClass_UsingSkipType).SafeName()}.{nameof(ConditionallySkippedTestsClass_UsingSkipType.ConditionallyAlwaysSkipped)}", skippedStarting.TestDisplayName);
			Assert.Equal("I am always skipped, conditionally", skippedMessage.Reason);

			var passedMessage = Assert.Single(results.OfType<ITestPassed>());
			var passedStarting = Assert.Single(results.OfType<ITestStarting>(), s => s.TestUniqueID == passedMessage.TestUniqueID);
			Assert.Equal($"{typeof(ConditionallySkippedTestsClass_UsingSkipType).SafeName()}.{nameof(ConditionallySkippedTestsClass_UsingSkipType.ConditionallyNeverSkipped)}", passedStarting.TestDisplayName);

			var classFinishedMessage = Assert.Single(results.OfType<ITestClassFinished>());
			Assert.Equal(2, classFinishedMessage.TestsTotal);
			Assert.Equal(1, classFinishedMessage.TestsSkipped);

			var collectionFinishedMessage = Assert.Single(results.OfType<ITestCollectionFinished>());
			Assert.Equal(2, collectionFinishedMessage.TestsTotal);
			Assert.Equal(1, collectionFinishedMessage.TestsSkipped);
		}

		class ConditionallySkippedTestsClass_UsingSkipType
		{
			[Fact(Skip = "I am always skipped, conditionally", SkipType = typeof(ConditionallySkippedTestClass), SkipWhen = nameof(ConditionallySkippedTestClass.Always))]
			public void ConditionallyAlwaysSkipped() => Assert.True(false);

			[Fact(Skip = "I am never skipped, conditionally", SkipType = typeof(ConditionallySkippedTestClass), SkipUnless = nameof(ConditionallySkippedTestClass.Always))]
			public void ConditionallyNeverSkipped() { }
		}
	}

	public class ExplicitTests : AcceptanceTestV3
	{
		[Theory]
		[InlineData([null])]
		[InlineData(ExplicitOption.Off)]
		public async ValueTask OnlyRunNonExplicit(ExplicitOption? @explicit)
		{
			var results = await RunForResultsAsync(typeof(ClassWithExplicitTest), explicitOption: @explicit);

			Assert.Equal(2, results.Count);
			var passed = Assert.Single(results.OfType<TestPassedWithDisplayName>());
			Assert.Equal("Xunit3AcceptanceTests+ExplicitTests+ClassWithExplicitTest.NonExplicitTest", passed.TestDisplayName);
			var notRun = Assert.Single(results.OfType<TestNotRunWithDisplayName>());
			Assert.Equal("Xunit3AcceptanceTests+ExplicitTests+ClassWithExplicitTest.ExplicitTest", notRun.TestDisplayName);
		}

		[Fact]
		public async ValueTask OnlyRunExplicit()
		{
			var results = await RunForResultsAsync(typeof(ClassWithExplicitTest), explicitOption: ExplicitOption.Only);

			Assert.Equal(2, results.Count);
			var notRun = Assert.Single(results.OfType<TestNotRunWithDisplayName>());
			Assert.Equal("Xunit3AcceptanceTests+ExplicitTests+ClassWithExplicitTest.NonExplicitTest", notRun.TestDisplayName);
			var failed = Assert.Single(results.OfType<TestFailedWithDisplayName>());
			Assert.Equal("Xunit3AcceptanceTests+ExplicitTests+ClassWithExplicitTest.ExplicitTest", failed.TestDisplayName);
		}

		[Fact]
		public async ValueTask RunEverything()
		{
			var results = await RunForResultsAsync(typeof(ClassWithExplicitTest), explicitOption: ExplicitOption.On);

			Assert.Equal(2, results.Count);
			var passed = Assert.Single(results.OfType<TestPassedWithDisplayName>());
			Assert.Equal("Xunit3AcceptanceTests+ExplicitTests+ClassWithExplicitTest.NonExplicitTest", passed.TestDisplayName);
			var failed = Assert.Single(results.OfType<TestFailedWithDisplayName>());
			Assert.Equal("Xunit3AcceptanceTests+ExplicitTests+ClassWithExplicitTest.ExplicitTest", failed.TestDisplayName);
		}

		class ClassWithExplicitTest
		{
			[Fact]
			public void NonExplicitTest() => Assert.True(true);

			[Fact(Explicit = true)]
			public void ExplicitTest() => Assert.True(false);
		}
	}

	// Disable parallelization because the timing performance here is important
	[CollectionDefinition(DisableParallelization = true)]
	public class TimeoutTestsCollection { }

	public class NonStartedTasks : AcceptanceTestV3
	{
		[Fact]
		public async ValueTask TestWithUnstartedTaskThrows()
		{
			var stopwatch = Stopwatch.StartNew();
			var results = await RunAsync(typeof(ClassUnderTest));
			stopwatch.Stop();

			var failedMessage = Assert.Single(results.OfType<ITestFailed>());
			var failedStarting = results.OfType<ITestStarting>().Single(s => s.TestUniqueID == failedMessage.TestUniqueID);
			Assert.Equal("Xunit3AcceptanceTests+NonStartedTasks+ClassUnderTest.NonStartedTask", failedStarting.TestDisplayName);
			Assert.Equal("Test method returned a non-started Task (tasks must be started before being returned)", failedMessage.Messages.Single());
		}

		class ClassUnderTest
		{
			[Fact]
			public Task NonStartedTask() => new(() => { Thread.Sleep(1000); });
		}
	}

	public class FailingTests : AcceptanceTestV3
	{
		[Fact]
		public async ValueTask SingleFailingTest()
		{
			var results = await RunAsync(typeof(SingleFailingTestClass));

			var failedMessage = Assert.Single(results.OfType<ITestFailed>());
			Assert.Equal(typeof(TrueException).FullName, failedMessage.ExceptionTypes.Single());

			var classFinishedMessage = Assert.Single(results.OfType<ITestClassFinished>());
			Assert.Equal(1, classFinishedMessage.TestsFailed);

			var collectionFinishedMessage = Assert.Single(results.OfType<ITestCollectionFinished>());
			Assert.Equal(1, collectionFinishedMessage.TestsFailed);
		}
		class SingleFailingTestClass
		{
			[Fact]
			public void TestMethod()
			{
				Assert.True(false);
			}
		}

		[Fact]
		public async ValueTask SingleFailingTestReturningValueTask()
		{
			var results = await RunAsync(typeof(SingleFailingValueTaskTestClass));

			var failedMessage = Assert.Single(results.OfType<ITestFailed>());
			Assert.Equal(typeof(TrueException).FullName, failedMessage.ExceptionTypes.Single());

			var classFinishedMessage = Assert.Single(results.OfType<ITestClassFinished>());
			Assert.Equal(1, classFinishedMessage.TestsFailed);

			var collectionFinishedMessage = Assert.Single(results.OfType<ITestCollectionFinished>());
			Assert.Equal(1, collectionFinishedMessage.TestsFailed);
		}

		class SingleFailingValueTaskTestClass
		{
			[Fact]
			public async ValueTask TestMethod()
			{
				await Task.Delay(1, TestContext.Current.CancellationToken);
				Assert.True(false);
			}
		}
	}

	public class ClassFailures : AcceptanceTestV3
	{
		[Fact]
		public async ValueTask TestFailureResultsFromThrowingCtorInTestClass()
		{
			var messages = await RunAsync<ITestFailed>(typeof(ClassUnderTest_CtorFailure));

			var msg = Assert.Single(messages);
			Assert.Equal(typeof(DivideByZeroException).FullName, msg.ExceptionTypes.Single());
		}

		[Fact]
		public async ValueTask TestFailureResultsFromThrowingDisposeInTestClass()
		{
			var messages = await RunAsync<ITestFailed>(typeof(ClassUnderTest_DisposeFailure));

			var msg = Assert.Single(messages);
			Assert.Equal(typeof(DivideByZeroException).FullName, msg.ExceptionTypes.Single());
		}

		[Fact]
		public async ValueTask CompositeTestFailureResultsFromFailingTestsPlusThrowingDisposeInTestClass()
		{
			var messages = await RunAsync<ITestFailed>(typeof(ClassUnderTest_FailingTestAndDisposeFailure));

			var msg = Assert.Single(messages);
			var combinedMessage = ExceptionUtility.CombineMessages(msg);
			Assert.StartsWith("System.AggregateException : One or more errors occurred.", combinedMessage);
			Assert.EndsWith(
				"---- Assert.Equal() Failure: Values differ" + Environment.NewLine +
				"Expected: 2" + Environment.NewLine +
				"Actual:   3" + Environment.NewLine +
				"---- System.DivideByZeroException : Attempted to divide by zero.",
				combinedMessage
			);
		}

		class ClassUnderTest_CtorFailure
		{
			public ClassUnderTest_CtorFailure()
			{
				throw new DivideByZeroException();
			}

			[Fact]
			public void TheTest() { }
		}

		class ClassUnderTest_DisposeFailure : IDisposable
		{
			public void Dispose()
			{
				throw new DivideByZeroException();
			}

			[Fact]
			public void TheTest() { }
		}

		class ClassUnderTest_FailingTestAndDisposeFailure : IDisposable
		{
			public void Dispose()
			{
				throw new DivideByZeroException();
			}

			[Fact]
			public void TheTest()
			{
				Assert.Equal(2, 3);
			}
		}
	}

	public class StaticClassSupport : AcceptanceTestV3
	{
		[Fact]
		public async ValueTask TestsCanBeInStaticClasses()
		{
			var testMessages = await RunAsync(typeof(StaticClassUnderTest));

			Assert.Single(testMessages.OfType<ITestPassed>());
			var starting = Assert.Single(testMessages.OfType<ITestStarting>());
			Assert.Equal("Xunit3AcceptanceTests+StaticClassSupport+StaticClassUnderTest.Passing", starting.TestDisplayName);
		}

		static class StaticClassUnderTest
		{
			[Fact]
			public static void Passing() { }
		}
	}

	public class ErrorAggregation : AcceptanceTestV3
	{
		[Fact]
		public async ValueTask EachTestMethodHasIndividualExceptionMessage()
		{
			var testMessages = await RunAsync(typeof(ClassUnderTest));

			var equalStarting = Assert.Single(testMessages.OfType<ITestStarting>(), msg => msg.TestDisplayName == "Xunit3AcceptanceTests+ErrorAggregation+ClassUnderTest.EqualFailure");
			var equalFailure = Assert.Single(testMessages.OfType<ITestFailed>(), msg => msg.TestUniqueID == equalStarting.TestUniqueID);
			Assert.Contains("Assert.Equal() Failure", equalFailure.Messages.Single());

			var notNullStarting = Assert.Single(testMessages.OfType<ITestStarting>(), msg => msg.TestDisplayName == "Xunit3AcceptanceTests+ErrorAggregation+ClassUnderTest.NotNullFailure");
			var notNullFailure = Assert.Single(testMessages.OfType<ITestFailed>(), msg => msg.TestUniqueID == notNullStarting.TestUniqueID);
			Assert.Contains("Assert.NotNull() Failure", notNullFailure.Messages.Single());
		}

		class ClassUnderTest
		{
			[Fact]
			public void EqualFailure()
			{
				Assert.Equal(42, 40);
			}

			[Fact]
			public void NotNullFailure()
			{
				Assert.NotNull(null);
			}
		}
	}

	public class TestOrdering : AcceptanceTestV3
	{
		[Fact]
		public async ValueTask OverrideOfOrderingAtCollectionLevel()
		{
			var testMessages = await RunAsync(typeof(TestClassUsingCollection));

			Assert.Collection(
				testMessages.OfType<ITestPassed>().Select(p => testMessages.OfType<ITestMethodStarting>().Where(s => s.TestMethodUniqueID == p.TestMethodUniqueID).Single().MethodName),
				methodName => Assert.Equal("Test1", methodName),
				methodName => Assert.Equal("Test2", methodName),
				methodName => Assert.Equal("Test3", methodName)
			);
		}

		[CollectionDefinition("Ordered Collection")]
		[TestCaseOrderer(typeof(AlphabeticalOrderer))]
		public class CollectionClass { }

		[Collection("Ordered Collection")]
		class TestClassUsingCollection
		{
			[Fact]
			public void Test1() { }

			[Fact]
			public void Test3() { }

			[Fact]
			public void Test2() { }
		}

		[Fact]
		public async ValueTask OverrideOfOrderingAtClassLevel()
		{
			var testMessages = await RunAsync(typeof(TestClassWithoutCollection));

			Assert.Collection(
				testMessages.OfType<ITestPassed>().Select(p => testMessages.OfType<ITestMethodStarting>().Where(s => s.TestMethodUniqueID == p.TestMethodUniqueID).Single().MethodName),
				methodName => Assert.Equal("Test1", methodName),
				methodName => Assert.Equal("Test2", methodName),
				methodName => Assert.Equal("Test3", methodName)
			);
		}

		[TestCaseOrderer(typeof(AlphabeticalOrderer))]
		public class TestClassWithoutCollection
		{
			[Fact]
			public void Test1() { }

			[Fact]
			public void Test3() { }

			[Fact]
			public void Test2() { }
		}

		public class AlphabeticalOrderer : ITestCaseOrderer
		{
			public IReadOnlyCollection<TTestCase> OrderTestCases<TTestCase>(IReadOnlyCollection<TTestCase> testCases)
				where TTestCase : notnull, ITestCase
			{
				var result = testCases.ToList();
				result.Sort((x, y) => StringComparer.OrdinalIgnoreCase.Compare(x.TestMethod?.MethodName, y.TestMethod?.MethodName));
				return result;
			}
		}
	}

	public class TestNonParallelOrdering : AcceptanceTestV3
	{
		[Fact]
		public async ValueTask NonParallelCollectionsRunLast()
		{
			var testMessages = await RunAsync([typeof(TestClassNonParallelCollection), typeof(TestClassParallelCollection)]);

			Assert.Collection(
				testMessages.OfType<ITestPassed>().Select(p => testMessages.OfType<ITestMethodStarting>().Where(s => s.TestMethodUniqueID == p.TestMethodUniqueID).Single().MethodName),
				methodName => Assert.Equal("Test1", methodName),
				methodName => Assert.Equal("Test2", methodName),
				methodName => Assert.Equal("IShouldBeLast1", methodName),
				methodName => Assert.Equal("IShouldBeLast2", methodName)
			);
		}

		[CollectionDefinition("Parallel Ordered Collection")]
		[TestCaseOrderer(typeof(TestOrdering.AlphabeticalOrderer))]
		public class CollectionClass { }

		[Collection("Parallel Ordered Collection")]
		class TestClassParallelCollection
		{
			[Fact]
			public void Test1() { }

			[Fact]
			public void Test2() { }
		}

		[CollectionDefinition("Non-Parallel Collection", DisableParallelization = true)]
		[TestCaseOrderer(typeof(TestOrdering.AlphabeticalOrderer))]
		public class TestClassNonParallelCollectionDefinition { }

		[Collection("Non-Parallel Collection")]
		class TestClassNonParallelCollection
		{
			[Fact]
			public void IShouldBeLast2() { }

			[Fact]
			public void IShouldBeLast1() { }
		}
	}

	public class CustomFacts : AcceptanceTestV3
	{
		[Fact]
		public async ValueTask CanUseCustomFactAttribute()
		{
			var msgs = await RunAsync(typeof(ClassWithCustomFact));

			var displayName = Assert.Single(
				msgs.OfType<ITestPassed>().Select(p => msgs.OfType<ITestStarting>().Single(s => s.TestMethodUniqueID == p.TestMethodUniqueID).TestDisplayName));
			Assert.Equal("Xunit3AcceptanceTests+CustomFacts+ClassWithCustomFact.Passing", displayName);
		}

		class MyCustomFact : FactAttribute { }

		class ClassWithCustomFact
		{
			[MyCustomFact]
			public void Passing() { }
		}

		[Fact]
		public async ValueTask CanUseCustomFactWithArrayParameters()
		{
			var msgs = await RunAsync(typeof(ClassWithCustomArrayFact));

			var displayName = Assert.Single(
				msgs.OfType<ITestPassed>().Select(
					p => msgs.OfType<ITestStarting>().Single(s => s.TestMethodUniqueID == p.TestMethodUniqueID)
						.TestDisplayName));
			Assert.Equal("Xunit3AcceptanceTests+CustomFacts+ClassWithCustomArrayFact.Passing", displayName);
		}

		class MyCustomArrayFact : FactAttribute
		{
			public MyCustomArrayFact(params string[] values) { }
		}

		class ClassWithCustomArrayFact
		{
			[MyCustomArrayFact("1", "2", "3")]
			public void Passing() { }
		}

		[Fact]
		public async ValueTask CannotMixMultipleFactDerivedAttributes()
		{
			var msgs = await RunAsync<ITestFailed>(typeof(ClassWithMultipleFacts));

			var msg = Assert.Single(msgs);
			Assert.Equal(typeof(TestPipelineException).SafeName(), msg.ExceptionTypes.Single());
			Assert.Equal("Test method 'Xunit3AcceptanceTests+CustomFacts+ClassWithMultipleFacts.Passing' has multiple [Fact]-derived attributes", msg.Messages.Single());
		}

#pragma warning disable xUnit1002 // Test methods cannot have multiple Fact or Theory attributes

		class ClassWithMultipleFacts
		{
			[Fact]
			[MyCustomFact]
			public void Passing() { }
		}

#pragma warning restore xUnit1002 // Test methods cannot have multiple Fact or Theory attributes

		// https://github.com/xunit/xunit/issues/2719
		[Fact]
		public async ValueTask ClassWithThrowingSkipGetterShouldReportThatAsFailure()
		{
			var msgs = await RunForResultsAsync(typeof(ClassWithThrowingSkip));

			var msg = Assert.Single(msgs);
			var fail = Assert.IsAssignableFrom<TestFailedWithDisplayName>(msg);
			Assert.Equal("Xunit3AcceptanceTests+CustomFacts+ClassWithThrowingSkip.TestMethod", fail.TestDisplayName);
			var message = Assert.Single(fail.Messages);
			Assert.StartsWith("Exception during discovery:" + Environment.NewLine + "System.DivideByZeroException: Attempted to divide by zero.", message);
		}

		class ClassWithThrowingSkip
		{
			[ThrowingSkipFact]
			public void TestMethod()
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
			public int Timeout => 0;
		}
	}

	public class TestContextAccessor : AcceptanceTestV3
	{
		[Fact]
		public async ValueTask CanInjectTestContextAccessor()
		{
			var msgs = await RunAsync(typeof(ClassUnderTest));

			var displayName = Assert.Single(
				msgs
					.OfType<ITestPassed>()
					.Select(p => msgs.OfType<ITestStarting>().Single(s => s.TestMethodUniqueID == p.TestMethodUniqueID).TestDisplayName)
			);
			Assert.Equal("Xunit3AcceptanceTests+TestContextAccessor+ClassUnderTest.Passing", displayName);
		}

		class ClassUnderTest
		{
			ITestContextAccessor accessor;

			public ClassUnderTest(ITestContextAccessor accessor)
			{
				this.accessor = accessor;
			}

			[Fact]
			public void Passing()
			{
				Assert.NotNull(accessor);
				Assert.Same(TestContext.Current, accessor.Current);
			}
		}
	}

	public class TestOutput : AcceptanceTestV3
	{
		[Fact]
		public async ValueTask SendOutputMessages()
		{
			var msgs = await RunAsync(typeof(ClassUnderTest));

			var idxOfTestPassed = msgs.FindIndex(msg => msg is ITestPassed);
			Assert.True(idxOfTestPassed >= 0, "Test should have passed");

			var idxOfFirstTestOutput = msgs.FindIndex(msg => msg is ITestOutput);
			Assert.True(idxOfFirstTestOutput >= 0, "Test should have output");
			Assert.True(idxOfFirstTestOutput < idxOfTestPassed, "Test output messages should precede test result");

			Assert.Collection(
				msgs.OfType<ITestOutput>(),
				msg =>
				{
					var outputMessage = Assert.IsAssignableFrom<ITestOutput>(msg);
					Assert.Equal("This is output in the constructor" + Environment.NewLine, outputMessage.Output);
				},
				msg =>
				{
					var outputMessage = Assert.IsAssignableFrom<ITestOutput>(msg);
					Assert.Equal("This is ITest output" + Environment.NewLine, outputMessage.Output);
				},
				msg =>
				{
					var outputMessage = Assert.IsAssignableFrom<ITestOutput>(msg);
					Assert.Equal("This is output in Dispose" + Environment.NewLine, outputMessage.Output);
				}
			);
		}

		class ClassUnderTest : IDisposable
		{
			readonly ITestOutputHelper output;

			public ClassUnderTest(ITestOutputHelper output)
			{
				this.output = output;

				output.WriteLine("This is output in the constructor");
			}

			public void Dispose()
			{
				output.WriteLine("This is {0} in Dispose", "output");
			}

			[Fact]
			public void TestMethod()
			{
				output.WriteLine("This is ITest output");
			}
		}
	}

	public class AsyncLifetime : AcceptanceTestV3
	{
		[Fact]
		public async ValueTask AsyncLifetimeAcceptanceTest()
		{
			var messages = await RunAsync<ITestPassed>(typeof(ClassWithAsyncLifetime));

			var message = Assert.Single(messages);
			AssertOperations(message, "Constructor", "InitializeAsync", "Run Test", "DisposeAsync");
		}

		class ClassWithAsyncLifetime : IAsyncLifetime
		{
			protected readonly ITestOutputHelper output;

			public ClassWithAsyncLifetime(ITestOutputHelper output)
			{
				this.output = output;

				output.WriteLine("Constructor");
			}

			public virtual ValueTask InitializeAsync()
			{
				output.WriteLine("InitializeAsync");
				return default;
			}

			public virtual ValueTask DisposeAsync()
			{
				output.WriteLine("DisposeAsync");
				return default;
			}

			[Fact]
			public virtual void TheTest()
			{
				output.WriteLine("Run Test");
			}
		}

		[Fact]
		public async ValueTask AsyncDisposableAcceptanceTest()
		{
			var messages = await RunAsync<ITestPassed>(typeof(ClassWithAsyncDisposable));

			var message = Assert.Single(messages);
			// We prefer DisposeAsync over Dispose, so Dispose won't be in the call list
			AssertOperations(message, "Constructor", "Run Test", "DisposeAsync");
		}

		class ClassWithAsyncDisposable : IAsyncDisposable, IDisposable
		{
			protected readonly ITestOutputHelper output;

			public ClassWithAsyncDisposable(ITestOutputHelper output)
			{
				this.output = output;

				output.WriteLine("Constructor");
			}

			public virtual void Dispose()
			{
				output.WriteLine("Dispose");
			}

			public virtual ValueTask DisposeAsync()
			{
				output.WriteLine("DisposeAsync");
				return default;
			}

			[Fact]
			public virtual void TheTest()
			{
				output.WriteLine("Run Test");
			}
		}

		[Fact]
		public async ValueTask DisposableAcceptanceTest()
		{
			var messages = await RunAsync<ITestPassed>(typeof(ClassWithDisposable));

			var message = Assert.Single(messages);
			AssertOperations(message, "Constructor", "Run Test", "Dispose");
		}

		class ClassWithDisposable : IDisposable
		{
			protected readonly ITestOutputHelper output;

			public ClassWithDisposable(ITestOutputHelper output)
			{
				this.output = output;

				output.WriteLine("Constructor");
			}

			public virtual void Dispose()
			{
				output.WriteLine("Dispose");
			}

			[Fact]
			public virtual void TheTest()
			{
				output.WriteLine("Run Test");
			}
		}

		[Fact]
		public async ValueTask ThrowingConstructor()
		{
			var messages = await RunAsync<ITestFailed>(typeof(ClassWithAsyncLifetime_ThrowingCtor));

			var message = Assert.Single(messages);
			AssertOperations(message, "Constructor");
		}

		class ClassWithAsyncLifetime_ThrowingCtor : ClassWithAsyncLifetime
		{
			public ClassWithAsyncLifetime_ThrowingCtor(ITestOutputHelper output)
				: base(output)
			{
				throw new DivideByZeroException();
			}
		}

		[Fact]
		public async ValueTask ThrowingInitializeAsync()
		{
			var messages = await RunAsync<ITestFailed>(typeof(ClassWithAsyncLifetime_ThrowingInitializeAsync));

			var message = Assert.Single(messages);
			AssertOperations(message, "Constructor", "InitializeAsync");
		}

		class ClassWithAsyncLifetime_ThrowingInitializeAsync : ClassWithAsyncLifetime
		{
			public ClassWithAsyncLifetime_ThrowingInitializeAsync(ITestOutputHelper output) : base(output) { }

			public override async ValueTask InitializeAsync()
			{
				await base.InitializeAsync();

				throw new DivideByZeroException();
			}
		}

		[Fact]
		public async ValueTask ThrowingDisposeAsync()
		{
			var messages = await RunAsync<ITestFailed>(typeof(ClassWithAsyncLifetime_ThrowingDisposeAsync));

			var message = Assert.Single(messages);
			AssertOperations(message, "Constructor", "InitializeAsync", "Run Test", "DisposeAsync");
		}

		class ClassWithAsyncLifetime_ThrowingDisposeAsync : ClassWithAsyncLifetime
		{
			public ClassWithAsyncLifetime_ThrowingDisposeAsync(ITestOutputHelper output) : base(output) { }

			public override async ValueTask DisposeAsync()
			{
				await base.DisposeAsync();

				throw new DivideByZeroException();
			}
		}

		[Fact]
		public async ValueTask ThrowingDisposeAsync_Disposable()
		{
			var messages = await RunAsync<ITestFailed>(typeof(ClassWithAsyncDisposable_ThrowingDisposeAsync));

			var message = Assert.Single(messages);
			AssertOperations(message, "Constructor", "Run Test", "DisposeAsync");
		}

		class ClassWithAsyncDisposable_ThrowingDisposeAsync : ClassWithAsyncDisposable
		{
			public ClassWithAsyncDisposable_ThrowingDisposeAsync(ITestOutputHelper output) : base(output) { }

			public override async ValueTask DisposeAsync()
			{
				await base.DisposeAsync();

				throw new DivideByZeroException();
			}
		}

		[Fact]
		public async ValueTask FailingTest()
		{
			var messages = await RunAsync<ITestFailed>(typeof(ClassWithAsyncLifetime_FailingTest));

			var message = Assert.Single(messages);
			AssertOperations(message, "Constructor", "InitializeAsync", "Run Test", "DisposeAsync");
		}

		class ClassWithAsyncLifetime_FailingTest : ClassWithAsyncLifetime
		{
			public ClassWithAsyncLifetime_FailingTest(ITestOutputHelper output) : base(output) { }

			public override void TheTest()
			{
				base.TheTest();

				throw new DivideByZeroException();
			}
		}

		void AssertOperations(ITestResultMessage result, params string[] operations)
		{
			Assert.Collection(
				result.Output.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries),
				operations.Select<string, Action<string>>(expected => actual => Assert.Equal(expected, actual)).ToArray()
			);
		}
	}

	public class Warnings : AcceptanceTestV3
	{
		[Fact]
		public async ValueTask LegalWarnings()
		{
			var results = await RunForResultsAsync(typeof(ClassWithLegalWarnings));

			Assert.Collection(
				results.OrderBy(result => result.TestDisplayName),
				msg =>
				{
					var failed = Assert.IsAssignableFrom<TestFailedWithDisplayName>(msg);
					Assert.Equal($"{typeof(ClassWithLegalWarnings).FullName}.{nameof(ClassWithLegalWarnings.Failing)}", failed.TestDisplayName);
					Assert.NotNull(failed.Warnings);
					Assert.Collection(
						failed.Warnings,
						warning => Assert.Equal("This is a warning message from the constructor", warning),
						warning => Assert.Equal("This is a warning message from Failing()", warning),
						warning => Assert.Equal("This is a warning message from Dispose()", warning)
					);
				},
				msg =>
				{
					var passed = Assert.IsAssignableFrom<TestPassedWithDisplayName>(msg);
					Assert.Equal($"{typeof(ClassWithLegalWarnings).FullName}.{nameof(ClassWithLegalWarnings.Passing)}", passed.TestDisplayName);
					Assert.NotNull(passed.Warnings);
					Assert.Collection(
						passed.Warnings,
						warning => Assert.Equal("This is a warning message from the constructor", warning),
						warning => Assert.Equal("This is a warning message from Passing()", warning),
						warning => Assert.Equal("This is a warning message from Dispose()", warning)
					);
				},
				msg =>
				{
					var skipped = Assert.IsAssignableFrom<TestSkippedWithDisplayName>(msg);
					Assert.Equal($"{typeof(ClassWithLegalWarnings).FullName}.{nameof(ClassWithLegalWarnings.Skipping)}", skipped.TestDisplayName);
					Assert.Null(skipped.Warnings);  // Ctor and Dispose are skipped, so no warnings
				},
				msg =>
				{
					var skipped = Assert.IsAssignableFrom<TestSkippedWithDisplayName>(msg);
					Assert.Equal($"{typeof(ClassWithLegalWarnings).FullName}.{nameof(ClassWithLegalWarnings.SkippingDynamic)}", skipped.TestDisplayName);
					Assert.NotNull(skipped.Warnings);
					Assert.Collection(
						skipped.Warnings,
						warning => Assert.Equal("This is a warning message from the constructor", warning),
						warning => Assert.Equal("This is a warning message from SkippingDynamic()", warning),
						warning => Assert.Equal("This is a warning message from Dispose()", warning)
					);
				}
			);
		}

		class ClassWithLegalWarnings : IDisposable
		{
			public ClassWithLegalWarnings()
			{
				TestContext.Current.AddWarning("This is a warning message from the constructor");
			}

			public void Dispose()
			{
				TestContext.Current.AddWarning("This is a warning message from Dispose()");
			}

			[Fact]
			public void Passing()
			{
				TestContext.Current.AddWarning("This is a warning message from Passing()");
			}

			[Fact]
			public void Failing()
			{
				TestContext.Current.AddWarning("This is a warning message from Failing()");
				Assert.True(false);
			}

			[Fact(Skip = "I never run")]
			public void Skipping()
			{ }

			[Fact]
			public void SkippingDynamic()
			{
				TestContext.Current.AddWarning("This is a warning message from SkippingDynamic()");
				Assert.Skip("I decided not to run");
			}
		}

		[Fact]
		public async ValueTask IllegalWarning()
		{
			var diagnosticSink = SpyMessageSink.Capture();

			var results = await RunForResultsAsync(typeof(ClassWithIllegalWarnings), diagnosticMessageSink: diagnosticSink);

			var diagnosticMessage = Assert.Single(diagnosticSink.Messages.OfType<IDiagnosticMessage>());
			Assert.Equal("Attempted to log a test warning message while not running a test (pipeline stage = TestClassExecution); message: This is a warning from an illegal part of the pipeline", diagnosticMessage.Message);
			var result = Assert.Single(results);
			// Illegal warning messages won't show up here, and won't prevent running tests
			var passed = Assert.IsAssignableFrom<TestPassedWithDisplayName>(result);
			Assert.Equal($"{typeof(ClassWithIllegalWarnings).FullName}.{nameof(ClassWithIllegalWarnings.Passing)}", passed.TestDisplayName);
			Assert.Null(passed.Warnings);
		}

		class FixtureWithIllegalWarning
		{
			public FixtureWithIllegalWarning() =>
				TestContext.Current.AddWarning("This is a warning from an illegal part of the pipeline");
		}

		class ClassWithIllegalWarnings : IClassFixture<FixtureWithIllegalWarning>
		{
			[Fact]
			public void Passing()
			{ }
		}
	}

	public class AsyncVoid : AcceptanceTestV3
	{
		[Fact]
		public async ValueTask AsyncVoidTestsAreFastFailed()
		{
			var results = await RunAsync(typeof(ClassWithAsyncVoidTest));

			var failedMessage = Assert.Single(results.OfType<ITestFailed>());
			var failedStarting = Assert.Single(results.OfType<ITestStarting>(), s => s.TestUniqueID == failedMessage.TestUniqueID);
			Assert.Equal("Xunit3AcceptanceTests+AsyncVoid+ClassWithAsyncVoidTest.TestMethod", failedStarting.TestDisplayName);
			var message = Assert.Single(failedMessage.Messages);
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
	}
}
