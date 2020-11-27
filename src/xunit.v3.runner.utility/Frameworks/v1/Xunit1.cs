#if NETFRAMEWORK

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Xml;
using Xunit.Abstractions;
using Xunit.Internal;
using Xunit.Runner.Common;
using Xunit.Runner.v1;
using Xunit.Runner.v2;
using Xunit.v3;

namespace Xunit
{
	/// <summary>
	/// This class be used to do discovery and execution of xUnit.net v1 tests
	/// using a reflection-based implementation of <see cref="IAssemblyInfo"/>.
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
		public string TestFrameworkDisplayName => Executor.TestFrameworkDisplayName;

		/// <summary>
		/// Creates a wrapper to call the Executor call from xUnit.net v1.
		/// </summary>
		/// <returns>The executor wrapper.</returns>
		protected virtual IXunit1Executor CreateExecutor() =>
			new Xunit1Executor(diagnosticMessageSink, appDomainSupport != AppDomainSupport.Denied, assemblyFileName, configFileName, shadowCopy, shadowCopyFolder);

		/// <inheritdoc/>
		public ITestCase? Deserialize(string value)
		{
			Guard.ArgumentNotNull(nameof(value), value);

			return SerializationHelper.Deserialize<ITestCase>(value);
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
		public void Find(
			bool includeSourceInformation,
			_IMessageSink messageSink)
		{
			Guard.ArgumentNotNull(nameof(messageSink), messageSink);

			Find(msg => true, includeSourceInformation, messageSink);
		}

		/// <inheritdoc/>
		void _ITestFrameworkDiscoverer.Find(
			bool includeSourceInformation,
			_IMessageSink? messageSink,
			_ITestFrameworkDiscoveryOptions? discoveryOptions)
		{
			Guard.ArgumentNotNull(nameof(messageSink), messageSink);

			Find(msg => true, includeSourceInformation, messageSink);
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
			Find(msg => msg.TestCase.TestMethod.TestClass.Class.Name == typeName, includeSourceInformation, messageSink);
		}

		/// <inheritdoc/>
		void _ITestFrameworkDiscoverer.Find(
			string typeName,
			bool includeSourceInformation,
			_IMessageSink messageSink,
			_ITestFrameworkDiscoveryOptions discoveryOptions)
		{
			Find(msg => msg.TestCase.TestMethod.TestClass.Class.Name == typeName, includeSourceInformation, messageSink);
		}

		void Find(
			Predicate<_TestCaseDiscovered> filter,
			bool includeSourceInformation,
			_IMessageSink messageSink)
		{
			IAssemblyInfo assembly = new Xunit1AssemblyInfo(assemblyFileName);
			var assemblyUniqueID = UniqueIDGenerator.ForAssembly(assembly.Name, assembly.AssemblyPath, configFileName);
			var discoveryStarting = new _DiscoveryStarting
			{
				AssemblyName = assembly.Name,
				AssemblyPath = assembly.AssemblyPath,
				AssemblyUniqueID = assemblyUniqueID,
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
					foreach (var method in assemblyXml.SelectNodes("//method").Cast<XmlNode>())
					{
						var testCase = method.ToTestCase(assemblyFileName, configFileName);
						if (testCase != null)
						{
							if (includeSourceInformation)
							{
								var result = sourceInformationProvider.GetSourceInformation(testCase.TestMethod.TestClass.Class.Name, testCase.TestMethod.Method.Name);
								testCase.SourceInformation = new SourceInformation { FileName = result.FileName, LineNumber = result.LineNumber };
							}

							var testCollectionUniqueID = UniqueIDGenerator.ForTestCollection(assemblyUniqueID, ((ITestCollection)testCase).DisplayName, null);
							var testClassUniqueID = UniqueIDGenerator.ForTestClass(testCollectionUniqueID, ((ITestClass)testCase).Class?.Name);
							var testMethodUniqueID = UniqueIDGenerator.ForTestMethod(testClassUniqueID, ((ITestMethod)testCase).Method?.Name);
							var message = new _TestCaseDiscovered
							{
								AssemblyUniqueID = assemblyUniqueID,
								SkipReason = testCase.SkipReason,
								SourceFilePath = testCase.SourceInformation?.FileName,
								SourceLineNumber = testCase.SourceInformation?.LineNumber,
								TestCase = testCase,
								TestCaseDisplayName = testCase.DisplayName,
								TestCaseUniqueID = testCase.UniqueID,
								TestClassUniqueID = testClassUniqueID,
								TestCollectionUniqueID = testCollectionUniqueID,
								TestMethodUniqueID = testMethodUniqueID,
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
				var discoveryComplete = new _DiscoveryComplete { AssemblyUniqueID = assemblyUniqueID };
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

			Find(false, discoverySink);
			discoverySink.Finished.WaitOne();

			Run(discoverySink.TestCases, messageSink);
		}

		void _ITestFrameworkExecutor.RunAll(
			_IMessageSink messageSink,
			_ITestFrameworkDiscoveryOptions discoveryOptions,
			_ITestFrameworkExecutionOptions executionOptions)
		{
			Run(messageSink);
		}

		/// <summary>
		/// Starts the process of running all the xUnit.net v1 tests.
		/// </summary>
		/// <param name="testCases">The test cases to run; if null, all tests in the assembly are run.</param>
		/// <param name="messageSink">The message sink to report results back to.</param>
		public void Run(
			IEnumerable<ITestCase> testCases,
			_IMessageSink messageSink)
		{
			var results = new Xunit1RunSummary();
			var environment = $"{IntPtr.Size * 8}-bit .NET {Environment.Version}";
			var firstTestCase = testCases.FirstOrDefault();
			var testCollection = firstTestCase == null ? null : firstTestCase.TestMethod.TestClass.TestCollection;

			if (testCollection != null)
			{
				var assemblyUniqueID = UniqueIDGenerator.ForAssembly(
					testCollection.TestAssembly.Assembly.Name,
					testCollection.TestAssembly.Assembly.AssemblyPath,
					testCollection.TestAssembly.ConfigFileName
				);

				try
				{
					var testAssemblyStartingMessage = new _TestAssemblyStarting
					{
						AssemblyName = testCollection.TestAssembly.Assembly.Name,
						AssemblyPath = testCollection.TestAssembly.Assembly.AssemblyPath,
						AssemblyUniqueID = assemblyUniqueID,
						ConfigFilePath = testCollection.TestAssembly.ConfigFileName,
						StartTime = DateTimeOffset.Now,
						TestEnvironment = environment,
						TestFrameworkDisplayName = TestFrameworkDisplayName,
					};

					if (messageSink.OnMessage(testAssemblyStartingMessage))
						results = RunTestCollection(assemblyUniqueID, testCollection, testCases, messageSink);
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
						AssemblyUniqueID = assemblyUniqueID,
						ExecutionTime = results.Time,
						TestsFailed = results.Failed,
						TestsRun = results.Total,
						TestsSkipped = results.Skipped
					};

					messageSink.OnMessage(assemblyFinished);
				}
			}
		}

		void _ITestFrameworkExecutor.RunTests(
			IEnumerable<ITestCase> testCases,
			_IMessageSink messageSink,
			_ITestFrameworkExecutionOptions executionOptions)
		{
			Run(testCases, messageSink);
		}

		Xunit1RunSummary RunTestCollection(
			string assemblyUniqueID,
			ITestCollection testCollection,
			IEnumerable<ITestCase> testCases,
			_IMessageSink messageSink)
		{
			var collectionStarting = new _TestCollectionStarting
			{
				TestCollectionClass = testCollection.CollectionDefinition?.Name,
				TestCollectionDisplayName = testCollection.DisplayName
			};
			collectionStarting.TestCollectionUniqueID = UniqueIDGenerator.ForTestCollection(
				assemblyUniqueID,
				collectionStarting.TestCollectionDisplayName,
				collectionStarting.TestCollectionClass
			);

			var results = new Xunit1RunSummary
			{
				Continue = messageSink.OnMessage(collectionStarting)
			};

			try
			{
				if (results.Continue)
					foreach (var testClassGroup in testCases.GroupBy(tc => tc.TestMethod.TestClass, Comparer.Instance))
					{
						var classResults = RunTestClass(assemblyUniqueID, collectionStarting.TestCollectionUniqueID, testClassGroup.Key, testClassGroup.ToList(), messageSink);
						results.Aggregate(classResults);
						if (!classResults.Continue)
							break;
					}
			}
			finally
			{
				var collectionFinished = new _TestCollectionFinished
				{
					AssemblyUniqueID = assemblyUniqueID,
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
			string assemblyUniqueID,
			string collectionUniqueID,
			ITestClass testClass,
			IList<ITestCase> testCases,
			_IMessageSink messageSink)
		{
			var handler = new TestClassCallbackHandler(testCases, messageSink);
			var results = handler.TestClassResults;
			var testClassStarting = new _TestClassStarting
			{
				AssemblyUniqueID = assemblyUniqueID,
				TestClass = testClass.Class.Name,
				TestCollectionUniqueID = collectionUniqueID
			};
			testClassStarting.TestClassUniqueID = UniqueIDGenerator.ForTestClass(collectionUniqueID, testClassStarting.TestClass);

			results.Continue = messageSink.OnMessage(testClassStarting);

			try
			{
				if (results.Continue)
				{
					var methodNames = testCases.Select(tc => tc.TestMethod.Method.Name).ToList();
					Executor.RunTests(testClass.Class.Name, methodNames, handler);
					handler.LastNodeArrived.WaitOne();
				}
			}
			finally
			{
				var testClassFinished = new _TestClassFinished
				{
					AssemblyUniqueID = assemblyUniqueID,
					ExecutionTime = results.Time,
					TestClassUniqueID = testClassStarting.TestClassUniqueID,
					TestCollectionUniqueID = collectionUniqueID,
					TestsFailed = results.Failed,
					TestsRun = results.Total,
					TestsSkipped = results.Skipped
				};

				results.Continue = messageSink.OnMessage(testClassFinished) && results.Continue;
			}

			return results;
		}

		/// <inheritdoc/>
		public string Serialize(ITestCase testCase) => SerializationHelper.Serialize(testCase);

		class Comparer : IEqualityComparer<ITestClass>
		{
			public static readonly Comparer Instance = new Comparer();

			public bool Equals(ITestClass? x, ITestClass? y) => x?.Class.Name == y?.Class.Name;

			public int GetHashCode(ITestClass obj) => obj.Class.Name.GetHashCode();
		}
	}
}

#endif
