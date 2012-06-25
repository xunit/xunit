using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Runtime.Remoting.Messaging;
using System.Security;
using System.Security.Permissions;
using System.Threading;
using System.Xml;

namespace Xunit
{
    /// <summary>
    /// Wraps calls to the Executor. Used by runners to perform version-resilient test
    /// enumeration and execution.
    /// </summary>
    public class ExecutorWrapper : IExecutorWrapper
    {
        delegate IntCallbackHandler IntCallbackHandlerFactory();
        delegate XmlNodeCallbackHandler XmlNodeCallbackHandlerFactory(Predicate<XmlNode> callback, string lastNodeName);

        readonly IntCallbackHandlerFactory MakeIntCallbackHandler;
        readonly XmlNodeCallbackHandlerFactory MakeXmlNodeCallbackHandler;

        static Type typeICallbackEventHandler;
        static ConstructorInfo intCallbackHandlerCtor;
        static ConstructorInfo xmlNodeCallbackHandlerCtor;

        readonly AppDomain appDomain;
        readonly object executor;
        readonly AssemblyName xunitAssemblyName;

        string configFilename;

        /// <summary>
        /// Initializes the <see cref="ExecutorWrapper"/> class.
        /// </summary>
        static ExecutorWrapper()
        {
            typeICallbackEventHandler = Type.GetType("System.Web.UI.ICallbackEventHandler, System.Web, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", false);

            if (typeICallbackEventHandler != null)
            {
                Type intCallbackHandlerType = DynamicTypeGenerator.GenerateType("IntCallbackEventHandler", typeof(IntCallbackHandlerWithICallbackEventHandler), typeICallbackEventHandler);
                intCallbackHandlerCtor = intCallbackHandlerType.GetConstructor(new Type[0]);

                Type xmlNodeCallbackHandlerType = DynamicTypeGenerator.GenerateType("XmlNodeCallbackEventHandler", typeof(XmlNodeCallbackHandlerWithICallbackEventHandler), typeICallbackEventHandler);
                xmlNodeCallbackHandlerCtor = xmlNodeCallbackHandlerType.GetConstructor(new[] { typeof(Predicate<XmlNode>), typeof(string) });
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ExecutorWrapper"/> class.
        /// </summary>
        /// <param name="assemblyFilename">The assembly filename.</param>
        /// <param name="configFilename">The config filename. If null, the default config filename will be used.</param>
        /// <param name="shadowCopy">Set to true to enable shadow copying; false, otherwise.</param>
        public ExecutorWrapper(string assemblyFilename, string configFilename, bool shadowCopy)
        {
            assemblyFilename = Path.GetFullPath(assemblyFilename);
            if (!File.Exists(assemblyFilename))
                throw new ArgumentException("Could not find file: " + assemblyFilename);

            if (configFilename == null)
                configFilename = GetDefaultConfigFile(assemblyFilename);

            if (configFilename != null)
                configFilename = Path.GetFullPath(configFilename);

            AssemblyFilename = assemblyFilename;
            ConfigFilename = configFilename;

            appDomain = CreateAppDomain(assemblyFilename, configFilename, shadowCopy);

            try
            {
                string xunitAssemblyFilename = Path.Combine(Path.GetDirectoryName(assemblyFilename), "xunit.dll");

                if (!File.Exists(xunitAssemblyFilename))
                    throw new ArgumentException("Could not find file: " + xunitAssemblyFilename);

                xunitAssemblyName = AssemblyName.GetAssemblyName(xunitAssemblyFilename);
                executor = CreateObject("Xunit.Sdk.Executor", AssemblyFilename);

                Version xunitVersion = new Version(XunitVersion);

                if (xunitVersion.Major == 1 && xunitVersion.Minor < 6)
                {
                    if (typeICallbackEventHandler == null)
                        throw new InvalidOperationException("Attempted to run assembly linked to xUnit.net older than 1.6. This requires the full server version of .NET, which does not appear to be installed.");

                    MakeIntCallbackHandler = () => (IntCallbackHandler)intCallbackHandlerCtor.Invoke(new object[0]);
                    MakeXmlNodeCallbackHandler = (callback, lastNodeName) => (XmlNodeCallbackHandler)xmlNodeCallbackHandlerCtor.Invoke(new object[] { callback, lastNodeName });
                }
                else
                {
                    MakeIntCallbackHandler = () => new IntCallbackHandlerWithIMessageSink();
                    MakeXmlNodeCallbackHandler = (callback, lastNodeName) => new XmlNodeCallbackHandlerWithIMessageSink(callback, lastNodeName);
                }
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
        public string AssemblyFilename { get; private set; }

        /// <inheritdoc/>
        public string ConfigFilename
        {
            get
            {
                return configFilename ?? appDomain.SetupInformation.ConfigurationFile;
            }
            set
            {
                configFilename = value;
            }
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
                return appDomain.CreateInstanceAndUnwrap(xunitAssemblyName.FullName, typeName, false, 0, null, args, null, null, null);
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
        public XmlNode EnumerateTests()
        {
            var handler = MakeXmlNodeCallbackHandler(null, null);

            CreateObject("Xunit.Sdk.Executor+EnumerateTests", executor, handler);

            return handler.LastNode;
        }

        /// <inheritdoc/>
        public int GetAssemblyTestCount()
        {
            var handler = MakeIntCallbackHandler();

            CreateObject("Xunit.Sdk.Executor+AssemblyTestCount", executor, handler);

            return handler.Result;
        }

        static string GetDefaultConfigFile(string assemblyFile)
        {
            string configFilename = assemblyFile + ".config";

            if (File.Exists(configFilename))
                return configFilename;

            return null;
        }

        /// <inheritdoc/>
        public XmlNode RunAssembly(Predicate<XmlNode> callback)
        {
            var handler = MakeXmlNodeCallbackHandler(callback, "assembly");

            CreateObject("Xunit.Sdk.Executor+RunAssembly", executor, handler);

            handler.LastNodeArrived.WaitOne();

            return handler.LastNode;
        }

        /// <inheritdoc/>
        public XmlNode RunClass(string type, Predicate<XmlNode> callback)
        {
            var handler = MakeXmlNodeCallbackHandler(callback, "class");

            CreateObject("Xunit.Sdk.Executor+RunClass", executor, type, handler);

            handler.LastNodeArrived.WaitOne();

            return handler.LastNode;
        }

        /// <inheritdoc/>
        public XmlNode RunTest(string type, string method, Predicate<XmlNode> callback)
        {
            var handler = MakeXmlNodeCallbackHandler(callback, "class");

            CreateObject("Xunit.Sdk.Executor+RunTest", executor, type, method, handler);

            handler.LastNodeArrived.WaitOne();

            return handler.LastNode;
        }

        /// <inheritdoc/>
        public XmlNode RunTests(string type, List<string> methods, Predicate<XmlNode> callback)
        {
            var handler = MakeXmlNodeCallbackHandler(callback, "class");

            CreateObject("Xunit.Sdk.Executor+RunTests", executor, type, methods, handler);

            handler.LastNodeArrived.WaitOne();

            return handler.LastNode;
        }

        static void RethrowWithNoStackTraceLoss(Exception ex)
        {
            FieldInfo remoteStackTraceString = typeof(Exception).GetField("_remoteStackTraceString", BindingFlags.Instance | BindingFlags.NonPublic);
            remoteStackTraceString.SetValue(ex, ex.StackTrace + Environment.NewLine);
            throw ex;
        }

        /// <summary>
        /// THIS CLASS IS FOR INTERNAL USE ONLY.
        /// </summary>
        public abstract class IntCallbackHandler : MarshalByRefObject
        {
            /// <summary/>
            public int Result { get; protected set; }

            /// <summary/>
            public override object InitializeLifetimeService()
            {
                return null;
            }
        }

        /// <summary>
        /// THIS CLASS IS FOR INTERNAL USE ONLY.
        /// </summary>
        public class IntCallbackHandlerWithIMessageSink : IntCallbackHandler, IMessageSink
        {
            /// <summary/>
            IMessageCtrl IMessageSink.AsyncProcessMessage(IMessage msg, IMessageSink replySink)
            {
                throw new NotImplementedException();
            }

            /// <summary/>
            IMessageSink IMessageSink.NextSink
            {
                get { throw new NotImplementedException(); }
            }

            /// <summary/>
            IMessage IMessageSink.SyncProcessMessage(IMessage msg)
            {
                object value = msg.Properties["data"];
                Result = Convert.ToInt32(value);
                return new OutgoingMessage(true);
            }
        }

        /// <summary>
        /// THIS CLASS IS FOR INTERNAL USE ONLY.
        /// </summary>
        public class IntCallbackHandlerWithICallbackEventHandler : IntCallbackHandler
        {
            /// <summary/>
            public string GetCallbackResult()
            {
                throw new NotImplementedException();
            }

            /// <summary/>
            public void RaiseCallbackEvent(string eventArgument)
            {
                Result = Convert.ToInt32(eventArgument);
            }

            /// <summary/>
            public override Object InitializeLifetimeService()
            {
                return null;
            }
        }

        /// <summary>
        /// THIS CLASS IS FOR INTERNAL USE ONLY.
        /// </summary>
        public abstract class XmlNodeCallbackHandler : MarshalByRefObject
        {
            /// <summary/>
            protected readonly Predicate<XmlNode> callback;

            /// <summary/>
            protected readonly string lastNodeName;

            /// <summary/>
            public XmlNode LastNode { get; protected set; }

            /// <summary/>
            public ManualResetEvent LastNodeArrived { get; protected set; }

            /// <summary/>
            protected XmlNodeCallbackHandler(Predicate<XmlNode> callback, string lastNodeName)
            {
                this.callback = callback;
                this.lastNodeName = lastNodeName;

                LastNodeArrived = new ManualResetEvent(false);
            }

            /// <summary/>
            public override object InitializeLifetimeService()
            {
                return null;
            }
        }

        /// <summary>
        /// THIS CLASS IS FOR INTERNAL USE ONLY.
        /// </summary>
        public class XmlNodeCallbackHandlerWithIMessageSink : XmlNodeCallbackHandler, IMessageSink
        {
            /// <summary/>
            public XmlNodeCallbackHandlerWithIMessageSink(Predicate<XmlNode> callback, string lastNodeName)
                : base(callback, lastNodeName) { }

            /// <summary/>
            IMessageCtrl IMessageSink.AsyncProcessMessage(IMessage msg, IMessageSink replySink)
            {
                throw new NotImplementedException();
            }

            /// <summary/>
            IMessageSink IMessageSink.NextSink
            {
                get { throw new NotImplementedException(); }
            }

            /// <summary/>
            IMessage IMessageSink.SyncProcessMessage(IMessage msg)
            {
                bool @continue = true;
                string value = msg.Properties["data"] as string;

                if (value != null)
                {
                    XmlDocument doc = new XmlDocument();
                    doc.LoadXml(value);
                    LastNode = doc.ChildNodes[0];

                    if (callback != null)
                        @continue = callback(LastNode);

                    if (lastNodeName != null && LastNode.Name == lastNodeName)
                        LastNodeArrived.Set();
                }

                return new OutgoingMessage(@continue);
            }
        }

        /// <summary>
        /// THIS CLASS IS FOR INTERNAL USE ONLY.
        /// </summary>
        public class XmlNodeCallbackHandlerWithICallbackEventHandler : XmlNodeCallbackHandler
        {
            bool @continue = true;

            /// <summary/>
            public XmlNodeCallbackHandlerWithICallbackEventHandler(Predicate<XmlNode> callback, string lastNodeName)
                : base(callback, lastNodeName) { }

            /// <summary/>
            public void RaiseCallbackEvent(string result)
            {
                if (result != null)
                {
                    XmlDocument doc = new XmlDocument();
                    doc.LoadXml(result);
                    LastNode = doc.ChildNodes[0];

                    if (callback != null)
                        @continue = callback(LastNode);

                    if (lastNodeName != null && LastNode.Name == lastNodeName)
                        LastNodeArrived.Set();
                }
            }

            /// <summary/>
            public string GetCallbackResult()
            {
                return @continue.ToString();
            }
        }

        /// <summary>
        /// THIS CLASS IS FOR INTERNAL USE ONLY.
        /// </summary>
        public class OutgoingMessage : MarshalByRefObject, IMessage
        {
            Hashtable values = new Hashtable();

            /// <summary/>
            public OutgoingMessage(object value)
            {
                values["data"] = value;
            }

            /// <summary/>
            public override object InitializeLifetimeService()
            {
                return null;
            }

            IDictionary IMessage.Properties
            {
                get { return values; }
            }
        }
    }
}