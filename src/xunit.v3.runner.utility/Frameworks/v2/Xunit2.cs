using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Xunit.Abstractions;
using Xunit.Internal;
using Xunit.Runner.Common;
using Xunit.v3;

#if NETSTANDARD
using System.IO;
#endif

namespace Xunit.Runner.v2
{
	/// <summary>
	/// This class be used to do discovery and execution of xUnit.net v2 tests
	/// using a reflection-based implementation of <see cref="_IAssemblyInfo"/>.
	/// </summary>
	public class Xunit2 : Xunit2Discoverer, IFrontController
	{
		ITestCaseBulkDeserializer? bulkDeserializer;
		readonly ITestFrameworkExecutor remoteExecutor;

		/// <summary>
		/// Initializes a new instance of the <see cref="Xunit2"/> class.
		/// </summary>
		/// <param name="diagnosticMessageSink">The message sink which receives <see cref="_DiagnosticMessage"/> messages.</param>
		/// <param name="appDomainSupport">Determines whether tests should be run in a separate app domain.</param>
		/// <param name="sourceInformationProvider">The source code information provider.</param>
		/// <param name="assemblyFileName">The test assembly.</param>
		/// <param name="configFileName">The test assembly configuration file.</param>
		/// <param name="shadowCopy">If set to <c>true</c>, runs tests in a shadow copied app domain, which allows
		/// tests to be discovered and run without locking assembly files on disk.</param>
		/// <param name="shadowCopyFolder">The path on disk to use for shadow copying; if <c>null</c>, a folder
		/// will be automatically (randomly) generated</param>
		/// <param name="verifyTestAssemblyExists">Determines whether or not the existence of the test assembly is verified.</param>
		public Xunit2(
			_IMessageSink diagnosticMessageSink,
			AppDomainSupport appDomainSupport,
			_ISourceInformationProvider sourceInformationProvider,
			string assemblyFileName,
			string? configFileName = null,
			bool shadowCopy = true,
			string? shadowCopyFolder = null,
			bool verifyTestAssemblyExists = true)
				: base(diagnosticMessageSink, appDomainSupport, sourceInformationProvider, assemblyFileName, configFileName, shadowCopy, shadowCopyFolder, verifyTestAssemblyExists)
		{
			Guard.ArgumentNotNull(nameof(diagnosticMessageSink), diagnosticMessageSink);

#if NETFRAMEWORK
			var assemblyName = AssemblyName.GetAssemblyName(assemblyFileName);
#else
			var an = Assembly.Load(new AssemblyName { Name = Path.GetFileNameWithoutExtension(assemblyFileName) }).GetName();
			var assemblyName = new AssemblyName { Name = an.Name, Version = an.Version };
#endif
			remoteExecutor = RemoteFramework.GetExecutor(assemblyName);
			DisposalTracker.Add(remoteExecutor);
		}

		List<KeyValuePair<string?, ITestCase?>> BulkDeserialize(List<string> serializations)
		{
			var callbackContainer = new DeserializeCallback();
			Action<List<KeyValuePair<string?, ITestCase?>>> callback = callbackContainer.Callback;

			if (bulkDeserializer == null)
			{
				if (AppDomain.HasAppDomain)
				{
					try
					{
						AppDomain.CreateObject<object>(TestFrameworkAssemblyName, "Xunit.Sdk.TestCaseBulkDeserializer", RemoteDiscoverer, remoteExecutor, serializations, callback);
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
		/// Starts the process of running all the xUnit.net v2 tests in the assembly.
		/// </summary>
		/// <param name="messageSink">The message sink to report results back to.</param>
		/// <param name="discoveryOptions">The options to be used during test discovery.</param>
		/// <param name="executionOptions">The options to be used during test execution.</param>
		public void RunAll(
			_IMessageSink messageSink,
			_ITestFrameworkDiscoveryOptions discoveryOptions,
			_ITestFrameworkExecutionOptions executionOptions) =>
				remoteExecutor.RunAll(
					CreateOptimizedRemoteMessageSink(messageSink),
					Xunit2OptionsAdapter.Adapt(discoveryOptions),
					Xunit2OptionsAdapter.Adapt(executionOptions)
				);

		/// <summary>
		/// Starts the process of running the selected xUnit.net v2 tests.
		/// </summary>
		/// <param name="testCases">The test cases to run; if null, all tests in the assembly are run.</param>
		/// <param name="messageSink">The message sink to report results back to.</param>
		/// <param name="executionOptions">The options to be used during test execution.</param>
		public void RunTests(
			IEnumerable<_ITestCase> testCases,
			_IMessageSink messageSink,
			_ITestFrameworkExecutionOptions executionOptions) =>
				remoteExecutor.RunTests(
					testCases.Cast<Xunit3TestCase>().Select(tc => tc.V2TestCase).ToList(),
					CreateOptimizedRemoteMessageSink(messageSink),
					Xunit2OptionsAdapter.Adapt(executionOptions)
				);

		/// <summary>
		/// Starts the process of running the selected xUnit.net v2 tests.
		/// </summary>
		/// <param name="serializedTestCases">The test cases to run; if null, all tests in the assembly are run.</param>
		/// <param name="messageSink">The message sink to report results back to.</param>
		/// <param name="executionOptions">The options to be used during test execution.</param>
		public void RunTests(
			IEnumerable<string> serializedTestCases,
			_IMessageSink messageSink,
			_ITestFrameworkExecutionOptions executionOptions)
		{
			var testCases = BulkDeserialize(serializedTestCases.ToList()).Select(kvp => kvp.Value).ToList();
			remoteExecutor.RunTests(testCases, CreateOptimizedRemoteMessageSink(messageSink), Xunit2OptionsAdapter.Adapt(executionOptions));
		}

		class DeserializeCallback : LongLivedMarshalByRefObject
		{
			public List<KeyValuePair<string?, ITestCase?>>? Results;

			public void Callback(List<KeyValuePair<string?, ITestCase?>> results) => Results = results;
		}
	}
}
