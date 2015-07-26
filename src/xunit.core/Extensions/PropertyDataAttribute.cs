using System;
using System.ComponentModel;

namespace Xunit.Extensions
{
    /// <summary/>
    [EditorBrowsable(EditorBrowsableState.Never)]
    [Obsolete("Please replace [PropertyData] with [MemberData]", true)]
    public sealed class PropertyDataAttribute : Attribute
    {
        /// <summary/>
        public PropertyDataAttribute(string propertyName) { }

        /// <summary/>
        public Type PropertyType { get; set; }
    }
}
