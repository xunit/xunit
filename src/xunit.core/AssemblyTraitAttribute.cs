using System;
using Xunit.Sdk;

namespace Xunit
{
    /// <summary>
    /// Attribute used to decorate an assembly with arbitrary name/value pairs ("traits").
    /// </summary>
    [TraitDiscoverer("Xunit.Sdk.AssemblyTraitDiscoverer", "xunit.core")]
    [AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
    public sealed class AssemblyTraitAttribute : Attribute, ITraitAttribute
    {
        /// <summary>
        /// Creates a new instance of the <see cref="AssemblyTraitAttribute"/> class.
        /// </summary>
        /// <param name="name">The trait name</param>
        /// <param name="value">The trait value</param>
        public AssemblyTraitAttribute(string name, string value) { }
    }
}
