#pragma warning disable CA1040 // Avoid empty interfaces

using System.ComponentModel;

namespace Xunit.v3;

/// <summary>
/// Please use <see cref="ICodeGenTestCollection"/> in Native AOT
/// </summary>
[Obsolete("Please use ICodeGenTestCollection in Native AOT", error: true)]
[EditorBrowsable(EditorBrowsableState.Never)]
public interface IXunitTestCollection
{ }
