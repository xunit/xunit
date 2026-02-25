using System.ComponentModel;
using Xunit.v3;

namespace Xunit;

/// <summary>
/// Attribute used to decorate a test method, test class, or assembly with an arbitrary name/value pair ("trait").
/// </summary>
/// <param name="name">The trait name</param>
/// <param name="value">The trait value</param>
[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class | AttributeTargets.Assembly, AllowMultiple = true)]
public sealed class TraitAttribute(
	string name,
	string value) :
		Attribute
{
	/// <summary>
	/// Get the trait name.
	/// </summary>
	public string Name { get; } = Guard.ArgumentNotNull(name);

	/// <summary>
	/// Gets the trait value.
	/// </summary>
	public string Value { get; } = Guard.ArgumentNotNull(value);

#pragma warning disable CA1024 // This is an obsolete interface contract

	/// <summary>
	/// Support for <c><see cref="ITraitAttribute"/>.GetTraits</c> is not available in Native AOT
	/// </summary>
	[Obsolete("Support for ITraitAttribute.GetTraits is not available in Native AOT", error: true)]
	[EditorBrowsable(EditorBrowsableState.Never)]
	public IReadOnlyCollection<KeyValuePair<string, string>> GetTraits() =>
		throw new PlatformNotSupportedException("Support for ITraitAttribute.GetTraits is not available in Native AOT");

#pragma warning restore CA1024
}
