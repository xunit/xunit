using System;

namespace SpecificationBaseStyle
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
    public class ObservationAttribute : Attribute { }
}