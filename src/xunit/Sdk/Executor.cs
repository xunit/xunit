using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Reflection;
using System.Security.Permissions;
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
        readonly string assemblyFilename;

        /// <summary/>
        public Executor(string assemblyFilename)
        {
            this.assemblyFilename = Path.GetFullPath(assemblyFilename);
            assembly = Assembly.Load(AssemblyName.GetAssemblyName(this.assemblyFilename));
        }

        /// <summary/>
        [SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.Infrastructure)]
        public override Object InitializeLifetimeService()
        {
            return null;
        }

        /// <summary/>
        public class AssemblyTestCount : MarshalByRefObject
        {
            /// <summary/>
            [SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0", Justification = "This parameter is verified elsewhere.")]
            [SuppressMessage("Microsoft.Performance", "CA1804:RemoveUnusedLocals", MessageId = "method", Justification = "No can do.")]
            public AssemblyTestCount(Executor executor, object _handler)
            {
                ExecutorCallback handler = ExecutorCallback.Wrap(_handler);
                int result = 0;

                foreach (Type type in executor.assembly.GetExportedTypes())
                {
                    ITestClassCommand testClassCommand = TestClassCommandFactory.Make(type);

                    if (testClassCommand != null)
                        foreach (IMethodInfo method in testClassCommand.EnumerateTestMethods())
                            result++;
                }

                handler.Notify(result.ToString());
            }

            /// <summary/>
            [SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.Infrastructure)]
            public override Object InitializeLifetimeService()
            {
                return null;
            }
        }

        /// <summary/>
        public class EnumerateTests : MarshalByRefObject
        {
            /// <summary/>
            [SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0", Justification = "This parameter is verified elsewhere.")]
            public EnumerateTests(Executor executor, object _handler)
            {
                ExecutorCallback handler = ExecutorCallback.Wrap(_handler);

                XmlDocument doc = new XmlDocument();
                doc.LoadXml("<dummy/>");

                XmlNode assemblyNode = XmlUtility.AddElement(doc.ChildNodes[0], "assembly");
                XmlUtility.AddAttribute(assemblyNode, "name", executor.assemblyFilename);

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

                handler.Notify(assemblyNode.OuterXml);
            }

            /// <summary/>
            [SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.Infrastructure)]
            public override Object InitializeLifetimeService()
            {
                return null;
            }
        }

        /// <summary/>
        public class RunAssembly : MarshalByRefObject
        {
            /// <summary/>
            public RunAssembly(Executor executor, object _handler)
            {
                ExecutorCallback handler = ExecutorCallback.Wrap(_handler);

                executor.RunOnSTAThreadWithPreservedWorkingDirectory(() =>
                    {
                        bool @continue = true;
                        AssemblyResult results =
                            new AssemblyResult(executor.assemblyFilename,
                                               AppDomain.CurrentDomain.SetupInformation.ConfigurationFile);

                        foreach (Type type in executor.assembly.GetExportedTypes())
                        {
                            ITestClassCommand testClassCommand = TestClassCommandFactory.Make(type);

                            if (testClassCommand != null)
                            {
                                ClassResult classResult =
                                    TestClassCommandRunner.Execute(testClassCommand,
                                                                   null,
                                                                   command => @continue = OnTestStart(command, handler),
                                                                   result => @continue = OnTestResult(result, handler));

                                results.Add(classResult);
                            }

                            if (!@continue)
                                break;
                        }

                        OnTestResult(results, handler);
                    });
            }

            /// <summary/>
            [SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.Infrastructure)]
            public override Object InitializeLifetimeService()
            {
                return null;
            }
        }

        /// <summary/>
        public class RunClass : MarshalByRefObject
        {
            /// <summary/>
            [SuppressMessage("Microsoft.Usage", "CA1806:DoNotIgnoreMethodResults", MessageId = "Xunit.Sdk.Executor+RunTests", Justification = "All important work is done in the constructor.")]
            public RunClass(Executor executor, string _type, object _handler)
            {
                new RunTests(executor, _type, new List<string>(), _handler);
            }

            /// <summary/>
            [SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.Infrastructure)]
            public override Object InitializeLifetimeService()
            {
                return null;
            }
        }

        /// <summary/>
        public class RunTest : MarshalByRefObject
        {
            /// <summary/>
            [SuppressMessage("Microsoft.Usage", "CA1806:DoNotIgnoreMethodResults", MessageId = "Xunit.Sdk.Executor+RunTests", Justification = "All important work is done in the constructor.")]
            public RunTest(Executor executor, string _type, string _method, object _handler)
            {
                Guard.ArgumentNotNull("_type", _type);
                Guard.ArgumentNotNull("_method", _method);

                List<string> _methods = new List<string>();
                _methods.Add(_method);
                new RunTests(executor, _type, _methods, _handler);
            }

            /// <summary/>
            [SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.Infrastructure)]
            public override Object InitializeLifetimeService()
            {
                return null;
            }
        }

        /// <summary/>
        public class RunTests : MarshalByRefObject
        {
            /// <summary/>
            [SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0", Justification = "This parameter is verified elsewhere.")]
            [SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "2", Justification = "This parameter is verified elsewhere.")]
            public RunTests(Executor executor, string _type, List<string> _methods, object _handler)
            {
                Guard.ArgumentNotNull("_type", _type);
                Guard.ArgumentNotNull("_methods", _methods);

                ExecutorCallback handler = ExecutorCallback.Wrap(_handler);
                Type realType = executor.assembly.GetType(_type);
                Guard.ArgumentValid("_type", "Type " + _type + " could not be found", realType != null);

                ITypeInfo type = Reflector.Wrap(realType);
                ITestClassCommand testClassCommand = TestClassCommandFactory.Make(type);

                List<IMethodInfo> methods = new List<IMethodInfo>();

                foreach (string _method in _methods)
                {
                    try
                    {
                        IMethodInfo method = type.GetMethod(_method);
                        Guard.ArgumentValid("_methods", "Could not find method " + _method + " in type " + _type, method != null);
                        methods.Add(method);
                    }
                    catch (AmbiguousMatchException)
                    {
                        throw new ArgumentException("Ambiguous method named " + _method + " in type " + _type);
                    }
                }

                if (testClassCommand == null)
                {
                    ClassResult result = new ClassResult(type.Type);
                    OnTestResult(result, handler);
                    return;
                }

                executor.RunOnSTAThreadWithPreservedWorkingDirectory(() =>
                    TestClassCommandRunner.Execute(testClassCommand,
                                                   methods,
                                                   command => OnTestStart(command, handler),
                                                   result => OnTestResult(result, handler)));
            }

            /// <summary/>
            [SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.Infrastructure)]
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
                Directory.SetCurrentDirectory(Path.GetDirectoryName(assemblyFilename));
                ((ThreadStart)threadStart)();
            }
            finally
            {
                Directory.SetCurrentDirectory(preservedDirectory);
            }
        }
    }
}