#if NETFRAMEWORK

using System;
using System.IO;
using System.Security;
using System.Threading;
using System.Web.UI;
using System.Xml;

namespace Xunit.Runner.v1;

/// <summary>
/// An implementation of <see cref="ICallbackEventHandler"/> used to translate v1 Executor XML
/// messages.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="XmlNodeCallbackHandler" /> class.
/// </remarks>
/// <param name="callback">The callback to call when each XML node arrives.</param>
/// <param name="lastNodeName">The name of the expected final XML node, which triggers <see cref="LastNodeArrived"/>.</param>
public class XmlNodeCallbackHandler(
	Predicate<XmlNode?>? callback = null,
	string? lastNodeName = null) :
		MarshalByRefObject, ICallbackEventHandler
{
	bool @continue = true;

	/// <summary>
	/// Gets the last node that was sent.
	/// </summary>
	public XmlNode? LastNode { get; protected set; }

	/// <summary>
	/// Gets an event that is triggered when the last node has arrived.
	/// </summary>
	public ManualResetEvent LastNodeArrived { get; protected set; } = new ManualResetEvent(false);

	/// <inheritdoc/>
	[SecurityCritical]
	public sealed override object InitializeLifetimeService() => null!;

	/// <summary>
	/// Called when an XML node arrives. Dispatches the XML node to the callback.
	/// </summary>
	/// <param name="node">The arriving XML node.</param>
	/// <returns>Return <c>true</c> to continue running tests; <c>false</c> to stop running tests.</returns>
	public virtual bool OnXmlNode(XmlNode? node) =>
		callback is null || callback(node);

#pragma warning disable CA1033 // These are not intended to be part of the public interface of this class

	string ICallbackEventHandler.GetCallbackResult() => @continue.ToString();

	void ICallbackEventHandler.RaiseCallbackEvent(string eventArgument)
	{
		if (eventArgument is not null)
		{
			// REVIEW: Would this be cheaper with XDocument instead of XmlDocument?
			var doc = new XmlDocument();
			using var xmlReader = XmlReader.Create(new StringReader(eventArgument), new XmlReaderSettings() { XmlResolver = null });
			doc.Load(xmlReader);
			LastNode = doc.ChildNodes[0];
			@continue = OnXmlNode(LastNode);

			if (lastNodeName is not null && LastNode?.Name == lastNodeName)
				LastNodeArrived.Set();
		}
	}

#pragma warning restore CA1033
}

#endif
