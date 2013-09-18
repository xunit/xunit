using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Web.UI;
using System.Xml;

namespace Xunit
{
    public class XmlNodeCallbackHandler : LongLivedMarshalByRefObject, ICallbackEventHandler
    {
        readonly Predicate<XmlNode> callback;
        bool @continue = true;
        readonly string lastNodeName;

        public XmlNodeCallbackHandler(Predicate<XmlNode> callback = null, string lastNodeName = null)
        {
            this.callback = callback;
            this.lastNodeName = lastNodeName;

            LastNodeArrived = new ManualResetEvent(false);
        }

        [SuppressMessage("Microsoft.Design", "CA1059:MembersShouldNotExposeCertainConcreteTypes", MessageId = "System.Xml.XmlNode", Justification = "This would be a breaking change.")]
        public XmlNode LastNode { get; protected set; }

        public ManualResetEvent LastNodeArrived { get; protected set; }

        protected virtual bool OnXmlNode(XmlNode node)
        {
            if (callback != null)
                return callback(node);

            return true;
        }

        string ICallbackEventHandler.GetCallbackResult()
        {
            return @continue.ToString();
        }

        void ICallbackEventHandler.RaiseCallbackEvent(string eventArgument)
        {
            if (eventArgument != null)
            {
                // REVIEW: Would this be cheaper with XDocument instead of XmlDocument?
                var doc = new XmlDocument();
                doc.LoadXml(eventArgument);
                LastNode = doc.ChildNodes[0];
                @continue = OnXmlNode(LastNode);

                if (lastNodeName != null && LastNode.Name == lastNodeName)
                    LastNodeArrived.Set();
            }
        }
    }
}