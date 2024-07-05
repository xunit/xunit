using System;
using System.Xml.Linq;
using Xunit.Internal;

namespace Xunit.Runner.Common;

/// <summary>
/// Represents a single report transformation from XML.
/// </summary>
/// <param name="id">The transform ID</param>
/// <param name="description">The transform description</param>
/// <param name="outputHandler">The handler which will write the v2 XML to the given file</param>
public class Transform(
	string id,
	string description,
	Action<XElement, string> outputHandler)
{
	/// <summary>
	/// Gets the transform ID.
	/// </summary>
	public string ID { get; } = Guard.ArgumentNotNull(id);

	/// <summary>
	/// Gets description of the transformation. Suitable for displaying to end users.
	/// </summary>
	public string Description { get; } = Guard.ArgumentNotNull(description);

	/// <summary>
	/// Gets the output handler for the transformation. Converts XML to a file on the
	/// file system.
	/// </summary>
	public Action<XElement, string> OutputHandler { get; } = Guard.ArgumentNotNull(outputHandler);
}
