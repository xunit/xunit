using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Xml;

namespace Xunit
{
    /// <summary>
    /// Runs tests in an assembly, and transforms the XML results into calls to
    /// the provided <see cref="IRunnerLogger"/>.
    /// </summary>
    public class TestRunner : ITestRunner
    {
        readonly IExecutorWrapper wrapper;
        readonly IRunnerLogger logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="TestRunner"/> class.
        /// </summary>
        /// <param name="executorWrapper">The executor wrapper.</param>
        /// <param name="logger">The logger.</param>
        public TestRunner(IExecutorWrapper executorWrapper, IRunnerLogger logger)
        {
            wrapper = executorWrapper;
            this.logger = logger;
        }

        /// <inheritdoc/>
        public virtual TestRunnerResult RunAssembly()
        {
            return RunAssembly(new IResultXmlTransform[0]);
        }

        /// <inheritdoc/>
        public virtual TestRunnerResult RunAssembly(IEnumerable<IResultXmlTransform> transforms)
        {
            Guard.ArgumentNotNull("transforms", transforms);

            XmlNode assemblyNode = null;
            logger.AssemblyStart(wrapper.AssemblyFilename, wrapper.ConfigFilename, wrapper.XunitVersion);

            TestRunnerResult result = CatchExceptions(() =>
            {
                assemblyNode = wrapper.RunAssembly(node => XmlLoggerAdapter.LogNode(node, logger));
                return TestRunnerResult.NoTests;
            });

            if (result == TestRunnerResult.Failed)
                return TestRunnerResult.Failed;
            if (assemblyNode == null)
                return TestRunnerResult.NoTests;

            string assemblyXml = assemblyNode.OuterXml;

            foreach (IResultXmlTransform transform in transforms)
                transform.Transform(assemblyXml);

            return ParseNodeForTestRunnerResult(assemblyNode);
        }

        /// <inheritdoc/>
        public virtual TestRunnerResult RunClass(string type)
        {
            return CatchExceptions(() =>
            {
                XmlNode classNode = wrapper.RunClass(type, node => XmlLoggerAdapter.LogNode(node, logger));
                return ParseNodeForTestRunnerResult(classNode);
            });
        }

        /// <inheritdoc/>
        public virtual TestRunnerResult RunTest(string type, string method)
        {
            return CatchExceptions(() =>
            {
                XmlNode classNode = wrapper.RunTest(type, method, node => XmlLoggerAdapter.LogNode(node, logger));
                return ParseNodeForTestRunnerResult(classNode);
            });
        }

        /// <inheritdoc/>
        public virtual TestRunnerResult RunTests(string type, List<string> methods)
        {
            return CatchExceptions(() =>
            {
                XmlNode classNode = wrapper.RunTests(type, methods, node => XmlLoggerAdapter.LogNode(node, logger));
                return ParseNodeForTestRunnerResult(classNode);
            });
        }

        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "The caught exception is resurfaced to the user.")]
        TestRunnerResult CatchExceptions(Func<TestRunnerResult> func)
        {
            try
            {
                return func();
            }
            catch (Exception ex)
            {
                logger.ExceptionThrown(wrapper.AssemblyFilename, ex);
                return TestRunnerResult.Failed;
            }
        }

        static TestRunnerResult ParseNodeForTestRunnerResult(XmlNode node)
        {
            string total = node.Attributes["total"].Value;
            string failed = node.Attributes["failed"].Value;

            if (total == "0")
                return TestRunnerResult.NoTests;
            if (failed == "0")
                return TestRunnerResult.Passed;

            return TestRunnerResult.Failed;
        }
    }
}