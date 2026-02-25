using System.ComponentModel;

namespace Xunit.Runner.v1;

/// <summary>
/// xUnit.net v1 support in not available in Native AOT
/// </summary>
[Obsolete("xUnit.net v1 support in not available in Native AOT", error: true)]
[EditorBrowsable(EditorBrowsableState.Never)]
public class Xunit1RunSummary
{ }
