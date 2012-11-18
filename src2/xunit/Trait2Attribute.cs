using System;
using System.Diagnostics.CodeAnalysis;

namespace Xunit
{
    /// <summary>
    /// Attribute used to decorate a test method with arbitrary name/value pairs ("traits").
    /// </summary>
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, AllowMultiple = true)]
    [SuppressMessage("Microsoft.Performance", "CA1813:AvoidUnsealedAttributes", Justification = "This attribute is designed as an extensibility point.")]
    public class Trait2Attribute : Attribute
    {
        /// <summary>
        /// Creates a new instance of the <see cref="TraitAttribute"/> class.
        /// </summary>
        /// <param name="name">The trait name</param>
        /// <param name="value">The trait value</param>
        public Trait2Attribute(string name, string value)
        {
            Name = name;
            Value = value;
        }

        /// <summary>
        /// Gets the trait name.
        /// </summary>
        public string Name { get; private set; }

        /// <inheritdoc/>
        public override object TypeId
        {
            get { return this; }
        }

        /// <summary>
        /// Gets the trait value.
        /// </summary>
        public string Value { get; private set; }
    }
}