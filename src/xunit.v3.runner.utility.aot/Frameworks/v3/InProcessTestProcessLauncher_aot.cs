using System.ComponentModel;

namespace Xunit.v3;

/// <summary>
/// In-process testing is not supported for Native AOT
/// </summary>
[Obsolete("In-process testing is not supported for Native AOT", error: true)]
[EditorBrowsable(EditorBrowsableState.Never)]
public class InProcessTestProcessLauncher
{ }
