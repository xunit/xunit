using System.Collections.Generic;
using Xunit.Abstractions;

namespace Xunit.Sdk
{
    /// <summary>
    /// This interface is implemented by discoverers that provide trait values to
    /// xUnit.net v2 tests and require method information.
    /// </summary>
    public interface IMethodedTraitDiscoverer
    {
        /// <summary>
        /// Gets the trait values from the trait attribute.
        /// </summary>
        /// <param name="traitAttribute">The trait attribute containing the trait values.</param>
        /// <param name="methodInfo">The method for which traits should be produced.</param>
        /// <returns>The trait values.</returns>
        IEnumerable<KeyValuePair<string, string>> GetTraits(IAttributeInfo traitAttribute, IMethodInfo methodInfo);
    }
}