using Xunit.Abstractions;

namespace Xunit.Sdk
{
    /// <summary>
    /// Implementation of <see cref="IDataDiscoverer"/> for discovering <see cref="MemberDataAttribute"/>.
    /// </summary>
    public class MemberDataDiscoverer : DataDiscoverer
    {
        /// <inheritdoc/>
        public override bool SupportsDiscoveryEnumeration(IAttributeInfo dataAttribute, IMethodInfo testMethod)
        {
            return !dataAttribute.GetNamedArgument<bool>("DisableDiscoveryEnumeration");
        }
    }
}
