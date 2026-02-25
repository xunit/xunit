using System.ComponentModel;
using System.Xml.Linq;

namespace Xunit.Runner.Common;

/// <summary>
/// Please use <see cref="RegisteredRunnerConfig.GetConsoleResultWriters"/> for console usage, or
/// <see cref="RegisteredRunnerConfig.GetMicrosoftTestingPlatformResultWriters"/> for Microsoft
/// Testing Platform usage. This class will be removed in the next major version.
/// </summary>
[Obsolete("Please use RegisteredRunnerConfig.GetConsoleResultWriters for console usage, or RegisteredRunnerConfig.GetMicrosoftTestingPlatformResultWriters for Microsoft Testing Platform usage. This class will be removed in the next major version.", error: true)]
[EditorBrowsable(EditorBrowsableState.Never)]
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
