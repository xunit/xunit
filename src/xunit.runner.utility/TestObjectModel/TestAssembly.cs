using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Xml;

namespace Xunit
{
    /// <summary>
    /// Represents a single test assembly with test classes.
    /// </summary>
    public class TestAssembly : ITestMethodEnumerator, IDisposable
    {
        IEnumerable<TestClass> testClasses;

        /// <summary>
        /// Initializes a new instance of the <see cref="TestAssembly"/> class.
        /// </summary>
        /// <param name="executorWrapper">The executor wrapper.</param>
        /// <param name="testClasses">The test classes.</param>
        public TestAssembly(IExecutorWrapper executorWrapper, IEnumerable<TestClass> testClasses)
        {
            Guard.ArgumentNotNull("executorWrapper", executorWrapper);
            Guard.ArgumentNotNull("testClasses", testClasses);

            ExecutorWrapper = executorWrapper;
            this.testClasses = testClasses;

            foreach (TestClass testClass in testClasses)
                testClass.TestAssembly = this;
        }

        /// <summary>
        /// Gets the assembly filename.
        /// </summary>
        [SuppressMessage("Microsoft.Naming", "CA1702:CompoundWordsShouldBeCasedCorrectly", MessageId = "Filename", Justification = "This would be a breaking change.")]
        public string AssemblyFilename
        {
            get { return ExecutorWrapper.AssemblyFilename; }
        }

        /// <summary>
        /// Gets the config filename.
        /// </summary>
        [SuppressMessage("Microsoft.Naming", "CA1702:CompoundWordsShouldBeCasedCorrectly", MessageId = "Filename", Justification = "This would be a breaking change.")]
        public string ConfigFilename
        {
            get { return ExecutorWrapper.ConfigFilename; }
        }

        /// <summary>
        /// Gets the executor wrapper.
        /// </summary>
        public IExecutorWrapper ExecutorWrapper { get; private set; }

        /// <summary>
        /// Gets the version of xunit.dll that the tests are linked against.
        /// </summary>
        public string XunitVersion
        {
            get { return ExecutorWrapper.XunitVersion; }
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            ExecutorWrapper.Dispose();
        }

        /// <summary>
        /// Enumerates the test classes in the assembly.
        /// </summary>
        public IEnumerable<TestClass> EnumerateClasses()
        {
            return testClasses;
        }

        /// <inheritdoc/>
        public IEnumerable<TestMethod> EnumerateTestMethods()
        {
            return EnumerateTestMethods(m => true);
        }

        /// <inheritdoc/>
        public IEnumerable<TestMethod> EnumerateTestMethods(Predicate<TestMethod> filter)
        {
            Guard.ArgumentNotNull("filter", filter);

            foreach (TestClass testClass in testClasses)
                foreach (TestMethod testMethod in testClass.EnumerateTestMethods(filter))
                    yield return testMethod;
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

            var sortedMethods = new Dictionary<TestClass, List<TestMethod>>();

            foreach (TestClass testClass in testClasses)
                sortedMethods[testClass] = new List<TestMethod>();

            foreach (TestMethod testMethod in testMethods)
            {
                List<TestMethod> methodList = null;

                if (!sortedMethods.TryGetValue(testMethod.TestClass, out methodList))
                    throw new ArgumentException("Test method " + testMethod.MethodName +
                                                " on test class " + testMethod.TestClass.TypeName +
                                                " is not in this assembly", "testMethods");

                methodList.Add(testMethod);
            }

            XmlDocument doc = new XmlDocument();
            doc.LoadXml("<assembly/>");
            XmlNode assemblyNode = doc.ChildNodes[0];

            AddAttribute(assemblyNode, "name", ExecutorWrapper.AssemblyFilename);
            AddAttribute(assemblyNode, "run-date", DateTime.Now.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture));
            AddAttribute(assemblyNode, "run-time", DateTime.Now.ToString("HH:mm:ss", CultureInfo.InvariantCulture));
            if (ExecutorWrapper.ConfigFilename != null)
                AddAttribute(assemblyNode, "configFile", ExecutorWrapper.ConfigFilename);

            callback.AssemblyStart(this);

            var callbackWrapper = new TestMethodRunnerCallbackWrapper(callback);

            int passed = 0;
            int failed = 0;
            int skipped = 0;
            double duration = 0.0;
            string result = "";

            foreach (var kvp in sortedMethods)
                if (kvp.Value.Count > 0)
                {
                    result += kvp.Key.Run(kvp.Value, callbackWrapper);

                    foreach (TestMethod testMethod in kvp.Value)
                        foreach (TestResult runResult in testMethod.RunResults)
                        {
                            duration += runResult.Duration;

                            if (runResult is TestPassedResult)
                                passed++;
                            else if (runResult is TestFailedResult)
                                failed++;
                            else
                                skipped++;
                        }
                }

            callback.AssemblyFinished(this, callbackWrapper.Total, callbackWrapper.Failed,
                                      callbackWrapper.Skipped, callbackWrapper.Time);

            AddAttribute(assemblyNode, "time", duration.ToString("0.000", CultureInfo.InvariantCulture));
            AddAttribute(assemblyNode, "total", passed + failed + skipped);
            AddAttribute(assemblyNode, "passed", passed);
            AddAttribute(assemblyNode, "failed", failed);
            AddAttribute(assemblyNode, "skipped", skipped);
            AddAttribute(assemblyNode, "environment", String.Format(CultureInfo.InvariantCulture, "{0}-bit .NET {1}", IntPtr.Size * 8, Environment.Version));
            AddAttribute(assemblyNode, "test-framework", String.Format(CultureInfo.InvariantCulture, "xUnit.net {0}", ExecutorWrapper.XunitVersion));

            return assemblyNode.OuterXml.Replace(" />", ">") + result + "</assembly>";
        }

        static void AddAttribute(XmlNode node, string name, object value)
        {
            XmlAttribute attr = node.OwnerDocument.CreateAttribute(name);
            attr.Value = value.ToString();
            node.Attributes.Append(attr);
        }

        class TestMethodRunnerCallbackWrapper : ITestMethodRunnerCallback
        {
            public int Total = 0;
            public int Failed = 0;
            public int Skipped = 0;
            public double Time = 0.0;

            ITestMethodRunnerCallback innerCallback;

            public TestMethodRunnerCallbackWrapper(ITestMethodRunnerCallback innerCallback)
            {
                this.innerCallback = innerCallback;
            }

            public void AssemblyFinished(TestAssembly testAssembly, int total, int failed, int skipped, double time)
            {
                throw new NotImplementedException();
            }

            public void AssemblyStart(TestAssembly testAssembly)
            {
                throw new NotImplementedException();
            }

            public bool ClassFailed(TestClass testClass, string exceptionType, string message, string stackTrace)
            {
                return innerCallback.ClassFailed(testClass, exceptionType, message, stackTrace);
            }

            public void ExceptionThrown(TestAssembly testAssembly, Exception exception)
            {
                innerCallback.ExceptionThrown(testAssembly, exception);
            }

            public bool TestFinished(TestMethod testMethod)
            {
                if (testMethod == null)
                    return false;

                ++Total;

                var lastRunResult = testMethod.RunResults[testMethod.RunResults.Count - 1];

                if (lastRunResult is TestFailedResult)
                    ++Failed;
                if (lastRunResult is TestSkippedResult)
                    ++Skipped;

                Time += lastRunResult.Duration;

                return innerCallback.TestFinished(testMethod);
            }

            public bool TestStart(TestMethod testMethod)
            {
                return innerCallback.TestStart(testMethod);
            }
        }
    }
}