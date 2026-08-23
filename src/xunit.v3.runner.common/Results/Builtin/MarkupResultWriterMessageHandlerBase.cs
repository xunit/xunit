using System.Collections.Concurrent;
using Xunit.Runner.Common;
using Xunit.Sdk;

namespace Xunit.Internal;

/// <summary>
/// INTERNAL CLASS. DO NOT USE.
/// </summary>
public abstract class MarkupResultWriterMessageHandlerBase : ResultMetadataMessageHandlerBase<ResultMetadataBase>, IResultWriterMessageHandler
{
	internal ConcurrentBag<string> Assemblies { get; } = [];

	internal ConcurrentBag<TestResult> Tests { get; } = [];

	internal DateTimeOffset TimeFinish { get; set; } = DateTimeOffset.MinValue;

	internal ExecutionSummary Totals { get; } = new();

	internal override ResultMetadataBase CreateMetadata() =>
		new();

	/// <inheritdoc/>
	public abstract ValueTask DisposeAsync();

	void HandleTestFailed(MessageHandlerArgs<ITestFailed> args)
	{
		var message = args.Message;
		if (!TryGetResultMetadata(message.AssemblyUniqueID, out var resultMetadata))
			return;

		var testAssembly = resultMetadata.MetadataCache.TryGetAssemblyMetadata(message);
		if (testAssembly is null)
			return;

		var test = resultMetadata.MetadataCache.TryGetTestMetadata(message);
		if (test is null)
			return;

		var testClass = resultMetadata.MetadataCache.TryGetClassMetadata(message);

		Tests.Add(new()
		{
			Assembly = testAssembly.AssemblyPath,
			ClassName = testClass?.TestClassName,
			DisplayName = test.TestDisplayName,
			Message = ExceptionUtility.CombineMessages(message),
			Output = message.Output,
			StackTrace = ExceptionUtility.CombineStackTraces(message),
			Status = TestResultStatus.Failed,
			Time = message.ExecutionTime,
			Traits = test.Traits,
			Warnings = message.Warnings
		});
	}

	void HandleTestNotRun(MessageHandlerArgs<ITestNotRun> args)
	{
		var message = args.Message;
		if (!TryGetResultMetadata(message.AssemblyUniqueID, out var resultMetadata))
			return;

		var testAssembly = resultMetadata.MetadataCache.TryGetAssemblyMetadata(message);
		if (testAssembly is null)
			return;

		var test = resultMetadata.MetadataCache.TryGetTestMetadata(message);
		if (test is null)
			return;

		var testClass = resultMetadata.MetadataCache.TryGetClassMetadata(message);

		Tests.Add(new()
		{
			Assembly = testAssembly.AssemblyPath,
			ClassName = testClass?.TestClassName,
			DisplayName = test.TestDisplayName,
			Output = message.Output,
			Status = TestResultStatus.NotRun,
			Time = message.ExecutionTime,
			Traits = test.Traits,
			Warnings = message.Warnings
		});
	}

	void HandleTestPassed(MessageHandlerArgs<ITestPassed> args)
	{
		var message = args.Message;
		if (!TryGetResultMetadata(message.AssemblyUniqueID, out var resultMetadata))
			return;

		var testAssembly = resultMetadata.MetadataCache.TryGetAssemblyMetadata(message);
		if (testAssembly is null)
			return;

		var test = resultMetadata.MetadataCache.TryGetTestMetadata(message);
		if (test is null)
			return;

		var testClass = resultMetadata.MetadataCache.TryGetClassMetadata(message);

		Tests.Add(new()
		{
			Assembly = testAssembly.AssemblyPath,
			ClassName = testClass?.TestClassName,
			DisplayName = test.TestDisplayName,
			Output = message.Output,
			Status = TestResultStatus.Passed,
			Time = message.ExecutionTime,
			Traits = test.Traits,
			Warnings = message.Warnings
		});
	}

	void HandleTestSkipped(MessageHandlerArgs<ITestSkipped> args)
	{
		var message = args.Message;
		if (!TryGetResultMetadata(message.AssemblyUniqueID, out var resultMetadata))
			return;

		var testAssembly = resultMetadata.MetadataCache.TryGetAssemblyMetadata(message);
		if (testAssembly is null)
			return;

		var test = resultMetadata.MetadataCache.TryGetTestMetadata(message);
		if (test is null)
			return;

		var testClass = resultMetadata.MetadataCache.TryGetClassMetadata(message);

		Tests.Add(new()
		{
			Assembly = testAssembly.AssemblyPath,
			ClassName = testClass?.TestClassName,
			DisplayName = test.TestDisplayName,
			Message = message.Reason,
			Output = message.Output,
			Status = TestResultStatus.Skipped,
			Time = message.ExecutionTime,
			Traits = test.Traits,
			Warnings = message.Warnings
		});
	}

	void OnError(
		string type,
		IErrorMetadata error,
		string? assemblyUniqueID)
	{
		var assembly = default(string);

		if (assemblyUniqueID is not null && TryGetResultMetadata(assemblyUniqueID, out var resultMetadata))
			assembly = resultMetadata.MetadataCache.TryGetAssemblyMetadata(assemblyUniqueID)?.AssemblyPath;

		Tests.Add(new()
		{
			Assembly = assembly,
			DisplayName = type,
			Message = ExceptionUtility.CombineMessages(error),
			StackTrace = ExceptionUtility.CombineStackTraces(error),
			Status = TestResultStatus.Error,
		});

		lock (Totals)
			++Totals.Errors;
	}

	/// <inheritdoc/>
	public override bool OnMessage(IMessageSinkMessage message)
	{
		message.DispatchWhen<IErrorMessage>(a => OnError("Fatal Error", a.Message, a.Message.AssemblyUniqueID));
		message.DispatchWhen<ITestAssemblyCleanupFailure>(a => OnError("Test Assembly Cleanup Failure", a.Message, a.Message.AssemblyUniqueID));
		message.DispatchWhen<ITestCaseCleanupFailure>(a => OnError("Test Case Cleanup Failure", a.Message, a.Message.AssemblyUniqueID));
		message.DispatchWhen<ITestClassCleanupFailure>(a => OnError("Test Class Cleanup Failure", a.Message, a.Message.AssemblyUniqueID));
		message.DispatchWhen<ITestCleanupFailure>(a => OnError("Test Cleanup Failure", a.Message, a.Message.AssemblyUniqueID));
		message.DispatchWhen<ITestCollectionCleanupFailure>(a => OnError("Test Collection Cleanup Failure", a.Message, a.Message.AssemblyUniqueID));
		message.DispatchWhen<ITestMethodCleanupFailure>(a => OnError("Test Method Cleanup Failure", a.Message, a.Message.AssemblyUniqueID));

		message.DispatchWhen<ITestFailed>(HandleTestFailed);
		message.DispatchWhen<ITestNotRun>(HandleTestNotRun);
		message.DispatchWhen<ITestPassed>(HandleTestPassed);
		message.DispatchWhen<ITestSkipped>(HandleTestSkipped);

		return base.OnMessage(message);
	}

	internal override void OnTestAssemblyStarting(
		ITestAssemblyStarting message,
		ResultMetadataBase resultMetadata) =>
			Assemblies.Add(message.AssemblyPath);

	internal override void OnTestAssemblyFinished(
		ITestAssemblyFinished message,
		ResultMetadataBase resultMetadata)
	{
		if (TimeFinish < message.FinishTime)
			TimeFinish = message.FinishTime;

		lock (Totals)
		{
			Totals.Failed += message.TestsFailed;
			Totals.NotRun += message.TestsNotRun;
			Totals.Skipped += message.TestsSkipped;
			Totals.Time += message.ExecutionTime;
			Totals.Total += message.TestsTotal;
		}
	}

	internal sealed class TestResult
	{
		internal string? Assembly;
		internal string? ClassName;
		internal required string DisplayName;
		internal string? Message;
		internal string? Output;
		internal string? StackTrace;
		internal required TestResultStatus Status;
		internal decimal Time = 0m;
		internal IReadOnlyDictionary<string, IReadOnlyCollection<string>>? Traits;
		internal string[]? Warnings;
	}

	internal enum TestResultStatus { Passed, Failed, Skipped, NotRun, Error };
}
