using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Reflection;
using System.Security;
using System.Security.Permissions;
using Xunit.Abstractions;

namespace Xunit
{
    /// <summary>
    /// Wraps calls to the Executor. Used by runners to perform version-resilient test
    /// enumeration and execution.
    /// </summary>
    public class Executor2Wrapper : IExecutor2Wrapper
    {
        readonly AppDomain appDomain;
        readonly object executor;
        readonly AssemblyName xunitAssemblyName;

        string configFilename;

        /// <summary>
        /// Initializes a new instance of the <see cref="ExecutorWrapper"/> class.
        /// </summary>
        /// <param name="assemblyFileName">The assembly filename.</param>
        /// <param name="configFileName">The config filename. If null, the default config filename will be used.</param>
        /// <param name="shadowCopy">Set to true to enable shadow copying; false, otherwise.</param>
        public Executor2Wrapper(string assemblyFileName, string configFileName, bool shadowCopy)
        {
            assemblyFileName = Path.GetFullPath(assemblyFileName);
            if (!File.Exists(assemblyFileName))
                throw new ArgumentException("Could not find file: " + assemblyFileName);

            if (configFileName == null)
                configFileName = GetDefaultConfigFile(assemblyFileName);

            if (configFileName != null)
                configFileName = Path.GetFullPath(configFileName);

            AssemblyFileName = assemblyFileName;
            ConfigFileName = configFileName;

            appDomain = CreateAppDomain(assemblyFileName, configFileName, shadowCopy);

            try
            {
                string xunitAssemblyFilename = Path.Combine(Path.GetDirectoryName(assemblyFileName), "xunit2.dll");

                if (!File.Exists(xunitAssemblyFilename))
                    throw new ArgumentException("Could not find file: " + xunitAssemblyFilename);

                xunitAssemblyName = AssemblyName.GetAssemblyName(xunitAssemblyFilename);
                executor = CreateObject("Xunit.Sdk.Executor2", AssemblyFileName);
            }
            catch (TargetInvocationException ex)
            {
                Dispose();
                RethrowWithNoStackTraceLoss(ex.InnerException);
            }
            catch (Exception)
            {
                Dispose();
                throw;
            }
        }

        /// <inheritdoc/>
        public string AssemblyFileName { get; private set; }

        /// <inheritdoc/>
        public string ConfigFileName
        {
            get { return configFilename ?? appDomain.SetupInformation.ConfigurationFile; }
            set { configFilename = value; }
        }

        /// <inheritdoc/>
        public string XunitVersion
        {
            get { return xunitAssemblyName.Version.ToString(); }
        }

        static AppDomain CreateAppDomain(string assemblyFilename, string configFilename, bool shadowCopy)
        {
            AppDomainSetup setup = new AppDomainSetup();
            setup.ApplicationBase = Path.GetDirectoryName(assemblyFilename);
            setup.ApplicationName = Guid.NewGuid().ToString();

            if (shadowCopy)
            {
                setup.ShadowCopyFiles = "true";
                setup.ShadowCopyDirectories = setup.ApplicationBase;
                setup.CachePath = Path.Combine(Path.GetTempPath(), setup.ApplicationName);
            }

            setup.ConfigurationFile = configFilename;

            return AppDomain.CreateDomain(setup.ApplicationName, null, setup, new PermissionSet(PermissionState.Unrestricted));
        }

        object CreateObject(string typeName, params object[] args)
        {
            try
            {
                //return appDomain.CreateInstanceAndUnwrap(xunitAssemblyName.FullName, typeName, false, 0, null, args, null, null, null);
                return appDomain.CreateInstanceAndUnwrap(xunitAssemblyName.FullName, typeName, false, 0, null, args, null, null);
            }
            catch (TargetInvocationException ex)
            {
                RethrowWithNoStackTraceLoss(ex.InnerException);
                return null;
            }
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "Suppressing exceptions in Dispose is a common pattern")]
        public void Dispose()
        {
            if (appDomain != null)
            {
                string cachePath = appDomain.SetupInformation.CachePath;

                AppDomain.Unload(appDomain);

                try
                {
                    if (cachePath != null)
                        Directory.Delete(cachePath, true);
                }
                catch { }
            }
        }

        /// <inheritdoc/>
        public IEnumerable<ITestCase> EnumerateTests()
        {
            List<ITestCase> results = new List<ITestCase>();
            TestObserver<ITestCase> observer = new TestObserver<ITestCase>();
            observer.Next = testCase => results.Add(testCase);

            CreateObject("Xunit.Sdk.Executor2+EnumerateTests", executor, observer);

            return results;
        }

        class TestObserver<T> : MarshalByRefObject, ITestObserver<T>
        {
            public Action Completed { get; set; }
            public Action<Exception> Error { get; set; }
            public Action<T> Next { get; set; }

            public void OnCompleted()
            {
                if (Completed != null)
                    Completed();
            }

            public void OnError(Exception error)
            {
                if (Error != null)
                    Error(error);
            }

            public void OnNext(T value)
            {
                if (Next != null)
                    Next(value);
            }
        }

        ///// <inheritdoc/>
        //public int GetAssemblyTestCount()
        //{
        //    var handler = IntCallbackHandlerFactory();

        //    CreateObject("Xunit.Sdk.Executor+AssemblyTestCount", executor, handler);

        //    return handler.Result;
        //}

        static string GetDefaultConfigFile(string assemblyFile)
        {
            string configFilename = assemblyFile + ".config";

            if (File.Exists(configFilename))
                return configFilename;

            return null;
        }

        ///// <inheritdoc/>
        //public XmlNode RunAssembly(Predicate<XmlNode> callback)
        //{
        //    var handler = XmlNodeCallbackHandlerFactory(callback, "assembly");

        //    CreateObject("Xunit.Sdk.Executor+RunAssembly", executor, handler);

        //    handler.LastNodeArrived.WaitOne();

        //    return handler.LastNode;
        //}

        ///// <inheritdoc/>
        //public XmlNode RunClass(string type, Predicate<XmlNode> callback)
        //{
        //    var handler = XmlNodeCallbackHandlerFactory(callback, "class");

        //    CreateObject("Xunit.Sdk.Executor+RunClass", executor, type, handler);

        //    handler.LastNodeArrived.WaitOne();

        //    return handler.LastNode;
        //}

        ///// <inheritdoc/>
        //public XmlNode RunTest(string type, string method, Predicate<XmlNode> callback)
        //{
        //    var handler = XmlNodeCallbackHandlerFactory(callback, "class");

        //    CreateObject("Xunit.Sdk.Executor+RunTest", executor, type, method, handler);

        //    handler.LastNodeArrived.WaitOne();

        //    return handler.LastNode;
        //}

        ///// <inheritdoc/>
        //public XmlNode RunTests(string type, List<string> methods, Predicate<XmlNode> callback)
        //{
        //    var handler = XmlNodeCallbackHandlerFactory(callback, "class");

        //    CreateObject("Xunit.Sdk.Executor+RunTests", executor, type, methods, handler);

        //    handler.LastNodeArrived.WaitOne();

        //    return handler.LastNode;
        //}

        static void RethrowWithNoStackTraceLoss(Exception ex)
        {
            FieldInfo remoteStackTraceString = typeof(Exception).GetField("_remoteStackTraceString", BindingFlags.Instance | BindingFlags.NonPublic);
            remoteStackTraceString.SetValue(ex, ex.StackTrace + Environment.NewLine);
            throw ex;
        }

        ///// <summary>
        ///// THIS CLASS IS FOR INTERNAL USE ONLY.
        ///// </summary>
        //[SuppressMessage("Microsoft.Design", "CA1034:NestedTypesShouldNotBeVisible", Justification = "This class is public by necessity, but actually marked as 'for internal use only'")]
        //public abstract class IntCallbackHandler : MarshalByRefObject
        //{
        //    /// <summary/>
        //    public int Result { get; protected set; }

        //    /// <summary/>
        //    [SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.Infrastructure)]
        //    public override object InitializeLifetimeService()
        //    {
        //        return null;
        //    }
        //}

        ///// <summary>
        ///// THIS CLASS IS FOR INTERNAL USE ONLY.
        ///// </summary>
        //[SuppressMessage("Microsoft.Design", "CA1034:NestedTypesShouldNotBeVisible", Justification = "This class is public by necessity, but actually marked as 'for internal use only'")]
        //public class IntCallbackHandlerWithIMessageSink : IntCallbackHandler, IMessageSink
        //{
        //    /// <summary/>
        //    [SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.Infrastructure)]
        //    public IMessageCtrl AsyncProcessMessage(IMessage msg, IMessageSink replySink)
        //    {
        //        throw new NotImplementedException();
        //    }

        //    /// <summary/>
        //    public IMessageSink NextSink
        //    {
        //        [SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.Infrastructure)]
        //        get { return null; }
        //    }

        //    /// <summary/>
        //    [SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.Infrastructure)]
        //    public IMessage SyncProcessMessage(IMessage msg)
        //    {
        //        var methodCall = msg as IMethodCallMessage;
        //        if (msg == null || methodCall != null)
        //            return RemotingServices.ExecuteMessage(this, methodCall);

        //        object value = msg.Properties["data"];
        //        Result = Convert.ToInt32(value, CultureInfo.InvariantCulture);
        //        return new OutgoingMessage(true);
        //    }
        //}

        ///// <summary>
        ///// THIS CLASS IS FOR INTERNAL USE ONLY.
        ///// </summary>
        //[SuppressMessage("Microsoft.Design", "CA1034:NestedTypesShouldNotBeVisible", Justification = "This class is public by necessity, but actually marked as 'for internal use only'")]
        //[SuppressMessage("Microsoft.Naming", "CA1711:IdentifiersShouldNotHaveIncorrectSuffix", Justification = "This class is a handler for ICallbackEventHandler, which makes its name appropriate.")]
        //public class IntCallbackHandlerWithICallbackEventHandler : IntCallbackHandler
        //{
        //    /// <summary/>
        //    [SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate", Justification = "This method matches ICallbackEventHandler by convention rather than explicitly.")]
        //    [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "This method matches ICallbackEventHandler by convention rather than explicitly.")]
        //    public string GetCallbackResult()
        //    {
        //        throw new NotImplementedException();
        //    }

        //    /// <summary/>
        //    [SuppressMessage("Microsoft.Design", "CA1030:UseEventsWhereAppropriate", Justification = "This method matches ICallbackEventHandler by convention rather than explicitly.")]
        //    public void RaiseCallbackEvent(string eventArgument)
        //    {
        //        Result = Convert.ToInt32(eventArgument, CultureInfo.InvariantCulture);
        //    }

        //    /// <summary/>
        //    [SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.Infrastructure)]
        //    public override Object InitializeLifetimeService()
        //    {
        //        return null;
        //    }
        //}

        ///// <summary>
        ///// THIS CLASS IS FOR INTERNAL USE ONLY.
        ///// </summary>
        //[SuppressMessage("Microsoft.Design", "CA1034:NestedTypesShouldNotBeVisible", Justification = "This class is public by necessity, but actually marked as 'for internal use only'")]
        //public abstract class XmlNodeCallbackHandler : MarshalByRefObject
        //{
        //    /// <summary/>
        //    protected Predicate<XmlNode> Callback { get; private set; }

        //    /// <summary/>
        //    [SuppressMessage("Microsoft.Design", "CA1059:MembersShouldNotExposeCertainConcreteTypes", MessageId = "System.Xml.XmlNode", Justification = "This would be a breaking change.")]
        //    public XmlNode LastNode { get; protected set; }

        //    /// <summary/>
        //    protected string LastNodeName { get; private set; }

        //    /// <summary/>
        //    public ManualResetEvent LastNodeArrived { get; protected set; }

        //    /// <summary/>
        //    protected XmlNodeCallbackHandler(Predicate<XmlNode> callback, string lastNodeName)
        //    {
        //        Callback = callback;
        //        LastNodeName = lastNodeName;

        //        LastNodeArrived = new ManualResetEvent(false);
        //    }

        //    /// <summary/>
        //    [SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.Infrastructure)]
        //    public override object InitializeLifetimeService()
        //    {
        //        return null;
        //    }
        //}

        ///// <summary>
        ///// THIS CLASS IS FOR INTERNAL USE ONLY.
        ///// </summary>
        //[SuppressMessage("Microsoft.Design", "CA1034:NestedTypesShouldNotBeVisible", Justification = "This class is public by necessity, but actually marked as 'for internal use only'")]
        //public class XmlNodeCallbackHandlerWithIMessageSink : XmlNodeCallbackHandler, IMessageSink
        //{
        //    /// <summary/>
        //    public XmlNodeCallbackHandlerWithIMessageSink(Predicate<XmlNode> callback, string lastNodeName)
        //        : base(callback, lastNodeName) { }

        //    /// <summary/>
        //    [SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.Infrastructure)]
        //    public IMessageCtrl AsyncProcessMessage(IMessage msg, IMessageSink replySink)
        //    {
        //        throw new NotImplementedException();
        //    }

        //    /// <summary/>
        //    public IMessageSink NextSink
        //    {
        //        [SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.Infrastructure)]
        //        get { return null; }
        //    }

        //    /// <summary/>
        //    [SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.Infrastructure)]
        //    public IMessage SyncProcessMessage(IMessage msg)
        //    {
        //        var methodCall = msg as IMethodCallMessage;
        //        if (msg == null || methodCall != null)
        //            return RemotingServices.ExecuteMessage(this, methodCall);

        //        bool @continue = true;
        //        string value = msg.Properties["data"] as string;

        //        if (value != null)
        //        {
        //            XmlDocument doc = new XmlDocument();
        //            doc.LoadXml(value);
        //            LastNode = doc.ChildNodes[0];

        //            if (Callback != null)
        //                @continue = Callback(LastNode);

        //            if (LastNodeName != null && LastNode.Name == LastNodeName)
        //                LastNodeArrived.Set();
        //        }

        //        return new OutgoingMessage(@continue);
        //    }
        //}

        ///// <summary>
        ///// THIS CLASS IS FOR INTERNAL USE ONLY.
        ///// </summary>
        //[SuppressMessage("Microsoft.Design", "CA1034:NestedTypesShouldNotBeVisible", Justification = "This class is public by necessity, but actually marked as 'for internal use only'")]
        //[SuppressMessage("Microsoft.Naming", "CA1711:IdentifiersShouldNotHaveIncorrectSuffix", Justification = "This class is a handler for ICallbackEventHandler, which makes its name appropriate.")]
        //public class XmlNodeCallbackHandlerWithICallbackEventHandler : XmlNodeCallbackHandler
        //{
        //    bool @continue = true;

        //    /// <summary/>
        //    public XmlNodeCallbackHandlerWithICallbackEventHandler(Predicate<XmlNode> callback, string lastNodeName)
        //        : base(callback, lastNodeName) { }

        //    /// <summary/>
        //    [SuppressMessage("Microsoft.Design", "CA1030:UseEventsWhereAppropriate", Justification = "This method matches ICallbackEventHandler by convention rather than explicitly.")]
        //    public void RaiseCallbackEvent(string result)
        //    {
        //        if (result != null)
        //        {
        //            XmlDocument doc = new XmlDocument();
        //            doc.LoadXml(result);
        //            LastNode = doc.ChildNodes[0];

        //            if (Callback != null)
        //                @continue = Callback(LastNode);

        //            if (LastNodeName != null && LastNode.Name == LastNodeName)
        //                LastNodeArrived.Set();
        //        }
        //    }

        //    /// <summary/>
        //    [SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate", Justification = "This method matches ICallbackEventHandler by convention rather than explicitly.")]
        //    public string GetCallbackResult()
        //    {
        //        return @continue.ToString();
        //    }
        //}

        ///// <summary>
        ///// THIS CLASS IS FOR INTERNAL USE ONLY.
        ///// </summary>
        //[SuppressMessage("Microsoft.Design", "CA1034:NestedTypesShouldNotBeVisible", Justification = "This class is public by necessity, but actually marked as 'for internal use only'")]
        //public class OutgoingMessage : MarshalByRefObject, IMessage
        //{
        //    Hashtable values = new Hashtable();

        //    /// <summary/>
        //    public OutgoingMessage(object value)
        //    {
        //        values["data"] = value;
        //    }

        //    /// <summary/>
        //    [SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.Infrastructure)]
        //    public override object InitializeLifetimeService()
        //    {
        //        return null;
        //    }

        //    /// <summary/>
        //    public IDictionary Properties
        //    {
        //        [SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.Infrastructure)]
        //        get { return values; }
        //    }
        //}
    }
}