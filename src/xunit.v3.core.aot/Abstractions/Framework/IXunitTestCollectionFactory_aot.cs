#pragma warning disable CA1040 // Avoid empty interfaces

using System.ComponentModel;

namespace Xunit.v3;

/// <summary>
/// Please use <see cref="ICodeGenTestCollectionFactory"/>.
/// Reflection-based test collections are not supported in Native AOT.
/// </summary>
[Obsolete("Please use ICodeGenTestCollectionFactory. Reflection based test collections are not supported in Native AOT.", error: true)]
[EditorBrowsable(EditorBrowsableState.Never)]
public interface IXunitTestCollectionFactory
{ }
