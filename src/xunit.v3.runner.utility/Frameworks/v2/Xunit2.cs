using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using Xunit.Abstractions;
using Xunit.Internal;
using Xunit.Runner.Common;
using Xunit.Sdk;

namespace Xunit.Runner.v2;

/// <summary>
/// This class is used to do discovery and execution of xUnit.net v2 tests.
/// Discovery can be source-based; execution requires a file-system based assembly.
/// Runner authors are strongly encouraged to use <see cref="XunitFrontController"/>
/// instead of using this class directly, unless you are doing source-based
/// discovery of v2 tests.
/// </summary>
public class Xunit2 : IFrontController
{
	internal static readonly IReadOnlyDictionary<string, TestAttachment> EmptyAttachments = new Dictionary<string, TestAttachment>();
	internal static IReadOnlyDictionary<string, IReadOnlyCollection<string>> EmptyV3Traits = new Dictionary<string, IReadOnlyCollection<string>>();

#if NETFRAMEWORK
	static readonly string[] SupportedPlatforms = ["dotnet", "desktop"];
	static readonly string[] SupportedPlatforms_ForcedAppDomains = ["desktop"];
#pragma warning disable CA2213 // This is disposed by DisposalTracker
	readonly AssemblyHelper? assemblyHelper;
#pragma warning restore CA2213
#else
	static readonly string[] SupportedPlatforms = ["dotnet"];
#endif

	readonly IAssemblyInfo assemblyInfo;
	readonly string assemblyName;
	ITestCaseBulkDeserializer? bulkDeserializer;
	readonly string? configFileName;
	bool disposed;
#pragma warning disable CA2213 // These are disposed by DisposalTracker
	readonly ITestFrameworkDiscoverer remoteDiscoverer;
	readonly ITestFrameworkExecutor? remoteExecutor;
	readonly ITestFramework remoteFramework;
#pragma warning restore CA2213

	Xunit2(
		Sdk.IMessageSink diagnosticMessageSink,
		AppDomainSupport appDomainSupport,
		Common.ISourceInformationProvider? sourceInformationProvider,
		IAssemblyInfo? assemblyInfo,
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

		if (assemblyInfo is not null)
			Guard.ArgumentValid("Assembly info implementation must derive from MarshalByRefObject", assemblyInfo is MarshalByRefObject);
		else
			assemblyInfo = Guard.NotNull(
				"Could not create Xunit.Sdk.ReflectionAssemblyInfo for v2 unit test",
				AppDomain.CreateObject<IAssemblyInfo>(TestFrameworkAssemblyName, "Xunit.Sdk.ReflectionAssemblyInfo", assemblyFileName)
			);

		this.assemblyInfo = assemblyInfo;
		assemblyName = assemblyInfo.Name.Split(',')[0];
		this.configFileName = configFileName;
		TestAssemblyUniqueID = UniqueIDGenerator.ForAssembly(this.assemblyInfo.AssemblyPath, configFileName);

		if (sourceInformationProvider is null)
		{
			sourceInformationProvider = CecilSourceInformationProvider.Create(assemblyFileName);
			DisposalTracker.Add(sourceInformationProvider);
		}

		var v2SourceInformationProvider = Xunit2SourceInformationProviderAdapter.Adapt(sourceInformationProvider);
		var v2DiagnosticMessageSink = new Xunit2MessageSink(DiagnosticMessageSink);
		remoteFramework = Guard.NotNull(
			"Could not create Xunit.Sdk.TestFrameworkProxy for v2 unit test",
			AppDomain.CreateObject<ITestFramework>(
				TestFrameworkAssemblyName,
				"Xunit.Sdk.TestFrameworkProxy",
				assemblyInfo,
				v2SourceInformationProvider,
				v2DiagnosticMessageSink
			)
		);
		DisposalTracker.Add(remoteFramework);
		DisposalTracker.Add(v2SourceInformationProvider);

		remoteDiscoverer = Guard.NotNull("Could not get discoverer from test framework for v2 unit test", remoteFramework.GetDiscoverer(assemblyInfo));
		DisposalTracker.Add(remoteDiscoverer);

		// If we got an assembly file name, that means we can do execution as well as discovery.
		if (assemblyFileName is not null)
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
	public Sdk.IMessageSink DiagnosticMessageSink { get; }

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

	List<KeyValuePair<string?, Abstractions.ITestCase?>> BulkDeserialize(List<string> serializations)
	{
		Guard.NotNull(() => string.Format(CultureInfo.CurrentCulture, "This instance of {0} was created for discovery only; execution-related operations cannot be performed.", typeof(Xunit2).FullName), remoteExecutor);

		var callbackContainer = new DeserializeCallback();
		Action<List<KeyValuePair<string?, Abstractions.ITestCase?>>> callback = callbackContainer.Callback;

		if (bulkDeserializer is null)
		{
			if (AppDomain.HasAppDomain)
			{
				try
				{
					AppDomain.CreateObject<object>(TestFrameworkAssemblyName, "Xunit.Sdk.TestCaseBulkDeserializer", remoteDiscoverer, remoteExecutor, serializations, callback);
					if (callbackContainer.Results is not null)
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
	protected Abstractions.IMessageSink CreateOptimizedRemoteMessageSink(
		Sdk.IMessageSink sink,
		bool serializeDiscoveredTestCases = true)
	{
		Guard.ArgumentNotNull(sink);

		var v2MessageSink = new Xunit2MessageSink(sink, TestAssemblyUniqueID, serializeDiscoveredTestCases ? remoteDiscoverer : null);

		try
		{
			var asssemblyName = typeof(OptimizedRemoteMessageSink).Assembly.GetName();
			var optimizedSink = AppDomain.CreateObject<Abstractions.IMessageSink>(asssemblyName, typeof(OptimizedRemoteMessageSink).FullName!, v2MessageSink);
			if (optimizedSink is not null)
				return optimizedSink;
		}
		catch { }    // This really shouldn't happen, but falling back makes sense in catastrophic cases

		return v2MessageSink;
	}

	/// <inheritdoc/>
	public virtual ValueTask DisposeAsync()
	{
		if (disposed)
			return default;

		disposed = true;

		GC.SuppressFinalize(this);

		return DisposalTracker.SafeDisposeAsync();
	}

	/// <inheritdoc/>
	public void Find(
		Sdk.IMessageSink messageSink,
		FrontControllerFindSettings settings)
	{
		Guard.ArgumentNotNull(messageSink);
		Guard.ArgumentNotNull(settings);

		ThreadPool.QueueUserWorkItem(_ =>
		{
			var includeSourceInformation = settings.Options.GetIncludeSourceInformationOrDefault();
			using var filteringMessageSink = new FilteringMessageSink(messageSink, testCase => settings.Filters.Filter(assemblyName, testCase));
			var remoteMessageSink = CreateOptimizedRemoteMessageSink(filteringMessageSink);
			var v2DiscoveryOptions = Xunit2OptionsAdapter.Adapt(settings.Options);

			SendDiscoveryStartingMessage(messageSink);

			remoteDiscoverer.Find(includeSourceInformation, remoteMessageSink, v2DiscoveryOptions);
			filteringMessageSink.Finished.WaitOne();

			SendDiscoveryCompleteMessage(messageSink, filteringMessageSink.TestCasesToRun);
		});
	}

	/// <inheritdoc/>
	public void FindAndRun(
		Sdk.IMessageSink messageSink,
		FrontControllerFindAndRunSettings settings)
	{
		Guard.NotNull(() => string.Format(CultureInfo.CurrentCulture, "This instance of {0} was created for discovery only; execution-related operations cannot be performed.", typeof(Xunit2).FullName), remoteExecutor);

		Guard.ArgumentNotNull(messageSink);
		Guard.ArgumentNotNull(settings);

		ThreadPool.QueueUserWorkItem(_ =>
		{
			var explicitOption = settings.ExecutionOptions.GetExplicitOptionOrDefault();

			SendDiscoveryStartingMessage(messageSink);

			using var discoverySink = new Xunit2DiscoverySink(assemblyName, settings.Filters);
			var v2DiscoveryOptions = Xunit2OptionsAdapter.Adapt(settings.DiscoveryOptions);
			var testCases = new List<Abstractions.ITestCase>();

			remoteDiscoverer.Find(includeSourceInformation: false, discoverySink, v2DiscoveryOptions);
			discoverySink.Finished.WaitOne();

			SendDiscoveryCompleteMessage(messageSink, discoverySink.TestCases.Count);

			if (explicitOption == ExplicitOption.Only)
				ReportTestCasesAsNotRun(discoverySink.TestCases, messageSink);
			else
				remoteExecutor.RunTests(
					discoverySink.TestCases,
					CreateOptimizedRemoteMessageSink(messageSink),
					Xunit2OptionsAdapter.Adapt(settings.ExecutionOptions)
				);
		});
	}

	static string GetExecutionAssemblyFileName(AppDomainSupport appDomainSupport, string basePath)
	{
		var supportedPlatformSuffixes = GetSupportedPlatformSuffixes(appDomainSupport);

		foreach (var suffix in supportedPlatformSuffixes)
		{
#if NETFRAMEWORK
			var fileName = Path.Combine(basePath, string.Format(CultureInfo.InvariantCulture, "xunit.execution.{0}.dll", suffix));
			if (File.Exists(fileName))
				return fileName;
#else
			try
			{
				var assemblyName = string.Format(CultureInfo.InvariantCulture, "xunit.execution.{0}", suffix);
				Assembly.Load(new AssemblyName { Name = assemblyName });
				return assemblyName + ".dll";
			}
			catch { }
#endif
		}

		throw new InvalidOperationException("Could not find/load any of the following assemblies: " + string.Join(", ", supportedPlatformSuffixes.Select(suffix => string.Format(CultureInfo.InvariantCulture, "xunit.execution.{0}.dll", suffix)).ToArray()));
	}

	static string[] GetSupportedPlatformSuffixes(AppDomainSupport appDomainSupport) =>
#if NETFRAMEWORK
		appDomainSupport == AppDomainSupport.Required ? SupportedPlatforms_ForcedAppDomains : SupportedPlatforms;
#else
		SupportedPlatforms;
#endif

	static AssemblyName GetTestFrameworkAssemblyName(string xunitExecutionAssemblyPath) =>
#if NETFRAMEWORK
		AssemblyName.GetAssemblyName(xunitExecutionAssemblyPath);
#else
		// Make sure we only use the short form
		Assembly.Load(new AssemblyName { Name = Path.GetFileNameWithoutExtension(xunitExecutionAssemblyPath), Version = new Version(0, 0, 0, 0) }).GetName();
#endif

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
		IAssemblyInfo assemblyInfo)
	{
		Guard.ArgumentNotNull(assemblyInfo);
		Guard.ArgumentNotNullOrEmpty(assemblyInfo.AssemblyPath);

		return GetExecutionAssemblyFileName(appDomainSupport, Path.GetDirectoryName(assemblyInfo.AssemblyPath)!);
	}

#if NETFRAMEWORK
	static bool IsDotNet(string executionAssemblyFileName) =>
		executionAssemblyFileName.EndsWith(".dotnet.dll", StringComparison.Ordinal);
#endif

	void ReportTestCasesAsNotRun(
		IReadOnlyList<Abstractions.ITestCase?> testCases,
		Sdk.IMessageSink messageSink)
	{
		messageSink.OnMessage(new TestAssemblyStarting
		{
			AssemblyName = assemblyInfo.Name,
			AssemblyPath = assemblyInfo.AssemblyPath,
			AssemblyUniqueID = TestAssemblyUniqueID,
			ConfigFilePath = configFileName,
			Seed = null,
			StartTime = DateTimeOffset.Now,
			TargetFramework = TargetFramework,
			TestEnvironment = string.Format(CultureInfo.CurrentCulture, "{0}-bit {1}", IntPtr.Size * 8, RuntimeInformation.FrameworkDescription),  // This may not be exactly right, but without the remote app domain, we don't know for sure
			TestFrameworkDisplayName = TestFrameworkDisplayName,
			Traits = EmptyV3Traits,
		});

		// For reporting purposes, assume all tests are in the same collection
		var testCollectionDisplayName = "Not-run tests";
		var testCollectionUniqueID = UniqueIDGenerator.ForTestCollection(TestAssemblyUniqueID, testCollectionDisplayName, null);
		messageSink.OnMessage(new TestCollectionStarting
		{
			AssemblyUniqueID = TestAssemblyUniqueID,
			TestCollectionClassName = null,
			TestCollectionDisplayName = testCollectionDisplayName,
			TestCollectionUniqueID = testCollectionUniqueID,
			Traits = EmptyV3Traits,
		});

		foreach (var testCasesByClass in testCases.WhereNotNull().GroupBy(tc => tc.TestMethod?.TestClass.Class.Name))
		{
			var testClassUniqueID = UniqueIDGenerator.ForTestClass(testCollectionUniqueID, testCasesByClass.Key);
			var classTestCases = testCasesByClass.ToArray();

			if (testCasesByClass.Key is not null)
			{
				var typeName = testCasesByClass.Key;
				var lastDotIdx = typeName.LastIndexOf('.');
				var @namespace = lastDotIdx > -1 ? typeName.Substring(0, lastDotIdx) : null;
				var simpleName = lastDotIdx > -1 ? typeName.Substring(lastDotIdx + 1) : typeName;

				messageSink.OnMessage(new TestClassStarting
				{
					AssemblyUniqueID = TestAssemblyUniqueID,
					TestClassName = typeName,
					TestClassNamespace = @namespace,
					TestClassSimpleName = simpleName,
					TestClassUniqueID = testClassUniqueID,
					TestCollectionUniqueID = testCollectionUniqueID,
					Traits = EmptyV3Traits,
				});
			}

			foreach (var testCasesByMethod in classTestCases.GroupBy(tc => tc.TestMethod?.Method.Name))
			{
				var testMethodUniqueID = UniqueIDGenerator.ForTestMethod(testClassUniqueID, testCasesByMethod.Key);
				var methodTestCases = testCasesByMethod.ToArray();

				if (testCasesByMethod.Key is not null)
					messageSink.OnMessage(new TestMethodStarting
					{
						AssemblyUniqueID = TestAssemblyUniqueID,
						MethodName = testCasesByMethod.Key,
						TestClassUniqueID = testClassUniqueID,
						TestCollectionUniqueID = testCollectionUniqueID,
						TestMethodUniqueID = testMethodUniqueID,
						Traits = EmptyV3Traits,
					});

				var currentTestIdx = 0;

				foreach (var testCase in methodTestCases)
				{
					var typeName = testCasesByClass.Key;
					var lastDotIdx = typeName?.LastIndexOf('.') ?? -1;
					var @namespace = typeName is not null && lastDotIdx > -1 ? typeName.Substring(0, lastDotIdx) : null;
					var simpleName = typeName is not null && lastDotIdx > -1 ? typeName.Substring(lastDotIdx + 1) : typeName;
					var testCaseTraits = testCase.Traits.ToReadOnly();

					messageSink.OnMessage(new TestCaseStarting
					{
						AssemblyUniqueID = TestAssemblyUniqueID,
						Explicit = false,
						SkipReason = null,
						SourceFilePath = null,
						SourceLineNumber = null,
						TestCaseDisplayName = testCase.DisplayName,
						TestCaseUniqueID = testCase.UniqueID,
						TestClassMetadataToken = null,
						TestClassName = typeName,
						TestClassNamespace = @namespace,
						TestClassSimpleName = simpleName,
						TestClassUniqueID = testClassUniqueID,
						TestCollectionUniqueID = testCollectionUniqueID,
						TestMethodMetadataToken = null,
						TestMethodName = testCasesByMethod.Key,
						TestMethodParameterTypesVSTest = null,
						TestMethodReturnTypeVSTest = null,
						TestMethodUniqueID = testMethodUniqueID,
						Traits = testCaseTraits,
					});

					var testUniqueID = UniqueIDGenerator.ForTest(testCase.UniqueID, currentTestIdx++);
					var now = DateTimeOffset.UtcNow;

					messageSink.OnMessage(new TestStarting
					{
						AssemblyUniqueID = TestAssemblyUniqueID,
						Explicit = false,
						StartTime = now,
						TestCaseUniqueID = testCase.UniqueID,
						TestClassUniqueID = testClassUniqueID,
						TestCollectionUniqueID = testCollectionUniqueID,
						TestDisplayName = testCase.DisplayName,
						TestMethodUniqueID = testMethodUniqueID,
						TestUniqueID = testUniqueID,
						Timeout = 0,
						Traits = testCaseTraits,
					});

					messageSink.OnMessage(new TestNotRun
					{
						AssemblyUniqueID = TestAssemblyUniqueID,
						ExecutionTime = 0m,
						FinishTime = now,
						Output = "",
						TestCaseUniqueID = testCase.UniqueID,
						TestClassUniqueID = testClassUniqueID,
						TestCollectionUniqueID = testCollectionUniqueID,
						TestMethodUniqueID = testMethodUniqueID,
						TestUniqueID = testUniqueID,
						Warnings = null,
					});

					messageSink.OnMessage(new TestFinished
					{
						AssemblyUniqueID = TestAssemblyUniqueID,
						Attachments = EmptyAttachments,
						ExecutionTime = 0m,
						FinishTime = now,
						Output = "",
						TestCaseUniqueID = testCase.UniqueID,
						TestClassUniqueID = testClassUniqueID,
						TestCollectionUniqueID = testCollectionUniqueID,
						TestMethodUniqueID = testMethodUniqueID,
						TestUniqueID = testUniqueID,
						Warnings = null,
					});

					messageSink.OnMessage(new TestCaseFinished
					{
						AssemblyUniqueID = TestAssemblyUniqueID,
						ExecutionTime = 0m,
						TestCaseUniqueID = testCase.UniqueID,
						TestClassUniqueID = testClassUniqueID,
						TestCollectionUniqueID = testCollectionUniqueID,
						TestMethodUniqueID = testMethodUniqueID,
						TestsFailed = 0,
						TestsNotRun = 1,
						TestsSkipped = 0,
						TestsTotal = 1,
					});
				}

				if (testCasesByMethod.Key is not null)
					messageSink.OnMessage(new TestMethodFinished
					{
						AssemblyUniqueID = TestAssemblyUniqueID,
						ExecutionTime = 0m,
						TestClassUniqueID = testClassUniqueID,
						TestCollectionUniqueID = testCollectionUniqueID,
						TestMethodUniqueID = testMethodUniqueID,
						TestsFailed = 0,
						TestsNotRun = methodTestCases.Length,
						TestsSkipped = 0,
						TestsTotal = methodTestCases.Length,
					});
			}

			if (testCasesByClass.Key is not null)
				messageSink.OnMessage(new TestClassFinished
				{
					AssemblyUniqueID = TestAssemblyUniqueID,
					ExecutionTime = 0m,
					TestClassUniqueID = testClassUniqueID,
					TestCollectionUniqueID = testCollectionUniqueID,
					TestsFailed = 0,
					TestsNotRun = classTestCases.Length,
					TestsSkipped = 0,
					TestsTotal = classTestCases.Length,
				});
		}

		messageSink.OnMessage(new TestCollectionFinished
		{
			AssemblyUniqueID = TestAssemblyUniqueID,
			ExecutionTime = 0m,
			TestCollectionUniqueID = testCollectionUniqueID,
			TestsFailed = 0,
			TestsNotRun = testCases.Count,
			TestsSkipped = 0,
			TestsTotal = testCases.Count,
		});

		messageSink.OnMessage(new TestAssemblyFinished
		{
			AssemblyUniqueID = TestAssemblyUniqueID,
			ExecutionTime = 0m,
			FinishTime = DateTimeOffset.Now,
			TestsFailed = 0,
			TestsNotRun = testCases.Count,
			TestsSkipped = 0,
			TestsTotal = testCases.Count
		});
	}

	/// <inheritdoc/>
	public void Run(
		Sdk.IMessageSink messageSink,
		FrontControllerRunSettings settings)
	{
		Guard.NotNull(() => string.Format(CultureInfo.CurrentCulture, "This instance of {0} was created for discovery only; execution-related operations cannot be performed.", typeof(Xunit2).FullName), remoteExecutor);

		Guard.ArgumentNotNull(messageSink);
		Guard.ArgumentNotNull(settings);

		var testCases = BulkDeserialize(settings.SerializedTestCases.ToList()).Select(kvp => kvp.Value).ToList();

		if (settings.Options.GetExplicitOptionOrDefault() == ExplicitOption.Only)
			ReportTestCasesAsNotRun(testCases, messageSink);
		else
			remoteExecutor.RunTests(
				testCases,
				CreateOptimizedRemoteMessageSink(messageSink),
				Xunit2OptionsAdapter.Adapt(settings.Options)
			);
	}

	void SendDiscoveryStartingMessage(Sdk.IMessageSink messageSink)
	{
		// There is no v2 equivalent to this, so we manufacture it ourselves
		var discoveryStarting = new DiscoveryStarting
		{
			AssemblyName = assemblyInfo.Name,
			AssemblyPath = assemblyInfo.AssemblyPath,
			AssemblyUniqueID = UniqueIDGenerator.ForAssembly(assemblyInfo.AssemblyPath, configFileName),
			ConfigFilePath = configFileName,
		};

		messageSink.OnMessage(discoveryStarting);
	}

	void SendDiscoveryCompleteMessage(
		Sdk.IMessageSink messageSink,
		int testCasesToRun)
	{
		// We optimize discovery when filtering by class, so we filter out discovery complete
		// messages, and need to send a single one when we're finished.
		var discoveryComplete = new DiscoveryComplete
		{
			AssemblyUniqueID = UniqueIDGenerator.ForAssembly(assemblyInfo.AssemblyPath, configFileName),
			TestCasesToRun = testCasesToRun,
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
	/// <param name="sourceInformationProvider">The optional source information provider.</param>
	/// <param name="diagnosticMessageSink">The message sink which receives <see cref="Sdk.IDiagnosticMessage"/>
	/// and <see cref="IInternalDiagnosticMessage"/> messages.</param>
	/// <param name="verifyAssembliesOnDisk">Determines whether or not to check for the existence of assembly files.</param>
	public static IFrontControllerDiscoverer ForDiscovery(
		IAssemblyInfo assemblyInfo,
		XunitProjectAssembly projectAssembly,
		Common.ISourceInformationProvider? sourceInformationProvider = null,
		Sdk.IMessageSink? diagnosticMessageSink = null,
		bool verifyAssembliesOnDisk = true)
	{
		Guard.ArgumentNotNull(assemblyInfo);
		Guard.ArgumentNotNull(projectAssembly);

		var appDomainSupport = projectAssembly.Configuration.AppDomainOrDefault;

		return new Xunit2(
			diagnosticMessageSink ?? NullMessageSink.Instance,
			appDomainSupport,
			sourceInformationProvider,
			assemblyInfo,
			assemblyFileName: null,
			GetXunitExecutionAssemblyPath(appDomainSupport, assemblyInfo),
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
	/// <param name="diagnosticMessageSink">The message sink which receives <see cref="Sdk.IDiagnosticMessage"/>
	/// and <see cref="IInternalDiagnosticMessage"/> messages.</param>
	/// <param name="verifyAssembliesOnDisk">Determines whether or not to check for the existence of assembly files.</param>
	public static IFrontController ForDiscoveryAndExecution(
		XunitProjectAssembly projectAssembly,
		Common.ISourceInformationProvider? sourceInformationProvider = null,
		Sdk.IMessageSink? diagnosticMessageSink = null,
		bool verifyAssembliesOnDisk = true)
	{
		Guard.ArgumentNotNull(projectAssembly);

		var appDomainSupport = projectAssembly.Configuration.AppDomainOrDefault;
		var assemblyFileName = Guard.ArgumentNotNull(projectAssembly.AssemblyFileName);

		return new Xunit2(
			diagnosticMessageSink ?? NullMessageSink.Instance,
			appDomainSupport,
			sourceInformationProvider,
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

	sealed class DeserializeCallback : MarshalByRefObject
	{
		public List<KeyValuePair<string?, Abstractions.ITestCase?>>? Results;

		public void Callback(List<KeyValuePair<string?, Abstractions.ITestCase?>> results) => Results = results;

#if NETFRAMEWORK
		/// <inheritdoc/>
		[System.Security.SecurityCritical]
		public sealed override object InitializeLifetimeService() => null!;
#endif
	}

	// This message sink filters out _DiscoveryComplete (to let us run multiple discoveries at once) as well
	// as only reporting discovered test cases which pass the filter.
	sealed class FilteringMessageSink(
		Sdk.IMessageSink innerMessageSink,
		Predicate<ITestCaseDiscovered> filter,
		Action<ITestCaseDiscovered>? discoveryCallback = null) :
			Sdk.IMessageSink, IDisposable
	{
		readonly Action<ITestCaseDiscovered>? discoveryCallback = discoveryCallback;
		readonly Predicate<ITestCaseDiscovered> filter = filter;
		readonly Sdk.IMessageSink innerMessageSink = innerMessageSink;
		volatile int testCasesToRun;

		public AutoResetEvent Finished { get; } = new AutoResetEvent(initialState: false);

		public int TestCasesToRun =>
			testCasesToRun;

		public void Dispose() =>
			Finished.Dispose();

		public bool OnMessage(Sdk.IMessageSinkMessage message)
		{
			// Filter out discovery complete (and make it an event) so we can run multiple discoveries
			// while reporting a single complete message after they're all done
			if (message is IDiscoveryComplete)
			{
				Finished.Set();
				return true;
			}

			if (message is ITestCaseDiscovered discovered)
			{
				if (!filter(discovered))
					return true;

				Interlocked.Increment(ref testCasesToRun);

				if (discoveryCallback is not null)
					discoveryCallback(discovered);
			}

			return innerMessageSink.OnMessage(message);
		}
	}
}
