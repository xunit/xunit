using Xunit;
using Xunit.Sdk;

public partial class Xunit3AcceptanceTests
{
	public partial class AsyncLifetime : AcceptanceTestV3
	{
		[Fact]
		public async ValueTask AsyncLifetimeAcceptanceTest()
		{
#if XUNIT_AOT
			var messages = await RunForResultsAsync("Xunit3AcceptanceTests+AsyncLifetime+ClassWithAsyncLifetime");
#else
			var messages = await RunForResultsAsync(typeof(ClassWithAsyncLifetime));
#endif

			var message = Assert.Single(messages.OfType<TestPassedWithMetadata>());
			AssertOperations(message, "Constructor", "InitializeAsync", "Run Test", "DisposeAsync");
		}

		[Fact]
		public async ValueTask AsyncDisposableAcceptanceTest()
		{
#if XUNIT_AOT
			var messages = await RunForResultsAsync("Xunit3AcceptanceTests+AsyncLifetime+ClassWithAsyncDisposable");
#else
			var messages = await RunForResultsAsync(typeof(ClassWithAsyncDisposable));
#endif

			var message = Assert.Single(messages.OfType<TestPassedWithMetadata>());
			// We prefer DisposeAsync over Dispose, so Dispose won't be in the call list
			AssertOperations(message, "Constructor", "Run Test", "DisposeAsync");
		}

		[Fact]
		public async ValueTask DisposableAcceptanceTest()
		{
#if XUNIT_AOT
			var messages = await RunForResultsAsync("Xunit3AcceptanceTests+AsyncLifetime+ClassWithDisposable");
#else
			var messages = await RunForResultsAsync(typeof(ClassWithDisposable));
#endif

			var message = Assert.Single(messages.OfType<TestPassedWithMetadata>());
			AssertOperations(message, "Constructor", "Run Test", "Dispose");
		}

		[Fact]
		public async ValueTask ThrowingConstructor()
		{
#if XUNIT_AOT
			var messages = await RunForResultsAsync("Xunit3AcceptanceTests+AsyncLifetime+ClassWithAsyncLifetime_ThrowingCtor");
#else
			var messages = await RunForResultsAsync(typeof(ClassWithAsyncLifetime_ThrowingCtor));
#endif

			var message = Assert.Single(messages.OfType<TestFailedWithMetadata>());
			AssertOperations(message, "Constructor");
		}

		[Fact]
		public async ValueTask ThrowingInitializeAsync()
		{
#if XUNIT_AOT
			var messages = await RunForResultsAsync("Xunit3AcceptanceTests+AsyncLifetime+ClassWithAsyncLifetime_ThrowingInitializeAsync");
#else
			var messages = await RunForResultsAsync(typeof(ClassWithAsyncLifetime_ThrowingInitializeAsync));
#endif

			var message = Assert.Single(messages.OfType<TestFailedWithMetadata>());
			AssertOperations(message, "Constructor", "InitializeAsync");
		}

		[Fact]
		public async ValueTask ThrowingDisposeAsync()
		{
#if XUNIT_AOT
			var messages = await RunForResultsAsync("Xunit3AcceptanceTests+AsyncLifetime+ClassWithAsyncLifetime_ThrowingDisposeAsync");
#else
			var messages = await RunForResultsAsync(typeof(ClassWithAsyncLifetime_ThrowingDisposeAsync));
#endif

			var message = Assert.Single(messages.OfType<TestFailedWithMetadata>());
			AssertOperations(message, "Constructor", "InitializeAsync", "Run Test", "DisposeAsync");
		}

		[Fact]
		public async ValueTask ThrowingDisposeAsync_Disposable()
		{
#if XUNIT_AOT
			var messages = await RunForResultsAsync("Xunit3AcceptanceTests+AsyncLifetime+ClassWithAsyncDisposable_ThrowingDisposeAsync");
#else
			var messages = await RunForResultsAsync(typeof(ClassWithAsyncDisposable_ThrowingDisposeAsync));
#endif

			var message = Assert.Single(messages.OfType<TestFailedWithMetadata>());
			AssertOperations(message, "Constructor", "Run Test", "DisposeAsync");
		}

		[Fact]
		public async ValueTask FailingTest()
		{
#if XUNIT_AOT
			var messages = await RunForResultsAsync("Xunit3AcceptanceTests+AsyncLifetime+ClassWithAsyncLifetime_FailingTest");
#else
			var messages = await RunForResultsAsync(typeof(ClassWithAsyncLifetime_FailingTest));
#endif

			var message = Assert.Single(messages.OfType<TestFailedWithMetadata>());
			AssertOperations(message, "Constructor", "InitializeAsync", "Run Test", "DisposeAsync");
		}

		static void AssertOperations(
			ITestResultMessage result,
			params string[] operations)
		{
			Assert.Collection(
				result.Output.Split([Environment.NewLine], StringSplitOptions.RemoveEmptyEntries),
				operations.Select<string, Action<string>>(expected => actual => Assert.Equal(expected, actual)).ToArray()
			);
		}
	}

	public partial class ClassFailures : AcceptanceTestV3
	{
		[Fact]
		public async ValueTask TestFailureResultsFromThrowingCtorInTestClass()
		{
#if XUNIT_AOT
			var messages = await RunAsync<ITestFailed>("Xunit3AcceptanceTests+ClassFailures+ClassUnderTest_CtorFailure");
#else
			var messages = await RunAsync<ITestFailed>(typeof(ClassUnderTest_CtorFailure));
#endif

			var msg = Assert.Single(messages);
			Assert.Equal(typeof(DivideByZeroException).FullName, msg.ExceptionTypes.Single());
		}

		[Fact]
		public async ValueTask TestFailureResultsFromThrowingDisposeInTestClass()
		{
#if XUNIT_AOT
			var messages = await RunAsync<ITestFailed>("Xunit3AcceptanceTests+ClassFailures+ClassUnderTest_DisposeFailure");
#else
			var messages = await RunAsync<ITestFailed>(typeof(ClassUnderTest_DisposeFailure));
#endif

			var msg = Assert.Single(messages);
			Assert.Equal(typeof(DivideByZeroException).FullName, msg.ExceptionTypes.Single());
		}

		[Fact]
		public async ValueTask CompositeTestFailureResultsFromFailingTestsPlusThrowingDisposeInTestClass()
		{
#if XUNIT_AOT
			var messages = await RunAsync<ITestFailed>("Xunit3AcceptanceTests+ClassFailures+ClassUnderTest_FailingTestAndDisposeFailure");
#else
			var messages = await RunAsync<ITestFailed>(typeof(ClassUnderTest_FailingTestAndDisposeFailure));
#endif

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
	}

	public partial class EndToEndMessageInspection : AcceptanceTestV3
	{
		[Fact]
		public async ValueTask NoTests()
		{
#if XUNIT_AOT
			var results = await RunAsync("Xunit3AcceptanceTests+EndToEndMessageInspection+NoTestsClass");
#else
			var results = await RunAsync(typeof(NoTestsClass));
#endif

			Assert.Collection(
				results,
#if XUNIT_AOT
				message => Assert.IsType<IDiscoveryStarting>(message, exactMatch: false),
				message => Assert.IsType<IDiscoveryComplete>(message, exactMatch: false),
#endif
				message => Assert.IsType<ITestAssemblyStarting>(message, exactMatch: false),
				message =>
				{
					var finished = Assert.IsType<ITestAssemblyFinished>(message, exactMatch: false);
					Assert.Equal(0, finished.TestsFailed);
					Assert.Equal(0, finished.TestsNotRun);
					Assert.Equal(0, finished.TestsSkipped);
					Assert.Equal(0, finished.TestsTotal);
				}
			);
		}

		[Fact]
		public async ValueTask SinglePassingTest()
		{
			string? observedAssemblyID = default;
			string? observedCollectionID = default;
			string? observedClassID = default;
			string? observedMethodID = default;
			string? observedTestCaseID = default;
			string? observedTestID = default;

#if XUNIT_AOT
			var results = await RunAsync("Xunit3AcceptanceTests+EndToEndMessageInspection+SinglePassingTestClass");
#else
			var results = await RunAsync(typeof(SinglePassingTestClass));
#endif

			Assert.Collection(
				results,
#if XUNIT_AOT
				message => Assert.IsType<IDiscoveryStarting>(message, exactMatch: false),
				message => Assert.IsType<IDiscoveryComplete>(message, exactMatch: false),
#endif
				message =>
				{
					var assemblyStarting = Assert.IsType<ITestAssemblyStarting>(message, exactMatch: false);
					observedAssemblyID = assemblyStarting.AssemblyUniqueID;
				},
				message =>
				{
					var collectionStarting = Assert.IsType<ITestCollectionStarting>(message, exactMatch: false);
					Assert.Null(collectionStarting.TestCollectionClassName);
#if XUNIT_AOT
					Assert.Equal("Test collection for Xunit3AcceptanceTests+EndToEndMessageInspection+SinglePassingTestClass (id: 9ca1ef2f7586d532aa99c0c3ee8c83423397fa1a081d6710ddd469a53b86b896)", collectionStarting.TestCollectionDisplayName);
#elif BUILD_X86 && NETFRAMEWORK
					Assert.Equal($"Test collection for {typeof(SinglePassingTestClass).SafeName()} (id: baecc5d723250b796cb1837178bce452e7c9c835e44cd84afa6886c34e29bcf3)", collectionStarting.TestCollectionDisplayName);
#elif BUILD_X86 && NETCOREAPP
					Assert.Equal($"Test collection for {typeof(SinglePassingTestClass).SafeName()} (id: 34e1c9d5446707b84ac58a7a312fbe215b2165fc84e18b10f82da86012aa9d36)", collectionStarting.TestCollectionDisplayName);
#elif NETFRAMEWORK
					Assert.Equal($"Test collection for {typeof(SinglePassingTestClass).SafeName()} (id: c86ca6a0bc8fae00a67ce0ec2044024f53b2b138fff360c4e0c5823b2275f0fa)", collectionStarting.TestCollectionDisplayName);
#elif NETCOREAPP
					Assert.Equal($"Test collection for {typeof(SinglePassingTestClass).SafeName()} (id: 7eff8c323e4b45e5317effcd5e3c1d5c4b9a2561083dc0529663e94fd32dc9d0)", collectionStarting.TestCollectionDisplayName);
#else
#error Unknown target build environment
#endif
					Assert.NotEmpty(collectionStarting.TestCollectionUniqueID);
					Assert.Equal(observedAssemblyID, collectionStarting.AssemblyUniqueID);
					observedCollectionID = collectionStarting.TestCollectionUniqueID;
				},
				message =>
				{
					var classStarting = Assert.IsType<ITestClassStarting>(message, exactMatch: false);
					Assert.Equal("Xunit3AcceptanceTests+EndToEndMessageInspection+SinglePassingTestClass", classStarting.TestClassName);
					Assert.Equal(observedAssemblyID, classStarting.AssemblyUniqueID);
					Assert.Equal(observedCollectionID, classStarting.TestCollectionUniqueID);
					observedClassID = classStarting.TestClassUniqueID;
				},
				message =>
				{
					var testMethodStarting = Assert.IsType<ITestMethodStarting>(message, exactMatch: false);
					Assert.Equal("TestMethod", testMethodStarting.MethodName);
					Assert.Equal(observedAssemblyID, testMethodStarting.AssemblyUniqueID);
					Assert.Equal(observedCollectionID, testMethodStarting.TestCollectionUniqueID);
					Assert.Equal(observedClassID, testMethodStarting.TestClassUniqueID);
					observedMethodID = testMethodStarting.TestMethodUniqueID;
				},
				message =>
				{
					var testCaseStarting = Assert.IsType<ITestCaseStarting>(message, exactMatch: false);
					Assert.Equal(observedAssemblyID, testCaseStarting.AssemblyUniqueID);
					Assert.Equal("Xunit3AcceptanceTests+EndToEndMessageInspection+SinglePassingTestClass.TestMethod", testCaseStarting.TestCaseDisplayName);
					Assert.Equal(observedCollectionID, testCaseStarting.TestCollectionUniqueID);
					Assert.Equal(observedClassID, testCaseStarting.TestClassUniqueID);
					Assert.Equal(observedMethodID, testCaseStarting.TestMethodUniqueID);
					observedTestCaseID = testCaseStarting.TestCaseUniqueID;
				},
				message =>
				{
					var testStarting = Assert.IsType<ITestStarting>(message, exactMatch: false);
					Assert.Equal(observedAssemblyID, testStarting.AssemblyUniqueID);
					// Test display name == test case display name for Facts
					Assert.Equal("Xunit3AcceptanceTests+EndToEndMessageInspection+SinglePassingTestClass.TestMethod", testStarting.TestDisplayName);
					Assert.Equal(observedTestCaseID, testStarting.TestCaseUniqueID);
					Assert.Equal(observedCollectionID, testStarting.TestCollectionUniqueID);
					Assert.Equal(observedClassID, testStarting.TestClassUniqueID);
					Assert.Equal(observedMethodID, testStarting.TestMethodUniqueID);
					observedTestID = testStarting.TestUniqueID;
				},
				message =>
				{
					var classConstructionStarting = Assert.IsType<ITestClassConstructionStarting>(message, exactMatch: false);
					Assert.Equal(observedAssemblyID, classConstructionStarting.AssemblyUniqueID);
					Assert.Equal(observedTestCaseID, classConstructionStarting.TestCaseUniqueID);
					Assert.Equal(observedCollectionID, classConstructionStarting.TestCollectionUniqueID);
					Assert.Equal(observedClassID, classConstructionStarting.TestClassUniqueID);
					Assert.Equal(observedMethodID, classConstructionStarting.TestMethodUniqueID);
					Assert.Equal(observedTestID, classConstructionStarting.TestUniqueID);
				},
				message =>
				{
					var classConstructionFinished = Assert.IsType<ITestClassConstructionFinished>(message, exactMatch: false);
					Assert.Equal(observedAssemblyID, classConstructionFinished.AssemblyUniqueID);
					Assert.Equal(observedTestCaseID, classConstructionFinished.TestCaseUniqueID);
					Assert.Equal(observedCollectionID, classConstructionFinished.TestCollectionUniqueID);
					Assert.Equal(observedClassID, classConstructionFinished.TestClassUniqueID);
					Assert.Equal(observedMethodID, classConstructionFinished.TestMethodUniqueID);
					Assert.Equal(observedTestID, classConstructionFinished.TestUniqueID);
				},
				message =>
				{
					var testPassed = Assert.IsType<ITestPassed>(message, exactMatch: false);
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
					var testFinished = Assert.IsType<ITestFinished>(message, exactMatch: false);
					Assert.Equal(observedAssemblyID, testFinished.AssemblyUniqueID);
					Assert.Equal(observedTestCaseID, testFinished.TestCaseUniqueID);
					Assert.Equal(observedClassID, testFinished.TestClassUniqueID);
					Assert.Equal(observedCollectionID, testFinished.TestCollectionUniqueID);
					Assert.Equal(observedMethodID, testFinished.TestMethodUniqueID);
					Assert.Equal(observedTestID, testFinished.TestUniqueID);
				},
				message =>
				{
					var testCaseFinished = Assert.IsType<ITestCaseFinished>(message, exactMatch: false);
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
					var testMethodFinished = Assert.IsType<ITestMethodFinished>(message, exactMatch: false);
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
					var classFinished = Assert.IsType<ITestClassFinished>(message, exactMatch: false);
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
					var collectionFinished = Assert.IsType<ITestCollectionFinished>(message, exactMatch: false);
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
					var assemblyFinished = Assert.IsType<ITestAssemblyFinished>(message, exactMatch: false);
					Assert.Equal(observedAssemblyID, assemblyFinished.AssemblyUniqueID);
					Assert.NotEqual(0M, assemblyFinished.ExecutionTime);
					Assert.Equal(0, assemblyFinished.TestsFailed);
					Assert.Equal(0, assemblyFinished.TestsNotRun);
					Assert.Equal(0, assemblyFinished.TestsSkipped);
					Assert.Equal(1, assemblyFinished.TestsTotal);
				}
			);
		}
	}

	public partial class ErrorAggregation : AcceptanceTestV3
	{
		[Fact]
		public async ValueTask EachTestMethodHasIndividualExceptionMessage()
		{
#if XUNIT_AOT
			var testMessages = await RunAsync("Xunit3AcceptanceTests+ErrorAggregation+ClassUnderTest");
#else
			var testMessages = await RunAsync(typeof(ClassUnderTest));
#endif

			var equalStarting = Assert.Single(testMessages.OfType<ITestStarting>(), msg => msg.TestDisplayName == "Xunit3AcceptanceTests+ErrorAggregation+ClassUnderTest.EqualFailure");
			var equalFailure = Assert.Single(testMessages.OfType<ITestFailed>(), msg => msg.TestUniqueID == equalStarting.TestUniqueID);
			Assert.Contains("Assert.Equal() Failure", equalFailure.Messages.Single());

			var notNullStarting = Assert.Single(testMessages.OfType<ITestStarting>(), msg => msg.TestDisplayName == "Xunit3AcceptanceTests+ErrorAggregation+ClassUnderTest.NotNullFailure");
			var notNullFailure = Assert.Single(testMessages.OfType<ITestFailed>(), msg => msg.TestUniqueID == notNullStarting.TestUniqueID);
			Assert.Contains("Assert.NotNull() Failure", notNullFailure.Messages.Single());
		}
	}

	public partial class ExplicitTests : AcceptanceTestV3
	{
		[Theory]
		[InlineData(null)]
		[InlineData(ExplicitOption.Off)]
		public async ValueTask OnlyRunNonExplicit(ExplicitOption? @explicit)
		{
#if XUNIT_AOT
			var results = await RunForResultsAsync("Xunit3AcceptanceTests+ExplicitTests+ClassWithExplicitTest", explicitOption: @explicit);
#else
			var results = await RunForResultsAsync(typeof(ClassWithExplicitTest), explicitOption: @explicit);
#endif

			Assert.Equal(2, results.Count);
			var passed = Assert.Single(results.OfType<TestPassedWithMetadata>());
			Assert.Equal("Xunit3AcceptanceTests+ExplicitTests+ClassWithExplicitTest.NonExplicitTest", passed.Test.TestDisplayName);
			var notRun = Assert.Single(results.OfType<TestNotRunWithMetadata>());
			Assert.Equal("Xunit3AcceptanceTests+ExplicitTests+ClassWithExplicitTest.ExplicitTest", notRun.Test.TestDisplayName);
		}

		[Fact]
		public async ValueTask OnlyRunExplicit()
		{
#if XUNIT_AOT
			var results = await RunForResultsAsync("Xunit3AcceptanceTests+ExplicitTests+ClassWithExplicitTest", explicitOption: ExplicitOption.Only);
#else
			var results = await RunForResultsAsync(typeof(ClassWithExplicitTest), explicitOption: ExplicitOption.Only);
#endif

			Assert.Equal(2, results.Count);
			var notRun = Assert.Single(results.OfType<TestNotRunWithMetadata>());
			Assert.Equal("Xunit3AcceptanceTests+ExplicitTests+ClassWithExplicitTest.NonExplicitTest", notRun.Test.TestDisplayName);
			var failed = Assert.Single(results.OfType<TestFailedWithMetadata>());
			Assert.Equal("Xunit3AcceptanceTests+ExplicitTests+ClassWithExplicitTest.ExplicitTest", failed.Test.TestDisplayName);
		}

		[Fact]
		public async ValueTask RunEverything()
		{
#if XUNIT_AOT
			var results = await RunForResultsAsync("Xunit3AcceptanceTests+ExplicitTests+ClassWithExplicitTest", explicitOption: ExplicitOption.On);
#else
			var results = await RunForResultsAsync(typeof(ClassWithExplicitTest), explicitOption: ExplicitOption.On);
#endif

			Assert.Equal(2, results.Count);
			var passed = Assert.Single(results.OfType<TestPassedWithMetadata>());
			Assert.Equal("Xunit3AcceptanceTests+ExplicitTests+ClassWithExplicitTest.NonExplicitTest", passed.Test.TestDisplayName);
			var failed = Assert.Single(results.OfType<TestFailedWithMetadata>());
			Assert.Equal("Xunit3AcceptanceTests+ExplicitTests+ClassWithExplicitTest.ExplicitTest", failed.Test.TestDisplayName);
		}
	}

	public partial class FailingTests : AcceptanceTestV3
	{
		[Fact]
		public async ValueTask SingleFailingTest()
		{
#if XUNIT_AOT
			var results = await RunAsync("Xunit3AcceptanceTests+FailingTests+SingleFailingTestClass");
#else
			var results = await RunAsync(typeof(SingleFailingTestClass));
#endif

			var failedMessage = Assert.Single(results.OfType<ITestFailed>());
			Assert.Equal(typeof(TrueException).FullName, failedMessage.ExceptionTypes.Single());

			var classFinishedMessage = Assert.Single(results.OfType<ITestClassFinished>());
			Assert.Equal(1, classFinishedMessage.TestsFailed);

			var collectionFinishedMessage = Assert.Single(results.OfType<ITestCollectionFinished>());
			Assert.Equal(1, collectionFinishedMessage.TestsFailed);
		}

		[Fact]
		public async ValueTask SingleFailingTestReturningValueTask()
		{
#if XUNIT_AOT
			var results = await RunAsync("Xunit3AcceptanceTests+FailingTests+SingleFailingValueTaskTestClass");
#else
			var results = await RunAsync(typeof(SingleFailingValueTaskTestClass));
#endif

			var failedMessage = Assert.Single(results.OfType<ITestFailed>());
			Assert.Equal(typeof(TrueException).FullName, failedMessage.ExceptionTypes.Single());

			var classFinishedMessage = Assert.Single(results.OfType<ITestClassFinished>());
			Assert.Equal(1, classFinishedMessage.TestsFailed);

			var collectionFinishedMessage = Assert.Single(results.OfType<ITestCollectionFinished>());
			Assert.Equal(1, collectionFinishedMessage.TestsFailed);
		}
	}

	public partial class NonStartedTasks : AcceptanceTestV3
	{
		[Fact]
		public async ValueTask TestWithUnstartedTaskThrows()
		{
#if XUNIT_AOT
			//Assert.Skip("This test is not running successfully in AOT yet");
			var results = await RunAsync("Xunit3AcceptanceTests+NonStartedTasks+ClassUnderTest");
#else
			var results = await RunAsync(typeof(ClassUnderTest));
#endif

			var failedMessage = Assert.Single(results.OfType<ITestFailed>());
			var failedStarting = results.OfType<ITestStarting>().Single(s => s.TestUniqueID == failedMessage.TestUniqueID);
			Assert.Equal("Xunit3AcceptanceTests+NonStartedTasks+ClassUnderTest.NonStartedTask", failedStarting.TestDisplayName);
			Assert.Equal("Test method returned a non-started Task (tasks must be started before being returned)", failedMessage.Messages.Single());
		}
	}

	public partial class SkippedTests : AcceptanceTestV3
	{
		[Fact]
		public async ValueTask SingleSkippedTest()
		{
#if XUNIT_AOT
			var results = await RunAsync("Xunit3AcceptanceTests+SkippedTests+SingleSkippedTestClass");
#else
			var results = await RunAsync(typeof(SingleSkippedTestClass));
#endif

			var skippedMessage = Assert.Single(results.OfType<ITestSkipped>());
			var skippedStarting = Assert.Single(results.OfType<ITestStarting>(), s => s.TestUniqueID == skippedMessage.TestUniqueID);
			Assert.Equal("Xunit3AcceptanceTests+SkippedTests+SingleSkippedTestClass.TestMethod", skippedStarting.TestDisplayName);
			Assert.Equal("This is a skipped test", skippedMessage.Reason);

			var classFinishedMessage = Assert.Single(results.OfType<ITestClassFinished>());
			Assert.Equal(1, classFinishedMessage.TestsTotal);
			Assert.Equal(1, classFinishedMessage.TestsSkipped);

			var collectionFinishedMessage = Assert.Single(results.OfType<ITestCollectionFinished>());
			Assert.Equal(1, collectionFinishedMessage.TestsTotal);
			Assert.Equal(1, collectionFinishedMessage.TestsSkipped);
		}

		[Fact]
		public async ValueTask ConditionallySkippedTests()
		{
#if XUNIT_AOT
			var results = await RunAsync("Xunit3AcceptanceTests+SkippedTests+ConditionallySkippedTestClass");
#else
			var results = await RunAsync(typeof(ConditionallySkippedTestClass));
#endif

			var skippedMessage = Assert.Single(results.OfType<ITestSkipped>());
			var skippedStarting = Assert.Single(results.OfType<ITestStarting>(), s => s.TestUniqueID == skippedMessage.TestUniqueID);
			Assert.Equal("Xunit3AcceptanceTests+SkippedTests+ConditionallySkippedTestClass.ConditionallyAlwaysSkipped(value: False)", skippedStarting.TestDisplayName);
			Assert.Equal("I am always skipped, conditionally", skippedMessage.Reason);

			var passedMessage = Assert.Single(results.OfType<ITestPassed>());
			var passedStarting = Assert.Single(results.OfType<ITestStarting>(), s => s.TestUniqueID == passedMessage.TestUniqueID);
			Assert.Equal("Xunit3AcceptanceTests+SkippedTests+ConditionallySkippedTestClass.ConditionallyNeverSkipped(value: True)", passedStarting.TestDisplayName);

			var failedMessage = Assert.Single(results.OfType<ITestFailed>());
			var failedStarting = Assert.Single(results.OfType<ITestStarting>(), s => s.TestUniqueID == failedMessage.TestUniqueID);
			Assert.Equal("Xunit3AcceptanceTests+SkippedTests+ConditionallySkippedTestClass.ConditionallyNeverSkipped(value: False)", failedStarting.TestDisplayName);
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

		[Fact]
		public async ValueTask ConditionallySkippedTests_UsingSkipType()
		{
#if XUNIT_AOT
			var results = await RunAsync("Xunit3AcceptanceTests+SkippedTests+ConditionallySkippedTestsClass_UsingSkipType");
#else
			var results = await RunAsync(typeof(ConditionallySkippedTestsClass_UsingSkipType));
#endif

			var skippedMessage = Assert.Single(results.OfType<ITestSkipped>());
			var skippedStarting = Assert.Single(results.OfType<ITestStarting>(), s => s.TestUniqueID == skippedMessage.TestUniqueID);
			Assert.Equal("Xunit3AcceptanceTests+SkippedTests+ConditionallySkippedTestsClass_UsingSkipType.ConditionallyAlwaysSkipped", skippedStarting.TestDisplayName);
			Assert.Equal("I am always skipped, conditionally", skippedMessage.Reason);

			var passedMessage = Assert.Single(results.OfType<ITestPassed>());
			var passedStarting = Assert.Single(results.OfType<ITestStarting>(), s => s.TestUniqueID == passedMessage.TestUniqueID);
			Assert.Equal("Xunit3AcceptanceTests+SkippedTests+ConditionallySkippedTestsClass_UsingSkipType.ConditionallyNeverSkipped", passedStarting.TestDisplayName);

			var classFinishedMessage = Assert.Single(results.OfType<ITestClassFinished>());
			Assert.Equal(2, classFinishedMessage.TestsTotal);
			Assert.Equal(1, classFinishedMessage.TestsSkipped);

			var collectionFinishedMessage = Assert.Single(results.OfType<ITestCollectionFinished>());
			Assert.Equal(2, collectionFinishedMessage.TestsTotal);
			Assert.Equal(1, collectionFinishedMessage.TestsSkipped);
		}
	}

	public partial class StaticClassSupport : AcceptanceTestV3
	{
		[Fact]
		public async ValueTask TestsCanBeInStaticClasses()
		{
#if XUNIT_AOT
			var testMessages = await RunAsync("Xunit3AcceptanceTests+StaticClassSupport+StaticClassUnderTest");
#else
			var testMessages = await RunAsync(typeof(StaticClassUnderTest));
#endif

			Assert.Single(testMessages.OfType<ITestPassed>());
			var starting = Assert.Single(testMessages.OfType<ITestStarting>());
			Assert.Equal("Xunit3AcceptanceTests+StaticClassSupport+StaticClassUnderTest.Passing", starting.TestDisplayName);
		}
	}

	public partial class TestContextAccessor : AcceptanceTestV3
	{
		[Fact]
		public async ValueTask CanInjectTestContextAccessor()
		{
#if XUNIT_AOT
			var msgs = await RunAsync("Xunit3AcceptanceTests+TestContextAccessor+ClassUnderTest");
#else
			var msgs = await RunAsync(typeof(ClassUnderTest));
#endif

			var displayName = Assert.Single(
				msgs
					.OfType<ITestPassed>()
					.Select(p => msgs.OfType<ITestStarting>().Single(s => s.TestMethodUniqueID == p.TestMethodUniqueID).TestDisplayName)
			);
			Assert.Equal("Xunit3AcceptanceTests+TestContextAccessor+ClassUnderTest.Passing", displayName);
		}
	}

	public partial class TestNonParallelOrdering : AcceptanceTestV3
	{
		[Fact]
		public async ValueTask NonParallelCollectionsRunLast()
		{
#if XUNIT_AOT
			var testMessages = await RunAsync(["Xunit3AcceptanceTests+TestNonParallelOrdering+TestClassNonParallelCollection", "Xunit3AcceptanceTests+TestNonParallelOrdering+TestClassParallelCollection"]);
#else
			var testMessages = await RunAsync([typeof(TestClassNonParallelCollection), typeof(TestClassParallelCollection)]);
#endif

			Assert.Collection(
				testMessages.OfType<ITestPassed>().Select(p => testMessages.OfType<ITestMethodStarting>().Single(s => s.TestMethodUniqueID == p.TestMethodUniqueID).MethodName),
				methodName => Assert.Equal("Test1", methodName),
				methodName => Assert.Equal("Test2", methodName),
				methodName => Assert.Equal("IShouldBeLast1", methodName),
				methodName => Assert.Equal("IShouldBeLast2", methodName)
			);
		}
	}

	public partial class TestOrdering : AcceptanceTestV3
	{
		[Fact]
		public async ValueTask OverrideOfOrderingAtCollectionLevel()
		{
#if XUNIT_AOT
			var testMessages = await RunAsync("Xunit3AcceptanceTests+TestOrdering+TestClassUsingCollection");
#else
			var testMessages = await RunAsync(typeof(TestClassUsingCollection));
#endif

			Assert.Collection(
				testMessages.OfType<ITestPassed>().Select(p => testMessages.OfType<ITestMethodStarting>().Where(s => s.TestMethodUniqueID == p.TestMethodUniqueID).Single().MethodName),
				methodName => Assert.Equal("Test1", methodName),
				methodName => Assert.Equal("Test2", methodName),
				methodName => Assert.Equal("Test3", methodName)
			);
		}

		[Fact]
		public async ValueTask OverrideOfOrderingAtClassLevel()
		{
#if XUNIT_AOT
			var testMessages = await RunForResultsAsync("Xunit3AcceptanceTests+TestOrdering+TestClassWithoutCollection");
#else
			var testMessages = await RunForResultsAsync(typeof(TestClassWithoutCollection));
#endif

			Assert.Collection(
				testMessages.OfType<TestPassedWithMetadata>().Select(p => p.TestMethod?.MethodName).OrderBy(x => x),
				methodName => Assert.Equal("Test1", methodName),
				methodName => Assert.Equal("Test2", methodName),
				methodName => Assert.Equal("Test3", methodName)
			);
		}
	}

	public partial class TestOutput : AcceptanceTestV3
	{
		[Fact]
		public async ValueTask SendOutputMessages()
		{
#if XUNIT_AOT
			var msgs = await RunAsync("Xunit3AcceptanceTests+TestOutput+ClassUnderTest");
#else
			var msgs = await RunAsync(typeof(ClassUnderTest));
#endif

			var idxOfTestPassed = msgs.FindIndex(msg => msg is ITestPassed);
			Assert.True(idxOfTestPassed >= 0, "Test should have passed");

			var idxOfFirstTestOutput = msgs.FindIndex(msg => msg is ITestOutput);
			Assert.True(idxOfFirstTestOutput >= 0, "Test should have output");
			Assert.True(idxOfFirstTestOutput < idxOfTestPassed, "Test output messages should precede test result");

			Assert.Collection(
				msgs.OfType<ITestOutput>(),
				msg =>
				{
					var outputMessage = Assert.IsType<ITestOutput>(msg, exactMatch: false);
					Assert.Equal("This is output in the constructor" + Environment.NewLine, outputMessage.Output);
				},
				msg =>
				{
					var outputMessage = Assert.IsType<ITestOutput>(msg, exactMatch: false);
					Assert.Equal("This is ITest output" + Environment.NewLine, outputMessage.Output);
				},
				msg =>
				{
					var outputMessage = Assert.IsType<ITestOutput>(msg, exactMatch: false);
					Assert.Equal("This is output in Dispose" + Environment.NewLine, outputMessage.Output);
				}
			);
		}
	}

	public partial class Warnings : AcceptanceTestV3
	{
		[Fact]
		public async ValueTask LegalWarnings()
		{
#if XUNIT_AOT
			var results = await RunForResultsAsync("Xunit3AcceptanceTests+Warnings+ClassWithLegalWarnings");
#else
			var results = await RunForResultsAsync(typeof(ClassWithLegalWarnings));
#endif

			Assert.Collection(
				results.OrderBy(result => result.Test.TestDisplayName),
				msg =>
				{
					var failed = Assert.IsType<TestFailedWithMetadata>(msg, exactMatch: false);
					Assert.Equal("Xunit3AcceptanceTests+Warnings+ClassWithLegalWarnings.Failing", failed.Test.TestDisplayName);
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
					var passed = Assert.IsType<TestPassedWithMetadata>(msg, exactMatch: false);
					Assert.Equal("Xunit3AcceptanceTests+Warnings+ClassWithLegalWarnings.Passing", passed.Test.TestDisplayName);
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
					var skipped = Assert.IsType<TestSkippedWithMetadata>(msg, exactMatch: false);
					Assert.Equal("Xunit3AcceptanceTests+Warnings+ClassWithLegalWarnings.Skipping", skipped.Test.TestDisplayName);
					Assert.Null(skipped.Warnings);  // Ctor and Dispose are skipped, so no warnings
				},
				msg =>
				{
					var skipped = Assert.IsType<TestSkippedWithMetadata>(msg, exactMatch: false);
					Assert.Equal("Xunit3AcceptanceTests+Warnings+ClassWithLegalWarnings.SkippingDynamic", skipped.Test.TestDisplayName);
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

		[Fact]
		public async ValueTask IllegalWarning()
		{
			var diagnosticSink = SpyMessageSink.Capture();

#if XUNIT_AOT
			var results = await RunForResultsAsync("Xunit3AcceptanceTests+Warnings+ClassWithIllegalWarnings", diagnosticMessageSink: diagnosticSink);
#else
			var results = await RunForResultsAsync(typeof(ClassWithIllegalWarnings), diagnosticMessageSink: diagnosticSink);
#endif

			var diagnosticMessages = diagnosticSink.Messages.OfType<IDiagnosticMessage>().Select(dm => dm.Message).ToArray();
			Assert.Contains("Attempted to log a test warning message while not running a test (pipeline stage = TestClassExecution); message: This is a warning from an illegal part of the pipeline", diagnosticMessages);
			var result = Assert.Single(results);
			// Illegal warning messages won't show up here, and won't prevent running tests
			var passed = Assert.IsType<TestPassedWithMetadata>(result, exactMatch: false);
			Assert.Equal("Xunit3AcceptanceTests+Warnings+ClassWithIllegalWarnings.Passing", passed.Test.TestDisplayName);
			Assert.Null(passed.Warnings);
		}
	}
}
