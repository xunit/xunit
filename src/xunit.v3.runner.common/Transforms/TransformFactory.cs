using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Xml.Linq;

namespace Xunit.Runner.Common;

/// <summary>
/// Please use <see cref="RegisteredConsoleResultWriters"/> for console usage, or
/// <see cref="RegisteredMicrosoftTestingPlatformResultWriters"/> for MTP usage.
/// This class will be removed in the next major version.
/// </summary>
[Obsolete("Please use RegisteredConsoleResultWriters for console usage, or RegisteredMicrosoftTestingPlatformResultWriters for MTP usage. This class will be removed in the next major version.", error: true)]
public static class TransformFactory
{
	/// <summary/>
	[EditorBrowsable(EditorBrowsableState.Never)]
	public static IReadOnlyList<Transform> AvailableTransforms =>
		throw new NotSupportedException("This class has been deprecated.");

	/// <summary/>
	[EditorBrowsable(EditorBrowsableState.Never)]
	public static XElement CreateAssembliesElement() =>
		throw new NotSupportedException("This class has been deprecated.");

	/// <summary/>
	[EditorBrowsable(EditorBrowsableState.Never)]
	public static void FinishAssembliesElement(XElement assembliesElement) =>
		throw new NotSupportedException("This class has been deprecated.");

	/// <summary/>
	[EditorBrowsable(EditorBrowsableState.Never)]
	public static List<Action<XElement>> GetXmlTransformers(XunitProject project) =>
		throw new NotSupportedException("This class has been deprecated.");

	/// <summary/>
	[EditorBrowsable(EditorBrowsableState.Never)]
	public static void Transform(
		string id,
		XElement assembliesElement,
		string outputFileName) =>
			throw new NotSupportedException("This class has been deprecated.");
}
