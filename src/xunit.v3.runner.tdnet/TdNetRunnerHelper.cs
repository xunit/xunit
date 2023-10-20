using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using TestDriven.Framework;
using Xunit.Internal;
using Xunit.Runner.Common;
using Xunit.Runner.v2;
using Xunit.Sdk;
using Xunit.v3;

namespace Xunit.Runner.TdNet;

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
		this.testListener = testListener;

		var assemblyFileName = assembly.GetLocalCodeBase();
		var project = new XunitProject();
		projectAssembly = new XunitProjectAssembly(project)
		{
			Assembly = assembly,
			AssemblyFileName = assemblyFileName,
			TargetFramework = AssemblyUtility.GetTargetFramework(assemblyFileName)
		};
		projectAssembly.Configuration.ShadowCopy = false;
		ConfigReader.Load(projectAssembly.Configuration, assemblyFileName);

		var diagnosticMessages = projectAssembly.Configuration.DiagnosticMessagesOrDefault;
		var internalDiagnosticMessages = projectAssembly.Configuration.InternalDiagnosticMessagesOrDefault;
		var assemblyDisplayName = Path.GetFileNameWithoutExtension(assemblyFileName);
#pragma warning disable CA2000 // This object is disposed in the disposal tracker
		var diagnosticMessageSink = TdNetDiagnosticMessageSink.TryCreate(testListener, testListenerLock, diagnosticMessages, internalDiagnosticMessages, assemblyDisplayName);
		disposalTracker.Add(diagnosticMessageSink);
#pragma warning restore CA2000

		frontController = Xunit2.ForDiscoveryAndExecution(projectAssembly, diagnosticMessageSink: diagnosticMessageSink);
		disposalTracker.Add(frontController);
	}

	public virtual IReadOnlyList<_TestCaseDiscovered> Discover()
	{
		Guard.NotNull(() => string.Format(CultureInfo.CurrentCulture, "Attempted to use an uninitialized {0}.frontController", GetType().FullName), frontController);
		Guard.NotNull(() => string.Format(CultureInfo.CurrentCulture, "Attempted to use an uninitialized {0}.projectAssembly", GetType().FullName), projectAssembly);

		var settings = new FrontControllerFindSettings(_TestFrameworkOptions.ForDiscovery(projectAssembly.Configuration));
		return Discover(sink => frontController.Find(sink, settings));
	}

	IReadOnlyList<_TestCaseDiscovered> Discover(Type? type)
	{
		Guard.NotNull(() => string.Format(CultureInfo.CurrentCulture, "Attempted to use an uninitialized {0}.frontController", GetType().FullName), frontController);
		Guard.NotNull(() => string.Format(CultureInfo.CurrentCulture, "Attempted to use an uninitialized {0}.projectAssembly", GetType().FullName), projectAssembly);

		if (type is null || type.FullName is null)
			return Array.Empty<_TestCaseDiscovered>();

		var settings = new FrontControllerFindSettings(_TestFrameworkOptions.ForDiscovery(projectAssembly.Configuration));
		settings.Filters.IncludedClasses.Add(type.FullName);

		return Discover(sink => frontController.Find(sink, settings));
	}

	IReadOnlyList<_TestCaseDiscovered> Discover(Action<_IMessageSink> discoveryAction)
	{
		try
		{
			using var sink = new TestDiscoverySink();
			disposalTracker.Add(sink);
			discoveryAction(sink);
			sink.Finished.WaitOne();
			return sink.TestCases.ToList();
		}
		catch (Exception ex)
		{
			lock (testListenerLock)
				testListener?.WriteLine("Error during test discovery:\r\n" + ex, Category.Error);

			return Array.Empty<_TestCaseDiscovered>();
		}
	}

	public ValueTask DisposeAsync()
	{
		if (disposed)
			return default;

		disposed = true;

		GC.SuppressFinalize(this);

		return disposalTracker.DisposeAsync();
	}

	public virtual TestRunState Run(
		IReadOnlyList<_TestCaseDiscovered>? testCases = null,
		TestRunState initialRunState = TestRunState.NoTests,
		ExplicitOption? explicitOption = null)
	{
		Guard.NotNull(() => string.Format(CultureInfo.CurrentCulture, "Attempted to use an uninitialized {0}.testListener", GetType().FullName), testListener);
		Guard.NotNull(() => string.Format(CultureInfo.CurrentCulture, "Attempted to use an uninitialized {0}.frontController", GetType().FullName), frontController);
		Guard.NotNull(() => string.Format(CultureInfo.CurrentCulture, "Attempted to use an uninitialized {0}.projectAssembly", GetType().FullName), projectAssembly);

		// TODO: This does not yet support fail-on-skip or reporting long-running tests. The fact that these are done via
		// delegating implementations of IExecutionSink is a design problem for this runner.

		try
		{
			testCases ??= Discover();

			var resultSink = new ResultSink(testListener, testListenerLock, testCases.Count) { TestRunState = initialRunState };
			disposalTracker.Add(resultSink);

			var executionOptions = _TestFrameworkOptions.ForExecution(projectAssembly.Configuration);
			if (explicitOption.HasValue)
				executionOptions.SetExplicitOption(explicitOption);

			var settings = new FrontControllerRunSettings(executionOptions, testCases.Select(tc => tc.Serialization).CastOrToReadOnlyCollection());
			frontController.Run(resultSink, settings);

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

	public virtual TestRunState RunClass(
		Type type,
		TestRunState initialRunState = TestRunState.NoTests)
	{
		Guard.ArgumentNotNull(type);

		var state = Run(Discover(type), initialRunState);

		foreach (var memberInfo in type.GetMembers())
		{
			var childType = memberInfo as Type;
			if (childType is not null)
				state = RunClass(childType, state);
		}

		return state;
	}

	public virtual TestRunState RunMethod(
		MethodInfo method,
		TestRunState initialRunState = TestRunState.NoTests)
	{
		Guard.ArgumentNotNull(method);

		var testCases = Discover(method.ReflectedType).Where(tc =>
		{
			if (tc.TestClassNameWithNamespace is null || tc.TestMethodName is null)
				return false;

			var typeInfo = Type.GetType(tc.TestClassNameWithNamespace);
			if (typeInfo is null)
				return false;

			var methodInfo = typeInfo.GetMethod(tc.TestMethodName, BindingFlags.Public);
			if (methodInfo is null)
				return false;

			if (methodInfo == method)
				return true;

			if (methodInfo.IsGenericMethod)
				return methodInfo.GetGenericMethodDefinition() == method;

			return false;
		}).ToList();

		return Run(testCases, initialRunState, ExplicitOption.On);
	}
}
