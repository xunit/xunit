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
		ITestCaseBulkDeserializer? defaultTestCaseBulkDeserializer;
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

			if (defaultTestCaseBulkDeserializer == null)
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

				defaultTestCaseBulkDeserializer = new DefaultTestCaseBulkDeserializer(remoteExecutor);
			}

			return defaultTestCaseBulkDeserializer.BulkDeserialize(serializations);
		}

		/// <inheritdoc/>
		public void FindAndRun(
			_IMessageSink messageSink,
			FrontControllerFindAndRunSettings settings)
		{
			Guard.ArgumentNotNull(nameof(messageSink), messageSink);
			Guard.ArgumentNotNull(nameof(settings), settings);

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
			RemoteDiscoverer.Find(
				includeSourceInformation: false,
				discoverySink,
				Xunit2OptionsAdapter.Adapt(settings.DiscoveryOptions)
			);
			discoverySink.Finished.WaitOne();

			remoteExecutor.RunTests(
				discoverySink.TestCases,
				CreateOptimizedRemoteMessageSink(messageSink),
				Xunit2OptionsAdapter.Adapt(settings.ExecutionOptions)
			);
		}

		/// <inheritdoc/>
		public void Run(
			_IMessageSink messageSink,
			FrontControllerRunSettings settings)
		{
			Guard.ArgumentNotNull(nameof(messageSink), messageSink);
			Guard.ArgumentNotNull(nameof(settings), settings);

			remoteExecutor.RunTests(
				BulkDeserialize(settings.SerializedTestCases.ToList()).Select(kvp => kvp.Value).ToList(),
				CreateOptimizedRemoteMessageSink(messageSink),
				Xunit2OptionsAdapter.Adapt(settings.Options)
			);
		}

		class DeserializeCallback : LongLivedMarshalByRefObject
		{
			public List<KeyValuePair<string?, ITestCase?>>? Results;

			public void Callback(List<KeyValuePair<string?, ITestCase?>> results) => Results = results;
		}
	}
}
