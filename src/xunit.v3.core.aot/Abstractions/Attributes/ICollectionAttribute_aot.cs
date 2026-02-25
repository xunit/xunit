#pragma warning disable CA1040 // Avoid empty interfaces

using System.ComponentModel;

namespace Xunit.v3;

/// <summary>
/// Interface-based attributes are not supported in Native AOT; please use
/// <see cref="CollectionAttribute"/> instead
/// </summary>
[Obsolete("Interface-based attributes are not supported in Native AOT; please use CollectionAttribute instead", error: true)]
[EditorBrowsable(EditorBrowsableState.Never)]
public interface ICollectionAttribute
{ }
