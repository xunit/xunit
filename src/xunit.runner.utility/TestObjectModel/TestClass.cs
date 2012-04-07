using System;
using System.Collections.Generic;
using System.Xml;
using Xunit.Sdk;

namespace Xunit
{
    /// <summary>
    /// Represents a single class with test methods.
    /// </summary>
    public class TestClass : ITestMethodEnumerator
    {
        IEnumerable<TestMethod> testMethods;

        /// <summary>
        /// Initializes a new instance of the <see cref="TestClass"/> class.
        /// </summary>
        /// <param name="typeName">The namespace-qualified type name that
        /// this class represents.</param>
        /// <param name="testMethods">The test methods inside this test class.</param>
        public TestClass(string typeName, IEnumerable<TestMethod> testMethods)
        {
            Guard.ArgumentNotNull("testMethods", testMethods);

            TypeName = typeName;
            this.testMethods = testMethods;

            foreach (TestMethod testMethod in testMethods)
                testMethod.TestClass = this;
        }

        /// <summary>
        /// Gets the test assembly that this class belongs to.
        /// </summary>
        public TestAssembly TestAssembly { get; internal set; }

        /// <summary>
        /// Gets the namespace-qualified type name of this class.
        /// </summary>
        public string TypeName { get; private set; }

        /// <inheritdoc/>
        public IEnumerable<TestMethod> EnumerateTestMethods()
        {
            return EnumerateTestMethods(m => true);
        }

        /// <inheritdoc/>
        public IEnumerable<TestMethod> EnumerateTestMethods(Predicate<TestMethod> filter)
        {
            Guard.ArgumentNotNull("filter", filter);

            foreach (TestMethod testMethod in testMethods)
                if (filter(testMethod))
                    yield return testMethod;
        }

        internal TestMethod GetMethod(string methodName)
        {
            foreach (TestMethod testMethod in testMethods)
                if (testMethod.MethodName == methodName)
                    return testMethod;

            throw new InvalidOperationException("Got callback for test method " + methodName +
                                                " outside the scope of running class " + TypeName);
        }

        /// <summary>
        /// Runs the specified test methods.
        /// </summary>
        /// <param name="testMethods">The test methods to run.</param>
        /// <param name="callback">The run status information callback.</param>
        /// <returns>Returns the result as XML.</returns>
        public virtual string Run(IEnumerable<TestMethod> testMethods, ITestMethodRunnerCallback callback)
        {
            Guard.ArgumentNotNullOrEmpty("testMethods", testMethods);
            Guard.ArgumentNotNull("callback", callback);

            List<string> methodNames = new List<string>();

            foreach (TestMethod testMethod in testMethods)
            {
                if (testMethod.TestClass != this)
                    throw new ArgumentException("All test methods must belong to this test class");

                methodNames.Add(testMethod.MethodName);
                testMethod.RunResults.Clear();
            }

            return RunTests(methodNames, callback);
        }

        /// <summary>
        /// Runs the specified tests in the given type, calling the callback as appropriate.
        /// This override point exists primarily for unit testing purposes.
        /// </summary>
        /// <param name="methods">The test methods to run</param>
        /// <param name="callback">The run status information callback.</param>
        protected virtual string RunTests(List<string> methods, ITestMethodRunnerCallback callback)
        {
            IRunnerLogger logger = new TestClassCallbackDispatcher(this, callback);
            IExecutorWrapper wrapper = TestAssembly.ExecutorWrapper;

            try
            {
                XmlNode classNode = wrapper.RunTests(TypeName, methods, node => XmlLoggerAdapter.LogNode(node, logger));
                return classNode.OuterXml;
            }
            catch (Exception ex)
            {
                logger.ExceptionThrown(wrapper.AssemblyFilename, ex);
                return String.Empty;
            }
        }
    }
}