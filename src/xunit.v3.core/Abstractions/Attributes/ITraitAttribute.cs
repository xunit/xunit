using System.Collections.Generic;

namespace Xunit.v3;

/// <summary>
/// Base interface that all traits attributes (that is, anything with provides traits to a test).
/// Trait attributes are valid on assemblies, classes, and methods.
/// </summary>
public interface ITraitAttribute
{
	/// <summary>
	/// Gets the trait values from the trait attribute.
	/// </summary>
	/// <returns>The trait values.</returns>
	IReadOnlyCollection<KeyValuePair<string, string>> GetTraits();
}
