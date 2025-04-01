using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Threading;
using Xunit.Internal;
using Xunit.Runner.Common;
using Xunit.Sdk;

namespace Xunit.v3;

/// <summary>
/// Implementation of <see cref="ITestProcessLauncher"/> that will launch an xUnit.net v3 test
/// in-process.
/// </summary>
/// <remarks>
/// Note that this will require the runner author to implement dependency resolution, as no attempt
/// to do so is done here.
/// </remarks>
public sealed class InProcessTestProcessLauncher : ITestProcessLauncher, ITestProcessDirectLauncher
{
	Type? consoleRunnerInProcessType;
	MethodInfo? findMethod;
	MethodInfo? getTestAssemblyInfoMethod;
	Assembly? inprocRunnerAssembly;
	MethodInfo? runMethod;

	InProcessTestProcessLauncher()
	{ }

	/// <summary>
	/// Gets the singleton instance of <see cref="InProcessTestProcessLauncher"/>.
	/// </summary>
	public static InProcessTestProcessLauncher Instance { get; } = new();

	/// <inheritdoc/>
	public ITestProcessBase Find(
		XunitProjectAssembly projectAssembly,
		TestAssemblyInfo assemblyInfo,
		IMessageSink messageSink,
		IMessageSink? diagnosticMessageSink,
		ISourceInformationProvider? sourceInformationProvider)
	{
		Guard.ArgumentNotNull(projectAssembly);
		Guard.ArgumentNotNull(assemblyInfo);
		Guard.ArgumentNotNull(messageSink);

		projectAssembly.Assembly = Initialize(projectAssembly.AssemblyFileName);

		if (findMethod is not null)
			return InProcessDirectLauncherProcess.Create(projectAssembly, messageSink, diagnosticMessageSink, sourceInformationProvider, findMethod);
		else
			return TestProcessLauncherAdapter.Find(this, projectAssembly, assemblyInfo, messageSink, diagnosticMessageSink, sourceInformationProvider);
	}

	/// <inheritdoc/>
	public TestAssemblyInfo GetAssemblyInfo(XunitProjectAssembly projectAssembly)
	{
		Guard.ArgumentNotNull(projectAssembly);

		projectAssembly.Assembly = Initialize(projectAssembly.AssemblyFileName);

		if (getTestAssemblyInfoMethod is not null)
			return (TestAssemblyInfo)getTestAssemblyInfoMethod.Invoke(obj: null, [projectAssembly.Assembly])!;
		else
			return TestProcessLauncherAdapter.GetAssemblyInfo(this, projectAssembly);
	}

	static MethodInfo? GetPublicStaticMethod(
		Type type,
		string name,
		params Type[] parameterTypes) =>
			type.GetMethod(name, BindingFlags.Static | BindingFlags.Public, binder: null, parameterTypes, modifiers: null);

	Assembly Initialize(string testAssemblyFileName)
	{
		Guard.ArgumentNotNull(testAssemblyFileName);

		var testAssembly = Assembly.LoadFrom(testAssemblyFileName);
		if (testAssembly is null)
			throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, "Could not load test assembly '{0}'", testAssemblyFileName));

		inprocRunnerAssembly ??= AppDomain.CurrentDomain.GetAssemblies().FirstOrDefault(a => a.GetName().Name == "xunit.v3.runner.inproc.console");
		if (inprocRunnerAssembly is null)
			throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, "Test assembly '{0}' does not link against xunit.v3.runner.inproc.console", testAssemblyFileName));

		consoleRunnerInProcessType ??= inprocRunnerAssembly.GetType("Xunit.Runner.InProc.SystemConsole.ConsoleRunnerInProcess");
		if (consoleRunnerInProcessType is not null)
		{
			findMethod ??= GetPublicStaticMethod(
				consoleRunnerInProcessType,
				"Find",
				typeof(IMessageSink), typeof(IMessageSink), typeof(XunitProjectAssembly), typeof(CancellationTokenSource)
			);
			getTestAssemblyInfoMethod ??= GetPublicStaticMethod(
				consoleRunnerInProcessType,
				"GetTestAssemblyInfo",
				typeof(Assembly)
			);
			runMethod ??= GetPublicStaticMethod(
				consoleRunnerInProcessType,
				"Run",
				typeof(IMessageSink), typeof(IMessageSink), typeof(XunitProjectAssembly), typeof(CancellationTokenSource)
			);
		}

		return testAssembly;
	}

	/// <inheritdoc/>
	public ITestProcess? Launch(
		XunitProjectAssembly projectAssembly,
		IReadOnlyList<string> arguments)
	{
		Guard.ArgumentNotNull(projectAssembly);
		Guard.ArgumentNotNull(arguments);

		if (projectAssembly.AssemblyFileName is null)
			return default;
		if (projectAssembly.AssemblyMetadata is null || projectAssembly.AssemblyMetadata.TargetFrameworkIdentifier == TargetFrameworkIdentifier.UnknownTargetFramework)
			return default;

		return InProcessTestProcess.Create(projectAssembly.AssemblyFileName, arguments);
	}

	/// <inheritdoc/>
	public ITestProcessBase Run(
		XunitProjectAssembly projectAssembly,
		TestAssemblyInfo assemblyInfo,
		IMessageSink messageSink,
		IMessageSink? diagnosticMessageSink,
		ISourceInformationProvider? sourceInformationProvider)
	{
		Guard.ArgumentNotNull(projectAssembly);
		Guard.ArgumentNotNull(assemblyInfo);
		Guard.ArgumentNotNull(messageSink);

		projectAssembly.Assembly = Initialize(projectAssembly.AssemblyFileName);

		if (runMethod is not null)
			return InProcessDirectLauncherProcess.Create(projectAssembly, messageSink, diagnosticMessageSink, sourceInformationProvider, runMethod);
		else
			return TestProcessLauncherAdapter.Run(this, projectAssembly, assemblyInfo, messageSink, diagnosticMessageSink, sourceInformationProvider);
	}
}
