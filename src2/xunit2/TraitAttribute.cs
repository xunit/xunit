using System;
using Xunit.Sdk;

namespace Xunit
{
    /// <summary>
    /// Attribute used to decorate a test method with arbitrary name/value pairs ("traits").
    /// </summary>
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, AllowMultiple = true)]
    // TODO: This is sealed today; we may want to come back and revisit opening this up again, and
    // using the discoverer pattern so that it can support Resharper. We may also want to visit the
    // issue of supporting 0..n trait values rather than a forced single value pair model that
    // we have today.
    public sealed class TraitAttribute : AttributeBase
    {
        /// <summary>
        /// Creates a new instance of the <see cref="TraitAttribute"/> class.
        /// </summary>
        /// <param name="name">The trait name</param>
        /// <param name="value">The trait value</param>
        public TraitAttribute(string name, string value)
        {
            Name = name;
            Value = value;
        }

        /// <summary>
        /// Gets the trait name.
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// Gets the trait value.
        /// </summary>
        public string Value { get; private set; }
    }
}