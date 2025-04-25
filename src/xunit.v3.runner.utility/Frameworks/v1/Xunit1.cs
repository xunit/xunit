#if NETFRAMEWORK

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using Xunit.Internal;
using Xunit.Runner.Common;
using Xunit.Sdk;

namespace Xunit.Runner.v1;

/// <summary>
/// This class is used to do discovery and execution of xUnit.net v1 tests.
/// Runner authors are strongly encouraged to use <see cref="XunitFrontController"/>
/// instead of using this class directly.
/// </summary>
public class Xunit1 : IFrontController
{
	internal static IReadOnlyDictionary<string, IReadOnlyCollection<string>> EmptyV3Traits = new Dictionary<string, IReadOnlyCollection<string>>();

	readonly AppDomainSupport appDomainSupport;
	readonly string assemblyFileName;
	readonly string? configFileName;
	readonly IMessageSink diagnosticMessageSink;
	readonly DisposalTracker disposalTracker = new();
	bool disposed;
	IXunit1Executor? executor;
	readonly bool shadowCopy;
	readonly string? shadowCopyFolder;
#pragma warning disable CA2213  // This is disposed by DisposalTracker when appropriate
	readonly ISourceInformationProvider sourceInformationProvider;
#pragma warning restore CA2213
	readonly string testAssemblyName;

	/// <summary>
	/// This constructor is used by factory methods and unit tests only.
	/// </summary>
	protected Xunit1(
		IMessageSink diagnosticMessageSink,
		AppDomainSupport appDomainSupport,
		ISourceInformationProvider? sourceInformationProvider,
		string assemblyFileName,
		string? configFileName = null,
		bool shadowCopy = true,
		string? shadowCopyFolder = null)
	{
		Guard.ArgumentNotNullOrEmpty(assemblyFileName);

		if (sourceInformationProvider is null)
		{
			sourceInformationProvider = CecilSourceInformationProvider.Create(assemblyFileName);
			disposalTracker.Add(sourceInformationProvider);
		}

		this.diagnosticMessageSink = diagnosticMessageSink;
		this.appDomainSupport = appDomainSupport;
		this.sourceInformationProvider = sourceInformationProvider;
		this.assemblyFileName = assemblyFileName;
		this.configFileName = configFileName;
		this.shadowCopy = shadowCopy;
		this.shadowCopyFolder = shadowCopyFolder;

		testAssemblyName = Path.GetFileNameWithoutExtension(assemblyFileName);
		TestAssemblyUniqueID = string.Format(CultureInfo.InvariantCulture, ":v1:assembly:{0}:{1}", assemblyFileName, configFileName ?? "(null)");
	}

	/// <inheritdoc/>
	public bool CanUseAppDomains => true;

	IXunit1Executor Executor =>
		executor ??= CreateExecutor();

	/// <inheritdoc/>
	// This is not supported with v1, since there is no code in the remote AppDomain
	// that would give us this information.
	public string TargetFramework => string.Empty;

	/// <inheritdoc/>
	public string TestAssemblyUniqueID { get; }

	/// <inheritdoc/>
	public string TestFrameworkDisplayName => Executor.TestFrameworkDisplayName;

	/// <summary>
	/// Creates a wrapper to call the Executor call from xUnit.net v1.
	/// </summary>
	/// <returns>The executor wrapper.</returns>
	protected virtual IXunit1Executor CreateExecutor() =>
		new Xunit1Executor(diagnosticMessageSink, appDomainSupport != AppDomainSupport.Denied, assemblyFileName, configFileName, shadowCopy, shadowCopyFolder);

	/// <inheritdoc/>
	public async ValueTask DisposeAsync()
	{
		if (disposed)
			return;

		disposed = true;

		GC.SuppressFinalize(this);

		await disposalTracker.SafeDisposeAsync();
		executor?.SafeDispose();
	}

	/// <summary>
	/// INTERNAL METHOD, FOR TESTING PURPOSES ONLY. DO NOT CALL.
	/// </summary>
	protected void Find(
		IMessageSink messageSink,
		bool includeSourceInformation,
		Predicate<ITestCaseDiscovered>? filter)
	{
		Guard.ArgumentNotNull(messageSink);

		ThreadPool.QueueUserWorkItem(_ =>
		{
			var testCasesToRun = 0;

			Find(
				messageSink,
				includeSourceInformation,
				testCase =>
				{
					var msg = testCase.ToTestCaseDiscovered(includeSerialization: true);
					if (filter is null || filter(msg))
					{
						messageSink.OnMessage(msg);
						++testCasesToRun;
					}
				}
			);

			var discoveryComplete = new DiscoveryComplete
			{
				AssemblyUniqueID = TestAssemblyUniqueID,
				TestCasesToRun = testCasesToRun,
			};
			messageSink.OnMessage(discoveryComplete);
		});
	}

	/// <inheritdoc/>
	public void Find(
		IMessageSink messageSink,
		FrontControllerFindSettings settings)
	{
		Guard.ArgumentNotNull(messageSink);
		Guard.ArgumentNotNull(settings);

		Find(
			messageSink,
			settings.Options.GetIncludeSourceInformationOrDefault(),
			settings.Filters.Empty ? null : testCase => settings.Filters.Filter(testAssemblyName, testCase)
		);
	}

	void Find(
		IMessageSink messageSink,
		bool includeSourceInformation,
		Action<Xunit1TestCase> callback)
	{
		var discoveryStarting = new DiscoveryStarting
		{
			AssemblyName = testAssemblyName,
			AssemblyPath = assemblyFileName,
			AssemblyUniqueID = TestAssemblyUniqueID,
			ConfigFilePath = configFileName,
		};
		messageSink.OnMessage(discoveryStarting);

		XmlNode? assemblyXml = null;

		var handler = new XmlNodeCallbackHandler(xml => { assemblyXml = xml; return true; });
		Executor.EnumerateTests(handler);

#pragma warning disable CA1508 // This is incorrectly detected as a dead condition; the callback handler above can set the value during EnumerateTests
		if (assemblyXml is not null)
#pragma warning restore CA1508
		{
			var methodNodes = assemblyXml.SelectNodes("//method")?.Cast<XmlNode>();
			if (methodNodes is not null)
			{
				foreach (var methodXml in methodNodes)
				{
					var typeName = methodXml.Attributes?["type"]?.Value;
					var methodName = methodXml.Attributes?["method"]?.Value;
					if (typeName is null || methodName is null)
						continue;

					string? displayName = null;
					var displayNameAttribute = methodXml.Attributes?["name"];
					if (displayNameAttribute is not null)
						displayName = displayNameAttribute.Value;

					string? skipReason = null;
					var skipReasonAttribute = methodXml.Attributes?["skip"];
					if (skipReasonAttribute is not null)
						skipReason = skipReasonAttribute.Value;

					var traits = new Dictionary<string, HashSet<string>>(StringComparer.OrdinalIgnoreCase);
					var traitNodes = methodXml.SelectNodes("traits/trait")?.Cast<XmlNode>();
					if (traitNodes is not null)
						foreach (var traitNode in traitNodes)
						{
							var traitName = traitNode.Attributes?["name"]?.Value;
							var traitValue = traitNode.Attributes?["value"]?.Value;

							if (traitName is not null && traitValue is not null)
								traits.Add(traitName, traitValue);
						}

					var sourceInformation = default(SourceInformation);
					if (includeSourceInformation)
						sourceInformation = sourceInformationProvider.GetSourceInformation(typeName, methodName);

					var testCase = new Xunit1TestCase
					{
						AssemblyUniqueID = TestAssemblyUniqueID,
						SkipReason = skipReason,
						SourceFilePath = sourceInformation.SourceFile,
						SourceLineNumber = sourceInformation.SourceLine,
						TestCaseDisplayName = displayName ?? string.Format(CultureInfo.InvariantCulture, "{0}.{1}", typeName, methodName),
						TestCaseUniqueID = string.Format(CultureInfo.InvariantCulture, ":v1:case:{0}.{1}:{2}:{3}", typeName, methodName, assemblyFileName, configFileName ?? "(null)"),
						TestClass = typeName,
						TestClassUniqueID = string.Format(CultureInfo.InvariantCulture, ":v1:class:{0}:{1}:{2}", typeName, assemblyFileName, configFileName ?? "(null)"),
						TestCollectionUniqueID = string.Format(CultureInfo.InvariantCulture, ":v1:collection:{0}:{1}", assemblyFileName, configFileName ?? "(null)"),
						TestMethod = methodName,
						TestMethodUniqueID = string.Format(CultureInfo.InvariantCulture, ":v1:method:{0}.{1}:{2}:{3}", typeName, methodName, assemblyFileName, configFileName ?? "(null)"),
						Traits = traits.ToReadOnly()
					};

					callback(testCase);
				}
			}
		}
	}

	/// <summary>
	/// INTERNAL METHOD, FOR TESTING PURPOSES ONLY. DO NOT CALL.
	/// </summary>
	protected void FindAndRun(
		IMessageSink messageSink,
		bool includeSourceInformation,
		Predicate<ITestCaseDiscovered>? filter,
		bool markAllAsNotRun)
	{
		Guard.ArgumentNotNull(messageSink);

		ThreadPool.QueueUserWorkItem(_ =>
		{
			var testCases = new List<Xunit1TestCase>();

			Find(
				messageSink,
				includeSourceInformation,
				testCase =>
				{
					var include = true;

					if (filter is not null)
					{
						var msg = testCase.ToTestCaseDiscovered(includeSerialization: false);
						include = filter(msg);
					}

					if (include)
						testCases.Add(testCase);
				}
			);

			var discoveryComplete = new DiscoveryComplete
			{
				AssemblyUniqueID = TestAssemblyUniqueID,
				TestCasesToRun = testCases.Count,
			};
			messageSink.OnMessage(discoveryComplete);

			Run(testCases, messageSink, markAllAsNotRun);
		});
	}

	/// <inheritdoc/>
	public void FindAndRun(
		IMessageSink messageSink,
		FrontControllerFindAndRunSettings settings)
	{
		Guard.ArgumentNotNull(messageSink);
		Guard.ArgumentNotNull(settings);

		// Pass null for empty filter list, since it bypasses _TestCaseDiscovered creation
		FindAndRun(
			messageSink,
			settings.DiscoveryOptions.GetIncludeSourceInformationOrDefault(),
			settings.Filters.Empty ? null : testCase => settings.Filters.Filter(testAssemblyName, testCase),
			settings.ExecutionOptions.GetExplicitOptionOrDefault() == ExplicitOption.Only
		);
	}

	/// <summary>
	/// INTERNAL METHOD, FOR TESTING PURPOSES ONLY. DO NOT CALL.
	/// </summary>
	protected void Run(
		IReadOnlyCollection<Xunit1TestCase> testCases,
		IMessageSink messageSink,
		bool markAllAsNotRun)
	{
		Guard.ArgumentNotNull(testCases);
		Guard.ArgumentNotNull(messageSink);

		var results = new Xunit1RunSummary();
		var environment = string.Format(CultureInfo.CurrentCulture, "{0}-bit .NET {1}", IntPtr.Size * 8, Environment.Version);
		var testCasesList = testCases.ToList();

		var testAssemblyStartingMessage = new TestAssemblyStarting
		{
			AssemblyName = testAssemblyName,
			AssemblyPath = assemblyFileName,
			AssemblyUniqueID = TestAssemblyUniqueID,
			ConfigFilePath = configFileName,
			Seed = null,
			StartTime = DateTimeOffset.Now,
			TestEnvironment = environment,
			TestFrameworkDisplayName = TestFrameworkDisplayName,
			TargetFramework = null,
			Traits = EmptyV3Traits,
		};

		if (messageSink.OnMessage(testAssemblyStartingMessage))
		{
			try
			{
				if (testCasesList.Count != 0)
					results = RunTestCollection(testCasesList, messageSink, markAllAsNotRun);
			}
			catch (Exception ex)
			{
				var (exceptionTypes, messages, stackTraces, exceptionParentIndices) = Xunit1ExceptionUtility.ConvertToErrorMetadata(ex);
				var errorMessage = new ErrorMessage
				{
					ExceptionParentIndices = exceptionParentIndices,
					ExceptionTypes = exceptionTypes,
					Messages = messages,
					StackTraces = stackTraces
				};
				messageSink.OnMessage(errorMessage);
			}
			finally
			{
				var assemblyFinished = new TestAssemblyFinished
				{
					AssemblyUniqueID = testAssemblyStartingMessage.AssemblyUniqueID,
					ExecutionTime = results.Time,
					FinishTime = DateTimeOffset.Now,
					TestsFailed = results.Failed,
					TestsNotRun = results.NotRun,
					TestsTotal = results.Total,
					TestsSkipped = results.Skipped
				};

				messageSink.OnMessage(assemblyFinished);
			}
		}
	}

	/// <inheritdoc/>
	public void Run(
		IMessageSink messageSink,
		FrontControllerRunSettings settings)
	{
		Guard.ArgumentNotNull(messageSink);
		Guard.ArgumentNotNull(settings);

		var testCases =
			settings
				.SerializedTestCases
				.Select(SerializationHelper.Instance.Deserialize<Xunit1TestCase>)
				.WhereNotNull()
				.CastOrToReadOnlyCollection();

		Run(testCases, messageSink, settings.Options.GetExplicitOptionOrDefault() == ExplicitOption.Only);
	}

	Xunit1RunSummary RunTestCollection(
		IList<Xunit1TestCase> testCases,
		IMessageSink messageSink,
		bool markAllAsNotRun)
	{
		Guard.ArgumentValid("testCases must contain at least one test case", testCases.Count > 0, nameof(testCases));

		var collectionStarting = new TestCollectionStarting
		{
			AssemblyUniqueID = testCases[0].AssemblyUniqueID,
			TestCollectionClassName = null,
			TestCollectionDisplayName = string.Format(CultureInfo.CurrentCulture, "xUnit.net v1 Tests for {0}", assemblyFileName),
			TestCollectionUniqueID = testCases[0].TestCollectionUniqueID,
			Traits = EmptyV3Traits,
		};

		var results = new Xunit1RunSummary
		{
			Continue = messageSink.OnMessage(collectionStarting)
		};

		try
		{
			if (results.Continue)
				foreach (var testClassGroup in testCases.GroupBy(tc => tc.TestClass))
				{
					var classResults = RunTestClass(testClassGroup.Key, testClassGroup.ToList(), messageSink, markAllAsNotRun);
					results.Aggregate(classResults);
					if (!classResults.Continue)
						break;
				}
		}
		finally
		{
			var collectionFinished = new TestCollectionFinished
			{
				AssemblyUniqueID = collectionStarting.AssemblyUniqueID,
				ExecutionTime = results.Time,
				TestCollectionUniqueID = collectionStarting.TestCollectionUniqueID,
				TestsFailed = results.Failed,
				TestsNotRun = results.NotRun,
				TestsTotal = results.Total,
				TestsSkipped = results.Skipped
			};

			results.Continue = messageSink.OnMessage(collectionFinished) && results.Continue;
		}

		return results;
	}

	Xunit1RunSummary RunTestClass(
		string typeName,
		IList<Xunit1TestCase> testCases,
		IMessageSink messageSink,
		bool markAllAsNotRun)
	{
		Guard.ArgumentValid("testCases must contain at least one test case", testCases.Count > 0, nameof(testCases));

		var lastDotIdx = typeName.LastIndexOf('.');
		var @namespace = lastDotIdx > -1 ? typeName.Substring(0, lastDotIdx) : null;
		var simpleName = lastDotIdx > -1 ? typeName.Substring(lastDotIdx + 1) : typeName;

		var handler = new TestClassCallbackHandler(testCases, messageSink);
		var results = handler.TestClassResults;
		var testClassStarting = new TestClassStarting
		{
			AssemblyUniqueID = testCases[0].AssemblyUniqueID,
			TestClassName = typeName,
			TestClassNamespace = @namespace,
			TestClassSimpleName = simpleName,
			TestClassUniqueID = testCases[0].TestClassUniqueID,
			TestCollectionUniqueID = testCases[0].TestCollectionUniqueID,
			Traits = EmptyV3Traits,
		};

		results.Continue = messageSink.OnMessage(testClassStarting);

		try
		{
			if (results.Continue)
			{
				if (markAllAsNotRun)
				{
					foreach (var testCase in testCases)
					{
						results.NotRun++;
						results.Total++;

						messageSink.OnMessage(testCase.ToTestMethodStarting());
						messageSink.OnMessage(testCase.ToTestCaseStarting());
						messageSink.OnMessage(testCase.ToTestStarting(testCase.TestClass, -1));  // We don't know the test display name because it's on the other side of the app domain
						messageSink.OnMessage(testCase.ToTestNotRun(-1));
						messageSink.OnMessage(testCase.ToTestFinishedNotRun(-1));
						messageSink.OnMessage(testCase.ToTestCaseFinishedNotRun());
						messageSink.OnMessage(testCase.ToTestMethodFinishedNotRun());
					}
				}
				else
				{
					var methodNames = testCases.Select(tc => tc.TestMethod).ToList();
					Executor.RunTests(typeName, methodNames, handler);
					handler.LastNodeArrived.WaitOne();
				}
			}
		}
		finally
		{
			var testClassFinished = new TestClassFinished
			{
				AssemblyUniqueID = testClassStarting.AssemblyUniqueID,
				ExecutionTime = results.Time,
				TestClassUniqueID = testClassStarting.TestClassUniqueID,
				TestCollectionUniqueID = testClassStarting.TestCollectionUniqueID,
				TestsFailed = results.Failed,
				TestsNotRun = results.NotRun,
				TestsTotal = results.Total,
				TestsSkipped = results.Skipped
			};

			results.Continue = messageSink.OnMessage(testClassFinished) && results.Continue;
		}

		return results;
	}

	// Factory method

	/// <summary>
	/// Returns an implementation of <see cref="IFrontController"/> which can be used
	/// for both discovery and execution of xUnit.net v1 tests.
	/// </summary>
	/// <param name="projectAssembly">The test project assembly.</param>
	/// <param name="sourceInformationProvider">The optional source information provider.</param>
	/// <param name="diagnosticMessageSink">The optional message sink which receives <see cref="IDiagnosticMessage"/>
	/// and <see cref="IInternalDiagnosticMessage"/> messages.</param>
	public static IFrontController ForDiscoveryAndExecution(
		XunitProjectAssembly projectAssembly,
		ISourceInformationProvider? sourceInformationProvider = null,
		IMessageSink? diagnosticMessageSink = null)
	{
		Guard.ArgumentNotNull(projectAssembly);
		var assemblyFileName = Guard.ArgumentNotNull(projectAssembly.AssemblyFileName);

		return new Xunit1(
			diagnosticMessageSink ?? NullMessageSink.Instance,
			projectAssembly.Configuration.AppDomainOrDefault,
			sourceInformationProvider,
			assemblyFileName,
			projectAssembly.ConfigFileName,
			projectAssembly.Configuration.ShadowCopyOrDefault,
			projectAssembly.Configuration.ShadowCopyFolder
		);
	}
}

#endif
