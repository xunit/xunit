using System.ComponentModel;

namespace Xunit.v3;

/// <summary>
/// Reflection-based test discovery and execution is not supported in Native AOT
/// </summary>
[Obsolete("Reflection-based test discovery and execution is not supported in Native AOT", error: true)]
[EditorBrowsable(EditorBrowsableState.Never)]
public class CulturedXunitDelayEnumeratedTheoryTestCase
{ }
