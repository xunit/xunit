using System;
using Xunit.Sdk;

namespace Xunit
{
    /// <summary>
    /// Attribute used to decorate a namespace with arbitrary name/value pairs ("traits").
    /// </summary>
    [TraitDiscoverer("Xunit.Sdk.NamespaceTraitDiscoverer", "xunit.core")]
    [AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
    public sealed class NamespaceTraitAttribute : Attribute, ITraitAttribute
    {
        /// <summary>
        /// Creates a new instance of the <see cref="NamespaceTraitAttribute"/> class.
        /// </summary>
        /// <param name="namespace">The namespace to apply the trait to. Null or empty value will be ignored.</param>
        /// <param name="name">The trait name</param>
        /// <param name="value">The trait value</param>
        public NamespaceTraitAttribute(string @namespace, string name, string value) { }

        /// <summary>
        /// Gets or sets if namespace is case insensitive.
        /// </summary>
        public bool NamespaceCaseInsensitive { get; set; }

        /// <summary>
        /// Gets or sets if the trait should not be applied to nested namespaces.
        /// </summary>
        public bool NotApplicableToNestedNamespaces { get; set; }
    }
}