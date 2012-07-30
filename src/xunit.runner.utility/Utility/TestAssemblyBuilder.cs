using System;
using System.Collections.Generic;
using System.Xml;

namespace Xunit
{
    /// <summary>
    /// Responsible for building <see cref="TestAssembly"/> instances. Uses an instance
    /// of <see cref="IExecutorWrapper"/> to interrogate the list of available tests
    /// and create the entire object model tree.
    /// </summary>
    public static class TestAssemblyBuilder
    {
        /// <summary>
        /// Creates a <see cref="TestAssembly"/> which is a complete object model over
        /// the tests inside of instance of <see cref="IExecutorWrapper"/>.
        /// </summary>
        /// <param name="executorWrapper">The executor wrapper</param>
        /// <returns>The fully populated object model</returns>
        public static TestAssembly Build(IExecutorWrapper executorWrapper)
        {
            Guard.ArgumentNotNull("executorWrapper", executorWrapper);

            List<TestClass> classes = new List<TestClass>();

            foreach (XmlNode classNode in executorWrapper.EnumerateTests().SelectNodes("//class"))
            {
                List<TestMethod> methods = new List<TestMethod>();
                string typeName = classNode.Attributes["name"].Value;

                foreach (XmlNode testNode in classNode.SelectNodes("method"))
                {
                    string methodName = testNode.Attributes["method"].Value;
                    string displayName = null;
                    string skipReason = null;

                    if (testNode.Attributes["name"] != null)
                        displayName = testNode.Attributes["name"].Value;
                    if (testNode.Attributes["skip"] != null)
                        skipReason = testNode.Attributes["skip"].Value;

                    var traits = new MultiValueDictionary<string, string>();
                    foreach (XmlNode traitNode in testNode.SelectNodes("traits/trait"))
                        traits.AddValue(traitNode.Attributes["name"].Value,
                                        traitNode.Attributes["value"].Value);

                    TestMethod testMethod = new TestMethod(methodName,
                                                           displayName ?? typeName + "." + methodName,
                                                           traits);

                    methods.Add(testMethod);

                    if (!String.IsNullOrEmpty(skipReason))
                        testMethod.RunResults.Add(new TestSkippedResult(testMethod.DisplayName, skipReason));
                }

                classes.Add(new TestClass(typeName, methods));
            }

            return new TestAssembly(executorWrapper, classes);
        }
    }
}