using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Xml.Linq;
using Xunit.Runner.Common;
using Xunit.Sdk;
using Xunit.v3;

namespace Xunit.Runner.InProc.SystemConsole;

internal sealed class ProjectRunner
{
	readonly Func<bool> cancelThunk;
	readonly object consoleLock;
	bool failed;

	public ProjectRunner(
		object consoleLock,
		Func<bool> cancelThunk)
	{
		this.consoleLock = consoleLock;
		this.cancelThunk = cancelThunk;
	}

	public TestExecutionSummaries TestExecutionSummaries { get; } = new();

	public static void PrintHeader() =>
		Console.WriteLine(
			"xUnit.net v3 In-Process Runner v{0} ({1}-bit {2})",
			ThisAssembly.AssemblyInformationalVersion,
			IntPtr.Size * 8,
			RuntimeInformation.FrameworkDescription
		);

	public async ValueTask<int> RunProject(
		XunitProject project,
		_IMessageSink reporterMessageHandler)
	{
		XElement? assembliesElement = null;
		var clockTime = Stopwatch.StartNew();
		var xmlTransformers = TransformFactory.GetXmlTransformers(project);
		var needsXml = xmlTransformers.Count > 0;

		if (needsXml)
			assembliesElement = TransformFactory.CreateAssembliesElement();

		var originalWorkingFolder = Directory.GetCurrentDirectory();

		var assembly = project.Assemblies.Single();
		var assemblyElement = await RunProjectAssembly(
			assembly,
			needsXml,
			reporterMessageHandler
		);

		if (assemblyElement is not null)
			assembliesElement?.Add(assemblyElement);

		clockTime.Stop();

		TestExecutionSummaries.ElapsedClockTime = clockTime.Elapsed;
		reporterMessageHandler.OnMessage(TestExecutionSummaries);

		Directory.SetCurrentDirectory(originalWorkingFolder);

		if (assembliesElement is not null)
		{
			TransformFactory.FinishAssembliesElement(assembliesElement);
			xmlTransformers.ForEach(transformer => transformer(assembliesElement));
		}

		return failed ? 1 : TestExecutionSummaries.SummariesByAssemblyUniqueID.Sum(s => s.Summary.Failed + s.Summary.Errors);
	}

	async ValueTask<XElement?> RunProjectAssembly(
		XunitProjectAssembly assembly,
		bool needsXml,
		_IMessageSink reporterMessageHandler)
	{
		if (cancelThunk())
			return null;

		var assemblyElement = needsXml ? new XElement("assembly") : null;

		try
		{
			// Default to false for console runners
			assembly.Configuration.PreEnumerateTheories ??= false;

			// Setup discovery and execution options with command-line overrides
			var discoveryOptions = _TestFrameworkOptions.ForDiscovery(assembly.Configuration);
			var executionOptions = _TestFrameworkOptions.ForExecution(assembly.Configuration);

			var noColor = assembly.Project.Configuration.NoColorOrDefault;
			var diagnosticMessages = assembly.Configuration.DiagnosticMessagesOrDefault;
			var internalDiagnosticMessages = assembly.Configuration.InternalDiagnosticMessagesOrDefault;
			var diagnosticMessageSink = ConsoleDiagnosticMessageSink.TryCreate(consoleLock, noColor, diagnosticMessages, internalDiagnosticMessages);
			var longRunningSeconds = assembly.Configuration.LongRunningTestSecondsOrDefault;

			TestContext.SetForInitialization(diagnosticMessageSink, diagnosticMessages, internalDiagnosticMessages);

			var assemblyInfo = new ReflectionAssemblyInfo(assembly.Assembly!);

			await using var disposalTracker = new DisposalTracker();
			var testFramework = ExtensibilityPointFactory.GetTestFramework(assemblyInfo);
			disposalTracker.Add(testFramework);

			var frontController = new InProcessFrontController(testFramework, assemblyInfo, assembly.ConfigFileName);

			var sinkOptions = new ExecutionSinkOptions
			{
				AssemblyElement = assemblyElement,
				CancelThunk = cancelThunk,
				DiagnosticMessageSink = diagnosticMessageSink,
				FailSkips = assembly.Configuration.FailSkipsOrDefault,
				FailWarn = assembly.Configuration.FailWarnsOrDefault,
				LongRunningTestTime = TimeSpan.FromSeconds(longRunningSeconds),
			};

			using var resultsSink = new ExecutionSink(assembly, discoveryOptions, executionOptions, AppDomainOption.NotAvailable, shadowCopy: false, reporterMessageHandler, sinkOptions);
			await frontController.FindAndRun(resultsSink, discoveryOptions, executionOptions, assembly.Configuration.Filters.Filter);

			TestExecutionSummaries.Add(frontController.TestAssemblyUniqueID, resultsSink.ExecutionSummary);

			// Technically speaking, this code is common to the meta runners and not necessary here, but I don't want to delete
			// it yet, in case this class gets moved up a layer and consumed by the meta-runners. This code would require us to
			// be able to trigger cancellation as well as read it, which probably means a CancellationTokenSource.
			/*
			if (resultsSink.ExecutionSummary.Failed != 0 && executionOptions.GetStopOnTestFailOrDefault())
			{
				Console.WriteLine("Canceling due to test failure...");
				cancel = true;
			}
			*/
		}
		catch (Exception ex)
		{
			failed = true;

			var e = ex;
			while (e is not null)
			{
				Console.WriteLine("{0}: {1}", e.GetType().FullName, e.Message);

				if (assembly.Configuration.InternalDiagnosticMessagesOrDefault)
					Console.WriteLine(e.StackTrace);

				e = e.InnerException;
			}
		}

		return assemblyElement;
	}
}
