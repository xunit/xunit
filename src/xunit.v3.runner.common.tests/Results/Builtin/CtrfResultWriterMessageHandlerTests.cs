using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Xunit;
using Xunit.Internal;
using Xunit.Runner.Common;
using Xunit.Sdk;

[CleanEnvironment("COMPUTERNAME", "HOSTNAME", "NAME", "HOST", "USERNAME", "LOGNAME", "USER")]
public class CtrfResultWriterMessageHandlerTests
{
	static readonly string CurrentOsPlatform;

	static CtrfResultWriterMessageHandlerTests()
	{
		CurrentOsPlatform = "Unknown";
		if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
			CurrentOsPlatform = "Windows";
		else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
			CurrentOsPlatform = "Linux";
		else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
			CurrentOsPlatform = "macOS";
	}

	[Theory]
	// Windows
	[InlineData("COMPUTERNAME", "USERNAME")]
	// Linux
	[InlineData("HOSTNAME", "LOGNAME")]
	[InlineData("HOSTNAME", "USER")]
	[InlineData("NAME", "LOGNAME")]
	[InlineData("NAME", "USER")]
	// macOS
	[InlineData("HOST", "LOGNAME")]
	[InlineData("HOST", "USER")]
	public async ValueTask CoreData(
		string computerEnvName,
		string userEnvName)
	{
		Environment.SetEnvironmentVariable(computerEnvName, "expected-computer");
		Environment.SetEnvironmentVariable(userEnvName, "expected-user");

		await using var handler = TestableCtrfResultWriterMessageHandler.Create();

		var root = await handler.Root();
		Assert.Equal("CTRF", JsonDeserializer.TryGetString(root, "reportFormat"));
		Assert.Equal("0.0.0", JsonDeserializer.TryGetString(root, "specVersion"));
		Assert.Matches(@"^[0-9a-f]{8}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{12}$", JsonDeserializer.TryGetString(root, "reportId"));
		Assert.Matches(@"^\d{4}-\d{2}-\d{2}T\d{2}:\d{2}:\d{2}Z$", JsonDeserializer.TryGetString(root, "timestamp"));
		Assert.Equal(typeof(TestableCtrfResultWriterMessageHandler).AssemblyQualifiedName, JsonDeserializer.TryGetString(root, "generatedBy"));

		var results = JsonDeserializer.TryGetObject(root, "results");
		Assert.NotNull(results);
		VerifySummary(results);

		var tool = JsonDeserializer.TryGetObject(results, "tool");
		Assert.NotNull(tool);
		Assert.Equal("xUnit.net v3", JsonDeserializer.TryGetString(tool, "name"));
		Assert.Equal(ThisAssembly.AssemblyInformationalVersion, JsonDeserializer.TryGetString(tool, "version"));

		var environment = JsonDeserializer.TryGetObject(results, "environment");
		Assert.NotNull(environment);
		Assert.Equal(CurrentOsPlatform, JsonDeserializer.TryGetString(environment, "osPlatform"));
		Assert.Equal(RuntimeInformation.OSDescription.Trim(), JsonDeserializer.TryGetString(environment, "osRelease"));

		var extra = JsonDeserializer.TryGetObject(results, "extra");
		Assert.NotNull(extra);
		Assert.Equal("expected-computer", JsonDeserializer.TryGetString(extra, "computer"));
		Assert.Equal("expected-user", JsonDeserializer.TryGetString(extra, "user"));

		var suites = JsonDeserializer.TryGetArray(extra, "suites");
		Assert.NotNull(suites);
		Assert.Empty(suites);

		var tests = JsonDeserializer.TryGetArray(results, "tests");
		Assert.NotNull(tests);
		Assert.Empty(tests);
	}

	[Fact]
	public async ValueTask CoreDataDoesNotIncludeOptionalValues()
	{
		await using var handler = TestableCtrfResultWriterMessageHandler.Create();

		var root = await handler.Root();

		var results = JsonDeserializer.TryGetObject(root, "results");
		Assert.NotNull(results);

		var extra = JsonDeserializer.TryGetObject(results, "extra");
		Assert.NotNull(extra);
		Assert.Null(JsonDeserializer.TryGetString(extra, "computer"));
		Assert.Null(JsonDeserializer.TryGetString(extra, "user"));
	}

	[Fact]
	public async ValueTask TestAssemblies()
	{
		var assembly1Starting = TestData.TestAssemblyStarting(assemblyUniqueID: "asm1", assemblyPath: "asm1.dll");
		var assembly2Starting = TestData.TestAssemblyStarting(assemblyUniqueID: "asm2", assemblyPath: "asm2.dll", startTime: TestData.DefaultStartTime.AddSeconds(1));
		var assembly1Finished = TestData.TestAssemblyFinished(
			assemblyUniqueID: "asm1",
			executionTime: 123.4567M,
			testsFailed: 42,
			testsNotRun: 3,
			testsSkipped: 6,
			testsTotal: 2112
		);
		var assembly2Finished = TestData.TestAssemblyFinished(
			assemblyUniqueID: "asm2",
			executionTime: 1.23M,
			testsFailed: 0,
			testsNotRun: 0,
			testsSkipped: 0,
			testsTotal: 5
		);
		await using var handler = TestableCtrfResultWriterMessageHandler.Create();

		handler.OnMessage(assembly1Starting);
		handler.OnMessage(assembly1Finished);
		handler.OnMessage(assembly2Starting);
		handler.OnMessage(assembly2Finished);

		var root = await handler.Root();

		var results = JsonDeserializer.TryGetObject(root, "results");
		Assert.NotNull(results);
		var start = assembly1Starting.StartTime.ToUnixTimeMilliseconds();
		var stop = assembly2Finished.FinishTime.ToUnixTimeMilliseconds();
		VerifySummary(results, total: 2117, failed: 42, pending: 3, skipped: 6, suites: 2, start: start, stop: stop);

		var extra = JsonDeserializer.TryGetObject(results, "extra");
		Assert.NotNull(extra);

		var suites = JsonDeserializer.TryGetArray(extra, "suites");
		Assert.NotNull(suites);
		Assert.Collection(
			suites,
			suite => VerifySuite(suite, assembly1Starting, assembly1Finished),
			suite => VerifySuite(suite, assembly2Starting, assembly2Finished)
		);
	}

	[CulturedFactDefault]
	public async ValueTask TestPassed()
	{
		var assemblyStarting = TestData.TestAssemblyStarting();
		var collectionStarting = TestData.TestCollectionStarting();
		var classStarting = TestData.TestClassStarting(testClassName: typeof(ClassUnderTest).FullName!);
		var methodStarting = TestData.TestMethodStarting(methodName: nameof(ClassUnderTest.TestMethod));
		var caseStarting = TestData.TestCaseStarting(traits: TestData.EmptyTraits);
		var testStarting = TestData.TestStarting(testDisplayName: "Test Display Name");
		var testPassed = TestData.TestPassed(executionTime: 123.4567809m, output: "test output");
		var assemblyFinished = TestData.TestAssemblyFinished();
		await using var handler = TestableCtrfResultWriterMessageHandler.Create();

		handler.OnMessage(assemblyStarting);
		handler.OnMessage(collectionStarting);
		handler.OnMessage(classStarting);
		handler.OnMessage(methodStarting);
		handler.OnMessage(caseStarting);
		handler.OnMessage(testStarting);
		handler.OnMessage(testPassed);
		handler.OnMessage(assemblyFinished);

		var root = await handler.Root();

		var results = JsonDeserializer.TryGetObject(root, "results");
		Assert.NotNull(results);

		var extra = JsonDeserializer.TryGetObject(results, "extra");
		Assert.NotNull(extra);

		var suites = JsonDeserializer.TryGetArray(extra, "suites");
		Assert.NotNull(suites);
		var suite = Assert.Single(suites);
		VerifySuite(suite, assemblyStarting, assemblyFinished, collectionStarting);

		var tests = JsonDeserializer.TryGetArray(results, "tests");
		Assert.NotNull(tests);
		VerifyTest(handler.FileSystem, Assert.Single(tests), testPassed, classStarting, methodStarting, caseStarting, testStarting);
	}

	[CulturedFactDefault]
	public async ValueTask TestFailed()
	{
		var assemblyStarting = TestData.TestAssemblyStarting();
		var collectionStarting = TestData.TestCollectionStarting();
		var classStarting = TestData.TestClassStarting(testClassName: typeof(ClassUnderTest).FullName!);
		var methodStarting = TestData.TestMethodStarting(methodName: nameof(ClassUnderTest.TestMethod));
		var caseStarting = TestData.TestCaseStarting(traits: TestData.EmptyTraits);
		var testStarting = TestData.TestStarting(testDisplayName: "Test Display Name");
		var testFailed = TestData.TestFailed(executionTime: 123.4567809m);
		var assemblyFinished = TestData.TestAssemblyFinished();
		await using var handler = TestableCtrfResultWriterMessageHandler.Create();

		handler.OnMessage(assemblyStarting);
		handler.OnMessage(collectionStarting);
		handler.OnMessage(classStarting);
		handler.OnMessage(methodStarting);
		handler.OnMessage(caseStarting);
		handler.OnMessage(testStarting);
		handler.OnMessage(testFailed);
		handler.OnMessage(assemblyFinished);

		var root = await handler.Root();

		var results = JsonDeserializer.TryGetObject(root, "results");
		Assert.NotNull(results);

		var extra = JsonDeserializer.TryGetObject(results, "extra");
		Assert.NotNull(extra);

		var suites = JsonDeserializer.TryGetArray(extra, "suites");
		Assert.NotNull(suites);
		var suite = Assert.Single(suites);
		VerifySuite(suite, assemblyStarting, assemblyFinished, collectionStarting);

		var tests = JsonDeserializer.TryGetArray(results, "tests");
		Assert.NotNull(tests);
		VerifyTest(handler.FileSystem, Assert.Single(tests), testFailed, classStarting, methodStarting, caseStarting, testStarting);
	}

	[CulturedFactDefault]
	public async ValueTask TestSkipped()
	{
		var assemblyStarting = TestData.TestAssemblyStarting();
		var collectionStarting = TestData.TestCollectionStarting();
		var classStarting = TestData.TestClassStarting(testClassName: typeof(ClassUnderTest).FullName!);
		var methodStarting = TestData.TestMethodStarting(methodName: nameof(ClassUnderTest.TestMethod));
		var caseStarting = TestData.TestCaseStarting(traits: TestData.EmptyTraits);
		var testStarting = TestData.TestStarting(testDisplayName: "Test Display Name");
		var testSkipped = TestData.TestSkipped(reason: "Don't run me");
		var assemblyFinished = TestData.TestAssemblyFinished();
		await using var handler = TestableCtrfResultWriterMessageHandler.Create();

		handler.OnMessage(assemblyStarting);
		handler.OnMessage(collectionStarting);
		handler.OnMessage(classStarting);
		handler.OnMessage(methodStarting);
		handler.OnMessage(caseStarting);
		handler.OnMessage(testStarting);
		handler.OnMessage(testSkipped);
		handler.OnMessage(assemblyFinished);

		var root = await handler.Root();

		var results = JsonDeserializer.TryGetObject(root, "results");
		Assert.NotNull(results);

		var extra = JsonDeserializer.TryGetObject(results, "extra");
		Assert.NotNull(extra);

		var suites = JsonDeserializer.TryGetArray(extra, "suites");
		Assert.NotNull(suites);
		var suite = Assert.Single(suites);
		VerifySuite(suite, assemblyStarting, assemblyFinished, collectionStarting);

		var tests = JsonDeserializer.TryGetArray(results, "tests");
		Assert.NotNull(tests);
		VerifyTest(handler.FileSystem, Assert.Single(tests), testSkipped, classStarting, methodStarting, caseStarting, testStarting);
	}

	[CulturedFactDefault]
	public async ValueTask TestNotRun()
	{
		var assemblyStarting = TestData.TestAssemblyStarting();
		var collectionStarting = TestData.TestCollectionStarting();
		var classStarting = TestData.TestClassStarting(testClassName: typeof(ClassUnderTest).FullName!);
		var methodStarting = TestData.TestMethodStarting(methodName: nameof(ClassUnderTest.TestMethod));
		var caseStarting = TestData.TestCaseStarting(traits: TestData.EmptyTraits);
		var testStarting = TestData.TestStarting(testDisplayName: "Test Display Name");
		var testNotRun = TestData.TestNotRun();
		var assemblyFinished = TestData.TestAssemblyFinished();
		await using var handler = TestableCtrfResultWriterMessageHandler.Create();

		handler.OnMessage(assemblyStarting);
		handler.OnMessage(collectionStarting);
		handler.OnMessage(classStarting);
		handler.OnMessage(methodStarting);
		handler.OnMessage(caseStarting);
		handler.OnMessage(testStarting);
		handler.OnMessage(testNotRun);
		handler.OnMessage(assemblyFinished);

		var root = await handler.Root();

		var results = JsonDeserializer.TryGetObject(root, "results");
		Assert.NotNull(results);

		var extra = JsonDeserializer.TryGetObject(results, "extra");
		Assert.NotNull(extra);

		var suites = JsonDeserializer.TryGetArray(extra, "suites");
		Assert.NotNull(suites);
		var suite = Assert.Single(suites);
		VerifySuite(suite, assemblyStarting, assemblyFinished, collectionStarting);

		var tests = JsonDeserializer.TryGetArray(results, "tests");
		Assert.NotNull(tests);
		VerifyTest(handler.FileSystem, Assert.Single(tests), testNotRun, classStarting, methodStarting, caseStarting, testStarting);
	}

	static void VerifySuite(
		object? suiteObj,
		ITestAssemblyStarting starting,
		ITestAssemblyFinished finished,
		ITestCollectionStarting? collectionStarting = null)
	{
		Assert.NotNull(suiteObj);

		var suite = JsonDeserializer.TryGetObject(suiteObj);
		Assert.NotNull(suite);
		Assert.Equal(starting.AssemblyUniqueID, JsonDeserializer.TryGetString(suite, "id"));
		Assert.Equal(starting.AssemblyPath, JsonDeserializer.TryGetString(suite, "filePath"));
		Assert.Equal(starting.TestEnvironment, JsonDeserializer.TryGetString(suite, "environment"));
		Assert.Equal(starting.TestFrameworkDisplayName, JsonDeserializer.TryGetString(suite, "testFramework"));
		Assert.Equal(starting.TargetFramework, JsonDeserializer.TryGetString(suite, "targetFramework"));
		Assert.Equal(starting.StartTime.ToUnixTimeMilliseconds(), JsonDeserializer.TryGetLong(suite, "start"));
		Assert.Equal(finished.FinishTime.ToUnixTimeMilliseconds(), JsonDeserializer.TryGetLong(suite, "stop"));
		Assert.Equal((long)(finished.ExecutionTime * 1000), JsonDeserializer.TryGetLong(suite, "duration"));

		var collections = JsonDeserializer.TryGetArray(suite, "collections");
		Assert.NotNull(collections);
		if (collectionStarting is null)
			Assert.Empty(collections);
		else
		{
			var collection = JsonDeserializer.TryGetObject(Assert.Single(collections));
			Assert.NotNull(collection);
			Assert.Equal(collectionStarting.TestCollectionUniqueID, JsonDeserializer.TryGetString(collection, "id"));
			Assert.Equal(collectionStarting.TestCollectionDisplayName, JsonDeserializer.TryGetString(collection, "name"));
		}

		var errors = JsonDeserializer.TryGetArray(suite, "errors");
		Assert.NotNull(errors);
		Assert.Empty(errors);
	}

	static void VerifySummary(
		IReadOnlyDictionary<string, object?> results,
		int total = 0,
		int failed = 0,
		int pending = 0,
		int skipped = 0,
		int other = 0,
		int suites = 0,
		long start = 0,
		long stop = 0)
	{
		var summary = JsonDeserializer.TryGetObject(results, "summary");
		Assert.NotNull(summary);
		Assert.Equal(total, JsonDeserializer.TryGetInt(summary, "tests"));
		Assert.Equal(total - failed - pending - skipped, JsonDeserializer.TryGetInt(summary, "passed"));
		Assert.Equal(failed, JsonDeserializer.TryGetInt(summary, "failed"));
		Assert.Equal(pending, JsonDeserializer.TryGetInt(summary, "pending"));
		Assert.Equal(skipped, JsonDeserializer.TryGetInt(summary, "skipped"));
		Assert.Equal(other, JsonDeserializer.TryGetInt(summary, "other"));
		Assert.Equal(suites, JsonDeserializer.TryGetInt(summary, "suites"));
		Assert.Equal(start, JsonDeserializer.TryGetLong(summary, "start"));
		Assert.Equal(stop, JsonDeserializer.TryGetLong(summary, "stop"));
	}

	static void VerifyTest(
		IFileSystem fileSystem,
		object? testObj,
		ITestResultMessage testResult,
		ITestClassStarting classStarting,
		ITestMethodStarting methodStarting,
		ITestCaseStarting caseStarting,
		ITestStarting testStarting,
		ITestFinished? testFinished = null)
	{
		Assert.NotNull(testObj);

		var (status, message, exception, trace) = testResult switch
		{
			ITestPassed => ("passed", null, null, null),
			ITestFailed failed => ("failed", ExceptionUtility.CombineMessages(failed), failed.ExceptionTypes[0], ExceptionUtility.CombineStackTraces(failed)),
			ITestSkipped skipped => ("skipped", skipped.Reason, null, null),
			ITestNotRun => ("pending", null, null, null),
			_ => throw new ArgumentException("Unknown test result type"),
		};

		var test = JsonDeserializer.TryGetObject(testObj);
		Assert.NotNull(test);
		Assert.Equal(testStarting.TestDisplayName, JsonDeserializer.TryGetString(test, "name"));
		Assert.Equal(status, JsonDeserializer.TryGetString(test, "status"));
		Assert.Equal((long)(testResult.ExecutionTime * 1000), JsonDeserializer.TryGetLong(test, "duration"));
		Assert.Equal(testResult.AssemblyUniqueID, JsonDeserializer.TryGetString(test, "suite"));
		Assert.Equal(caseStarting.SourceFilePath, JsonDeserializer.TryGetString(test, "filePath"));
		Assert.Equal(caseStarting.SourceLineNumber, JsonDeserializer.TryGetInt(test, "line"));
		Assert.Equal(message, JsonDeserializer.TryGetString(test, "message"));
		Assert.Equal(trace, JsonDeserializer.TryGetString(test, "trace"));

		var tags = JsonDeserializer.TryGetArrayOfString(test, "tags");
		if (testStarting.Traits.TryGetValue("Category", out var values))
			Assert.Equal(values, tags);
		else
			Assert.Null(tags);

		var attachments = JsonDeserializer.TryGetArray(test, "attachments");
		if (testFinished?.Attachments is not null && testFinished.Attachments.Count != 0)
		{
			Assert.NotNull(attachments);
			Assert.Equal(testFinished.Attachments.Count, attachments.Length);
			for (var idx = 0; idx < attachments.Length; ++idx)
			{
				var attachmentJson = JsonDeserializer.TryGetObject(attachments[idx]);
				Assert.NotNull(attachmentJson);

				var name = JsonDeserializer.TryGetString(attachmentJson, "name");
				Assert.NotNull(name);

				if (!testFinished.Attachments.TryGetValue(name, out var attachment))
					Assert.Fail($"Could not find attachment named {name}");

				if (attachment.AttachmentType == TestAttachmentType.String)
				{
					Assert.Equal("text/plain", JsonDeserializer.TryGetString(attachmentJson, "contentType"));
					var filePath = JsonDeserializer.TryGetString(attachmentJson, "path");
					Assert.True(fileSystem.Exists(filePath));
					Assert.Equal(attachment.AsString(), fileSystem.ReadAllText(filePath));
				}
				else
				{
					var (bytes, contentType) = attachment.AsByteArray();
					Assert.Equal(contentType, JsonDeserializer.TryGetString(attachmentJson, "contentType"));
					var filePath = JsonDeserializer.TryGetString(attachmentJson, "path");
					Assert.True(fileSystem.Exists(filePath));
					Assert.Equal(bytes, fileSystem.ReadAllBytes(filePath));
				}
			}
		}
		else
			Assert.Null(attachments);

		var extra = JsonDeserializer.TryGetObject(test, "extra");
		Assert.NotNull(extra);
		Assert.Equal(testResult.TestUniqueID, JsonDeserializer.TryGetString(extra, "id"));
		Assert.Equal(testResult.TestCollectionUniqueID, JsonDeserializer.TryGetString(extra, "collection"));
		Assert.Equal(classStarting.TestClassName, JsonDeserializer.TryGetString(extra, "type"));
		Assert.Equal(methodStarting.MethodName, JsonDeserializer.TryGetString(extra, "method"));
		Assert.Equal(exception, JsonDeserializer.TryGetString(extra, "exception"));
		Assert.Equal(testResult.Output, JsonDeserializer.TryGetString(extra, "output") ?? string.Empty);

		var warnings = JsonDeserializer.TryGetArrayOfString(extra, "warnings");
		if (testResult.Warnings is not null && testResult.Warnings.Length != 0)
			Assert.Equal(testResult.Warnings, warnings);
		else
			Assert.Null(warnings);

		var traits = JsonDeserializer.TryGetObject(extra, "traits");
		if (testStarting.Traits.Count == 0)
			Assert.Null(traits);
		else
		{
			Assert.NotNull(traits);
			foreach (var kvp in testStarting.Traits)
				Assert.Equal(kvp.Value, JsonDeserializer.TryGetArrayOfString(traits, kvp.Key));
		}
	}

	class ClassUnderTest
	{
		[Fact]
		public async ValueTask TestMethod() { }
	}

	class TestableCtrfResultWriterMessageHandler : CtrfResultWriterMessageHandler
	{
		string? json;

		TestableCtrfResultWriterMessageHandler(IFileSystem fileSystem) :
			base(new MemoryStream(), fileSystem)
		{
			OnDisposed += text => json = text;
			FileSystem = fileSystem;
		}

		public IFileSystem FileSystem { get; }

		public async ValueTask<IReadOnlyDictionary<string, object?>> Root()
		{
			await DisposeAsync();

			if (json is null)
				throw new InvalidOperationException("The JSON callback was never called");

			if (!JsonDeserializer.TryDeserialize(json, out var result))
				throw new InvalidOperationException("The rendered JSON is invalid: " + json);

			if (result is not IReadOnlyDictionary<string, object?> root)
				throw new InvalidOperationException("The rendered JSON was not an object: " + json);

			return root;
		}

		public static TestableCtrfResultWriterMessageHandler Create() => new(new SpyFileSystem());
	}
}
