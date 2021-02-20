#if NETFRAMEWORK

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Xml;
using Xunit.Internal;
using Xunit.Runner.Common;
using Xunit.Sdk;
using Xunit.v3;

namespace Xunit.Runner.v1
{
	/// <summary>
	/// This class be used to do discovery and execution of xUnit.net v1 tests
	/// using a reflection-based implementation of <see cref="_IAssemblyInfo"/>.
	/// Runner authors are strongly encouraged to use <see cref="XunitFrontController"/>
	/// instead of using this class directly.
	/// </summary>
	public class Xunit1 : IFrontController, IAsyncDisposable
	{
		readonly AppDomainSupport appDomainSupport;
		readonly string assemblyFileName;
		readonly string? configFileName;
		readonly _IMessageSink diagnosticMessageSink;
		readonly DisposalTracker disposalTracker = new DisposalTracker();
		bool disposed;
		IXunit1Executor? executor;
		readonly bool shadowCopy;
		readonly string? shadowCopyFolder;
		readonly _ISourceInformationProvider sourceInformationProvider;
		readonly string testAssemblyName;
		readonly string testAssemblyUniqueID;

		/// <summary>
		/// Initializes a new instance of the <see cref="Xunit1"/> class.
		/// </summary>
		/// <param name="diagnosticMessageSink">The message sink which receives <see cref="_DiagnosticMessage"/> messages.</param>
		/// <param name="appDomainSupport">Determines whether tests should be run in a separate app domain.</param>
		/// <param name="sourceInformationProvider">Source code information provider.</param>
		/// <param name="assemblyFileName">The test assembly.</param>
		/// <param name="configFileName">The test assembly configuration file.</param>
		/// <param name="shadowCopy">If set to <c>true</c>, runs tests in a shadow copied app domain, which allows
		/// tests to be discovered and run without locking assembly files on disk.</param>
		/// <param name="shadowCopyFolder">The path on disk to use for shadow copying; if <c>null</c>, a folder
		/// will be automatically (randomly) generated</param>
		public Xunit1(
			_IMessageSink diagnosticMessageSink,
			AppDomainSupport appDomainSupport,
			_ISourceInformationProvider sourceInformationProvider,
			string assemblyFileName,
			string? configFileName = null,
			bool shadowCopy = true,
			string? shadowCopyFolder = null)
		{
			Guard.ArgumentNotNull(nameof(sourceInformationProvider), sourceInformationProvider);
			Guard.ArgumentNotNullOrEmpty(nameof(assemblyFileName), assemblyFileName);

			this.diagnosticMessageSink = diagnosticMessageSink;
			this.appDomainSupport = appDomainSupport;
			this.sourceInformationProvider = sourceInformationProvider;
			this.assemblyFileName = assemblyFileName;
			this.configFileName = configFileName;
			this.shadowCopy = shadowCopy;
			this.shadowCopyFolder = shadowCopyFolder;

			testAssemblyName = Path.GetFileNameWithoutExtension(assemblyFileName);
			testAssemblyUniqueID = $":v1:assembly:{assemblyFileName}:{configFileName ?? "(null)"}";
		}

		/// <inheritdoc/>
		public bool CanUseAppDomains => true;

		IXunit1Executor Executor
		{
			get
			{
				if (executor == null)
					executor = CreateExecutor();

				return executor;
			}
		}

		/// <inheritdoc/>
		// This is not supported with v1, since there is no code in the remote AppDomain
		// that would give us this information.
		public string TargetFramework => string.Empty;

		/// <inheritdoc/>
		string IFrontController.TestAssemblyUniqueID => testAssemblyUniqueID;

		/// <inheritdoc/>
		public string TestFrameworkDisplayName => Executor.TestFrameworkDisplayName;

		/// <summary>
		/// Creates a wrapper to call the Executor call from xUnit.net v1.
		/// </summary>
		/// <returns>The executor wrapper.</returns>
		protected virtual IXunit1Executor CreateExecutor() =>
			new Xunit1Executor(diagnosticMessageSink, appDomainSupport != AppDomainSupport.Denied, assemblyFileName, configFileName, shadowCopy, shadowCopyFolder);

		/// <inheritdoc/>
		public Xunit1TestCase? Deserialize(string value)
		{
			Guard.ArgumentNotNull(nameof(value), value);

			return SerializationHelper.Deserialize<Xunit1TestCase>(value);
		}

		/// <inheritdoc/>
		public async ValueTask DisposeAsync()
		{
			if (disposed)
				throw new ObjectDisposedException(GetType().FullName);

			disposed = true;

			await disposalTracker.DisposeAsync();
			executor?.Dispose();
		}

		/// <summary>
		/// Starts the process of finding all xUnit.net v1 tests in an assembly.
		/// </summary>
		/// <param name="includeSourceInformation">Whether to include source file information, if possible.</param>
		/// <param name="messageSink">The message sink to report results back to.</param>
		/// <param name="filters">The optional filters used to filter discovery messages.</param>
		public void Find(
			bool includeSourceInformation,
			_IMessageSink messageSink,
			XunitFilters? filters = null)
		{
			Guard.ArgumentNotNull(nameof(messageSink), messageSink);

			Predicate<_TestCaseDiscovered> filter = filters != null ? filters.Filter : msg => true;

			Find(filter, includeSourceInformation, messageSink);
		}

		/// <summary>
		/// Starts the process of finding all xUnit.net v1 tests in a class.
		/// </summary>
		/// <param name="typeName">The fully qualified type name to find tests in.</param>
		/// <param name="includeSourceInformation">Whether to include source file information, if possible.</param>
		/// <param name="messageSink">The message sink to report results back to.</param>
		public void Find(
			string typeName,
			bool includeSourceInformation,
			_IMessageSink messageSink)
		{
			Guard.ArgumentNotNullOrEmpty(nameof(typeName), typeName);
			Guard.ArgumentNotNull(nameof(messageSink), messageSink);

			Find(
				msg => msg.TestClassWithNamespace == typeName,
				includeSourceInformation,
				messageSink
			);
		}

		/// <inheritdoc/>
		void IFrontController.Find(
			_IMessageSink messageSink,
			FrontControllerDiscoverySettings settings)
		{
			Guard.ArgumentNotNull(nameof(messageSink), messageSink);
			Guard.ArgumentNotNull(nameof(settings), settings);

			Find(
				settings.Filters.Filter,
				settings.Options.GetIncludeSourceInformationOrDefault(),
				messageSink
			);
		}

		void Find(
			Predicate<_TestCaseDiscovered> filter,
			bool includeSourceInformation,
			_IMessageSink messageSink)
		{
			var discoveryStarting = new _DiscoveryStarting
			{
				AssemblyName = testAssemblyName,
				AssemblyPath = assemblyFileName,
				AssemblyUniqueID = testAssemblyUniqueID,
				ConfigFilePath = configFileName
			};
			messageSink.OnMessage(discoveryStarting);

			try
			{
				XmlNode? assemblyXml = null;

				var handler = new XmlNodeCallbackHandler(xml => { assemblyXml = xml; return true; });
				Executor.EnumerateTests(handler);

				if (assemblyXml != null)
				{
					var methodNodes = assemblyXml.SelectNodes("//method")?.Cast<XmlNode>();
					if (methodNodes != null)
					{
						foreach (var methodXml in methodNodes)
						{
							var typeName = methodXml.Attributes?["type"]?.Value;
							var methodName = methodXml.Attributes?["method"]?.Value;
							if (typeName == null || methodName == null)
								continue;

							string? displayName = null;
							var displayNameAttribute = methodXml.Attributes?["name"];
							if (displayNameAttribute != null)
								displayName = displayNameAttribute.Value;

							string? skipReason = null;
							var skipReasonAttribute = methodXml.Attributes?["skip"];
							if (skipReasonAttribute != null)
								skipReason = skipReasonAttribute.Value;

							var traits = new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase);
							var traitNodes = methodXml.SelectNodes("traits/trait")?.Cast<XmlNode>();
							if (traitNodes != null)
								foreach (var traitNode in traitNodes)
								{
									var traitName = traitNode.Attributes?["name"]?.Value;
									var traitValue = traitNode.Attributes?["value"]?.Value;

									if (traitName != null && traitValue != null)
										traits.Add(traitName, traitValue);
								}

							var sourceInformation = default(_ISourceInformation);
							if (includeSourceInformation)
								sourceInformation = sourceInformationProvider.GetSourceInformation(typeName, methodName);

							string? @namespace = null;
							string? @class = null;

							var namespaceIdx = typeName.LastIndexOf('.');
							if (namespaceIdx < 0)
								@class = typeName;
							else
							{
								@namespace = typeName.Substring(0, namespaceIdx);
								@class = typeName.Substring(namespaceIdx + 1);

								var innerClassIdx = @class.LastIndexOf('+');
								if (innerClassIdx >= 0)
									@class = @class.Substring(innerClassIdx + 1);
							}

							var testCase = new Xunit1TestCase
							{
								AssemblyUniqueID = testAssemblyUniqueID,
								SkipReason = skipReason,
								SourceFilePath = sourceInformation?.FileName,
								SourceLineNumber = sourceInformation?.LineNumber,
								TestCaseDisplayName = displayName ?? $"{typeName}.{methodName}",
								TestCaseUniqueID = $":v1:case:{typeName}.{methodName}:{assemblyFileName}:{configFileName ?? "(null)"}",
								TestClass = typeName,
								TestClassUniqueID = $":v1:class:{typeName}:{assemblyFileName}:{configFileName ?? "(null)"}",
								TestCollectionUniqueID = $":v1:collection:{assemblyFileName}:{configFileName ?? "(null)"}",
								TestMethod = methodName,
								TestMethodUniqueID = $":v1:method:{typeName}.{methodName}:{assemblyFileName}:{configFileName ?? "(null)"}",
								Traits = traits
							};

							var message = new _TestCaseDiscovered
							{
								AssemblyUniqueID = testCase.AssemblyUniqueID,
								Serialization = SerializationHelper.Serialize(testCase),
								SkipReason = testCase.SkipReason,
								SourceFilePath = testCase.SourceFilePath,
								SourceLineNumber = testCase.SourceLineNumber,
								TestCaseDisplayName = testCase.TestCaseDisplayName,
								TestCaseUniqueID = testCase.TestCaseUniqueID,
								TestClass = @class,
								TestClassUniqueID = testCase.TestClassUniqueID,
								TestClassWithNamespace = testCase.TestClass,
								TestCollectionUniqueID = testCase.TestCollectionUniqueID,
								TestMethod = testCase.TestMethod,
								TestMethodUniqueID = testCase.TestMethodUniqueID,
								TestNamespace = @namespace,
								Traits = testCase.Traits
							};

							if (filter(message))
								messageSink.OnMessage(message);
						}
					}
				}
			}
			finally
			{
				var discoveryComplete = new _DiscoveryComplete { AssemblyUniqueID = testAssemblyUniqueID };
				messageSink.OnMessage(discoveryComplete);
			}
		}

		/// <summary>
		/// Starts the process of running all the xUnit.net v1 tests in the assembly.
		/// </summary>
		/// <param name="messageSink">The message sink to report results back to.</param>
		public void Run(_IMessageSink messageSink)
		{
			var discoverySink = new TestDiscoverySink();
			disposalTracker.Add(discoverySink);

			Find(includeSourceInformation: false, discoverySink);
			discoverySink.Finished.WaitOne();

			Run(discoverySink.TestCases.Select(tc => tc.Serialization), messageSink);
		}

		void IFrontController.RunAll(
			_IMessageSink messageSink,
			_ITestFrameworkDiscoveryOptions discoveryOptions,
			_ITestFrameworkExecutionOptions executionOptions)
		{
			Run(messageSink);
		}

		/// <summary>
		/// Starts the process of running all the xUnit.net v1 tests.
		/// </summary>
		/// <param name="serializedTestCases">The serialized test cases to run</param>
		/// <param name="messageSink">The message sink to report results back to.</param>
		public void Run(
			IEnumerable<string> serializedTestCases,
			_IMessageSink messageSink)
		{
			var testCases = serializedTestCases.Select(x => Deserialize(x)).WhereNotNull().ToList();
			Run(testCases, messageSink);
		}

		/// <summary>
		/// Starts the process of running all the xUnit.net v1 tests.
		/// </summary>
		/// <param name="testCases">The test cases to run; if null, all tests in the assembly are run.</param>
		/// <param name="messageSink">The message sink to report results back to.</param>
		public void Run(
			IEnumerable<Xunit1TestCase> testCases,
			_IMessageSink messageSink)
		{
			var results = new Xunit1RunSummary();
			var environment = $"{IntPtr.Size * 8}-bit .NET {Environment.Version}";

			var testCasesList = testCases.ToList();

			if (testCasesList.Count != 0)
			{
				var testAssemblyStartingMessage = new _TestAssemblyStarting
				{
					AssemblyName = testAssemblyName,
					AssemblyPath = assemblyFileName,
					AssemblyUniqueID = testCasesList[0].AssemblyUniqueID,
					ConfigFilePath = configFileName,
					StartTime = DateTimeOffset.Now,
					TestEnvironment = environment,
					TestFrameworkDisplayName = TestFrameworkDisplayName,
				};

				try
				{
					if (messageSink.OnMessage(testAssemblyStartingMessage))
						results = RunTestCollection(testCasesList, messageSink);
				}
				catch (Exception ex)
				{
					var errorMetadata = Xunit1ExceptionUtility.ConvertToErrorMetadata(ex);
					var errorMessage = new _ErrorMessage
					{
						ExceptionParentIndices = errorMetadata.ExceptionParentIndices,
						ExceptionTypes = errorMetadata.ExceptionTypes,
						Messages = errorMetadata.Messages,
						StackTraces = errorMetadata.StackTraces
					};
					messageSink.OnMessage(errorMessage);
				}
				finally
				{
					var assemblyFinished = new _TestAssemblyFinished
					{
						AssemblyUniqueID = testAssemblyStartingMessage.AssemblyUniqueID,
						ExecutionTime = results.Time,
						TestsFailed = results.Failed,
						TestsRun = results.Total,
						TestsSkipped = results.Skipped
					};

					messageSink.OnMessage(assemblyFinished);
				}
			}
		}

		void IFrontController.RunTests(
			IEnumerable<string> serializedTestCases,
			_IMessageSink executionMessageSink,
			_ITestFrameworkExecutionOptions executionOptions) =>
				Run(serializedTestCases, executionMessageSink);

		Xunit1RunSummary RunTestCollection(
			IList<Xunit1TestCase> testCases,
			_IMessageSink messageSink)
		{
			Guard.ArgumentValid(nameof(testCases), "testCases must contain at least one test case", testCases.Count > 0);

			var collectionStarting = new _TestCollectionStarting
			{
				AssemblyUniqueID = testCases[0].AssemblyUniqueID,
				TestCollectionClass = null,
				TestCollectionDisplayName = $"xUnit.net v1 Tests for {assemblyFileName}",
				TestCollectionUniqueID = testCases[0].TestCollectionUniqueID
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
						var classResults = RunTestClass(testClassGroup.Key, testClassGroup.ToList(), messageSink);
						results.Aggregate(classResults);
						if (!classResults.Continue)
							break;
					}
			}
			finally
			{
				var collectionFinished = new _TestCollectionFinished
				{
					AssemblyUniqueID = collectionStarting.AssemblyUniqueID,
					ExecutionTime = results.Time,
					TestCollectionUniqueID = collectionStarting.TestCollectionUniqueID,
					TestsFailed = results.Failed,
					TestsRun = results.Total,
					TestsSkipped = results.Skipped
				};

				results.Continue = messageSink.OnMessage(collectionFinished) && results.Continue;
			}

			return results;
		}

		Xunit1RunSummary RunTestClass(
			string typeName,
			IList<Xunit1TestCase> testCases,
			_IMessageSink messageSink)
		{
			Guard.ArgumentValid(nameof(testCases), "testCases must contain at least one test case", testCases.Count > 0);

			var handler = new TestClassCallbackHandler(testCases, messageSink);
			var results = handler.TestClassResults;
			var testClassStarting = new _TestClassStarting
			{
				AssemblyUniqueID = testCases[0].AssemblyUniqueID,
				TestClass = typeName,
				TestClassUniqueID = testCases[0].TestClassUniqueID,
				TestCollectionUniqueID = testCases[0].TestCollectionUniqueID
			};

			results.Continue = messageSink.OnMessage(testClassStarting);

			try
			{
				if (results.Continue)
				{
					var methodNames = testCases.Select(tc => tc.TestMethod).ToList();
					Executor.RunTests(typeName, methodNames, handler);
					handler.LastNodeArrived.WaitOne();
				}
			}
			finally
			{
				var testClassFinished = new _TestClassFinished
				{
					AssemblyUniqueID = testClassStarting.AssemblyUniqueID,
					ExecutionTime = results.Time,
					TestClassUniqueID = testClassStarting.TestClassUniqueID,
					TestCollectionUniqueID = testClassStarting.TestCollectionUniqueID,
					TestsFailed = results.Failed,
					TestsRun = results.Total,
					TestsSkipped = results.Skipped
				};

				results.Continue = messageSink.OnMessage(testClassFinished) && results.Continue;
			}

			return results;
		}
	}
}

#endif
