using System;
using System.Collections.Generic;
using Xunit.Internal;
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
		Attribute, ITraitAttribute
{
	/// <summary>
	/// Get the trait name.
	/// </summary>
	public string Name { get; } = Guard.ArgumentNotNull(name);

	/// <summary>
	/// Gets the trait value.
	/// </summary>
	public string Value { get; } = Guard.ArgumentNotNull(value);

	/// <inheritdoc/>
	public IReadOnlyCollection<KeyValuePair<string, string>> GetTraits() =>
		[new(Name, Value)];
}
