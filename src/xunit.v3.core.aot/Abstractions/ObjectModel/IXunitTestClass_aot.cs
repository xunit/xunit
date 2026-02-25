#pragma warning disable CA1040 // Avoid empty interfaces

using System.ComponentModel;

namespace Xunit.v3;

/// <summary>
/// Please use <see cref="ICodeGenTestClass"/> in Native AOT
/// </summary>
[Obsolete("Please use ICodeGenTestClass in Native AOT", error: true)]
[EditorBrowsable(EditorBrowsableState.Never)]
public interface IXunitTestClass
{ }
