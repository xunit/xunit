using Xunit.Internal;
using Xunit.v3;

namespace Xunit.Sdk
{
	/// <summary>
	/// Implementation of <see cref="IDataDiscoverer"/> for discovering <see cref="MemberDataAttribute"/>.
	/// </summary>
	public class MemberDataDiscoverer : DataDiscoverer
	{
		/// <inheritdoc/>
		public override bool SupportsDiscoveryEnumeration(
			_IAttributeInfo dataAttribute,
			_IMethodInfo testMethod)
		{
			Guard.ArgumentNotNull(dataAttribute);
			Guard.ArgumentNotNull(testMethod);

			return !dataAttribute.GetNamedArgument<bool>("DisableDiscoveryEnumeration");
		}
	}
}
