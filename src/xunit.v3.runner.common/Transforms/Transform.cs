using System;
using System.Globalization;
using System.Xml.Linq;
using Xunit.Internal;

namespace Xunit.Runner.Common;

/// <summary>
/// Represents a single report transformation from XML.
/// </summary>
/// <param name="id">The transform ID</param>
/// <param name="description">The transform description</param>
/// <param name="outputHandler">The handler which will write the v2 XML to the given file</param>
/// <param name="mtpShortDescription">The short description of the transform (i.e., <c>"CTRF (JSON)"</c>); will not be presented in MTP if <see langword="null"/></param>
/// <param name="mtpFileExtension">The file extension to use in MTP mode (defaults to <paramref name="id"/>)</param>
public class Transform(
	string id,
	string description,
	Action<XElement, string> outputHandler,
	string? mtpShortDescription = null,
	string? mtpFileExtension = null)
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
	/// Gets the description of the primary option in Microsoft.Testing.Platform mode.
	/// </summary>
	public string? MTPDescription { get; } =
		mtpShortDescription is not null
			? string.Format(
				CultureInfo.CurrentCulture,
				"Enable generating {0} report",
				Guard.ArgumentNotNull(mtpShortDescription)
			)
			: null;

	/// <summary>
	/// Gets the file extension for the Microsoft.Testing.Platform report.
	/// </summary>
#pragma warning disable CA1308 // This is for UX purposes, not comparison purposes
	public string MTPFileExtension { get; } = mtpFileExtension ?? id.ToLowerInvariant();
#pragma warning restore CA1308

	/// <summary>
	/// Gets the description of the filename option in Microsoft.Testing.Platform mode.
	/// </summary>
	public string? MTPFileNameDescription { get; } =
		mtpShortDescription is not null
			? string.Format(
				CultureInfo.CurrentCulture,
				"The name of the generated {0} report",
				Guard.ArgumentNotNull(mtpShortDescription)
			)
			: null;

	/// <summary>
	/// Gets the output handler for the transformation. Converts XML to a file on the
	/// file system.
	/// </summary>
	public Action<XElement, string> OutputHandler { get; } = Guard.ArgumentNotNull(outputHandler);
}
