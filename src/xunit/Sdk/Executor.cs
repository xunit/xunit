using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Security;
using System.Threading;
using System.Xml;

namespace Xunit.Sdk
{
    /// <summary>
    /// Internal class used for version-resilient test runners. DO NOT CALL DIRECTLY.
    /// Version-resilient runners should link against xunit.runner.utility.dll and use
    /// ExecutorWrapper instead.
    /// </summary>
    public class Executor : MarshalByRefObject
    {
        readonly Assembly assembly;
        readonly string assemblyFileName;

        /// <summary/>
        public Executor(string assemblyFileName)
        {
            this.assemblyFileName = Path.GetFullPath(assemblyFileName);
            assembly = Assembly.Load(AssemblyName.GetAssemblyName(this.assemblyFileName));
        }

        /// <summary/>
        [SecurityCritical]
        public override Object InitializeLifetimeService()
        {
            return null;
        }

        /// <summary/>
        [SuppressMessage("Microsoft.Design", "CA1034:NestedTypesShouldNotBeVisible", Justification = "This type is not intended to be directly consumed.")]
        public class AssemblyTestCount : MarshalByRefObject
        {
            /// <summary/>
            [SuppressMessage("Microsoft.Performance", "CA1804:RemoveUnusedLocals", MessageId = "method", Justification = "No can do.")]
            public AssemblyTestCount(Executor executor, object handler)
            {
                Guard.ArgumentNotNull("executor", executor);

                ExecutorCallback callback = ExecutorCallback.Wrap(handler);
                int result = 0;

                foreach (Type type in executor.assembly.GetExportedTypes())
                {
                    ITestClassCommand testClassCommand = TestClassCommandFactory.Make(type);

                    if (testClassCommand != null)
                        foreach (IMethodInfo method in testClassCommand.EnumerateTestMethods())
                            result++;
                }

                callback.Notify(result.ToString(CultureInfo.InvariantCulture));
            }

            /// <summary/>
            [SecurityCritical]
            public override Object InitializeLifetimeService()
            {
                return null;
            }
        }

        /// <summary/>
        [SuppressMessage("Microsoft.Design", "CA1034:NestedTypesShouldNotBeVisible", Justification = "This type is not intended to be directly consumed.")]
        public class EnumerateTests : MarshalByRefObject
        {
            /// <summary/>
            public EnumerateTests(Executor executor, object handler)
            {
                Guard.ArgumentNotNull("executor", executor);

                ExecutorCallback callback = ExecutorCallback.Wrap(handler);

                XmlDocument doc = new XmlDocument();
                doc.LoadXml("<dummy/>");

                XmlNode assemblyNode = XmlUtility.AddElement(doc.ChildNodes[0], "assembly");
                XmlUtility.AddAttribute(assemblyNode, "name", executor.assemblyFileName);

                foreach (Type type in executor.assembly.GetExportedTypes())
                {
                    ITestClassCommand testClassCommand = TestClassCommandFactory.Make(type);

                    if (testClassCommand != null)
                    {
                        string typeName = type.FullName;

                        XmlNode classNode = XmlUtility.AddElement(assemblyNode, "class");
                        XmlUtility.AddAttribute(classNode, "name", typeName);

                        foreach (IMethodInfo method in testClassCommand.EnumerateTestMethods())
                        {
                            string methodName = method.Name;
                            string displayName = null;

                            foreach (IAttributeInfo attr in method.GetCustomAttributes(typeof(FactAttribute)))
                                displayName = attr.GetPropertyValue<string>("Name");

                            XmlNode methodNode = XmlUtility.AddElement(classNode, "method");
                            XmlUtility.AddAttribute(methodNode, "name", displayName ?? typeName + "." + methodName);
                            XmlUtility.AddAttribute(methodNode, "type", typeName);
                            XmlUtility.AddAttribute(methodNode, "method", methodName);

                            string skipReason = MethodUtility.GetSkipReason(method);
                            if (skipReason != null)
                                XmlUtility.AddAttribute(methodNode, "skip", skipReason);

                            var traits = MethodUtility.GetTraits(method);
                            if (traits.Count > 0)
                            {
                                XmlNode traitsNode = XmlUtility.AddElement(methodNode, "traits");

                                traits.ForEach((name, value) =>
                                {
                                    XmlNode traitNode = XmlUtility.AddElement(traitsNode, "trait");
                                    XmlUtility.AddAttribute(traitNode, "name", name);
                                    XmlUtility.AddAttribute(traitNode, "value", value);
                                });
                            }
                        }
                    }
                }

                callback.Notify(assemblyNode.OuterXml);
            }

            /// <summary/>
            [SecurityCritical]
            public override Object InitializeLifetimeService()
            {
                return null;
            }
        }

        /// <summary/>
        [SuppressMessage("Microsoft.Design", "CA1034:NestedTypesShouldNotBeVisible", Justification = "This type is not intended to be directly consumed.")]
        public class RunAssembly : MarshalByRefObject
        {
            /// <summary/>
            public RunAssembly(Executor executor, object handler)
            {
                ExecutorCallback callback = ExecutorCallback.Wrap(handler);

                executor.RunOnSTAThreadWithPreservedWorkingDirectory(() =>
                    {
                        bool @continue = true;
                        AssemblyResult results =
                            new AssemblyResult(executor.assemblyFileName,
                                               AppDomain.CurrentDomain.SetupInformation.ConfigurationFile);

                        foreach (Type type in executor.assembly.GetExportedTypes())
                        {
                            ITestClassCommand testClassCommand = TestClassCommandFactory.Make(type);

                            if (testClassCommand != null)
                            {
                                ClassResult classResult =
                                    TestClassCommandRunner.Execute(testClassCommand,
                                                                   null,
                                                                   command => @continue = OnTestStart(command, callback),
                                                                   result => @continue = OnTestResult(result, callback));

                                results.Add(classResult);
                            }

                            if (!@continue)
                                break;
                        }

                        OnTestResult(results, callback);
                    });
            }

            /// <summary/>
            [SecurityCritical]
            public override Object InitializeLifetimeService()
            {
                return null;
            }
        }

        /// <summary/>
        [SuppressMessage("Microsoft.Design", "CA1034:NestedTypesShouldNotBeVisible", Justification = "This type is not intended to be directly consumed.")]
        public class RunClass : MarshalByRefObject
        {
            /// <summary/>
            [SuppressMessage("Microsoft.Usage", "CA1806:DoNotIgnoreMethodResults", MessageId = "Xunit.Sdk.Executor+RunTests", Justification = "All important work is done in the constructor.")]
            public RunClass(Executor executor, string type, object handler)
            {
                new RunTests(executor, type, new List<string>(), handler);
            }

            /// <summary/>
            [SecurityCritical]
            public override Object InitializeLifetimeService()
            {
                return null;
            }
        }

        /// <summary/>
        [SuppressMessage("Microsoft.Design", "CA1034:NestedTypesShouldNotBeVisible", Justification = "This type is not intended to be directly consumed.")]
        public class RunTest : MarshalByRefObject
        {
            /// <summary/>
            [SuppressMessage("Microsoft.Usage", "CA1806:DoNotIgnoreMethodResults", MessageId = "Xunit.Sdk.Executor+RunTests", Justification = "All important work is done in the constructor.")]
            public RunTest(Executor executor, string type, string method, object handler)
            {
                Guard.ArgumentNotNull("type", type);
                Guard.ArgumentNotNull("method", method);

                new RunTests(executor, type, new List<string> { method }, handler);
            }

            /// <summary/>
            [SecurityCritical]
            public override Object InitializeLifetimeService()
            {
                return null;
            }
        }

        /// <summary/>
        [SuppressMessage("Microsoft.Design", "CA1034:NestedTypesShouldNotBeVisible", Justification = "This type is not intended to be directly consumed.")]
        public class RunTests : MarshalByRefObject
        {
            /// <summary/>
            public RunTests(Executor executor, string type, List<string> methods, object handler)
            {
                Guard.ArgumentNotNull("executor", executor);
                Guard.ArgumentNotNull("type", type);
                Guard.ArgumentNotNull("methods", methods);

                ExecutorCallback callback = ExecutorCallback.Wrap(handler);
                Type realType = executor.assembly.GetType(type);
                Guard.ArgumentValid("type", "Type " + type + " could not be found", realType != null);

                ITypeInfo typeInfo = Reflector.Wrap(realType);
                ITestClassCommand testClassCommand = TestClassCommandFactory.Make(typeInfo);

                List<IMethodInfo> methodInfos = new List<IMethodInfo>();

                foreach (string method in methods)
                {
                    try
                    {
                        IMethodInfo methodInfo = typeInfo.GetMethod(method);
                        Guard.ArgumentValid("methods", "Could not find method " + method + " in type " + type, methodInfo != null);
                        methodInfos.Add(methodInfo);
                    }
                    catch (AmbiguousMatchException)
                    {
                        throw new ArgumentException("Ambiguous method named " + method + " in type " + type);
                    }
                }

                if (testClassCommand == null)
                {
                    ClassResult result = new ClassResult(typeInfo.Type);
                    OnTestResult(result, callback);
                    return;
                }

                executor.RunOnSTAThreadWithPreservedWorkingDirectory(() =>
                    TestClassCommandRunner.Execute(testClassCommand,
                                                   methodInfos,
                                                   command => OnTestStart(command, callback),
                                                   result => OnTestResult(result, callback)));
            }

            /// <summary/>
            [SecurityCritical]
            public override Object InitializeLifetimeService()
            {
                return null;
            }
        }

        static bool OnTestStart(ITestCommand command, ExecutorCallback callback)
        {
            XmlNode node = command.ToStartXml();

            if (node != null)
                callback.Notify(node.OuterXml);

            return callback.ShouldContinue();
        }

        static bool OnTestResult(ITestResult result, ExecutorCallback callback)
        {
            XmlDocument doc = new XmlDocument();
            doc.LoadXml("<foo/>");

            XmlNode node = result.ToXml(doc.ChildNodes[0]);

            if (node != null)
                callback.Notify(node.OuterXml);

            return callback.ShouldContinue();
        }

        void RunOnSTAThreadWithPreservedWorkingDirectory(ThreadStart threadStart)
        {
            Thread thread = new Thread(ThreadRunner) { Name = "xUnit.net STA Test Execution Thread" };
            thread.SetApartmentState(ApartmentState.STA);
            thread.Start(threadStart);
        }

        void ThreadRunner(object threadStart)
        {
            string preservedDirectory = Directory.GetCurrentDirectory();

            try
            {
                Directory.SetCurrentDirectory(Path.GetDirectoryName(assemblyFileName));
                ((ThreadStart)threadStart)();
            }
            finally
            {
                Directory.SetCurrentDirectory(preservedDirectory);
            }
        }
    }
}