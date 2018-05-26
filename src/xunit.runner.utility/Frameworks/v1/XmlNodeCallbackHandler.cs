#if NETFRAMEWORK

using System;
using System.Threading;
using System.Web.UI;
using System.Xml;
using Xunit.Sdk;

namespace Xunit
{
    /// <summary>
    /// An implementation of <see cref="ICallbackEventHandler"/> used to translate v1 Executor XML
    /// messages.
    /// </summary>
    public class XmlNodeCallbackHandler : LongLivedMarshalByRefObject, ICallbackEventHandler
    {
        readonly Predicate<XmlNode> callback;
        bool @continue = true;
        readonly string lastNodeName;

        /// <summary>
        /// Initializes a new instance of the <see cref="XmlNodeCallbackHandler" /> class.
        /// </summary>
        /// <param name="callback">The callback to call when each XML node arrives.</param>
        /// <param name="lastNodeName">The name of the expected final XML node, which triggers <see cref="LastNodeArrived"/>.</param>
        public XmlNodeCallbackHandler(Predicate<XmlNode> callback = null, string lastNodeName = null)
        {
            this.callback = callback;
            this.lastNodeName = lastNodeName;

            LastNodeArrived = new ManualResetEvent(false);
        }

        /// <summary>
        /// Gets the last node that was sent.
        /// </summary>
        public XmlNode LastNode { get; protected set; }

        /// <summary>
        /// Gets an event that is triggered when the last node has arrived.
        /// </summary>
        public ManualResetEvent LastNodeArrived { get; protected set; }

        /// <summary>
        /// Called when an XML node arrives. Dispatches the XML node to the callback.
        /// </summary>
        /// <param name="node">The arriving XML node.</param>
        /// <returns>Return <c>true</c> to continue running tests; <c>false</c> to stop running tests.</returns>
        public virtual bool OnXmlNode(XmlNode node)
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

#endif
