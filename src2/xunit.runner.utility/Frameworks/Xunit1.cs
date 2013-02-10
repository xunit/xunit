//using System;
//using System.Collections.Generic;
//using System.Diagnostics.CodeAnalysis;
//using System.Globalization;
//using System.IO;
//using System.Reflection;
//using System.Threading;
//using System.Web.UI;
//using System.Xml;
//using Xunit.Abstractions;

//namespace Xunit.Controllers
//{
//    /// <summary>
//    /// This is an internal class, and is not intended to be called from end-user code.
//    /// </summary>
//    public class Xunit1 : AppDomainXunitController
//    {
//        static readonly IXunitControllerFactory factory = new Xunit1ControllerFactory();

//        readonly object executor;

//        public Xunit1Controller(string assemblyFileName, string configFileName, bool shadowCopy)
//            : base(assemblyFileName, configFileName, shadowCopy, Path.Combine(Path.GetDirectoryName(assemblyFileName), "xunit.dll"))
//        {
//            try
//            {
//                executor = CreateObject<object>("Xunit.Sdk.Executor", assemblyFileName);
//            }
//            catch (TargetInvocationException ex)
//            {
//                Dispose();
//                ex.InnerException.RethrowWithNoStackTraceLoss();
//            }
//            catch (Exception)
//            {
//                Dispose();
//                throw;
//            }
//        }

//        public static IXunitControllerFactory Factory
//        {
//            get { return factory; }
//        }

//        public override void Find(bool includeSourceInformation, IMessageSink messageSink)
//        {
//            //var handler = XmlNodeCallbackHandlerFactory(null, null);

//            //CreateObject("Xunit.Sdk.Executor+EnumerateTests", executor, handler);

//            //return handler.LastNode;

//            throw new NotImplementedException();
//        }

//        public override void Find(ITypeInfo type, bool includeSourceInformation, IMessageSink messageSink)
//        {
//            throw new NotImplementedException();
//        }

//        public override void Run(IEnumerable<ITestCase> testMethods, IMessageSink messageSink)
//        {
//            //var handler = XmlNodeCallbackHandlerFactory(callback, "class");

//            //CreateObject("Xunit.Sdk.Executor+RunTests", executor, type, methods, handler);

//            //handler.LastNodeArrived.WaitOne();

//            throw new NotImplementedException();
//        }

//        class IntCallbackHandler : LongLivedMarshalByRefObject, ICallbackEventHandler
//        {
//            public int Result { get; protected set; }

//            public string GetCallbackResult()
//            {
//                return null;
//            }

//            public void RaiseCallbackEvent(string eventArgument)
//            {
//                Result = Convert.ToInt32(eventArgument, CultureInfo.InvariantCulture);
//            }
//        }

//        class XmlNodeCallbackHandler : LongLivedMarshalByRefObject, ICallbackEventHandler
//        {
//            Predicate<XmlNode> callback;
//            bool @continue = true;
//            string lastNodeName;

//            public XmlNodeCallbackHandler(Predicate<XmlNode> callback, string lastNodeName)
//            {
//                this.callback = callback;
//                this.lastNodeName = lastNodeName;

//                LastNodeArrived = new ManualResetEvent(false);
//            }

//            [SuppressMessage("Microsoft.Design", "CA1059:MembersShouldNotExposeCertainConcreteTypes", MessageId = "System.Xml.XmlNode", Justification = "This would be a breaking change.")]
//            public XmlNode LastNode { get; protected set; }

//            public ManualResetEvent LastNodeArrived { get; protected set; }

//            public string GetCallbackResult()
//            {
//                return @continue.ToString();
//            }

//            public void RaiseCallbackEvent(string eventArgument)
//            {
//                if (eventArgument != null)
//                {
//                    XmlDocument doc = new XmlDocument();
//                    doc.LoadXml(eventArgument);
//                    LastNode = doc.ChildNodes[0];

//                    if (callback != null)
//                        @continue = callback(LastNode);

//                    if (lastNodeName != null && LastNode.Name == lastNodeName)
//                        LastNodeArrived.Set();
//                }
//            }
//        }

//        class Xunit1ControllerFactory : IXunitControllerFactory
//        {
//            public IXunitController Create(string assemblyFileName, string configFileName, bool shadowCopy)
//            {
//                return new Xunit1Controller(assemblyFileName, configFileName, shadowCopy);
//            }
//        }
//    }
//}
