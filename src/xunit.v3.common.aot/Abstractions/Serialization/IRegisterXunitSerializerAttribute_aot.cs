#pragma warning disable CA1040 // Avoid empty interfaces

using System.ComponentModel;

namespace Xunit.Sdk;

/// <summary>
/// Serialization is not supported in Native AOT.
/// </summary>
[Obsolete("Serialization is not supported in Native AOT", error: true)]
[EditorBrowsable(EditorBrowsableState.Never)]
public interface IRegisterXunitSerializerAttribute
{ }
