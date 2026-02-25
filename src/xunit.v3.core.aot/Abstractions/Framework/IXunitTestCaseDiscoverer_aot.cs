#pragma warning disable CA1040 // Avoid empty interfaces

using System.ComponentModel;

namespace Xunit.v3;

/// <summary>
/// Test case discovery is done via code generators in Native AOT
/// </summary>
[Obsolete("Test case discovery is done via code generators in Native AOT", error: true)]
[EditorBrowsable(EditorBrowsableState.Never)]
public interface IXunitTestCaseDiscoverer
{ }
