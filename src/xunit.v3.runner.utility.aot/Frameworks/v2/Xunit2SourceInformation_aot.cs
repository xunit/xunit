using System.ComponentModel;

namespace Xunit.Runner.v2;

/// <summary>
/// xUnit.net v2 support in not available in Native AOT
/// </summary>
[Obsolete("xUnit.net v2 support in not available in Native AOT", error: true)]
[EditorBrowsable(EditorBrowsableState.Never)]
public class Xunit2SourceInformation
{ }
