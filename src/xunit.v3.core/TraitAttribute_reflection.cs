#pragma warning disable CA1813 // This attribute is unsealed because it's an extensibility point

using Xunit.v3;

namespace Xunit;

/// <summary>
/// Attribute used to decorate a test method, test class, or assembly with an arbitrary name/value pair ("trait").
/// </summary>
/// <param name="name">The trait name</param>
/// <param name="value">The trait value</param>
[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class | AttributeTargets.Assembly, AllowMultiple = true)]
public class TraitAttribute(
	string name,
	string value) :
		Attribute, ITraitAttribute
{
	/// <summary>
	/// Get the trait name.
	/// </summary>
	public string Name { get; } =
		Guard.ArgumentNotNull(name);

	/// <summary>
	/// Gets the trait value.
	/// </summary>
	public string Value { get; } =
		Guard.ArgumentNotNull(value);

#pragma warning disable CA1024 // This is implementing an interface contract

	/// <inheritdoc/>
	public IReadOnlyCollection<KeyValuePair<string, string>> GetTraits() =>
		[new(Name, Value)];

#pragma warning restore CA1024
}
