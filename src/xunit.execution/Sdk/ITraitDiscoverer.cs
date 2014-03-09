using System.Collections.Generic;
using Xunit.Abstractions;

namespace Xunit.Sdk
{
    public interface ITraitDiscoverer
    {
        IEnumerable<KeyValuePair<string, string>> GetTraits(IAttributeInfo traitAttribute);
    }
}
