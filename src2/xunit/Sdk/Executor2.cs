using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Reflection;
using System.Security;
using Xunit.Abstractions;

namespace Xunit.Sdk
{
    /// <summary>
    /// Internal class used for version-resilient test runners. DO NOT CALL DIRECTLY.
    /// Version-resilient runners should link against xunit.runner.utility.dll and use
    /// ExecutorWrapper instead.
    /// </summary>
    public class Executor2 : MarshalByRefObject
    {
        readonly Assembly assembly;
        readonly string assemblyFileName;

        /// <summary/>
        public Executor2(string assemblyFileName)
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
        public class EnumerateTests : MarshalByRefObject
        {
            /// <summary/>
            public EnumerateTests(Executor2 executor, ITestObserver<ITestCase> callback)
            {
                Guard.ArgumentNotNull("executor", executor);

                try
                {
                    foreach (ITestFramework framework in GetFrameworks())
                    {
                        IAssemblyInfo assembly = Reflector2.Wrap(executor.assembly);

                        foreach (ITestCase testCase in framework.Find(assembly))
                            callback.OnNext(testCase);
                    }

                    callback.OnCompleted();
                }
                catch (Exception ex)
                {
                    callback.OnError(ex);
                }
            }

            private IEnumerable<ITestFramework> GetFrameworks()
            {
                // TODO: Need to support framework discovery here
                yield return new XunitTestFramework();
            }

            /// <summary/>
            [SecurityCritical]
            public override Object InitializeLifetimeService()
            {
                return null;
            }
        }

        ///// <summary/>
        //[SuppressMessage("Microsoft.Design", "CA1034:NestedTypesShouldNotBeVisible", Justification = "This type is not intended to be directly consumed.")]
        //public class RunAssembly : MarshalByRefObject
        //{
        //    /// <summary/>
        //    public RunAssembly(Executor executor, object handler)
        //    {
        //        ExecutorCallback callback = ExecutorCallback.Wrap(handler);

        //        executor.RunOnSTAThreadWithPreservedWorkingDirectory(() =>
        //        {
        //            bool @continue = true;
        //            AssemblyResult results =
        //                new AssemblyResult(executor.assemblyFileName,
        //                                   AppDomain.CurrentDomain.SetupInformation.ConfigurationFile);

        //            foreach (Type type in executor.assembly.GetExportedTypes())
        //            {
        //                ITestClassCommand testClassCommand = TestClassCommandFactory.Make(type);

        //                if (testClassCommand != null)
        //                {
        //                    ClassResult classResult =
        //                        TestClassCommandRunner.Execute(testClassCommand,
        //                                                       null,
        //                                                       command => @continue = OnTestStart(command, callback),
        //                                                       result => @continue = OnTestResult(result, callback));

        //                    results.Add(classResult);
        //                }

        //                if (!@continue)
        //                    break;
        //            }

        //            OnTestResult(results, callback);
        //        });
        //    }

        //    /// <summary/>
        //    [SecurityCritical]
        //    public override Object InitializeLifetimeService()
        //    {
        //        return null;
        //    }
        //}

        ///// <summary/>
        //[SuppressMessage("Microsoft.Design", "CA1034:NestedTypesShouldNotBeVisible", Justification = "This type is not intended to be directly consumed.")]
        //public class RunClass : MarshalByRefObject
        //{
        //    /// <summary/>
        //    [SuppressMessage("Microsoft.Usage", "CA1806:DoNotIgnoreMethodResults", MessageId = "Xunit.Sdk.Executor+RunTests", Justification = "All important work is done in the constructor.")]
        //    public RunClass(Executor executor, string type, object handler)
        //    {
        //        new RunTests(executor, type, new List<string>(), handler);
        //    }

        //    /// <summary/>
        //    [SecurityCritical]
        //    public override Object InitializeLifetimeService()
        //    {
        //        return null;
        //    }
        //}

        ///// <summary/>
        //[SuppressMessage("Microsoft.Design", "CA1034:NestedTypesShouldNotBeVisible", Justification = "This type is not intended to be directly consumed.")]
        //public class RunTest : MarshalByRefObject
        //{
        //    /// <summary/>
        //    [SuppressMessage("Microsoft.Usage", "CA1806:DoNotIgnoreMethodResults", MessageId = "Xunit.Sdk.Executor+RunTests", Justification = "All important work is done in the constructor.")]
        //    public RunTest(Executor executor, string type, string method, object handler)
        //    {
        //        Guard.ArgumentNotNull("type", type);
        //        Guard.ArgumentNotNull("method", method);

        //        new RunTests(executor, type, new List<string> { method }, handler);
        //    }

        //    /// <summary/>
        //    [SecurityCritical]
        //    public override Object InitializeLifetimeService()
        //    {
        //        return null;
        //    }
        //}

        ///// <summary/>
        //[SuppressMessage("Microsoft.Design", "CA1034:NestedTypesShouldNotBeVisible", Justification = "This type is not intended to be directly consumed.")]
        //public class RunTests : MarshalByRefObject
        //{
        //    /// <summary/>
        //    public RunTests(Executor executor, string type, List<string> methods, object handler)
        //    {
        //        Guard.ArgumentNotNull("executor", executor);
        //        Guard.ArgumentNotNull("type", type);
        //        Guard.ArgumentNotNull("methods", methods);

        //        ExecutorCallback callback = ExecutorCallback.Wrap(handler);
        //        Type realType = executor.assembly.GetType(type);
        //        Guard.ArgumentValid("type", "Type " + type + " could not be found", realType != null);

        //        ITypeInfo typeInfo = Reflector.Wrap(realType);
        //        ITestClassCommand testClassCommand = TestClassCommandFactory.Make(typeInfo);

        //        List<IMethodInfo> methodInfos = new List<IMethodInfo>();

        //        foreach (string method in methods)
        //        {
        //            try
        //            {
        //                IMethodInfo methodInfo = typeInfo.GetMethod(method);
        //                Guard.ArgumentValid("methods", "Could not find method " + method + " in type " + type, methodInfo != null);
        //                methodInfos.Add(methodInfo);
        //            }
        //            catch (AmbiguousMatchException)
        //            {
        //                throw new ArgumentException("Ambiguous method named " + method + " in type " + type);
        //            }
        //        }

        //        if (testClassCommand == null)
        //        {
        //            ClassResult result = new ClassResult(typeInfo.Type);
        //            OnTestResult(result, callback);
        //            return;
        //        }

        //        executor.RunOnSTAThreadWithPreservedWorkingDirectory(() =>
        //            TestClassCommandRunner.Execute(testClassCommand,
        //                                           methodInfos,
        //                                           command => OnTestStart(command, callback),
        //                                           result => OnTestResult(result, callback)));
        //    }

        //    /// <summary/>
        //    [SecurityCritical]
        //    public override Object InitializeLifetimeService()
        //    {
        //        return null;
        //    }
        //}

        //static bool OnTestStart(ITestCommand command, ExecutorCallback callback)
        //{
        //    XmlNode node = command.ToStartXml();

        //    if (node != null)
        //        callback.Notify(node.OuterXml);

        //    return callback.ShouldContinue();
        //}

        //static bool OnTestResult(ITestResult result, ExecutorCallback callback)
        //{
        //    XmlDocument doc = new XmlDocument();
        //    doc.LoadXml("<foo/>");

        //    XmlNode node = result.ToXml(doc.ChildNodes[0]);

        //    if (node != null)
        //        callback.Notify(node.OuterXml);

        //    return callback.ShouldContinue();
        //}

        //void RunOnSTAThreadWithPreservedWorkingDirectory(ThreadStart threadStart)
        //{
        //    Thread thread = new Thread(ThreadRunner) { Name = "xUnit.net STA Test Execution Thread" };
        //    thread.SetApartmentState(ApartmentState.STA);
        //    thread.Start(threadStart);
        //}

        //void ThreadRunner(object threadStart)
        //{
        //    string preservedDirectory = Directory.GetCurrentDirectory();

        //    try
        //    {
        //        Directory.SetCurrentDirectory(Path.GetDirectoryName(assemblyFileName));
        //        ((ThreadStart)threadStart)();
        //    }
        //    finally
        //    {
        //        Directory.SetCurrentDirectory(preservedDirectory);
        //    }
        //}
    }
}