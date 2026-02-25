#pragma warning disable CA1040 // Avoid empty interfaces

using System.ComponentModel;

namespace Xunit.v3;

/// <summary>
/// Type activation is not available in Native AOT because it depends on unavailable reflection features.
/// </summary>
[Obsolete("Type activation is not available in Native AOT because it depends on unavailable reflection features", error: true)]
[EditorBrowsable(EditorBrowsableState.Never)]
public interface ITypeActivator
{ }
