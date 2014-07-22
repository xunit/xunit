﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Xml;
using Xunit.Abstractions;

namespace Xunit
{
    /// <summary>
    /// This class be used to do discovery and execution of xUnit.net v1 tests
    /// using a reflection-based implementation of <see cref="IAssemblyInfo"/>.
    /// Runner authors are strongly encouraged to use <see cref="XunitFrontController"/>
    /// instead of using this class directly.
    /// </summary>
    public class Xunit1 : IFrontController
    {
        static readonly BinaryFormatter BinaryFormatter = new BinaryFormatter();

        readonly string assemblyFileName;
        readonly string configFileName;
        readonly IXunit1Executor executor;
        readonly ISourceInformationProvider sourceInformationProvider;
        readonly Stack<IDisposable> toDispose = new Stack<IDisposable>();

        /// <summary>
        /// Initializes a new instance of the <see cref="Xunit1"/> class.
        /// </summary>
        /// <param name="sourceInformationProvider">Source code information provider.</param>
        /// <param name="assemblyFileName">The test assembly.</param>
        /// <param name="configFileName">The test assembly configuration file.</param>
        /// <param name="shadowCopy">If set to <c>true</c>, runs tests in a shadow copied app domain, which allows
        /// tests to be discovered and run without locking assembly files on disk.</param>
        /// <param name="shadowCopyFolder">The path on disk to use for shadow copying; if <c>null</c>, a folder
        /// will be automatically (randomly) generated</param>
        public Xunit1(ISourceInformationProvider sourceInformationProvider, string assemblyFileName, string configFileName = null, bool shadowCopy = true, string shadowCopyFolder = null)
        {
            this.sourceInformationProvider = sourceInformationProvider;
            this.assemblyFileName = assemblyFileName;
            this.configFileName = configFileName;

            executor = CreateExecutor(assemblyFileName, configFileName, shadowCopy, shadowCopyFolder);
        }

        /// <inheritdoc/>
        public string TargetFramework
        {
            // This is not supported with v1, since there is no code in the remote AppDomain
            // that would give us this information.
            get { return String.Empty; }
        }

        /// <inheritdoc/>
        public string TestFrameworkDisplayName
        {
            get { return executor.TestFrameworkDisplayName; }
        }

        /// <summary>
        /// Creates a wrapper to call the Executor call from xUnit.net v1.
        /// </summary>
        /// <param name="testAssemblyFileName">The filename of the assembly under test.</param>
        /// <param name="configFileName">The configuration file to be used for the app domain (optional, may be <c>null</c>).</param>
        /// <param name="shadowCopy">Whether to enable shadow copy for the app domain.</param>
        /// <returns>The executor wrapper.</returns>
        /// <param name="shadowCopyFolder">The path on disk to use for shadow copying; if <c>null</c>, a folder
        /// will be automatically (randomly) generated</param>
        protected virtual IXunit1Executor CreateExecutor(string testAssemblyFileName, string configFileName, bool shadowCopy, string shadowCopyFolder)
        {
            return new Xunit1Executor(testAssemblyFileName, configFileName, shadowCopy, shadowCopyFolder);
        }

        /// <inheritdoc/>
        public ITestCase Deserialize(string value)
        {
            using (var stream = new MemoryStream(Convert.FromBase64String(value)))
                return (Xunit1TestCase)BinaryFormatter.Deserialize(stream);
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            foreach (var disposable in toDispose)
                disposable.Dispose();

            executor.SafeDispose();
        }

        /// <summary>
        /// Starts the process of finding all xUnit.net v1 tests in an assembly.
        /// </summary>
        /// <param name="includeSourceInformation">Whether to include source file information, if possible.</param>
        /// <param name="messageSink">The message sink to report results back to.</param>
        public void Find(bool includeSourceInformation, IMessageSink messageSink)
        {
            Find(msg => true, includeSourceInformation, messageSink);
        }

        /// <inheritdoc/>
        void ITestFrameworkDiscoverer.Find(bool includeSourceInformation, IMessageSink messageSink, ITestFrameworkOptions options)
        {
            Find(msg => true, includeSourceInformation, messageSink);
        }

        /// <summary>
        /// Starts the process of finding all xUnit.net v1 tests in a class.
        /// </summary>
        /// <param name="typeName">The fully qualified type name to find tests in.</param>
        /// <param name="includeSourceInformation">Whether to include source file information, if possible.</param>
        /// <param name="messageSink">The message sink to report results back to.</param>
        public void Find(string typeName, bool includeSourceInformation, IMessageSink messageSink)
        {
            Find(msg => msg.TestCase.TestMethod.TestClass.Class.Name == typeName, includeSourceInformation, messageSink);
        }

        /// <inheritdoc/>
        void ITestFrameworkDiscoverer.Find(string typeName, bool includeSourceInformation, IMessageSink messageSink, ITestFrameworkOptions options)
        {
            Find(msg => msg.TestCase.TestMethod.TestClass.Class.Name == typeName, includeSourceInformation, messageSink);
        }

        void Find(Predicate<ITestCaseDiscoveryMessage> filter, bool includeSourceInformation, IMessageSink messageSink)
        {
            try
            {
                XmlNode assemblyXml = null;

                var handler = new XmlNodeCallbackHandler(xml => { assemblyXml = xml; return true; });
                executor.EnumerateTests(handler);

                foreach (XmlNode method in assemblyXml.SelectNodes("//method"))
                {
                    var testCase = method.ToTestCase(assemblyFileName, configFileName);
                    if (testCase != null)
                    {
                        if (includeSourceInformation)
                            testCase.SourceInformation = sourceInformationProvider.GetSourceInformation(testCase);

                        var message = new TestCaseDiscoveryMessage(testCase);
                        if (filter(message))
                            messageSink.OnMessage(message);
                    }
                }
            }
            finally
            {
                messageSink.OnMessage(new DiscoveryCompleteMessage(new string[0]));
            }
        }

        /// <summary>
        /// Starts the process of running all the xUnit.net v1 tests in the assembly.
        /// </summary>
        /// <param name="messageSink">The message sink to report results back to.</param>
        public void Run(IMessageSink messageSink)
        {
            var discoverySink = new TestDiscoveryVisitor();
            toDispose.Push(discoverySink);

            Find(false, discoverySink);
            discoverySink.Finished.WaitOne();

            Run(discoverySink.TestCases, messageSink);
        }

        void ITestFrameworkExecutor.RunAll(IMessageSink messageSink, ITestFrameworkOptions discoveryOptions, ITestFrameworkOptions executionOptions)
        {
            Run(messageSink);
        }

        /// <summary>
        /// Starts the process of running all the xUnit.net v1 tests.
        /// </summary>
        /// <param name="testCases">The test cases to run; if null, all tests in the assembly are run.</param>
        /// <param name="messageSink">The message sink to report results back to.</param>
        public void Run(IEnumerable<ITestCase> testCases, IMessageSink messageSink)
        {
            var results = new Xunit1RunSummary();
            var environment = String.Format("{0}-bit .NET {1}", IntPtr.Size * 8, Environment.Version);
            var testCollection = testCases.First().TestMethod.TestClass.TestCollection;

            try
            {
                if (messageSink.OnMessage(new TestAssemblyStarting(testCases, testCollection.TestAssembly, DateTime.Now, environment, TestFrameworkDisplayName)))
                    results = RunTestCollection(testCollection, testCases, messageSink);
            }
            catch (Exception ex)
            {
                var failureInformation = Xunit1ExceptionUtility.ConvertToFailureInformation(ex);

                messageSink.OnMessage(new ErrorMessage(testCases, failureInformation.ExceptionTypes,
                    failureInformation.Messages, failureInformation.StackTraces,
                    failureInformation.ExceptionParentIndices));
            }
            finally
            {
                messageSink.OnMessage(new TestAssemblyFinished(testCases, testCollection.TestAssembly, results.Time, results.Total, results.Failed, results.Skipped));
            }
        }

        void ITestFrameworkExecutor.RunTests(IEnumerable<ITestCase> testCases, IMessageSink messageSink, ITestFrameworkOptions options)
        {
            Run(testCases, messageSink);
        }

        Xunit1RunSummary RunTestCollection(ITestCollection testCollection, IEnumerable<ITestCase> testCases, IMessageSink messageSink)
        {
            var results = new Xunit1RunSummary();
            results.Continue = messageSink.OnMessage(new TestCollectionStarting(testCases, testCollection));

            if (results.Continue)
                foreach (var testClassGroup in testCases.GroupBy(tc => tc.TestMethod.TestClass, Comparer.Instance))
                {
                    var classResults = RunTestClass(testClassGroup.Key, testClassGroup.ToList(), messageSink);
                    results.Aggregate(classResults);
                    if (!classResults.Continue)
                        break;
                }

            results.Continue = messageSink.OnMessage(new TestCollectionFinished(testCases, testCollection, results.Time, results.Total, results.Failed, results.Skipped)) && results.Continue;
            return results;
        }

        Xunit1RunSummary RunTestClass(ITestClass testClass, IList<ITestCase> testCases, IMessageSink messageSink)
        {
            var handler = new TestClassCallbackHandler(testCases, messageSink);
            var results = handler.TestClassResults;
            results.Continue = messageSink.OnMessage(new TestClassStarting(testCases, testClass));

            if (results.Continue)
            {
                var methodNames = testCases.Select(tc => tc.TestMethod.Method.Name).ToList();
                executor.RunTests(testClass.Class.Name, methodNames, handler);
                handler.LastNodeArrived.WaitOne();
            }

            results.Continue = messageSink.OnMessage(new TestClassFinished(testCases, testClass, results.Time, results.Total, results.Failed, results.Skipped)) && results.Continue;
            return results;
        }

        /// <inheritdoc/>
        public string Serialize(ITestCase testCase)
        {
            using (var stream = new MemoryStream())
            {
                BinaryFormatter.Serialize(stream, testCase);
                return Convert.ToBase64String(stream.GetBuffer());
            }
        }

        class Comparer : IEqualityComparer<ITestClass>
        {
            public static readonly Comparer Instance = new Comparer();

            public bool Equals(ITestClass x, ITestClass y)
            {
                return x.Class.Name == y.Class.Name;
            }

            public int GetHashCode(ITestClass obj)
            {
                return obj.Class.Name.GetHashCode();
            }
        }
    }
}