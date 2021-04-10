using System.Collections.Generic;
using Xunit.v3;

namespace Xunit.Sdk
{
	/// <summary>
	/// This interface is implemented by discoverers that provide trait values to
	/// xUnit.net v3 tests.
	/// </summary>
	public interface ITraitDiscoverer
	{
		/// <summary>
		/// Gets the trait values from the trait attribute.
		/// </summary>
		/// <param name="traitAttribute">The trait attribute containing the trait values.</param>
		/// <returns>The trait values.</returns>
		IReadOnlyCollection<KeyValuePair<string, string>> GetTraits(_IAttributeInfo traitAttribute);
	}
}
