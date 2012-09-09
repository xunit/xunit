using System;
using System.Diagnostics.CodeAnalysis;

namespace Xunit
{
    [CLSCompliant(false)]
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
    [SuppressMessage("Microsoft.Performance", "CA1813:AvoidUnsealedAttributes", Justification = "This attribute is designed as an extensibility point.")]
    public class InlineDataAttribute : Attribute
    {
        public InlineDataAttribute(params object[] data)
        {
        }
    }
}
