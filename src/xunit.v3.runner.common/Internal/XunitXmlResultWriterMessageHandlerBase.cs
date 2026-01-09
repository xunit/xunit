using System;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using Xunit.Runner.Common;
using Xunit.Sdk;

namespace Xunit.Internal;

/// <summary>
/// INTERNAL CLASS. DO NOT USE.
/// </summary>
public abstract class XunitXmlResultWriterMessageHandlerBase<TResultMetadata>(Lazy<XmlWriter> xmlWriter) :
	ResultMetadataMessageHandlerBase<TResultMetadata>, IResultWriterMessageHandler
		where TResultMetadata : ResultMetadataBase
{
	bool disposed;

	/// <summary/>
	internal XElement AssembliesElement { get; } = new XElement("assemblies");

	/// <summary/>
	internal static XElement CreateFailureElement(IErrorMetadata errorMetadata)
	{
		var result = new XElement("failure");

		var exceptionType = Guard.ArgumentNotNull(errorMetadata).ExceptionTypes[0];
		if (exceptionType is not null)
			result.Add(new XAttribute("exception-type", exceptionType));

		var message = ExceptionUtility.CombineMessages(errorMetadata);
		if (!string.IsNullOrWhiteSpace(message))
			result.Add(new XElement("message", XmlUtility.Escape(message, escapeNewlines: false)));

		var stackTrace = ExceptionUtility.CombineStackTraces(errorMetadata);
		if (stackTrace is not null)
			result.Add(new XElement("stack-trace", stackTrace));

		return result;
	}

	/// <inheritdoc/>
	public async ValueTask DisposeAsync()
	{
		GC.SuppressFinalize(this);

		if (!disposed)
			try
			{
				FinalizeXml();
				AssembliesElement.Save(xmlWriter.Value);
				xmlWriter.Value.SafeDispose();
			}
			finally
			{
				disposed = true;
			}
	}

	/// <summary/>
	internal virtual void FinalizeXml()
	{ }
}
