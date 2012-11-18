using System;
using System.Diagnostics.CodeAnalysis;

namespace Xunit
{
    [CLSCompliant(false)]
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
    [SuppressMessage("Microsoft.Performance", "CA1813:AvoidUnsealedAttributes", Justification = "This attribute is designed as an extensibility point.")]
    public class InlineData2Attribute : Attribute
    {
        public InlineData2Attribute(params object[] data)
        {
        }
    }
}
