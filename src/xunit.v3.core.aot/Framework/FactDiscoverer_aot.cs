using System.ComponentModel;

namespace Xunit;

/// <summary>
/// Discoverers are not available in Native AOT, and must be replaced with source generators.
/// </summary>
[Obsolete("Discoverers are not available in Native AOT, and must be replaced with source generators", error: true)]
[EditorBrowsable(EditorBrowsableState.Never)]
public class FactDiscoverer
{ }
