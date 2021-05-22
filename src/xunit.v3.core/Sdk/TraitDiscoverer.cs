using System.Collections.Generic;
using System.Linq;
using Xunit.Internal;
using Xunit.v3;

namespace Xunit.Sdk
{
	/// <summary>
	/// The implementation of <see cref="ITraitDiscoverer"/> which returns the trait values
	/// for <see cref="TraitAttribute"/>.
	/// </summary>
	public class TraitDiscoverer : ITraitDiscoverer
	{
		/// <inheritdoc/>
		public virtual IReadOnlyCollection<KeyValuePair<string, string>> GetTraits(_IAttributeInfo traitAttribute)
		{
			Guard.ArgumentNotNull(nameof(traitAttribute), traitAttribute);

			var ctorArgs =
				traitAttribute
					.GetConstructorArguments()
					.Cast<string>()
					.ToList();

			return new[] { new KeyValuePair<string, string>(ctorArgs[0], ctorArgs[1]) };
		}
	}
}
