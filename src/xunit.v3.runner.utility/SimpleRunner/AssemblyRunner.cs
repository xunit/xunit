using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;
using Xunit.Internal;
using Xunit.Runner.Common;
using Xunit.Sdk;

namespace Xunit.SimpleRunner;

/// <summary>
/// A class which makes it simpler for casual runner authors to find and run tests and get results.
/// </summary>
public sealed class AssemblyRunner :
	IMessageSink, IAsyncDisposable
{
	bool cancelled;
	readonly IFrontController controller;
	readonly DisposalTracker disposalTracker = new();
	readonly ManualResetEventSlim finishedEvent = new(initialState: true);
	MessageMetadataCache metadataCache = new();
	readonly AssemblyRunnerOptions options;
	Dictionary<string, ITestResultMessage> testResults = [];
	int totalErrors;

	/// <summary>
	/// Initializes a new instance of the <see cref="AssemblyRunner"/> class.
	/// </summary>
	/// <param name="options"></param>
	public AssemblyRunner(AssemblyRunnerOptions options)
	{
		this.options = Guard.ArgumentNotNull(options);

		controller =
			XunitFrontController.Create(options.ProjectAssembly, diagnosticMessageSink: this)
				?? throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, "Assembly '{0}' does not appear to be an xUnit.net test assembly", options.AssemblyFileName), nameof(options));

		disposalTracker.Add(controller);
	}

	/// <summary>
	/// Call to request that the current run be cancelled. Note that cancellation may not be
	/// instantaneous, and even after cancellation has been acknowledged, you can expect to
	/// receive all the cleanup-related messages.
	/// </summary>
	public void Cancel() =>
		cancelled = true;

	static bool DispatchMessage<TMessage>(
		IMessageSinkMessage message,
		Action<TMessage> handler)
			where TMessage : IMessageSinkMessage
	{
		if (message is not TMessage tMessage)
			return false;

		handler(tMessage);
		return true;
	}

	/// <inheritdoc/>
	public async ValueTask DisposeAsync()
	{
		await disposalTracker.SafeDisposeAsync();
		finishedEvent.SafeDispose();
	}

	(ITestCollectionStarting?, ITestClassStarting?, ITestMethodStarting?, ITestStarting?, ITestResultMessage?) GetTestMetadata(ITestMessage testMessage)
	{
		var collectionStaring = metadataCache.TryGetCollectionMetadata(testMessage) as ITestCollectionStarting;
		var classStarting = metadataCache.TryGetClassMetadata(testMessage) as ITestClassStarting;
		var methodStarting = metadataCache.TryGetMethodMetadata(testMessage) as ITestMethodStarting;
		var testStarting = metadataCache.TryGetTestMetadata(testMessage) as ITestStarting;
		var testResult = default(ITestResultMessage);

		if (testStarting is not null)
			testResults.TryGetValue(testStarting.TestUniqueID, out testResult);

		return (collectionStaring, classStarting, methodStarting, testStarting, testResult);
	}

	void OnDiagnosticMessage(IDiagnosticMessage diagnosticMessage)
	{
		if (options.OnDiagnosticMessage is null)
			return;

		options.OnDiagnosticMessage(new()
		{
			Message = diagnosticMessage.Message,
			MessageType = MessageType.DiagnosticMessage,
		});
	}

	void OnDiscoveryComplete(IDiscoveryComplete discoveryComplete)
	{
		if (options.OnDiscoveryComplete is null)
			return;

		options.OnDiscoveryComplete(new()
		{
			TestCasesToRun = discoveryComplete.TestCasesToRun,
		});

		// TODO: If we cancel during discovery, do we get ITestAssemblyStarting/Finished or not?
	}

	void OnDiscoveryStarting(IDiscoveryStarting discoveryStarting)
	{
		options.OnDiscoveryStarting?.Invoke();
		Cancel();
	}

	void OnErrorMessage(
		IErrorMetadata metadata,
		ErrorMessageType messageType)
	{
		++totalErrors;

		if (options.OnErrorMessage is null)
			return;

		options.OnErrorMessage(new()
		{
			Exception = ExceptionInfo.FromErrorMessage(metadata),
			ErrorMessageType = messageType,
		});
	}

	void OnInternalDiagnosticMessage(IInternalDiagnosticMessage internalDiagnosticMessage)
	{
		if (options.OnDiagnosticMessage is null)
			return;

		options.OnDiagnosticMessage(new()
		{
			Message = internalDiagnosticMessage.Message,
			MessageType = MessageType.InternalDiagnosticMessage,
		});
	}

	void OnTestAssemblyFinished(ITestAssemblyFinished assemblyFinished)
	{
		try
		{
			if (metadataCache.TryRemove(assemblyFinished) is not ITestAssemblyStarting assemblyStarting || options.OnExecutionComplete is null)
				return;

			options.OnExecutionComplete(new()
			{
				ExecutionTime = assemblyFinished.ExecutionTime,
				FinishTime = assemblyFinished.FinishTime,
				Seed = assemblyStarting.Seed,
				StartTime = assemblyStarting.StartTime,
				TargetFramework = assemblyStarting.TargetFramework,
				TestEnvironment = assemblyStarting.TestEnvironment,
				TestFrameworkDisplayName = assemblyStarting.TestFrameworkDisplayName,
				TestsFailed = assemblyFinished.TestsFailed,
				TestsNotRun = assemblyFinished.TestsNotRun,
				TestsSkipped = assemblyFinished.TestsSkipped,
				TestsTotal = assemblyFinished.TestsTotal,
				TotalErrors = totalErrors,
			});
		}
		finally
		{
			finishedEvent.Set();
		}
	}

	void OnTestAssemblyStarting(ITestAssemblyStarting assemblyStarting)
	{
		metadataCache.Set(assemblyStarting);

		if (options.OnExecutionStarting is null)
			return;

		options.OnExecutionStarting(new()
		{
			Seed = assemblyStarting.Seed,
			StartTime = assemblyStarting.StartTime,
			TargetFramework = assemblyStarting.TargetFramework,
			TestEnvironment = assemblyStarting.TestEnvironment,
			TestFrameworkDisplayName = assemblyStarting.TestFrameworkDisplayName,
		});
	}

	void OnTestFinished(ITestFinished testFinished)
	{
		var (collectionStarting, classStarting, methodStarting, _, testResult) = GetTestMetadata(testFinished);
		if (metadataCache.TryRemove(testFinished) is not ITestStarting testStarting || collectionStarting is null || testResult is null)
			return;

		var testFinishedInfo = default(TestFinishedInfo);

		// We wait to translate the results into a single finished message so that information can be compined
		// from both the test result and test finished messages.

		if (testResult is ITestFailed testFailed && (options.OnTestFinished is not null || options.OnTestFailed is not null))
		{
			var testFailedInfo = new TestFailedInfo()
			{
				Attachments = testFinished.Attachments,
				Cause = testFailed.Cause,
				Exception = ExceptionInfo.FromErrorMessage(testFailed),
				ExecutionTime = testFinished.ExecutionTime,
				Explicit = testStarting.Explicit,
				FinishTime = testFinished.FinishTime,
				MethodName = methodStarting?.MethodName,
				Output = testFinished.Output,
				StartTime = testStarting.StartTime,
				TestCollectionDisplayName = collectionStarting.TestCollectionDisplayName,
				TestDisplayName = testStarting.TestDisplayName,
				Timeout = testStarting.Timeout,
				Traits = testStarting.Traits,
				TypeName = classStarting?.TestClassName,
				Warnings = testFinished.Warnings ?? [],
			};
			testFinishedInfo = testFailedInfo;

			options.OnTestFailed?.Invoke(testFailedInfo);
		}

		if (testResult is ITestNotRun && (options.OnTestFinished is not null || options.OnTestNotRun is not null))
		{
			var testNotRunInfo = new TestNotRunInfo()
			{
				Attachments = testFinished.Attachments,
				ExecutionTime = testFinished.ExecutionTime,
				Explicit = testStarting.Explicit,
				FinishTime = testFinished.FinishTime,
				MethodName = methodStarting?.MethodName,
				Output = testFinished.Output,
				StartTime = testStarting.StartTime,
				TestCollectionDisplayName = collectionStarting.TestCollectionDisplayName,
				TestDisplayName = testStarting.TestDisplayName,
				Timeout = testStarting.Timeout,
				Traits = testStarting.Traits,
				TypeName = classStarting?.TestClassName,
				Warnings = testFinished.Warnings ?? [],
			};
			testFinishedInfo = testNotRunInfo;

			options.OnTestNotRun?.Invoke(testNotRunInfo);
		}

		if (testResult is ITestPassed && (options.OnTestFinished is not null || options.OnTestPassed is not null))
		{
			var testPassedInfo = new TestPassedInfo()
			{
				Attachments = testFinished.Attachments,
				ExecutionTime = testFinished.ExecutionTime,
				Explicit = testStarting.Explicit,
				FinishTime = testFinished.FinishTime,
				MethodName = methodStarting?.MethodName,
				Output = testFinished.Output,
				StartTime = testStarting.StartTime,
				TestCollectionDisplayName = collectionStarting.TestCollectionDisplayName,
				TestDisplayName = testStarting.TestDisplayName,
				Timeout = testStarting.Timeout,
				Traits = testStarting.Traits,
				TypeName = classStarting?.TestClassName,
				Warnings = testFinished.Warnings ?? [],
			};
			testFinishedInfo = testPassedInfo;

			options.OnTestPassed?.Invoke(testPassedInfo);
		}

		if (testResult is ITestSkipped testSkipped && (options.OnTestFinished is not null || options.OnTestFailed is not null))
		{
			var testSkippedInfo = new TestSkippedInfo()
			{
				Attachments = testFinished.Attachments,
				ExecutionTime = testFinished.ExecutionTime,
				Explicit = testStarting.Explicit,
				FinishTime = testFinished.FinishTime,
				MethodName = methodStarting?.MethodName,
				Output = testFinished.Output,
				SkipReason = testSkipped.Reason,
				StartTime = testStarting.StartTime,
				TestCollectionDisplayName = collectionStarting.TestCollectionDisplayName,
				TestDisplayName = testStarting.TestDisplayName,
				Timeout = testStarting.Timeout,
				Traits = testStarting.Traits,
				TypeName = classStarting?.TestClassName,
				Warnings = testFinished.Warnings ?? [],
			};
			testFinishedInfo = testSkippedInfo;

			options.OnTestSkipped?.Invoke(testSkippedInfo);
		}

		if (testFinishedInfo is not null)
			options.OnTestFinished?.Invoke(testFinishedInfo);
	}

	void OnTestOutput(ITestOutput testOutput)
	{
		if (options.OnTestOutput is null)
			return;

		var (collectionStarting, classStarting, methodStarting, testStarting, _) = GetTestMetadata(testOutput);
		if (collectionStarting is null || testStarting is null)
			return;

		options.OnTestOutput(new()
		{
			MethodName = methodStarting?.MethodName,
			Output = testOutput.Output,
			TestCollectionDisplayName = collectionStarting.TestCollectionDisplayName,
			TestDisplayName = testStarting.TestDisplayName,
			Traits = testStarting.Traits,
			TypeName = classStarting?.TestClassName,
		});
	}

	void OnTestStarting(ITestStarting testStarting)
	{
		metadataCache.Set(testStarting);

		if (options.OnTestStarting is null)
			return;

		var (collectionStarting, classStarting, methodStarting, _, _) = GetTestMetadata(testStarting);
		if (collectionStarting is null)
			return;

		options.OnTestStarting(new()
		{
			Explicit = testStarting.Explicit,
			MethodName = methodStarting?.MethodName,
			StartTime = testStarting.StartTime,
			TestCollectionDisplayName = collectionStarting.TestCollectionDisplayName,
			TestDisplayName = testStarting.TestDisplayName,
			Timeout = testStarting.Timeout,
			Traits = testStarting.Traits,
			TypeName = classStarting?.TestClassName,
		});
	}

	/// <summary>
	/// Runs the test assembly.
	/// </summary>
	public Task Run()
	{
		lock (finishedEvent)
		{
			if (!finishedEvent.IsSet)
				throw new InvalidOperationException("The test assembly is already running");

			cancelled = true;
			testResults = [];
			metadataCache = new();

			finishedEvent.Reset();
		}

		var tcs = new TaskCompletionSource<object?>();

		ThreadPool.QueueUserWorkItem(_ =>
		{
			try
			{
				var discoveryOptions = TestFrameworkOptions.ForDiscovery(options.ProjectAssembly.Configuration);
				discoveryOptions.SetSynchronousMessageReporting(true);

				var executionOptions = TestFrameworkOptions.ForExecution(options.ProjectAssembly.Configuration);
				executionOptions.SetSynchronousMessageReporting(true);

				var settings = new FrontControllerFindAndRunSettings(discoveryOptions, executionOptions, options.ProjectAssembly.Configuration.Filters);

				var assembliesElement = default(XElement);
				IMessageSink messageSink = this;
				if (options.ProjectAssembly.Project.Configuration.Output.Count != 0)
				{
					var assemblyElement = new XElement("assembly");
					var executionSinkOptions = new ExecutionSinkOptions { AssemblyElement = assemblyElement };

					assembliesElement = TransformFactory.CreateAssembliesElement();
					assembliesElement.Add(assemblyElement);
					messageSink = new ExecutionSink(
						options.ProjectAssembly,
						discoveryOptions,
						executionOptions,
#if NETFRAMEWORK
						(options.XunitVersion, options.AppDomain, options.TargetFrameworkIdentifier) switch
						{
							(3, _, _) => AppDomainOption.NotAvailable,
							(_, _, TargetFrameworkIdentifier.DotNetCore) => AppDomainOption.NotAvailable,
							(_, AppDomainSupport.Denied, _) => AppDomainOption.Disabled,
							_ => AppDomainOption.Enabled,
						},
						options.ShadowCopy ?? true,
#else
						AppDomainOption.NotAvailable,
						shadowCopy: false,
#endif
						this,
						executionSinkOptions
					);
				}

				controller.FindAndRun(messageSink, settings);
				finishedEvent.Wait();

				if (assembliesElement is not null)
				{
					TransformFactory.FinishAssembliesElement(assembliesElement);

					foreach (var kvp in options.ProjectAssembly.Project.Configuration.Output)
						TransformFactory.Transform(kvp.Key, assembliesElement, kvp.Value);
				}

				tcs.TrySetResult(null);
			}
			catch (Exception ex)
			{
				tcs.TrySetException(ex);
			}
		});

		return tcs.Task;
	}

	bool IMessageSink.OnMessage(IMessageSinkMessage message)
	{
		// Discovery

		if (DispatchMessage<IDiscoveryStarting>(message, OnDiscoveryStarting))
			return !cancelled;
		if (DispatchMessage<IDiscoveryComplete>(message, OnDiscoveryComplete))
			return !cancelled;

		// Messages

		if (DispatchMessage<IDiagnosticMessage>(message, OnDiagnosticMessage))
			return !cancelled;
		if (DispatchMessage<IInternalDiagnosticMessage>(message, OnInternalDiagnosticMessage))
			return !cancelled;

		// Execution

		if (DispatchMessage<ITestAssemblyStarting>(message, OnTestAssemblyStarting))
			return !cancelled;
		if (DispatchMessage<ITestAssemblyFinished>(message, OnTestAssemblyFinished))
			return !cancelled;

		if (DispatchMessage<ITestCollectionStarting>(message, metadataCache.Set))
			return !cancelled;
		if (DispatchMessage<ITestCollectionFinished>(message, collectionFinished => metadataCache.TryRemove(collectionFinished)))
			return !cancelled;

		if (DispatchMessage<ITestClassStarting>(message, metadataCache.Set))
			return !cancelled;
		if (DispatchMessage<ITestClassFinished>(message, classFinished => metadataCache.TryRemove(classFinished)))
			return !cancelled;

		if (DispatchMessage<ITestMethodStarting>(message, metadataCache.Set))
			return !cancelled;
		if (DispatchMessage<ITestMethodFinished>(message, methodFinished => metadataCache.TryRemove(methodFinished)))
			return !cancelled;

		if (DispatchMessage<ITestStarting>(message, OnTestStarting))
			return !cancelled;
		if (DispatchMessage<ITestOutput>(message, OnTestOutput))
			return !cancelled;
		if (DispatchMessage<ITestFinished>(message, OnTestFinished))
			return !cancelled;

		if (DispatchMessage<ITestFailed>(message, testResult => testResults[testResult.TestUniqueID] = testResult))
			return !cancelled;
		if (DispatchMessage<ITestNotRun>(message, testResult => testResults[testResult.TestUniqueID] = testResult))
			return !cancelled;
		if (DispatchMessage<ITestPassed>(message, testResult => testResults[testResult.TestUniqueID] = testResult))
			return !cancelled;
		if (DispatchMessage<ITestSkipped>(message, testResult => testResults[testResult.TestUniqueID] = testResult))
			return !cancelled;

		// Error messages

		if (DispatchMessage<IErrorMessage>(message, metadata => OnErrorMessage(metadata, ErrorMessageType.CatastrophicError)))
			return !cancelled;
		if (DispatchMessage<ITestAssemblyCleanupFailure>(message, metadata => OnErrorMessage(metadata, ErrorMessageType.TestAssemblyCleanupFailure)))
			return !cancelled;
		if (DispatchMessage<ITestCaseCleanupFailure>(message, metadata => OnErrorMessage(metadata, ErrorMessageType.TestCaseCleanupFailure)))
			return !cancelled;
		if (DispatchMessage<ITestClassCleanupFailure>(message, metadata => OnErrorMessage(metadata, ErrorMessageType.TestClassCleanupFailure)))
			return !cancelled;
		if (DispatchMessage<ITestCleanupFailure>(message, metadata => OnErrorMessage(metadata, ErrorMessageType.TestCleanupFailure)))
			return !cancelled;
		if (DispatchMessage<ITestCollectionCleanupFailure>(message, metadata => OnErrorMessage(metadata, ErrorMessageType.TestCollectionCleanupFailure)))
			return !cancelled;
		if (DispatchMessage<ITestMethodCleanupFailure>(message, metadata => OnErrorMessage(metadata, ErrorMessageType.TestMethodCleanupFailure)))
			return !cancelled;

		return !cancelled;
	}
}
