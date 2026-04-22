using Microsoft.Testing.Extensions.TrxReport.Abstractions;
using Microsoft.Testing.Platform.Configurations;
using Microsoft.Testing.Platform.Extensions.Messages;
using Microsoft.Testing.Platform.OutputDevice;
using Microsoft.Testing.Platform.TestHost;
using Xunit;
using Xunit.MicrosoftTestingPlatform;
using Xunit.Sdk;

public static class TestPlatformExecutionMessageSinkTests
{
	[Theory]
	[InlineData(true)]
	[InlineData(false)]
	public static void DelegatesMessages(bool returnValue)
	{
		var message = TestData.DiagnosticMessage();
		var classUnderTest = TestableTestPlatformExecutionMessageSink.Create();
		classUnderTest.InnerSink.Callback = _ => returnValue;

		var result = classUnderTest.OnMessage(message);

		Assert.Equal(returnValue, result);
		var received = Assert.Single(classUnderTest.InnerSink.Messages);
		Assert.Same(message, received);
	}

	public static class Attachments
	{
		[Fact]
		public static void WritesAttachmentsToResultsDirectory()
		{
			var resultsPath = Path.Combine(Path.GetTempPath(), "attachment-test-" + Guid.NewGuid().ToString("n"));
			Directory.CreateDirectory(resultsPath);

			try
			{
				var classUnderTest = TestableTestPlatformExecutionMessageSink.Create(resultsPath: resultsPath);
				var attachments = new Dictionary<string, TestAttachment>
				{
					["foo"] = TestAttachment.Create("bar"),
					["baz"] = TestAttachment.Create([1, 2, 3], "application/octet"),
				};
				var testStarting = TestData.TestStarting(testUniqueID: "def456");
				var testPassed = TestData.TestPassed(testUniqueID: "def456");
				var testFinished = TestData.TestFinished(testUniqueID: "def456", attachments: attachments);

				classUnderTest.OnMessage(testStarting);
				classUnderTest.OnMessage(testPassed);
				classUnderTest.OnMessage(testFinished);

				var basePath = Path.Combine(resultsPath, "def456");
				Assert.Equal("bar", File.ReadAllText(Path.Combine(basePath, "foo.txt")));
				Assert.Equal([1, 2, 3], File.ReadAllBytes(Path.Combine(basePath, "baz.bin")));
			}
			finally
			{
				// Best effort to clean up the temp folder behind us
				try
				{
					Directory.Delete(resultsPath, recursive: true);
				}
				catch { }
			}
		}
	}

	public static class MessageMapping
	{
		[Fact]
		public static void ITestAssemblyCleanupFailure()
		{
			var classUnderTest = TestableTestPlatformExecutionMessageSink.Create();
			SendStartingMessages(classUnderTest);

			classUnderTest.OnMessage(TestData.TestAssemblyCleanupFailure());

			AssertCleanupFailure(classUnderTest, "Test Assembly");
		}

		[Fact]
		public static void ITestCaseCleanupFailure()
		{
			var classUnderTest = TestableTestPlatformExecutionMessageSink.Create();
			SendStartingMessages(classUnderTest);

			classUnderTest.OnMessage(TestData.TestCaseCleanupFailure());

			AssertCleanupFailure(classUnderTest, "Test Case");
		}

		[Fact]
		public static void ITestClassCleanupFailure()
		{
			var classUnderTest = TestableTestPlatformExecutionMessageSink.Create();
			SendStartingMessages(classUnderTest);

			classUnderTest.OnMessage(TestData.TestClassCleanupFailure());

			AssertCleanupFailure(classUnderTest, "Test Class");
		}

		[Fact]
		public static void ITestCleanupFailure()
		{
			var classUnderTest = TestableTestPlatformExecutionMessageSink.Create();
			SendStartingMessages(classUnderTest);

			classUnderTest.OnMessage(TestData.TestCleanupFailure());

			AssertCleanupFailure(classUnderTest, "Test");
		}

		[Fact]
		public static void ITestCollectionCleanupFailure()
		{
			var classUnderTest = TestableTestPlatformExecutionMessageSink.Create();
			SendStartingMessages(classUnderTest);

			classUnderTest.OnMessage(TestData.TestCollectionCleanupFailure());

			AssertCleanupFailure(classUnderTest, "Test Collection");
		}

		[Theory]
		[InlineData(false)]
		[InlineData(true)]
		public static void ITestFailed_Assertion(bool trxEnabled)
		{
			var classUnderTest = TestableTestPlatformExecutionMessageSink.Create(trxEnabled: trxEnabled);
			SendStartingMessages(classUnderTest, traits: trxEnabled ? TestData.DefaultTraitsWithCategory : TestData.DefaultTraits);

			SendTestFailed(classUnderTest, FailureCause.Assertion);

			var testNode = AssertStandardMetadata(classUnderTest, expectTrx: trxEnabled, failure: true);
			var failed = testNode.Properties.Single<FailedTestNodeStateProperty>();

#if MTP_V2
			Assert.Null(failed.Explanation);
#else
			Assert.Equal($"exception 1 : message 1{Environment.NewLine}---- exception 2 : message 2", failed.Explanation);
#endif

			Assert.NotNull(failed.Exception);
			Assert.Equal($"exception 1 : message 1{Environment.NewLine}---- exception 2 : message 2", failed.Exception.Message);
			Assert.Equal($"stack trace 1{Environment.NewLine}----- Inner Stack Trace -----{Environment.NewLine}stack trace 2", failed.Exception.StackTrace);
		}

		[Fact]
		public static void ITestFailed_Timeout()
		{
			var classUnderTest = TestableTestPlatformExecutionMessageSink.Create();
			SendStartingMessages(classUnderTest);

			SendTestFailed(classUnderTest, FailureCause.Timeout);

			var testNode = AssertStandardMetadata(classUnderTest);
			var timeout = testNode.Properties.Single<TimeoutTestNodeStateProperty>();

#if MTP_V2
			Assert.Null(timeout.Explanation);
#else
			Assert.Equal($"exception 1 : message 1{Environment.NewLine}---- exception 2 : message 2", timeout.Explanation);
#endif

			Assert.NotNull(timeout.Exception);
			Assert.Equal($"exception 1 : message 1{Environment.NewLine}---- exception 2 : message 2", timeout.Exception.Message);
			Assert.Equal($"stack trace 1{Environment.NewLine}----- Inner Stack Trace -----{Environment.NewLine}stack trace 2", timeout.Exception.StackTrace);
		}

		[Theory]
		[InlineData(FailureCause.Exception)]
		[InlineData(FailureCause.Other)]
		[InlineData(FailureCause.Unknown)]
		public static void ITestFailed_Error(FailureCause failureCause)
		{
			var classUnderTest = TestableTestPlatformExecutionMessageSink.Create();
			SendStartingMessages(classUnderTest);

			SendTestFailed(classUnderTest, failureCause);

			var testNode = AssertStandardMetadata(classUnderTest);
			var error = testNode.Properties.Single<ErrorTestNodeStateProperty>();

#if MTP_V2
			Assert.Null(error.Explanation);
#else
			Assert.Equal($"exception 1 : message 1{Environment.NewLine}---- exception 2 : message 2", error.Explanation);
#endif

			Assert.NotNull(error.Exception);
			Assert.Equal($"exception 1 : message 1{Environment.NewLine}---- exception 2 : message 2", error.Exception.Message);
			Assert.Equal($"stack trace 1{Environment.NewLine}----- Inner Stack Trace -----{Environment.NewLine}stack trace 2", error.Exception.StackTrace);
		}

		[Fact]
		public static void ITestMethodCleanupFailure()
		{
			var classUnderTest = TestableTestPlatformExecutionMessageSink.Create();
			SendStartingMessages(classUnderTest);

			classUnderTest.OnMessage(TestData.TestMethodCleanupFailure());

			AssertCleanupFailure(classUnderTest, "Test Method");
		}

		[Fact]
		public static void ITestNotRun_ServerModeTrue()
		{
			var classUnderTest = TestableTestPlatformExecutionMessageSink.Create(serverMode: true);
			SendStartingMessages(classUnderTest);

			classUnderTest.OnMessage(TestData.TestNotRun());
			classUnderTest.OnMessage(TestData.TestFinished());

			Assert.Empty(classUnderTest.TestNodeMessageBus.PublishedData);
		}

		[Fact]
		public static void ITestNotRun_ServerModeFalse()
		{
			var classUnderTest = TestableTestPlatformExecutionMessageSink.Create(serverMode: false);
			SendStartingMessages(classUnderTest);

			classUnderTest.OnMessage(TestData.TestNotRun());
			classUnderTest.OnMessage(TestData.TestFinished());

			var testNode = AssertStandardMetadata(classUnderTest);
			var skipped = testNode.Properties.Single<SkippedTestNodeStateProperty>();
			Assert.Equal("Not run (due to explicit test filtering)", skipped.Explanation);
		}

		[Theory]
		[InlineData(false)]
		[InlineData(true)]
		public static void ITestPassed(bool trxEnabled)
		{
			var classUnderTest = TestableTestPlatformExecutionMessageSink.Create(trxEnabled: trxEnabled);
			SendStartingMessages(classUnderTest, traits: trxEnabled ? TestData.DefaultTraitsWithCategory : TestData.DefaultTraits);

			classUnderTest.OnMessage(TestData.TestPassed());
			classUnderTest.OnMessage(TestData.TestFinished());

			var testNode = AssertStandardMetadata(classUnderTest, expectTrx: trxEnabled);
			var passed = testNode.Properties.Single<PassedTestNodeStateProperty>();
			Assert.Null(passed.Explanation);
		}

		[Fact]
		public static void ITestSkipped()
		{
			var classUnderTest = TestableTestPlatformExecutionMessageSink.Create();
			SendStartingMessages(classUnderTest);

			classUnderTest.OnMessage(TestData.TestSkipped());
			classUnderTest.OnMessage(TestData.TestFinished());

			var testNode = AssertStandardMetadata(classUnderTest);
			var skipped = testNode.Properties.Single<SkippedTestNodeStateProperty>();
			Assert.Equal(TestData.DefaultSkipReason, skipped.Explanation);
		}

		[Fact]
		public static void ITestStarting()
		{
			var classUnderTest = TestableTestPlatformExecutionMessageSink.Create();
			SendStartingMessages(classUnderTest, includeTestStarting: false);

			classUnderTest.OnMessage(TestData.TestStarting());

			var testNode = AssertStandardMetadata(classUnderTest, expectTiming: false);
			var inProgress = testNode.Properties.Single<InProgressTestNodeStateProperty>();
			Assert.Null(inProgress.Explanation);
		}

		[Fact]
		public static void ITestOutput_Off()
		{
			var classUnderTest = TestableTestPlatformExecutionMessageSink.Create(showLiveOutput: false);

			classUnderTest.OnMessage(TestData.TestOutput());

			Assert.Empty(classUnderTest.OutputDevice.DisplayedData);
		}

		[Fact]
		public static void ITestOutput_On()
		{
			var classUnderTest = TestableTestPlatformExecutionMessageSink.Create(showLiveOutput: true);
			SendStartingMessages(classUnderTest);

			classUnderTest.OnMessage(TestData.TestOutput());

			var data = Assert.Single(classUnderTest.OutputDevice.DisplayedData);
			AssertColorOutput(data, $"OUTPUT: [{TestData.DefaultTestDisplayName}] {TestData.DefaultOutput}", ConsoleColor.DarkGray);
		}

		[Fact]
		public static void Warnings()
		{
			var classUnderTest = TestableTestPlatformExecutionMessageSink.Create();
			SendStartingMessages(classUnderTest);

			classUnderTest.OnMessage(TestData.TestPassed(warnings: ["w1", "w2"]));
			classUnderTest.OnMessage(TestData.TestFinished());

			AssertStandardMetadata(classUnderTest, expectWarnings: true);
		}

		static void AssertCleanupFailure(
			TestableTestPlatformExecutionMessageSink classUnderTest,
			string cleanupType)
		{
			var testNode = GetTestNode(classUnderTest);
			Assert.Equal($"[{cleanupType} Cleanup Failure (test-display-name)]", testNode.DisplayName);
			var error = testNode.Properties.Single<ErrorTestNodeStateProperty>();

#if MTP_V2
			Assert.Null(error.Explanation);
#else
			Assert.Equal($"{TestData.DefaultExceptionTypes[0]} : {TestData.DefaultExceptionMessages[0]}", error.Explanation);
#endif

			Assert.NotNull(error.Exception);
			Assert.Equal($"{TestData.DefaultExceptionTypes[0]} : {TestData.DefaultExceptionMessages[0]}", error.Exception.Message);
			Assert.Equal(TestData.DefaultStackTraces[0], error.Exception.StackTrace);
		}

		static void AssertColorOutput(
			IOutputDeviceData data,
			string expectedMessage,
			ConsoleColor expectedForegroundColor)
		{
			var formattedData = Assert.IsType<FormattedTextOutputDeviceData>(data);
			var foregroundColor = Assert.IsType<SystemConsoleColor>(formattedData.ForegroundColor);
			Assert.Equal(expectedForegroundColor, foregroundColor.ConsoleColor);
			Assert.Equal(expectedMessage, formattedData.Text);
		}

		static TestNode AssertStandardMetadata(
			TestableTestPlatformExecutionMessageSink classUnderTest,
			bool expectTiming = true,
			bool expectWarnings = false,
			bool expectTrx = false,
			bool failure = false)
		{
			var testNode = GetTestNode(classUnderTest);
			Assert.Equal(TestData.DefaultTestDisplayName, testNode.DisplayName);
			Assert.Equal(TestData.DefaultTestCaseUniqueID, testNode.Uid.Value);

			var testMethod = testNode.Properties.Single<TestMethodIdentifierProperty>();
			Assert.Equal(TestData.DefaultAssemblyName, testMethod.AssemblyFullName);
			Assert.Equal(TestData.DefaultMethodName, testMethod.MethodName);
			Assert.Equal(TestData.DefaultTestClassNamespace, testMethod.Namespace);
			Assert.Equal(TestData.DefaultMethodParameterTypes, testMethod.ParameterTypeFullNames);
			Assert.Equal(TestData.DefaultMethodReturnType, testMethod.ReturnTypeFullName);
			Assert.Equal(TestData.DefaultTestClassSimpleName, testMethod.TypeName);

			var testLocation = testNode.Properties.Single<TestFileLocationProperty>();
			Assert.Equal("/source/file/path.cs", testLocation.FilePath);
			Assert.Equal(42, testLocation.LineSpan.Start.Line);
			Assert.Equal(-1, testLocation.LineSpan.Start.Column);
			Assert.Equal(42, testLocation.LineSpan.End.Line);
			Assert.Equal(-1, testLocation.LineSpan.End.Column);

			if (expectTiming)
			{
				var timing = testNode.Properties.Single<TimingProperty>();
				Assert.Equal(TestData.DefaultStartTime, timing.GlobalTiming.StartTime);
				Assert.Equal(TestData.DefaultFinishTime, timing.GlobalTiming.EndTime);
			}

			if (expectWarnings)
			{
				var data = classUnderTest.OutputDevice.DisplayedData;

				Assert.Equal(2, data.Count);
				AssertColorOutput(data[0], $"WARNING: [{TestData.DefaultTestDisplayName}] w1", ConsoleColor.Yellow);
				AssertColorOutput(data[1], $"WARNING: [{TestData.DefaultTestDisplayName}] w2", ConsoleColor.Yellow);
			}

			if (expectTrx)
			{
				// Include a 'category' trait in this metadata, for TRX testing
				var testMetadata = testNode.Properties.OfType<TestMetadataProperty>();
				Assert.Collection(
					testMetadata.OrderBy(p => p.Key).ThenBy(p => p.Value).Select(p => $"'{p.Key}' = '{p.Value}'"),
					keyValue => Assert.Equal("'biff' = 'bang'", keyValue),
					keyValue => Assert.Equal("'category' = 'interesting'", keyValue),
					keyValue => Assert.Equal("'foo' = 'bar'", keyValue),
					keyValue => Assert.Equal("'foo' = 'baz'", keyValue)
				);

				var trxFQTN = testNode.Properties.Single<TrxFullyQualifiedTypeNameProperty>();
				Assert.Equal(TestData.DefaultTestClassName, trxFQTN.FullyQualifiedTypeName);

				var trxCategories = testNode.Properties.Single<TrxCategoriesProperty>();
				var category = Assert.Single(trxCategories.Categories);
				Assert.Equal("interesting", category);

				var trxMessages = testNode.Properties.Single<TrxMessagesProperty>();
				var output = trxMessages.Messages.Single();
				Assert.Equal(TestData.DefaultOutput, output.Message);

				if (failure)
				{
					var trxException = testNode.Properties.Single<TrxExceptionProperty>();
					Assert.Equal($"exception 1 : message 1{Environment.NewLine}---- exception 2 : message 2", trxException.Message);
					Assert.Equal($"stack trace 1{Environment.NewLine}----- Inner Stack Trace -----{Environment.NewLine}stack trace 2", trxException.StackTrace);
				}
			}
			else
			{
				// No 'category' trait in this metadata, we only add that for TRX testing
				var testMetadata = testNode.Properties.OfType<TestMetadataProperty>();
				Assert.Collection(
					testMetadata.OrderBy(p => p.Key).ThenBy(p => p.Value).Select(p => $"'{p.Key}' = '{p.Value}'"),
					keyValue => Assert.Equal("'biff' = 'bang'", keyValue),
					keyValue => Assert.Equal("'foo' = 'bar'", keyValue),
					keyValue => Assert.Equal("'foo' = 'baz'", keyValue)
				);
			}

			return testNode;
		}

		static TestNode GetTestNode(TestableTestPlatformExecutionMessageSink classUnderTest)
		{
			var message = Assert.Single(classUnderTest.TestNodeMessageBus.PublishedData);
			var updateMessage = Assert.IsType<TestNodeUpdateMessage>(message);

			return updateMessage.TestNode;
		}

		static void SendStartingMessages(
			TestableTestPlatformExecutionMessageSink messageSink,
			IReadOnlyDictionary<string, IReadOnlyCollection<string>>? traits = null,
			bool includeTestStarting = true)
		{
			messageSink.OnMessage(TestData.TestAssemblyStarting());
			messageSink.OnMessage(TestData.TestCollectionStarting());
			messageSink.OnMessage(TestData.TestClassStarting());
			messageSink.OnMessage(TestData.TestMethodStarting());
			messageSink.OnMessage(TestData.TestCaseStarting(sourceFilePath: "/source/file/path.cs", sourceLineNumber: 42));

			if (includeTestStarting)
				messageSink.OnMessage(TestData.TestStarting(traits: traits ?? TestData.DefaultTraits));

			// Clear the published data cache of the ITestStarting translation
			messageSink.TestNodeMessageBus.PublishedData.Clear();
		}

		static void SendTestFailed(
			TestableTestPlatformExecutionMessageSink classUnderTest,
			FailureCause failureCause)
		{
			classUnderTest.OnMessage(
				TestData.TestFailed(
					cause: failureCause,
					exceptionParentIndices: [-1, 0],
					exceptionTypes: ["exception 1", "exception 2"],
					messages: ["message 1", "message 2"],
					stackTraces: ["stack trace 1", "stack trace 2"]
				)
			);
			classUnderTest.OnMessage(TestData.TestFinished());
		}
	}

	class TestableTestPlatformExecutionMessageSink(
		SpyMessageSink innerSink,
		SessionUid sessionUid,
		SpyTestPlatformMessageBus testNodeMessageBus,
		XunitTrxCapability trxCapability,
		SpyTestPlatformOutputDevice outputDevice,
		IConfiguration configuration,
		bool showLiveOutput,
		bool serverMode) :
			TestPlatformExecutionMessageSink(innerSink, sessionUid, testNodeMessageBus, trxCapability, outputDevice, configuration, showLiveOutput, serverMode)
	{
		public SpyMessageSink InnerSink { get; } = innerSink;
		public SpyTestPlatformOutputDevice OutputDevice { get; } = outputDevice;
		public SpyTestPlatformMessageBus TestNodeMessageBus { get; } = testNodeMessageBus;

		public static TestableTestPlatformExecutionMessageSink Create(
			bool trxEnabled = false,
			bool showLiveOutput = false,
			bool serverMode = false,
			string? resultsPath = null) =>
				new(SpyMessageSink.Capture(), new(), new(), new(trxEnabled), new(), Mocks.MicrosoftTestingPlatform.Configuration(resultsPath), showLiveOutput, serverMode);
	}
}
