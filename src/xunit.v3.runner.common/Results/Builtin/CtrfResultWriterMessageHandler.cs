using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Xunit.Internal;
using Xunit.Sdk;

namespace Xunit.Runner.Common;

/// <summary>
/// The message handler for <see cref="CtrfResultWriter"/>.
/// </summary>
public class CtrfResultWriterMessageHandler : ResultMetadataMessageHandlerBase<CtrfResultWriterMessageHandler.ResultMetadata>, IResultWriterMessageHandler
{
	static readonly IReadOnlyDictionary<string, IReadOnlyCollection<string>> EmptyTraits = new Dictionary<string, IReadOnlyCollection<string>>();

	bool disposed;
	readonly IFileSystem fileSystem;
	readonly Lazy<Stream> stream;
	long? timeFinish;
	long? timeStart;

	/// <summary>
	/// Initializes a new instance of the <see cref="CtrfResultWriterMessageHandler"/> class.
	/// </summary>
	/// <param name="fileName">The output file name</param>
	public CtrfResultWriterMessageHandler(string fileName) :
		this(new Lazy<Stream>(() => File.Create(fileName), isThreadSafe: false), FileSystem.Instance)
	{ }

	/// <summary>
	/// This constructor is for testing purposes only. Please call the public constructor.
	/// </summary>
	protected CtrfResultWriterMessageHandler(
		Stream stream,
		IFileSystem fileSystem) :
			this(new Lazy<Stream>(() => stream), fileSystem)
	{ }

	CtrfResultWriterMessageHandler(
		Lazy<Stream> stream,
		IFileSystem fileSystem)
	{
		this.fileSystem = fileSystem;
		this.stream = stream;
	}

	/// <summary>
	/// This is for testing purposes only. Do not use.
	/// </summary>
	protected Action<string>? OnDisposed { get; set; }

	static void AddError(
		string type,
		string? name,
		ResultMetadata resultMetadata,
		IErrorMetadata errorMetadata) =>
			resultMetadata.Errors.Add((
				type,
				name,
				errorMetadata.ExceptionTypes[0],
				ExceptionUtility.CombineMessages(errorMetadata),
				ExceptionUtility.CombineStackTraces(errorMetadata)
			));

	internal override ResultMetadata CreateMetadata() =>
		new();

	/// <inheritdoc/>
	public ValueTask DisposeAsync()
	{
		GC.SuppressFinalize(this);

		if (!disposed)
			try
			{
				var buffer = new StringBuilder();

				var totalRun = 0L;
				var totalPassed = 0L;
				var totalFailed = 0L;
				var totalSkipped = 0L;
				var totalNotRun = 0L;
				var totalOther = 0L;
				var totalSuites = 0L;

				using (var rootJson = new JsonObjectSerializer(buffer))
				{
					rootJson.Serialize("reportFormat", "CTRF");
					rootJson.Serialize("specVersion", "0.0.0");
					rootJson.Serialize("reportId", Guid.NewGuid().ToString());
					rootJson.Serialize("timestamp", DateTimeOffset.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ", CultureInfo.InvariantCulture));
					rootJson.Serialize("generatedBy", GetType().AssemblyQualifiedName);

					using var resultsJson = rootJson.SerializeObject("results");
					using (var toolJson = resultsJson.SerializeObject("tool"))
					{
						toolJson.Serialize("name", "xUnit.net v3");
						toolJson.Serialize("version", ThisAssembly.AssemblyInformationalVersion);
					}

					using (var environmentJson = resultsJson.SerializeObject("environment"))
					{
						var osPlatform = "Unknown";
						if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
							osPlatform = "Windows";
						else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
							osPlatform = "Linux";
						else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
							osPlatform = "macOS";

						environmentJson.Serialize("osPlatform", osPlatform);
						environmentJson.Serialize("osRelease", RuntimeInformation.OSDescription.Trim());
					}

					using (var extraJson = resultsJson.SerializeObject("extra"))
					{
						extraJson.Serialize("computer", EnvironmentUtility.Computer);
						extraJson.Serialize("user", EnvironmentUtility.User);

						using var suitesJson = extraJson.SerializeArray("suites");
						foreach (var resultMetadata in ResultMetadataValues.OrderBy(r => r.ID))
						{
							using var suiteJson = suitesJson.SerializeObject();

							totalSuites++;
							totalRun += resultMetadata.TotalTests;
							totalPassed += (resultMetadata.TotalTests - resultMetadata.TestsFailed - resultMetadata.TestsSkipped - resultMetadata.TestsNotRun);
							totalFailed += resultMetadata.TestsFailed;
							totalSkipped += resultMetadata.TestsSkipped;
							totalNotRun += resultMetadata.TestsNotRun;
							totalOther += resultMetadata.Errors.Count;

							suiteJson.Serialize("id", resultMetadata.ID);
							suiteJson.Serialize("filePath", resultMetadata.FilePath);
							suiteJson.Serialize("configPath", resultMetadata.ConfigPath);
							suiteJson.Serialize("environment", resultMetadata.Environment);
							suiteJson.Serialize("testFramework", resultMetadata.TestFramework);
							suiteJson.Serialize("targetFramework", resultMetadata.TargetFramework);
							suiteJson.Serialize("start", resultMetadata.Start);
							suiteJson.Serialize("stop", resultMetadata.Stop);
							suiteJson.Serialize("duration", (long)(resultMetadata.Duration * 1000));

							using (var collectionsJson = suiteJson.SerializeArray("collections"))
								foreach (var collection in resultMetadata.TestCollections)
									using (var collectionJson = collectionsJson.SerializeObject())
									{
										collectionJson.Serialize("id", collection.ID);
										collectionJson.Serialize("name", collection.Name);
									}

							using var errorsJson = suiteJson.SerializeArray("errors");
							foreach (var error in resultMetadata.Errors)
								using (var errorJson = errorsJson.SerializeObject())
								{
									errorJson.Serialize("name", error.Name);
									errorJson.Serialize("type", error.Type);
									errorJson.Serialize("exception", error.Exception);
									errorJson.Serialize("message", error.Message);
									errorJson.Serialize("trace", error.Trace);
								}
						}
					}

					using (var summaryJson = resultsJson.SerializeObject("summary"))
					{
						summaryJson.Serialize("tests", totalRun);
						summaryJson.Serialize("passed", totalPassed);
						summaryJson.Serialize("failed", totalFailed);
						summaryJson.Serialize("pending", totalNotRun);
						summaryJson.Serialize("skipped", totalSkipped);
						summaryJson.Serialize("other", totalOther);
						summaryJson.Serialize("suites", totalSuites);
						summaryJson.Serialize("start", timeStart ?? 0);
						summaryJson.Serialize("stop", timeFinish ?? 0);
					}

					using var testsJson = resultsJson.SerializeArray("tests");
					foreach (var resultMetadata in ResultMetadataValues)
						foreach (var testResult in resultMetadata.TestResults.Values)
							using (var testJson = testsJson.SerializeObject())
							{
								testJson.Serialize("name", testResult.Name);
								testJson.Serialize("status", testResult.Status);
								testJson.Serialize("duration", testResult.Duration);
								testJson.Serialize("suite", testResult.Suite);
								testJson.Serialize("filePath", testResult.FilePath);
								testJson.Serialize("line", testResult.Line);
								testJson.Serialize("message", testResult.Message);
								testJson.Serialize("trace", testResult.Trace);

								if (testResult.Traits.TryGetValue("Category", out var categories))
									using (var tags = testJson.SerializeArray("tags"))
										foreach (var categoryValue in categories)
											tags.Serialize(categoryValue);

								if (testResult.Attachments is not null && testResult.Attachments.Count != 0)
									using (var attachmentsJson = testJson.SerializeArray("attachments"))
										foreach (var attachment in testResult.Attachments)
										{
											using var attachmentJson = attachmentsJson.SerializeObject();
											attachmentJson.Serialize("name", attachment.Name);
											attachmentJson.Serialize("contentType", attachment.ContentType);
											attachmentJson.Serialize("path", attachment.Path);
										}

								using var extraJson = testJson.SerializeObject("extra");

								extraJson.Serialize("id", testResult.ID);
								extraJson.Serialize("collection", testResult.Collection);
								extraJson.Serialize("type", testResult.Type);
								extraJson.Serialize("method", testResult.Method);
								extraJson.Serialize("exception", testResult.Exception);

								if (testResult.Output.Length != 0)
									extraJson.Serialize("output", testResult.Output);

								if (testResult.Warnings is not null && testResult.Warnings.Length != 0)
									extraJson.SerializeStringArray("warnings", testResult.Warnings);

								if (testResult.Traits.Count != 0)
									using (var traitsJson = extraJson.SerializeObject("traits"))
										foreach (var kvp in testResult.Traits)
											using (var traitNameJson = traitsJson.SerializeArray(kvp.Key))
												foreach (var value in kvp.Value)
													traitNameJson.Serialize(value);
							}
				}

				using (var streamWriter = new StreamWriter(stream.Value, new UTF8Encoding(encoderShouldEmitUTF8Identifier: false)))
					streamWriter.Write(buffer);

				stream.Value.SafeDispose();

				OnDisposed?.Invoke(buffer.ToString());
			}
			finally
			{
				disposed = true;
			}

		return default;
	}

	void HandleErrorMessage(MessageHandlerArgs<IErrorMessage> args)
	{
		var message = args.Message;
		if (message.AssemblyUniqueID is null || !TryGetResultMetadata(message.AssemblyUniqueID, out var resultMetadata))
			return;

		AddError("fatal", null, resultMetadata, message);
	}

	void HandleTestAssemblyCleanupFailure(MessageHandlerArgs<ITestAssemblyCleanupFailure> args)
	{
		var message = args.Message;
		if (!TryGetResultMetadata(message.AssemblyUniqueID, out var resultMetadata))
			return;

		AddError("assembly-cleanup", resultMetadata.MetadataCache.TryGetAssemblyMetadata(message)?.AssemblyPath, resultMetadata, message);
	}

	void HandleTestCaseCleanupFailure(MessageHandlerArgs<ITestCaseCleanupFailure> args)
	{
		var message = args.Message;
		if (!TryGetResultMetadata(message.AssemblyUniqueID, out var resultMetadata))
			return;

		AddError("test-case-cleanup", resultMetadata.MetadataCache.TryGetTestCaseMetadata(message)?.TestCaseDisplayName, resultMetadata, message);
	}

	void HandleTestClassCleanupFailure(MessageHandlerArgs<ITestClassCleanupFailure> args)
	{
		var message = args.Message;
		if (!TryGetResultMetadata(message.AssemblyUniqueID, out var resultMetadata))
			return;

		AddError("test-class-cleanup", resultMetadata.MetadataCache.TryGetClassMetadata(message)?.TestClassName, resultMetadata, message);
	}

	void HandleTestCleanupFailure(MessageHandlerArgs<ITestCleanupFailure> args)
	{
		var message = args.Message;
		if (!TryGetResultMetadata(message.AssemblyUniqueID, out var resultMetadata))
			return;

		AddError("test-cleanup", resultMetadata.MetadataCache.TryGetTestMetadata(message)?.TestDisplayName, resultMetadata, message);
	}

	void HandleTestCollectionCleanupFailure(MessageHandlerArgs<ITestCollectionCleanupFailure> args)
	{
		var message = args.Message;
		if (!TryGetResultMetadata(message.AssemblyUniqueID, out var resultMetadata))
			return;

		AddError("test-collection-cleanup", resultMetadata.MetadataCache.TryGetCollectionMetadata(message)?.TestCollectionDisplayName, resultMetadata, message);
	}

	void HandleTestFailed(MessageHandlerArgs<ITestFailed> args)
	{
		var message = args.Message;
		if (!TryGetResultMetadata(message.AssemblyUniqueID, out var resultMetadata))
			return;

		resultMetadata.TestResults.TryAdd(message.TestUniqueID, new(message, resultMetadata)
		{
			Exception = message.ExceptionTypes[0],
			Message = ExceptionUtility.CombineMessages(message),
			Trace = ExceptionUtility.CombineStackTraces(message),
		});
	}

	void HandleTestMethodCleanupFailure(MessageHandlerArgs<ITestMethodCleanupFailure> args)
	{
		var message = args.Message;
		if (!TryGetResultMetadata(message.AssemblyUniqueID, out var resultMetadata))
			return;

		AddError("test-method-cleanup", resultMetadata.MetadataCache.TryGetMethodMetadata(message)?.MethodName, resultMetadata, message);
	}

	void HandleTestNotRun(MessageHandlerArgs<ITestNotRun> args)
	{
		var message = args.Message;
		if (!TryGetResultMetadata(message.AssemblyUniqueID, out var resultMetadata))
			return;

		resultMetadata.TestResults.TryAdd(message.TestUniqueID, new(message, resultMetadata));
	}

	void HandleTestPassed(MessageHandlerArgs<ITestPassed> args)
	{
		var message = args.Message;
		if (!TryGetResultMetadata(message.AssemblyUniqueID, out var resultMetadata))
			return;

		resultMetadata.TestResults.TryAdd(message.TestUniqueID, new(message, resultMetadata));
	}

	void HandleTestSkipped(MessageHandlerArgs<ITestSkipped> args)
	{
		var message = args.Message;
		if (!TryGetResultMetadata(message.AssemblyUniqueID, out var resultMetadata))
			return;

		resultMetadata.TestResults.TryAdd(message.TestUniqueID, new(message, resultMetadata) { Message = message.Reason });
	}

	/// <inheritdoc/>
	public override bool OnMessage(IMessageSinkMessage message)
	{
		message.DispatchWhen<IErrorMessage>(HandleErrorMessage);
		message.DispatchWhen<ITestAssemblyCleanupFailure>(HandleTestAssemblyCleanupFailure);
		message.DispatchWhen<ITestCaseCleanupFailure>(HandleTestCaseCleanupFailure);
		message.DispatchWhen<ITestClassCleanupFailure>(HandleTestClassCleanupFailure);
		message.DispatchWhen<ITestCleanupFailure>(HandleTestCleanupFailure);
		message.DispatchWhen<ITestCollectionCleanupFailure>(HandleTestCollectionCleanupFailure);
		message.DispatchWhen<ITestFailed>(HandleTestFailed);
		message.DispatchWhen<ITestMethodCleanupFailure>(HandleTestMethodCleanupFailure);
		message.DispatchWhen<ITestNotRun>(HandleTestNotRun);
		message.DispatchWhen<ITestPassed>(HandleTestPassed);
		message.DispatchWhen<ITestSkipped>(HandleTestSkipped);

		return base.OnMessage(message);
	}

	internal override void OnTestAssemblyFinished(
		ITestAssemblyFinished message,
		ResultMetadata resultMetadata)
	{
		resultMetadata.TestsFailed = message.TestsFailed;
		resultMetadata.TestsNotRun = message.TestsNotRun;
		resultMetadata.TestsSkipped = message.TestsSkipped;
		resultMetadata.TotalTests = message.TestsTotal;
		resultMetadata.Duration = message.ExecutionTime;
		resultMetadata.Stop = message.FinishTime.ToUnixTimeMilliseconds();

		if (timeFinish is null || timeFinish < resultMetadata.Stop)
			timeFinish = resultMetadata.Stop;
	}

	internal override void OnTestAssemblyStarting(
		ITestAssemblyStarting message,
		ResultMetadata resultMetadata)
	{
		resultMetadata.ConfigPath = message.ConfigFilePath;
		resultMetadata.Environment = message.TestEnvironment;
		resultMetadata.FilePath = message.AssemblyPath;
		resultMetadata.ID = message.AssemblyUniqueID;
		resultMetadata.TargetFramework = message.TargetFramework;
		resultMetadata.TestFramework = message.TestFrameworkDisplayName;
		resultMetadata.Start = message.StartTime.ToUnixTimeMilliseconds();

		if (timeStart is null || timeStart > resultMetadata.Start)
			timeStart = resultMetadata.Start;
	}

	internal override void OnTestCollectionStarting(
		ITestCollectionStarting message,
		ResultMetadata resultMetadata) =>
			resultMetadata.TestCollections.Add((message.TestCollectionUniqueID, message.TestCollectionDisplayName));

	internal override void OnTestFinished(
		ITestFinished message,
		ResultMetadata resultMetadata)
	{
		if (message.Attachments.Count == 0 || !resultMetadata.TestResults.TryGetValue(message.TestUniqueID, out var testResult))
			return;

		var basePath = Path.Combine(Path.GetTempPath(), message.TestUniqueID);
		fileSystem.CreateDirectory(basePath);

		foreach (var attachment in message.Attachments)
		{
			string contentType;
			byte[] content;

			if (attachment.Value.AttachmentType == TestAttachmentType.String)
			{
				contentType = "text/plain";
				content = Encoding.UTF8.GetBytes(attachment.Value.AsString());
			}
			else if (attachment.Value.AttachmentType == TestAttachmentType.ByteArray)
				(content, contentType) = attachment.Value.AsByteArray();
			else
				throw new InvalidOperationException("Unknown attachment type");

			var localFilePath = Path.Combine(basePath, MediaTypeUtility.GetSanitizedFileNameWithExtension(attachment.Key, contentType));
			fileSystem.WriteAllBytes(localFilePath, content);

			testResult.Attachments.Add((attachment.Key, contentType, localFilePath));
		}
	}

	/// <summary>
	/// INTERNAL CLASS. DO NOT USE.
	/// </summary>
	public sealed class ResultMetadata : ResultMetadataBase
	{
		internal string? ConfigPath;
		internal decimal Duration;
		internal string? Environment;
		internal string? FilePath;
		internal string? ID;
		internal long Start;
		internal long Stop;
		internal string? TargetFramework;
		internal string? TestFramework;
		internal int TestsFailed;
		internal int TestsNotRun;
		internal int TestsSkipped;
		internal int TotalTests;

		internal ConcurrentBag<(string Type, string? Name, string? Exception, string Message, string? Trace)> Errors = [];

		internal ConcurrentBag<(string ID, string Name)> TestCollections = [];

		internal ConcurrentDictionary<string, TestResult> TestResults = [];
	}

	internal sealed class TestResult
	{
		public TestResult(
			ITestResultMessage result,
			ResultMetadata resultMetadata)
		{
			var testClassMetadata = resultMetadata.MetadataCache.TryGetClassMetadata(result);
			var testMethodMetadata = resultMetadata.MetadataCache.TryGetMethodMetadata(result);
			var testCaseMetadata = resultMetadata.MetadataCache.TryGetTestCaseMetadata(result);
			var testMetadata = resultMetadata.MetadataCache.TryGetTestMetadata(result);

			Collection = result.TestCollectionUniqueID;
			Duration = (long)(result.ExecutionTime * 1000);
			FilePath = testCaseMetadata?.SourceFilePath;
			ID = result.TestUniqueID;
			Line = testCaseMetadata?.SourceLineNumber;
			Method = testMethodMetadata?.MethodName;
			Name = testMetadata?.TestDisplayName ?? "<unknown test>";
			Output = result.Output;
			Status = result switch
			{
				ITestFailed => "failed",
				ITestNotRun => "pending",
				ITestPassed => "passed",
				ITestSkipped => "skipped",
				_ => "other",
			};
			Suite = result.AssemblyUniqueID;
			Traits = testMetadata?.Traits ?? EmptyTraits;
			Type = testClassMetadata?.TestClassName;
			Warnings = result.Warnings;
		}

		internal readonly List<(string Name, string ContentType, string Path)> Attachments = [];
		internal string Collection;
		internal long Duration;
		internal string? Exception;
		internal string? FilePath;
		internal string ID;
		internal int? Line;
		internal string? Message;
		internal string? Method;
		internal string Name;
		internal string Output;
		internal string Status;
		internal string Suite;
		internal string? Trace;
		internal IReadOnlyDictionary<string, IReadOnlyCollection<string>> Traits;
		internal string? Type;
		internal string[]? Warnings;
	}
}
