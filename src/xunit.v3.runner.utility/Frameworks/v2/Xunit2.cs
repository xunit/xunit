﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Xunit.Abstractions;
using Xunit.Internal;
using Xunit.Runner.Common;
using Xunit.Sdk;
using Xunit.v3;

namespace Xunit.Runner.v2;

/// <summary>
/// This class be used to do discovery and execution of xUnit.net v2 tests.
/// Discovery can be source-based; execution requires a file-system based assembly.
/// </summary>
public class Xunit2 : IFrontController
{
#if NETFRAMEWORK
	static readonly string[] SupportedPlatforms = { "dotnet", "desktop" };
	static readonly string[] SupportedPlatforms_ForcedAppDomains = { "desktop" };
	readonly AssemblyHelper? assemblyHelper;
#else
	static readonly string[] SupportedPlatforms = { "dotnet" };
#endif

	readonly _IAssemblyInfo assemblyInfo;
	ITestCaseBulkDeserializer? bulkDeserializer;
	readonly string? configFileName;
	bool disposed;
	readonly ITestFrameworkDiscoverer remoteDiscoverer;
	readonly ITestFrameworkExecutor? remoteExecutor;
	readonly ITestFramework remoteFramework;

	Xunit2(
		_IMessageSink diagnosticMessageSink,
		AppDomainSupport appDomainSupport,
		_ISourceInformationProvider sourceInformationProvider,
		_IAssemblyInfo? assemblyInfo,
		string? assemblyFileName,
		string xunitExecutionAssemblyPath,
		string? configFileName,
		bool shadowCopy,
		string? shadowCopyFolder,
		bool verifyAssembliesOnDisk)
	{
#if NETFRAMEWORK
		// Only safe to assume the execution reference is copied in a desktop project
		if (verifyAssembliesOnDisk)
			Guard.FileExists(xunitExecutionAssemblyPath);

		CanUseAppDomains = !IsDotNet(xunitExecutionAssemblyPath);
#else
		CanUseAppDomains = false;
#endif

		DiagnosticMessageSink = diagnosticMessageSink;

		var appDomainAssembly = assemblyFileName ?? xunitExecutionAssemblyPath;
		AppDomain = AppDomainManagerFactory.Create(appDomainSupport != AppDomainSupport.Denied && CanUseAppDomains, appDomainAssembly, configFileName, shadowCopy, shadowCopyFolder, diagnosticMessageSink);
		DisposalTracker.Add(AppDomain);

#if NETFRAMEWORK
		var runnerUtilityAssemblyLocation = Path.GetDirectoryName(typeof(AssemblyHelper).Assembly.GetLocalCodeBase());
		assemblyHelper = AppDomain.CreateObjectFrom<AssemblyHelper>(typeof(AssemblyHelper).Assembly.Location, typeof(AssemblyHelper).FullName!, runnerUtilityAssemblyLocation);
		DisposalTracker.Add(assemblyHelper);
#endif

		TestFrameworkAssemblyName = GetTestFrameworkAssemblyName(xunitExecutionAssemblyPath);

		// We need both a v2 and v3 assembly info, so manufacture the things we're missing
		IAssemblyInfo remoteAssemblyInfo;
		if (assemblyInfo != null)
			remoteAssemblyInfo = new Xunit2AssemblyInfo(assemblyInfo);
		else
		{
			remoteAssemblyInfo = Guard.NotNull(
				"Could not create Xunit.Sdk.TestFrameworkProxy for v2 unit test",
				AppDomain.CreateObject<IAssemblyInfo>(TestFrameworkAssemblyName, "Xunit.Sdk.ReflectionAssemblyInfo", assemblyFileName)
			);
			assemblyInfo = new Xunit3AssemblyInfo(remoteAssemblyInfo);
		}

		this.assemblyInfo = assemblyInfo;
		this.configFileName = configFileName;
		TestAssemblyUniqueID = UniqueIDGenerator.ForAssembly(this.assemblyInfo.Name, this.assemblyInfo.AssemblyPath, configFileName);

		var v2SourceInformationProvider = Xunit2SourceInformationProviderAdapter.Adapt(sourceInformationProvider);
		var v2DiagnosticMessageSink = new Xunit2MessageSink(DiagnosticMessageSink);
		remoteFramework = Guard.NotNull(
			"Could not create Xunit.Sdk.TestFrameworkProxy for v2 unit test",
			AppDomain.CreateObject<ITestFramework>(
				TestFrameworkAssemblyName,
				"Xunit.Sdk.TestFrameworkProxy",
				remoteAssemblyInfo,
				v2SourceInformationProvider,
				v2DiagnosticMessageSink
			)
		);
		DisposalTracker.Add(remoteFramework);

		remoteDiscoverer = Guard.NotNull("Could not get discoverer from test framework for v2 unit test", remoteFramework.GetDiscoverer(remoteAssemblyInfo));
		DisposalTracker.Add(remoteDiscoverer);

		// If we got an assembly file name, that means we can do execution as well as discovery.
		if (assemblyFileName != null)
		{
#if NETFRAMEWORK
			var assemblyName = AssemblyName.GetAssemblyName(assemblyFileName);
#else
			var an = Assembly.Load(new AssemblyName { Name = Path.GetFileNameWithoutExtension(assemblyFileName) }).GetName();
			var assemblyName = new AssemblyName { Name = an.Name, Version = an.Version };
#endif
			remoteExecutor = remoteFramework.GetExecutor(assemblyName);
			DisposalTracker.Add(remoteExecutor);
		}
	}

	internal IAppDomainManager AppDomain { get; }

	/// <summary>
	/// Gets a value indicating whether the tests can use app domains (must be linked against desktop execution library).
	/// </summary>
	public bool CanUseAppDomains { get; }

	/// <summary>
	/// Gets the message sink used to report diagnostic messages.
	/// </summary>
	public _IMessageSink DiagnosticMessageSink { get; }

	/// <summary>
	/// Gets a tracker for disposable objects.
	/// </summary>
	protected DisposalTracker DisposalTracker { get; } = new DisposalTracker();

	/// <inheritdoc/>
	public string TestAssemblyUniqueID { get; protected set; }

	/// <inheritdoc/>
	public string TargetFramework => remoteDiscoverer.TargetFramework;

	internal AssemblyName TestFrameworkAssemblyName { get; }

	/// <inheritdoc/>
	public string TestFrameworkDisplayName => remoteDiscoverer.TestFrameworkDisplayName;

	List<KeyValuePair<string?, ITestCase?>> BulkDeserialize(List<string> serializations)
	{
		Guard.NotNull($"This instance of {typeof(Xunit2).FullName} was created for discovery only; execution-related operations cannot be performed.", remoteExecutor);

		var callbackContainer = new DeserializeCallback();
		Action<List<KeyValuePair<string?, ITestCase?>>> callback = callbackContainer.Callback;

		if (bulkDeserializer == null)
		{
			if (AppDomain.HasAppDomain)
			{
				try
				{
					AppDomain.CreateObject<object>(TestFrameworkAssemblyName, "Xunit.Sdk.TestCaseBulkDeserializer", remoteDiscoverer, remoteExecutor, serializations, callback);
					if (callbackContainer.Results != null)
						return callbackContainer.Results;
				}
				catch (TypeLoadException) { }    // Only be willing to eat "Xunit.Sdk.TestCaseBulkDeserialize" doesn't exist
			}

			bulkDeserializer = new DefaultTestCaseBulkDeserializer(remoteExecutor);
		}

		return bulkDeserializer.BulkDeserialize(serializations);
	}

	/// <summary>
	/// Creates a high performance cross-AppDomain message sink that utilizes <see cref="IMessageSinkWithTypes"/>
	/// which can be passed to <see cref="ITestFrameworkDiscoverer"/> and <see cref="ITestFrameworkExecutor"/>.
	/// </summary>
	/// <param name="sink">The local message sink to receive the messages.</param>
	/// <param name="serializeDiscoveredTestCases">A flag which indicates whether test case serialization is required</param>
	protected IMessageSink CreateOptimizedRemoteMessageSink(
		_IMessageSink sink,
		bool serializeDiscoveredTestCases = true)
	{
		Guard.ArgumentNotNull(sink);

		var v2MessageSink = new Xunit2MessageSink(sink, TestAssemblyUniqueID, serializeDiscoveredTestCases ? remoteDiscoverer : null);

		try
		{
			var assemblyName = typeof(OptimizedRemoteMessageSink).Assembly.GetName();
			var optimizedSink = AppDomain.CreateObject<IMessageSink>(assemblyName, typeof(OptimizedRemoteMessageSink).FullName!, v2MessageSink);
			if (optimizedSink != null)
				return optimizedSink;
		}
		catch { }    // This really shouldn't happen, but falling back makes sense in catastrophic cases

		return v2MessageSink;
	}

	/// <inheritdoc/>
	public virtual ValueTask DisposeAsync()
	{
		if (disposed)
			throw new ObjectDisposedException(GetType().FullName);

		disposed = true;

		return DisposalTracker.DisposeAsync();
	}

	/// <inheritdoc/>
	public void Find(
		_IMessageSink messageSink,
		FrontControllerFindSettings settings)
	{
		Guard.ArgumentNotNull(messageSink);
		Guard.ArgumentNotNull(settings);

		var includeSourceInformation = settings.Options.GetIncludeSourceInformationOrDefault();
		using var filteringMessageSink = new FilteringMessageSink(messageSink, settings.Filters.Filter);
		var remoteMessageSink = CreateOptimizedRemoteMessageSink(filteringMessageSink);
		var v2DiscoveryOptions = Xunit2OptionsAdapter.Adapt(settings.Options);

		SendDiscoveryStartingMessage(messageSink);

		if (settings.Filters.IncludedClasses.Count == 0)
		{
			remoteDiscoverer.Find(includeSourceInformation, remoteMessageSink, v2DiscoveryOptions);
			filteringMessageSink.Finished.WaitOne();
		}
		else
			foreach (var includedClass in settings.Filters.IncludedClasses)
			{
				remoteDiscoverer.Find(includedClass, includeSourceInformation, remoteMessageSink, v2DiscoveryOptions);
				filteringMessageSink.Finished.WaitOne();
			}

		SendDiscoveryCompleteMessage(messageSink);
	}

	/// <inheritdoc/>
	public void FindAndRun(
		_IMessageSink messageSink,
		FrontControllerFindAndRunSettings settings)
	{
		Guard.NotNull($"This instance of {typeof(Xunit2).FullName} was created for discovery only; execution-related operations cannot be performed.", remoteExecutor);

		Guard.ArgumentNotNull(messageSink);
		Guard.ArgumentNotNull(settings);

		if (settings.Filters.Empty)
		{
			remoteExecutor.RunAll(
				CreateOptimizedRemoteMessageSink(messageSink),
				Xunit2OptionsAdapter.Adapt(settings.DiscoveryOptions),
				Xunit2OptionsAdapter.Adapt(settings.ExecutionOptions)
			);

			return;
		}

		using var discoverySink = new Xunit2DiscoverySink(settings.Filters);
		var v2DiscoveryOptions = Xunit2OptionsAdapter.Adapt(settings.DiscoveryOptions);

		if (settings.Filters.IncludedClasses.Count == 0)
		{
			remoteDiscoverer.Find(includeSourceInformation: false, discoverySink, v2DiscoveryOptions);
			discoverySink.Finished.WaitOne();
		}
		else
			foreach (var includedClass in settings.Filters.IncludedClasses)
			{
				remoteDiscoverer.Find(includedClass, includeSourceInformation: false, discoverySink, v2DiscoveryOptions);
				discoverySink.Finished.WaitOne();
			}

		remoteExecutor.RunTests(
			discoverySink.TestCases,
			CreateOptimizedRemoteMessageSink(messageSink),
			Xunit2OptionsAdapter.Adapt(settings.ExecutionOptions)
		);
	}

	static string GetExecutionAssemblyFileName(AppDomainSupport appDomainSupport, string basePath)
	{
		var supportedPlatformSuffixes = GetSupportedPlatformSuffixes(appDomainSupport);

		foreach (var suffix in supportedPlatformSuffixes)
		{
#if NETFRAMEWORK
			var fileName = Path.Combine(basePath, $"xunit.execution.{suffix}.dll");
			if (File.Exists(fileName))
				return fileName;
#else
			try
			{
				var assemblyName = $"xunit.execution.{suffix}";
				Assembly.Load(new AssemblyName { Name = assemblyName });
				return assemblyName + ".dll";
			}
			catch { }
#endif
		}

		throw new InvalidOperationException("Could not find/load any of the following assemblies: " + string.Join(", ", supportedPlatformSuffixes.Select(suffix => $"xunit.execution.{suffix}.dll").ToArray()));
	}

	static string[] GetSupportedPlatformSuffixes(AppDomainSupport appDomainSupport)
	{
#if NETFRAMEWORK
		return appDomainSupport == AppDomainSupport.Required ? SupportedPlatforms_ForcedAppDomains : SupportedPlatforms;
#else
		return SupportedPlatforms;
#endif
	}

	static AssemblyName GetTestFrameworkAssemblyName(string xunitExecutionAssemblyPath)
	{
#if NETFRAMEWORK
		return AssemblyName.GetAssemblyName(xunitExecutionAssemblyPath);
#else
		// Make sure we only use the short form
		return Assembly.Load(new AssemblyName { Name = Path.GetFileNameWithoutExtension(xunitExecutionAssemblyPath), Version = new Version(0, 0, 0, 0) }).GetName();
#endif
	}

	static string GetXunitExecutionAssemblyPath(
		AppDomainSupport appDomainSupport,
		string assemblyFileName,
		bool verifyTestAssemblyExists)
	{
		Guard.ArgumentNotNullOrEmpty(assemblyFileName);
		if (verifyTestAssemblyExists)
			Guard.FileExists(assemblyFileName);

		return GetExecutionAssemblyFileName(appDomainSupport, Path.GetDirectoryName(assemblyFileName)!);
	}

	static string GetXunitExecutionAssemblyPath(
		AppDomainSupport appDomainSupport,
		_IAssemblyInfo assemblyInfo)
	{
		Guard.ArgumentNotNull(assemblyInfo);
		Guard.ArgumentNotNullOrEmpty(assemblyInfo.AssemblyPath);

		return GetExecutionAssemblyFileName(appDomainSupport, Path.GetDirectoryName(assemblyInfo.AssemblyPath)!);
	}

#if NETFRAMEWORK
	static bool IsDotNet(string executionAssemblyFileName) =>
		executionAssemblyFileName.EndsWith(".dotnet.dll", StringComparison.Ordinal);
#endif

	/// <inheritdoc/>
	public void Run(
		_IMessageSink messageSink,
		FrontControllerRunSettings settings)
	{
		Guard.NotNull($"This instance of {typeof(Xunit2).FullName} was created for discovery only; execution-related operations cannot be performed.", remoteExecutor);

		Guard.ArgumentNotNull(messageSink);
		Guard.ArgumentNotNull(settings);

		remoteExecutor.RunTests(
			BulkDeserialize(settings.SerializedTestCases.ToList()).Select(kvp => kvp.Value).ToList(),
			CreateOptimizedRemoteMessageSink(messageSink),
			Xunit2OptionsAdapter.Adapt(settings.Options)
		);
	}

	void SendDiscoveryStartingMessage(_IMessageSink messageSink)
	{
		// There is no v2 equivalent to this, so we manufacture it ourselves
		var discoveryStarting = new _DiscoveryStarting
		{
			AssemblyName = assemblyInfo.Name,
			AssemblyPath = assemblyInfo.AssemblyPath,
			AssemblyUniqueID = UniqueIDGenerator.ForAssembly(assemblyInfo.Name, assemblyInfo.AssemblyPath, configFileName),
			ConfigFilePath = configFileName,
		};

		messageSink.OnMessage(discoveryStarting);
	}

	void SendDiscoveryCompleteMessage(_IMessageSink messageSink)
	{
		// We optimize discovery when filtering by class, so we filter out discovery complete
		// messages, and need to send a single one when we're finished.
		var discoveryComplete = new _DiscoveryComplete
		{
			AssemblyUniqueID = UniqueIDGenerator.ForAssembly(assemblyInfo.Name, assemblyInfo.AssemblyPath, configFileName),
		};

		messageSink.OnMessage(discoveryComplete);
	}

	// Factory methods

	/// <summary>
	/// Returns an implementation of <see cref="IFrontControllerDiscoverer"/> which can be used
	/// to discover xUnit.net v2 tests, including source-based discovery.
	/// </summary>
	/// <param name="assemblyInfo">The assembly to use for discovery</param>
	/// <param name="projectAssembly">The test project assembly.</param>
	/// <param name="xunitExecutionAssemblyPath">The path on disk of xunit.execution.*.dll; if <c>null</c>, then
	/// the location of xunit.execution.*.dll is implied based on the location of the test assembly</param>
	/// <param name="sourceInformationProvider">The optional source information provider.</param>
	/// <param name="diagnosticMessageSink">The message sink which receives <see cref="_DiagnosticMessage"/> messages.</param>
	/// <param name="verifyAssembliesOnDisk">Determines whether or not to check for the existence of assembly files.</param>
	public static IFrontControllerDiscoverer ForDiscovery(
		_IAssemblyInfo assemblyInfo,
		XunitProjectAssembly projectAssembly,
		string? xunitExecutionAssemblyPath = null,
		_ISourceInformationProvider? sourceInformationProvider = null,
		_IMessageSink? diagnosticMessageSink = null,
		bool verifyAssembliesOnDisk = true)
	{
		var appDomainSupport = projectAssembly.Configuration.AppDomainOrDefault;

		Guard.ArgumentNotNull(assemblyInfo);

		if (diagnosticMessageSink == null)
			diagnosticMessageSink = _NullMessageSink.Instance;

#if NETFRAMEWORK
		if (sourceInformationProvider == null && assemblyInfo.AssemblyPath != null)
			sourceInformationProvider = new VisualStudioSourceInformationProvider(assemblyInfo.AssemblyPath, diagnosticMessageSink);
#endif

		return new Xunit2(
			diagnosticMessageSink ?? _NullMessageSink.Instance,
			appDomainSupport,
			sourceInformationProvider ?? _NullSourceInformationProvider.Instance,
			assemblyInfo,
			assemblyFileName: null,
			xunitExecutionAssemblyPath ?? GetXunitExecutionAssemblyPath(appDomainSupport, assemblyInfo),
			projectAssembly.ConfigFileName,
			projectAssembly.Configuration.ShadowCopyOrDefault,
			projectAssembly.Configuration.ShadowCopyFolder,
			verifyAssembliesOnDisk
		);
	}

	/// <summary>
	/// Returns an implementation of <see cref="IFrontController"/> which can be used
	/// for both discovery and execution of xUnit.net v2 tests.
	/// </summary>
	/// <param name="projectAssembly">The test project assembly.</param>
	/// <param name="sourceInformationProvider">The optional source information provider.</param>
	/// <param name="diagnosticMessageSink">The message sink which receives <see cref="_DiagnosticMessage"/> messages.</param>
	/// <param name="verifyAssembliesOnDisk">Determines whether or not to check for the existence of assembly files.</param>
	public static IFrontController ForDiscoveryAndExecution(
		XunitProjectAssembly projectAssembly,
		_ISourceInformationProvider? sourceInformationProvider = null,
		_IMessageSink? diagnosticMessageSink = null,
		bool verifyAssembliesOnDisk = true)
	{
		Guard.ArgumentNotNull(projectAssembly);

		var appDomainSupport = projectAssembly.Configuration.AppDomainOrDefault;
		var assemblyFileName = Guard.ArgumentNotNull(projectAssembly.AssemblyFileName);

		if (diagnosticMessageSink == null)
			diagnosticMessageSink = _NullMessageSink.Instance;

		return new Xunit2(
			diagnosticMessageSink,
			appDomainSupport,
#if NETSTANDARD
			sourceInformationProvider ?? _NullSourceInformationProvider.Instance,
#else
			sourceInformationProvider ?? new VisualStudioSourceInformationProvider(assemblyFileName, diagnosticMessageSink),
#endif
			assemblyInfo: null,
			assemblyFileName,
			GetXunitExecutionAssemblyPath(appDomainSupport, assemblyFileName, verifyAssembliesOnDisk),
			projectAssembly.ConfigFileName,
			projectAssembly.Configuration.ShadowCopyOrDefault,
			projectAssembly.Configuration.ShadowCopyFolder,
			verifyAssembliesOnDisk
		);
	}

	// Inner classes

	class DescriptorCallback : LongLivedMarshalByRefObject
	{
		public List<string>? Results;

		public void Callback(List<string> results) => Results = results;
	}

	class DeserializeCallback : LongLivedMarshalByRefObject
	{
		public List<KeyValuePair<string?, ITestCase?>>? Results;

		public void Callback(List<KeyValuePair<string?, ITestCase?>> results) => Results = results;
	}

	class FilteringMessageSink : _IMessageSink, IDisposable
	{
		readonly Predicate<_TestCaseDiscovered> filter;
		readonly _IMessageSink innerMessageSink;

		public FilteringMessageSink(
			_IMessageSink innerMessageSink,
			Predicate<_TestCaseDiscovered> filter)
		{
			this.innerMessageSink = innerMessageSink;
			this.filter = filter;
		}

		public AutoResetEvent Finished { get; } = new AutoResetEvent(initialState: false);

		public void Dispose() =>
			Finished.Dispose();

		public bool OnMessage(_MessageSinkMessage message)
		{
			// Filter out discovery complete (and make it an event) so we can run multiple discoveries
			// while reporting a single complete message after they're all done
			if (message is _DiscoveryComplete)
			{
				Finished.Set();
				return true;
			}

			if (message is _TestCaseDiscovered discovered)
				if (!filter(discovered))
					return true;

			return innerMessageSink.OnMessage(message);
		}
	}
}
