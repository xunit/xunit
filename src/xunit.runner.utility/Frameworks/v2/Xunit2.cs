using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Xunit.Abstractions;
using Xunit.Sdk;

namespace Xunit
{
    /// <summary>
    /// This class be used to do discovery and execution of xUnit.net v2 tests
    /// using a reflection-based implementation of <see cref="IAssemblyInfo"/>.
    /// </summary>
    public class Xunit2 : Xunit2Discoverer, IFrontController, ITestCaseBulkDeserializer
    {
        ITestCaseBulkDeserializer defaultTestCaseBulkDeserializer;
        readonly ITestFrameworkExecutor remoteExecutor;

        /// <summary>
        /// Initializes a new instance of the <see cref="Xunit2"/> class.
        /// </summary>
        /// <param name="appDomainSupport">Determines whether tests should be run in a separate app domain.</param>
        /// <param name="sourceInformationProvider">The source code information provider.</param>
        /// <param name="assemblyFileName">The test assembly.</param>
        /// <param name="configFileName">The test assembly configuration file.</param>
        /// <param name="shadowCopy">If set to <c>true</c>, runs tests in a shadow copied app domain, which allows
        /// tests to be discovered and run without locking assembly files on disk.</param>
        /// <param name="shadowCopyFolder">The path on disk to use for shadow copying; if <c>null</c>, a folder
        /// will be automatically (randomly) generated</param>
        /// <param name="diagnosticMessageSink">The message sink which received <see cref="IDiagnosticMessage"/> messages.</param>
        /// <param name="verifyTestAssemblyExists">Determines whether or not the existence of the test assembly is verified.</param>
        public Xunit2(AppDomainSupport appDomainSupport,
                      ISourceInformationProvider sourceInformationProvider,
                      string assemblyFileName,
                      string configFileName = null,
                      bool shadowCopy = true,
                      string shadowCopyFolder = null,
                      IMessageSink diagnosticMessageSink = null,
                      bool verifyTestAssemblyExists = true)
            : base(appDomainSupport, sourceInformationProvider, assemblyFileName, configFileName, shadowCopy, shadowCopyFolder, diagnosticMessageSink, verifyTestAssemblyExists)
        {
#if NETFRAMEWORK
            var assemblyName = AssemblyName.GetAssemblyName(assemblyFileName);
#else
            var an = Assembly.Load(new AssemblyName { Name = Path.GetFileNameWithoutExtension(assemblyFileName) }).GetName();
            var assemblyName = new AssemblyName { Name = an.Name, Version = an.Version };
#endif
            remoteExecutor = Framework.GetExecutor(assemblyName);
        }

        /// <inheritdoc/>
        public List<KeyValuePair<string, ITestCase>> BulkDeserialize(List<string> serializations)
        {
            var callbackContainer = new DeserializeCallback();
            Action<List<KeyValuePair<string, ITestCase>>> callback = callbackContainer.Callback;

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
        public ITestCase Deserialize(string value)
        {
            return remoteExecutor.Deserialize(value);
        }

        /// <inheritdoc/>
        public override sealed void Dispose()
        {
            remoteExecutor.SafeDispose();

            base.Dispose();
        }

        /// <summary>
        /// Starts the process of running all the xUnit.net v2 tests in the assembly.
        /// </summary>
        /// <param name="messageSink">The message sink to report results back to.</param>
        /// <param name="discoveryOptions">The options to be used during test discovery.</param>
        /// <param name="executionOptions">The options to be used during test execution.</param>
        public void RunAll(IMessageSink messageSink, ITestFrameworkDiscoveryOptions discoveryOptions, ITestFrameworkExecutionOptions executionOptions)
        {
            remoteExecutor.RunAll(CreateOptimizedRemoteMessageSink(messageSink), discoveryOptions, executionOptions);
        }

        /// <summary>
        /// Starts the process of running the selected xUnit.net v2 tests.
        /// </summary>
        /// <param name="testCases">The test cases to run; if null, all tests in the assembly are run.</param>
        /// <param name="messageSink">The message sink to report results back to.</param>
        /// <param name="executionOptions">The options to be used during test execution.</param>
        public void RunTests(IEnumerable<ITestCase> testCases, IMessageSink messageSink, ITestFrameworkExecutionOptions executionOptions)
        {
            remoteExecutor.RunTests(testCases, CreateOptimizedRemoteMessageSink(messageSink), executionOptions);
        }

        class DeserializeCallback : LongLivedMarshalByRefObject
        {
            public List<KeyValuePair<string, ITestCase>> Results;

            public void Callback(List<KeyValuePair<string, ITestCase>> results) => Results = results;
        }
    }
}
