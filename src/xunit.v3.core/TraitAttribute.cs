using System;
using Xunit.Sdk;

namespace Xunit
{
    /// <summary>
    /// Attribute used to decorate a test method with arbitrary name/value pairs ("traits").
    /// </summary>
    [TraitDiscoverer("Xunit.Sdk.TraitDiscoverer", "xunit.core")]
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, AllowMultiple = true)]
    public sealed class TraitAttribute : Attribute, ITraitAttribute
    {
        /// <summary>
        /// Creates a new instance of the <see cref="TraitAttribute"/> class.
        /// </summary>
        /// <param name="name">The trait name</param>
        /// <param name="value">The trait value</param>
        public TraitAttribute(string name, string value) { }
    }
}
