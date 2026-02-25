#pragma warning disable CA1040 // Avoid empty interfaces

using System.ComponentModel;

namespace Xunit.v3;

/// <summary>
/// Interface-based attributes are not supported in Native AOT; please use
/// <see cref="TheoryAttribute"/> instead
/// </summary>
[Obsolete("Interface-based attributes are not supported in Native AOT; please use TheoryAttribute instead", error: true)]
[EditorBrowsable(EditorBrowsableState.Never)]
public interface ITheoryAttribute : IFactAttribute
{ }
