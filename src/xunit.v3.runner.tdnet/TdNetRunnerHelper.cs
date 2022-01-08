using System;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using TestDriven.Framework;
using Xunit.Internal;
using Xunit.Runner.Common;
using Xunit.Runner.v3;
using Xunit.Sdk;

namespace Xunit.Runner.TdNet;

// This class does not use XunitFrontController, because the reference to xunit.runner.tdnet comes via
// the NuGet package for the specific version of xUnit.net your tests are written against. So this only
// needs to be written against the current version of xUnit.net.
public class TdNetRunnerHelper : IAsyncDisposable
{
	bool disposed;
	readonly DisposalTracker disposalTracker = new();
#pragma warning disable CA2213 // This is disposed by DisposalTracker
	readonly IFrontController? frontController;
#pragma warning restore CA2213
	readonly XunitProjectAssembly? projectAssembly;
	readonly ITestListener? testListener;
	readonly object testListenerLock = new();

	/// <summary>
	/// This constructor is for unit testing purposes only.
	/// </summary>
	[Obsolete("For unit testing purposes only")]
	protected TdNetRunnerHelper()
	{ }

	public TdNetRunnerHelper(
		Assembly assembly,
		ITestListener testListener)
	{
		this.testListener = Guard.ArgumentNotNull(testListener);

		var assemblyFileName = assembly.GetLocalCodeBase();
		var project = new XunitProject();
		projectAssembly = new XunitProjectAssembly(project)
		{
			Assembly = assembly,
			AssemblyFileName = assemblyFileName,
			TargetFramework = AssemblyUtility.GetTargetFramework(assemblyFileName)
		};
		projectAssembly.Configuration.ShadowCopy = false;

		var warnings = new List<string>();
		ConfigReader.Load(projectAssembly.Configuration, assemblyFileName, warnings: warnings);

		foreach (var warning in warnings)
			testListener.WriteLine(warning, Category.Warning);

		var diagnosticMessages = projectAssembly.Configuration.DiagnosticMessagesOrDefault;
		var internalDiagnosticMessages = projectAssembly.Configuration.InternalDiagnosticMessagesOrDefault;
		var assemblyDisplayName = Path.GetFileNameWithoutExtension(assemblyFileName);
#pragma warning disable CA2000 // This object is disposed in the disposal tracker
		var diagnosticMessageSink = TdNetDiagnosticMessageSink.TryCreate(testListener, testListenerLock, diagnosticMessages, internalDiagnosticMessages, assemblyDisplayName);
		disposalTracker.Add(diagnosticMessageSink);
#pragma warning restore CA2000

		frontController = Xunit3.ForDiscoveryAndExecution(projectAssembly, diagnosticMessageSink: diagnosticMessageSink);
		disposalTracker.Add(frontController);
	}

	public virtual ValueTask DisposeAsync()
	{
		if (disposed)
			return default;

		disposed = true;

		GC.SuppressFinalize(this);

		return disposalTracker.DisposeAsync();
	}

	TestRunState FindAndRun(
		TestRunState initialRunState,
		XunitFilters? filters = null)
	{
		Guard.NotNull(() => string.Format(CultureInfo.CurrentCulture, "Attempted to use an uninitialized {0} (testListener is null)", GetType().FullName), testListener);
		Guard.NotNull(() => string.Format(CultureInfo.CurrentCulture, "Attempted to use an uninitialized {0} (frontController is null)", GetType().FullName), frontController);
		Guard.NotNull(() => string.Format(CultureInfo.CurrentCulture, "Attempted to use an uninitialized {0} (projectAssembly is null)", GetType().FullName), projectAssembly);

		// TODO: This does not yet support fail-on-skip or reporting long-running tests. The fact that these are done via
		// delegating implementations of IExecutionSink is a design problem for this runner.

		try
		{
			var resultSink = new ResultSink(testListener, testListenerLock) { TestRunState = initialRunState };
			disposalTracker.Add(resultSink);

			var discoveryOptions = _TestFrameworkOptions.ForDiscovery(projectAssembly.Configuration);
			var executionOptions = _TestFrameworkOptions.ForExecution(projectAssembly.Configuration);
			var settings = new FrontControllerFindAndRunSettings(discoveryOptions, executionOptions, filters);
			frontController.FindAndRun(resultSink, settings);

			resultSink.Finished.WaitOne();

			return resultSink.TestRunState;
		}
		catch (Exception ex)
		{
			lock (testListenerLock)
				testListener.WriteLine("Error during test execution:\r\n" + ex, Category.Error);

			return TestRunState.Error;
		}
	}

	public virtual TestRunState RunAll(
		TestRunState initialRunState = TestRunState.NoTests) =>
			FindAndRun(initialRunState);

	public virtual TestRunState RunClass(
		Type type,
		TestRunState initialRunState = TestRunState.NoTests)
	{
		if (type is null || type.FullName is null)
			return initialRunState;

		var filters = new XunitFilters();
		filters.IncludedClasses.Add(type.FullName);
		return FindAndRun(initialRunState, filters);
	}

	public virtual TestRunState RunMethod(
		MethodInfo method,
		TestRunState initialRunState = TestRunState.NoTests)
	{
		if (method is null)
			return initialRunState;

		var type = method.ReflectedType ?? method.DeclaringType;
		if (type is null || type.FullName is null)
			return initialRunState;

		var filters = new XunitFilters();
		filters.IncludedMethods.Add(string.Format(CultureInfo.InvariantCulture, "{0}.{1}", type.FullName, method.Name));
		return FindAndRun(initialRunState, filters);
	}

	public virtual TestRunState RunNamespace(
		string @namespace,
		TestRunState initialRunState = TestRunState.NoTests)
	{
		if (string.IsNullOrWhiteSpace(@namespace))
			return initialRunState;

		var filters = new XunitFilters();
		filters.IncludedNamespaces.Add(@namespace);
		return FindAndRun(initialRunState, filters);
	}
}
