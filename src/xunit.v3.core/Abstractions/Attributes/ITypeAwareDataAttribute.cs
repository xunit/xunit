using System;
using System.Reflection;

namespace Xunit.v3;

/// <summary>
/// This interface supplements <see cref="IDataAttribute"/> for attributes that wish to be
/// notified of the <see cref="MemberInfo.ReflectedType"/> of the method that was used during
/// reflection that they were attached to.
/// </summary>
/// <remarks>
/// By default, this is called by <see cref="ExtensibilityPointFactory.GetMethodDataAttributes"/>.
/// If a third party test framework uses some other mechanism to discover data attributes that are
/// attached to methods, it should conditionally populate the <see cref="MemberType"/> during its
/// own discovery phase.
/// </remarks>
public interface ITypeAwareDataAttribute
{
	/// <summary>
	/// Gets or sets the reflected type of the method that this data attribute was attached to.
	/// </summary>
	/// <remarks>
	/// <see cref="ExtensibilityPointFactory"/> will not be overwrite this value if it's already non-<c>null</c>.
	/// </remarks>
	Type? MemberType { get; set; }
}
