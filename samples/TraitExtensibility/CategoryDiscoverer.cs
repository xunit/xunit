using System.Linq;
using Xunit.Sdk;
using Xunit.Abstractions;
using System.Collections.Generic;

/// <summary>
/// This class discovers all of the tests and test classes that have
/// applied the Category attribute
/// </summary>
public class CategoryDiscoverer : ITraitDiscoverer
{
    /// <summary>
    /// Gets the trait values from the Category attribute.
    /// </summary>
    /// <param name="traitAttribute">The trait attribute containing the trait values.</param>
    /// <returns>The trait values.</returns>
    public IEnumerable<KeyValuePair<string, string>> GetTraits(IAttributeInfo traitAttribute)
    {
        var ctorArgs = traitAttribute.GetConstructorArguments().ToList();
        yield return new KeyValuePair<string, string>("Category", ctorArgs[0].ToString());
    }
}