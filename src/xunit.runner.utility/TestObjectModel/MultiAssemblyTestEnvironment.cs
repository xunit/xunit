using System;
using System.Collections.Generic;
using Xunit.Sdk;

namespace Xunit
{
    /// <summary>
    /// Represents the ability to load and unload test assemblies, as well as enumerate
    /// the test assemblies, the test methods, and run tests.
    /// </summary>
    public class MultiAssemblyTestEnvironment : ITestMethodEnumerator, IDisposable
    {
        /// <summary>
        /// The test assemblies loaded into the environment.
        /// </summary>
        protected List<TestAssembly> testAssemblies = new List<TestAssembly>();

        /// <inheritdoc/>
        public void Dispose()
        {
            foreach (var testAssembly in testAssemblies)
                testAssembly.Dispose();

            testAssemblies.Clear();
        }

        /// <summary>
        /// Enumerates the test assemblies in the environment.
        /// </summary>
        public IEnumerable<TestAssembly> EnumerateTestAssemblies()
        {
            return testAssemblies;
        }

        /// <inheritdoc/>
        public IEnumerable<TestMethod> EnumerateTestMethods()
        {
            return EnumerateTestMethods(testMethod => true);
        }

        /// <inheritdoc/>
        public IEnumerable<TestMethod> EnumerateTestMethods(Predicate<TestMethod> filter)
        {
            Guard.ArgumentNotNull("filter", filter);

            foreach (TestAssembly testAssembly in testAssemblies)
                foreach (TestMethod method in testAssembly.EnumerateTestMethods(filter))
                    yield return method;
        }

        /// <summary>
        /// Enumerates the traits across all the loaded assemblies.
        /// </summary>
        public MultiValueDictionary<string, string> EnumerateTraits()
        {
            var results = new MultiValueDictionary<string, string>();

            foreach (TestAssembly testAssembly in testAssemblies)
                foreach (TestClass testClass in testAssembly.EnumerateClasses())
                    foreach (TestMethod testMethod in testClass.EnumerateTestMethods())
                        testMethod.Traits.ForEach((name, value) => results.AddValue(name, value));

            return results;
        }

        /// <summary>
        /// Loads the specified assembly, using the default configuration file.
        /// </summary>
        /// <param name="assemblyFilename">The assembly filename.</param>
        /// <returns>The <see cref="TestAssembly"/> which represents the newly
        /// loaded test assembly.</returns>
        public TestAssembly Load(string assemblyFilename)
        {
            return Load(assemblyFilename, null, true);
        }

        /// <summary>
        /// Loads the specified assembly using the specified configuration file.
        /// </summary>
        /// <param name="assemblyFilename">The assembly filename.</param>
        /// <param name="configFilename">The config filename.</param>
        /// <returns>The <see cref="TestAssembly"/> which represents the newly
        /// loaded test assembly.</returns>
        public TestAssembly Load(string assemblyFilename, string configFilename)
        {
            return Load(assemblyFilename, configFilename, true);
        }

        /// <summary>
        /// Loads the specified assembly using the specified configuration file.
        /// </summary>
        /// <param name="assemblyFilename">The assembly filename.</param>
        /// <param name="configFilename">The config filename.</param>
        /// <param name="shadowCopy">Whether the DLLs should be shadow copied.</param>
        /// <returns>The <see cref="TestAssembly"/> which represents the newly
        /// loaded test assembly.</returns>
        public TestAssembly Load(string assemblyFilename, string configFilename, bool shadowCopy)
        {
            Guard.ArgumentNotNull("assemblyFilename", assemblyFilename);

            return Load(new ExecutorWrapper(assemblyFilename, configFilename, shadowCopy));
        }

        /// <summary>
        /// Adds the assembly loaded into the given <see cref="IExecutorWrapper"/>
        /// into the environment.
        /// </summary>
        /// <param name="executorWrapper">The executor wrapper.</param>
        /// <returns>The <see cref="TestAssembly"/> which represents the newly
        /// loaded test assembly.</returns>
        protected TestAssembly Load(IExecutorWrapper executorWrapper)
        {
            Guard.ArgumentNotNull("executorWrapper", executorWrapper);

            TestAssembly testAssembly = TestAssemblyBuilder.Build(executorWrapper);

            testAssemblies.Add(testAssembly);

            return testAssembly;
        }

        /// <summary>
        /// Runs the specified test methods.
        /// </summary>
        /// <param name="testMethods">The test methods to run.</param>
        /// <param name="callback">The run status information callback.</param>
        /// <returns>Returns the result as XML.</returns>
        public string Run(IEnumerable<TestMethod> testMethods, ITestMethodRunnerCallback callback)
        {
            Guard.ArgumentNotNullOrEmpty("testMethods", testMethods);
            Guard.ArgumentNotNull("callback", callback);

            var sortedMethods = new Dictionary<TestAssembly, List<TestMethod>>();

            foreach (TestAssembly testAssembly in testAssemblies)
                sortedMethods[testAssembly] = new List<TestMethod>();

            foreach (TestMethod testMethod in testMethods)
            {
                List<TestMethod> methodList = null;

                if (!sortedMethods.TryGetValue(testMethod.TestClass.TestAssembly, out methodList))
                    throw new ArgumentException("Test method " + testMethod.MethodName +
                                                " on test class " + testMethod.TestClass.TypeName +
                                                " in test assembly " + testMethod.TestClass.TestAssembly.AssemblyFilename +
                                                " is not in this test environment", "testMethods");

                methodList.Add(testMethod);
            }

            string result = "";

            foreach (var kvp in sortedMethods)
                if (kvp.Value.Count > 0)
                    result += kvp.Key.Run(kvp.Value, callback);

            return "<assemblies>" + result + "</assemblies>";
        }

        /// <summary>
        /// Unloads the specified assembly.
        /// </summary>
        /// <param name="assembly">The assembly to unload.</param>
        public void Unload(TestAssembly assembly)
        {
            Guard.ArgumentNotNull("assembly", assembly);
            Guard.ArgumentValid("assembly", "Assembly not loaded in this environment", testAssemblies.Contains(assembly));

            testAssemblies.Remove(assembly);
            assembly.Dispose();
        }
    }
}
