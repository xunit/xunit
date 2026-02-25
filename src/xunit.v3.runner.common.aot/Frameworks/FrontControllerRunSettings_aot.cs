using System.ComponentModel;

namespace Xunit;

partial class FrontControllerRunSettings
{
	/// <summary/>
	[Obsolete("Serialization is not supported in Native AOT", error: true)]
	[EditorBrowsable(EditorBrowsableState.Never)]
	public IReadOnlyCollection<string> SerializedTestCases =>
		throw new PlatformNotSupportedException("Serialization is not supported in Native AOT");
}
