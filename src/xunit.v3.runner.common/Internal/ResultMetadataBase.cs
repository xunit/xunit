using Xunit.Runner.Common;

namespace Xunit.Internal;

/// <summary>
/// INTERNAL CLASS. DO NOT USE.
/// </summary>
public class ResultMetadataBase
{
	internal MessageMetadataCache MetadataCache { get; } = new();
}
