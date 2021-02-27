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
	public class Xunit1 : IFrontController
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

		/// <summary>
		/// This constructor is used by factory methods and unit tests only.
		/// </summary>
		protected Xunit1(
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
			TestAssemblyUniqueID = $":v1:assembly:{assemblyFileName}:{configFileName ?? "(null)"}";
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
		public string TestAssemblyUniqueID { get; }

		/// <inheritdoc/>
		public string TestFrameworkDisplayName => Executor.TestFrameworkDisplayName;

		/// <summary>
		/// Creates a wrapper to call the Executor call from xUnit.net v1.
		/// </summary>
		/// <returns>The executor wrapper.</returns>
		protected virtual IXunit1Executor CreateExecutor() =>
			new Xunit1Executor(diagnosticMessageSink, appDomainSupport != AppDomainSupport.Denied, assemblyFileName, configFileName, shadowCopy, shadowCopyFolder);

		/// <summary>
		/// INTERNAL METHOD, FOR TESTING PURPOSES ONLY. DO NOT CALL.
		/// </summary>
		protected Xunit1TestCase? Deserialize(string value) =>
			SerializationHelper.Deserialize<Xunit1TestCase>(value);

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
		/// INTERNAL METHOD, FOR TESTING PURPOSES ONLY. DO NOT CALL.
		/// </summary>
		protected void Find(
			_IMessageSink messageSink,
			bool includeSourceInformation,
			Predicate<_TestCaseDiscovered>? filter)
		{
			Guard.ArgumentNotNull(nameof(messageSink), messageSink);

			var discoveryStarting = new _DiscoveryStarting
			{
				AssemblyName = testAssemblyName,
				AssemblyPath = assemblyFileName,
				AssemblyUniqueID = TestAssemblyUniqueID,
				ConfigFilePath = configFileName
			};
			messageSink.OnMessage(discoveryStarting);

			try
			{
				Find(includeSourceInformation, testCase =>
				{
					var msg = testCase.ToTestCaseDiscovered(includeSerialization: true);
					if (filter == null || filter(msg))
						messageSink.OnMessage(msg);
				});
			}
			finally
			{
				var discoveryComplete = new _DiscoveryComplete { AssemblyUniqueID = TestAssemblyUniqueID };
				messageSink.OnMessage(discoveryComplete);
			}
		}

		/// <inheritdoc/>
		public void Find(
			_IMessageSink messageSink,
			FrontControllerFindSettings settings)
		{
			Guard.ArgumentNotNull(nameof(messageSink), messageSink);
			Guard.ArgumentNotNull(nameof(settings), settings);

			Find(
				messageSink,
				settings.Options.GetIncludeSourceInformationOrDefault(),
				settings.Filters.Empty ? null : settings.Filters.Filter
			);
		}

		void Find(
			bool includeSourceInformation,
			Action<Xunit1TestCase> callback)
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

						var testCase = new Xunit1TestCase
						{
							AssemblyUniqueID = TestAssemblyUniqueID,
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

						callback(testCase);
					}
				}
			}
		}

		/// <summary>
		/// INTERNAL METHOD, FOR TESTING PURPOSES ONLY. DO NOT CALL.
		/// </summary>
		protected void FindAndRun(
			_IMessageSink messageSink,
			bool includeSourceInformation,
			Predicate<_TestCaseDiscovered>? filter)
		{
			Guard.ArgumentNotNull(nameof(messageSink), messageSink);

			var testCases = new List<Xunit1TestCase>();

			Find(includeSourceInformation, testCase =>
			{
				var include = true;

				if (filter != null)
				{
					var msg = testCase.ToTestCaseDiscovered(includeSerialization: false);
					include = filter(msg);
				}

				if (include)
					testCases.Add(testCase);
			});

			Run(testCases, messageSink);
		}

		/// <inheritdoc/>
		public void FindAndRun(
			_IMessageSink messageSink,
			FrontControllerFindAndRunSettings settings)
		{
			Guard.ArgumentNotNull(nameof(messageSink), messageSink);
			Guard.ArgumentNotNull(nameof(settings), settings);

			// Pass null for empty filter list, since it bypasses _TestCaseDiscovered creation
			FindAndRun(
				messageSink,
				settings.DiscoveryOptions.GetIncludeSourceInformationOrDefault(),
				settings.Filters.Empty ? null : settings.Filters.Filter
			);
		}

		/// <summary>
		/// INTERNAL METHOD, FOR TESTING PURPOSES ONLY. DO NOT CALL.
		/// </summary>
		protected void Run(
			IEnumerable<Xunit1TestCase> testCases,
			_IMessageSink messageSink)
		{
			var results = new Xunit1RunSummary();
			var environment = $"{IntPtr.Size * 8}-bit .NET {Environment.Version}";
			var testCasesList = testCases.ToList();

			var testAssemblyStartingMessage = new _TestAssemblyStarting
			{
				AssemblyName = testAssemblyName,
				AssemblyPath = assemblyFileName,
				AssemblyUniqueID = TestAssemblyUniqueID,
				ConfigFilePath = configFileName,
				StartTime = DateTimeOffset.Now,
				TestEnvironment = environment,
				TestFrameworkDisplayName = TestFrameworkDisplayName,
			};

			if (messageSink.OnMessage(testAssemblyStartingMessage))
			{
				try
				{
					if (testCasesList.Count != 0)
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

		/// <inheritdoc/>
		public void Run(
			_IMessageSink messageSink,
			FrontControllerRunSettings settings)
		{
			Guard.ArgumentNotNull(nameof(messageSink), messageSink);
			Guard.ArgumentNotNull(nameof(settings), settings);

			Run(settings.SerializedTestCases.Select(tc => Deserialize(tc)).WhereNotNull(), messageSink);
		}

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

		/// <summary>
		/// INTERNAL METHOD, FOR TESTING PURPOSES ONLY. DO NOT CALL.
		/// </summary>
		protected string Serialize(Xunit1TestCase testCase) =>
			SerializationHelper.Serialize(testCase);

		// Factory methods

		/// <summary>
		/// Returns an implementation of <see cref="IFrontController"/> which can be used
		/// for both discovery and execution of xUnit.net v1 tests.
		/// </summary>
		/// <param name="projectAssembly">The test project assembly.</param>
		/// <param name="sourceInformationProvider">The optional source information provider.</param>
		/// <param name="diagnosticMessageSink">The optional message sink which receives <see cref="_DiagnosticMessage"/> messages.</param>
		public static IFrontController ForDiscoveryAndExecution(
			XunitProjectAssembly projectAssembly,
			_ISourceInformationProvider? sourceInformationProvider = null,
			_IMessageSink? diagnosticMessageSink = null)
		{
			Guard.ArgumentNotNull(nameof(projectAssembly), projectAssembly);

			var assemblyFileName = projectAssembly.AssemblyFilename;

			Guard.ArgumentNotNull($"{nameof(projectAssembly)}.{nameof(XunitProjectAssembly.AssemblyFilename)}", assemblyFileName);

			if (diagnosticMessageSink == null)
				diagnosticMessageSink = new _NullMessageSink();

			return new Xunit1(
				diagnosticMessageSink,
				projectAssembly.Configuration.AppDomainOrDefault,
#if NETSTANDARD
				sourceInformationProvider ?? _NullSourceInformationProvider.Instance,
#else
				sourceInformationProvider ?? new VisualStudioSourceInformationProvider(assemblyFileName, diagnosticMessageSink),
#endif
				assemblyFileName,
				projectAssembly.ConfigFilename,
				projectAssembly.Configuration.ShadowCopyOrDefault,
				projectAssembly.Configuration.ShadowCopyFolder
			);
		}
	}
}

#endif
