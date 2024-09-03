#if NETFRAMEWORK

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Web.UI;
using Xunit.Internal;
using Xunit.Sdk;

namespace Xunit.Runner.v1;

/// <summary>
/// Default implementation of <see cref="IXunit1Executor"/>. Creates a remote app domain for the test
/// assembly to be loaded into. Disposing of the executor releases the app domain.
/// </summary>
public class Xunit1Executor : IXunit1Executor
{
	readonly IAppDomainManager appDomain;
	bool disposed;
	readonly object executor;
	readonly AssemblyName xunitAssemblyName;
	readonly string xunitAssemblyPath;

	/// <summary>
	/// Initializes a new instance of the <see cref="Xunit1Executor" /> class.
	/// </summary>
	/// <param name="diagnosticMessageSink">The message sink which receives <see cref="IDiagnosticMessage"/>
	/// and <see cref="IInternalDiagnosticMessage"/> messages.</param>
	/// <param name="useAppDomain">Determines whether tests should be run in a separate app domain.</param>
	/// <param name="testAssemblyFileName">The filename of the test assembly.</param>
	/// <param name="configFileName">The filename of the configuration file.</param>
	/// <param name="shadowCopy">Set to <c>true</c> to enable shadow copying the assemblies.</param>
	/// <param name="shadowCopyFolder">The path on disk to use for shadow copying; if <c>null</c>, a folder
	/// will be automatically (randomly) generated</param>
	public Xunit1Executor(
		IMessageSink diagnosticMessageSink,
		bool useAppDomain,
		string testAssemblyFileName,
		string? configFileName = null,
		bool shadowCopy = true,
		string? shadowCopyFolder = null)
	{
		Guard.ArgumentNotNull(testAssemblyFileName);

		appDomain = AppDomainManagerFactory.Create(useAppDomain, testAssemblyFileName, configFileName, shadowCopy, shadowCopyFolder, diagnosticMessageSink);
		xunitAssemblyPath = GetXunitAssemblyPath(testAssemblyFileName);
		xunitAssemblyName = AssemblyName.GetAssemblyName(xunitAssemblyPath);
		executor = Guard.NotNull("Could not create Xunit.Sdk.Executor object for v1 test assembly", CreateObject("Xunit.Sdk.Executor", testAssemblyFileName));
		TestFrameworkDisplayName = string.Format(CultureInfo.InvariantCulture, "xUnit.net {0}", AssemblyName.GetAssemblyName(xunitAssemblyPath).Version);
	}

	/// <inheritdoc/>
	public string TestFrameworkDisplayName { get; private set; }

	object? CreateObject(
		string typeName,
		params object?[] args) =>
			appDomain.CreateObject<object>(xunitAssemblyName, typeName, args);

	/// <inheritdoc/>
	public void Dispose()
	{
		if (disposed)
			return;

		disposed = true;

		GC.SuppressFinalize(this);

		appDomain?.Dispose();
	}

	/// <inheritdoc/>
	public void EnumerateTests(ICallbackEventHandler? handler) =>
		CreateObject("Xunit.Sdk.Executor+EnumerateTests", executor, handler);

	static string GetXunitAssemblyPath(string testAssemblyFileName)
	{
		Guard.FileExists(testAssemblyFileName);

		var xunitPath = Path.Combine(Path.GetDirectoryName(testAssemblyFileName)!, "xunit.dll");
		Guard.FileExists(xunitPath, nameof(testAssemblyFileName));

		return xunitPath;
	}

	/// <inheritdoc/>
	public void RunTests(
		string type,
		List<string> methods,
		ICallbackEventHandler handler)
	{
		Guard.ArgumentNotNullOrEmpty(type);
		Guard.ArgumentNotNull(methods);
		Guard.ArgumentNotNull(handler);

		CreateObject("Xunit.Sdk.Executor+RunTests", executor, type, methods, handler);
	}
}

#endif
