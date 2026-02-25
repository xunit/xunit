#pragma warning disable CA1040 // Avoid empty interfaces

using System.ComponentModel;

namespace Xunit.v3;

/// <summary>
/// Interface-based attributes are not supported in Native AOT; please use
/// <see cref="TraitAttribute"/> instead
/// </summary>
[Obsolete("Interface-based attributes are not supported in Native AOT; please use TraitAttribute instead", error: true)]
[EditorBrowsable(EditorBrowsableState.Never)]
public interface ITraitAttribute
{ }
